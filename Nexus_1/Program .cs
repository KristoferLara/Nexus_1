using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

class Program
{
    const int EstabilidadMinimaDesconexion = 15;

    // ===== SISTEMA DE PODER =====
    const int UmbralPoderHabilidad = 100;       // Poder necesario para activar la capacidad especial
    const int DuracionCapacidadEspecial = 3;   // Turnos que dura el efecto activo
    // PoderPorEnemigoDerrotado se mudó a Combate: es una recompensa que pertenece
    // exclusivamente a la lógica de combate (ver clase Combate).

    // ===== SISTEMA DE BOTIQUINES =====
    static readonly Botiquin BotiquinMapa = new Botiquin("Botiquín del mapa", 25);

    static event Action<int> AnomaliaDetectada;

    class EstadoJuego
    {
        public string Nombre;
        public string Realidad;
        // Energia ya no se guarda aquí: ahora vive únicamente en MiCadete.Energia (POO).
        // Estabilidad ya no se guarda aquí: ahora vive únicamente en MiCadete.Estabilidad (POO).
        public bool Conectado = true;

        // ===== SISTEMA IRIS (POO) =====
        // El bloqueo temporal y la anomalía localizada ahora viven en la clase Iris.
        public Iris SistemaIris = new Iris();

        // ===== Sistema de clases =====
        public string TipoPersonaje = "";   // "FISICO", "ARMAMENTO", "MANA" o "HERRAMIENTAS"
        public bool TieneArma = false;      // Solo el cadete de ARMAS empieza con esto en true

        // ===== Vida y Poder =====
        // Vida ya no se guarda aquí: ahora vive únicamente en MiCadete.Vida (POO).
        // Poder ya no se guarda aquí: ahora vive únicamente en MiCadete.Poder (POO).
        // RecursosRecolectados, CapacidadActiva y TurnosCapacidadActiva ya no se guardan aquí:
        // ahora viven únicamente en MiCadete (POO).

        // ===== Exploración territorial =====
        // DistanciaRecorrida ya no vive aquí: ahora es responsabilidad de
        // ExploracionGenesis, que la actualiza junto con el resto del progreso
        // de una sesión de exploración (ver clase ExploracionGenesis).

        // ===== MAPA GENESIS =====
        // La cuadrícula, las zonas exploradas y la posición del jugador en GENESIS
        // ahora viven en la clase MapaGenesis (separación de responsabilidades).
        public MapaGenesis Genesis = new MapaGenesis();

        // ===== EXPLORACIÓN GENESIS (POO) =====
        // Coordina la sesión de exploración (enemigos, botiquines, distancia).
        // Se crea en Main una vez que MiCadete ya existe, porque la necesita
        // para aplicar botiquines (ver ExploracionGenesis).
        public ExploracionGenesis Exploracion;

        // ===== BASE OPERATIVA (POO) =====
        // El mapa de la Base y la posición del cadete dentro de ella ahora
        // viven en la clase BaseOperativa (mismo rol que MapaGenesis, pero
        // para la Base). Su posición inicial se fija sola en el constructor.
        public BaseOperativa Base = new BaseOperativa();

        // ===== ENEMIGOS GENESIS =====
        // Antes eran tres arreglos paralelos (bool/int/string); ahora cada enemigo
        // es un objeto Enemigo que agrupa su posición, tipo y vida. La lista sigue
        // viviendo aquí (es estado global de la partida); ExploracionGenesis recibe
        // una referencia a ella y la usa para los encuentros.
        public List<Enemigo> EnemigosGenesis = new List<Enemigo>();

        // ===== CLASE CADETE (POO) =====
        // Representa al cadete/explorador dentro de NEXUS.
        public Cadete MiCadete;

        // ===== SISTEMA DE MISIONES (POO) =====
        // Las 3 misiones, en el orden obligatorio: LIQUIDO, FUEGO, VENENO.
        // MisionActualIndice solo avanza cuando el Terminal de la Base entrega
        // la recompensa (ver InteractuarTerminalBase), nunca al derrotar al
        // enemigo. Eso es lo que garantiza que sean secuenciales.
        public List<Mision> Misiones = new List<Mision>
        {
            new Mision(1, "💧 EL PRIMER OBJETIVO", "Derrota al enemigo LIQUIDO (6,5).", "LIQUIDO",
                "Este enemigo reduce el daño de tus ataques. Ataca rápido antes de que te desgaste.", 20, 50),
            new Mision(2, "🔥 LA PRUEBA DE FUEGO", "Derrota al enemigo FUEGO (4,8).", "FUEGO",
                "Este enemigo golpea cada vez más fuerte. Cuida tu vida y no alargues el combate.", 20, 50),
            new Mision(3, "☠ EL ÚLTIMO VENENO", "Derrota al enemigo VENENO (2,3).", "VENENO",
                "Este enemigo te quita vida en cada turno. Termina el combate lo antes posible.", 20, 50),
        };
        public int MisionActualIndice = 0;

        // La misión vigente, o null si ya se completaron las 3.
        public Mision MisionActual => (MisionActualIndice < Misiones.Count) ? Misiones[MisionActualIndice] : null;
    }

    // =====================================================
    // CLASE MAPAGENESIS
    // Responsable exclusivamente de la cuadrícula de GENESIS,
    // la posición del jugador dentro de ella y las zonas exploradas.
    // No maneja combate, enemigos, IRIS, inventario ni interfaz completa:
    // esas responsabilidades siguen en EstadoJuego/Program.
    // =====================================================
    class MapaGenesis
    {
        public const int MetrosPorCelda = 5;

        public char[,] Mapa { get; }
        public bool[,] ZonasExploradas { get; }
        public int FilaJugador { get; private set; }
        public int ColumnaJugador { get; private set; }

        public MapaGenesis()
        {
            Mapa = new char[,]
            {
                { '|', '|', '|', '|', '|', '|', '|', '|', '|', '|', '|' },
                { '|', '⌂', '·', '·', '·', '◆', '·', '·', '·', '·', '|' },
                { '|', '·', '·', '·', '·', '·', '·', '◆', '·', '·', '|' },
                { '|', '·', '·', '·', '⚠', '·', '·', 'A', '·', '·', '|' },
                { '|', '·', '◆', '·', '·', '·', 'A', '·', '·', '·', '|' },
                { '|', '·', '·', '·', '·', '◆', '·', '·', '·', '·', '|' },
                { '|', '·', '·', '·', '·', '·', '·', '·', '·', '◆', '|' },
                { '|', '|', '|', '|', '|', '|', '|', '|', '|', '|', '|' }
            };

            ZonasExploradas = new bool[Mapa.GetLength(0), Mapa.GetLength(1)];

            EstablecerPosicionJugador(Mapa.GetLength(0) / 2, Mapa.GetLength(1) / 2);
            RevelarZonaCercana();
        }

        public char ObtenerCelda(int fila, int columna)
        {
            return Mapa[fila, columna];
        }

        public void CambiarCelda(int fila, int columna, char valor)
        {
            Mapa[fila, columna] = valor;
        }

        public bool EsPosicionValida(int fila, int columna)
        {
            return fila >= 0 && fila < Mapa.GetLength(0) &&
                   columna >= 0 && columna < Mapa.GetLength(1);
        }

        public bool EsPared(int fila, int columna)
        {
            return Mapa[fila, columna] == '|';
        }

        public bool EstaExplorada(int fila, int columna)
        {
            return ZonasExploradas[fila, columna];
        }

        public void EstablecerPosicionJugador(int fila, int columna)
        {
            FilaJugador = fila;
            ColumnaJugador = columna;
        }

        public void MoverJugador(int nuevaFila, int nuevaColumna)
        {
            EstablecerPosicionJugador(nuevaFila, nuevaColumna);
            RevelarZonaCercana();
        }

        public void RevelarZonaCercana()
        {
            for (int deltaFila = -1; deltaFila <= 1; deltaFila++)
            {
                for (int deltaColumna = -1; deltaColumna <= 1; deltaColumna++)
                {
                    int fila = FilaJugador + deltaFila;
                    int columna = ColumnaJugador + deltaColumna;

                    if (EsPosicionValida(fila, columna))
                    {
                        ZonasExploradas[fila, columna] = true;
                    }
                }
            }
        }
    }

    // =====================================================
    // CLASE BASEOPERATIVA
    // Mismo rol que MapaGenesis, pero para la Base: la cuadrícula, la
    // posición del cadete dentro de ella y las reglas de movimiento
    // (colisión con paredes). No maneja interacciones (cama, terminal,
    // equipamiento) ni interfaz: eso sigue siendo responsabilidad de
    // Program.
    // =====================================================
    class BaseOperativa
    {
        public char[,] Mapa { get; }
        public int FilaJugador { get; private set; }
        public int ColumnaJugador { get; private set; }

        public BaseOperativa()
        {
            Mapa = new char[,]
            {
                { '|', '|', '|', '|', '|', '|', '|', '|', '|', '|' },
                { '|', '·', '·', 'C', '·', '·', '·', 'T', '·', '|' },
                { '|', '·', '·', '·', '·', '·', '·', '·', '·', '|' },
                { '|', '·', '·', 'E', '·', '·', '·', '·', '·', '|' },
                { '|', '·', '·', '·', '·', 'S', '·', '·', 'B', '|' },
                { '|', '|', '|', '|', '|', '|', '|', '|', '|', '|' }
            };

            // La posición inicial se fija sola, igual que ya hace MapaGenesis
            // (antes dependía de un flag "PosicionBaseInicializada" en EstadoJuego).
            FilaJugador = 2;
            ColumnaJugador = 2;
        }

        public char ObtenerCelda(int fila, int columna)
        {
            return Mapa[fila, columna];
        }

        public bool EsPosicionValida(int fila, int columna)
        {
            return fila >= 0 && fila < Mapa.GetLength(0) &&
                   columna >= 0 && columna < Mapa.GetLength(1);
        }

        public bool EsPared(int fila, int columna)
        {
            return Mapa[fila, columna] == '|';
        }

        // Intenta mover al cadete a la posición indicada. Devuelve false (y no
        // mueve nada) si la posición está fuera del mapa o es una pared; las
        // mismas reglas que tenía MostrarBase antes de la migración.
        public bool IntentarMover(int nuevaFila, int nuevaColumna)
        {
            if (!EsPosicionValida(nuevaFila, nuevaColumna) || EsPared(nuevaFila, nuevaColumna))
            {
                return false;
            }

            FilaJugador = nuevaFila;
            ColumnaJugador = nuevaColumna;
            return true;
        }
    }

    // =====================================================
    // CLASE IRIS
    // Representa el estado del sistema IRIS dentro de NEXUS: si está
    // bloqueada temporalmente (interferencia de MANA) y si hay una
    // anomalía localizada. No imprime nada por consola ni decide cuándo
    // disparar la alerta crítica: eso sigue siendo responsabilidad de
    // Program (ver InterfazNexus.DispararAlertaIris), que solo consulta este estado.
    // =====================================================
    class Iris
    {
        public int TurnosBloqueo { get; private set; } = 0;
        public bool AnomaliaLocalizada { get; private set; } = false;
        public string UbicacionAnomalia { get; private set; } = "Desconocida";

        public bool Bloqueada => TurnosBloqueo > 0;

        // Intenta interferir a IRIS (habilidad de MANA). No tiene efecto si ya
        // está bloqueada. Devuelve true si la interferencia se aplicó.
        public bool Interferir(int turnos)
        {
            if (Bloqueada)
            {
                return false;
            }

            TurnosBloqueo = turnos;
            return true;
        }

        // Descuenta un turno del bloqueo actual, si lo hay.
        // Devuelve true si había bloqueo (y por lo tanto se descontó un turno).
        public bool DescontarTurnoBloqueo()
        {
            if (TurnosBloqueo <= 0)
            {
                return false;
            }

            TurnosBloqueo--;
            return true;
        }

        // Registra una anomalía localizada en el sector indicado.
        public void RegistrarAnomalia(string ubicacion)
        {
            UbicacionAnomalia = ubicacion;
            AnomaliaLocalizada = true;
        }
    }

    class Botiquin
    {
        public string Nombre { get; }
        public int Curación { get; }

        public Botiquin(string nombre, int curación)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                throw new ArgumentException("Nombre no nulo ni vacío.", nameof(nombre));
            }

            if (curación <= 0)
            {
                throw new ArgumentException("Curación mayor que cero.", nameof(curación));
            }

            Nombre = nombre;
            Curación = curación;
        }
    }

    // =====================================================
    // CLASE ENEMIGO
    // Representa a un enemigo del mapa GENESIS: su posición, su tipo
    // y su vida. Encapsula el daño recibido para que la vida no pueda
    // bajar de 0 desde fuera de la clase.
    // =====================================================
    class Enemigo
    {
        public const int VidaMaxima = 25;

        public int Fila { get; }
        public int Columna { get; }
        public string Tipo { get; }
        public int Vida { get; private set; }

        // Identidad propia de cada enemigo orgánico (VENENO, FUEGO, LIQUIDO):
        // decide qué efecto aplica Combate durante el combate animado, a partir
        // de la segunda vez que el cadete defiende. "" para cualquier enemigo
        // sin efecto especial todavía.
        public string Efecto { get; }

        public bool Derrotado => Vida <= 0;

        public Enemigo(int fila, int columna, string tipo, int vidaInicial, string efecto = "")
        {
            Fila = fila;
            Columna = columna;
            Tipo = tipo;
            Vida = vidaInicial;
            Efecto = efecto;
        }

        // Reduce la vida del enemigo sin dejar que baje de 0.
        public void RecibirDanio(int daño)
        {
            Vida -= daño;
            if (Vida < 0)
            {
                Vida = 0;
            }
        }
    }

    // =====================================================
    // CLASE CADETE
    // Representa al jugador/explorador de NEXUS.
    // Encapsula sus atributos vitales (Energia, Vida, etc.)
    // para que solo puedan cambiar a través de sus propios métodos.
    // =====================================================
    class Cadete
    {
        public string Nombre { get; private set; }
        public int Energia { get; private set; }
        public int Estabilidad { get; private set; }
        public int Vida { get; private set; }
        public int Poder { get; private set; }
        public string Realidad { get; private set; }
        public int RecursosRecolectados { get; private set; }
        public bool CapacidadActiva { get; private set; }
        public int TurnosCapacidadActiva { get; private set; }

        // Experiencia del sistema de misiones. A diferencia de Energia/Estabilidad/
        // Poder, no tiene techo: cada misión suma 50 y no hay motivo para limitarla.
        public int Experiencia { get; private set; }

        // Cada 100 XP el cadete sube un nivel. Es una property calculada: no se
        // guarda, así que nunca puede desincronizarse de Experiencia.
        public const int ExperienciaPorNivel = 100;
        public int Nivel
        {
            get { return (Experiencia / ExperienciaPorNivel) + 1; }
        }

        public Cadete(string nombre, int energia, int estabilidad, int vida, int poder, string realidad)
        {
            Nombre = nombre;
            Energia = energia;
            Estabilidad = estabilidad;
            Vida = vida;
            Poder = poder;
            Realidad = realidad;
            RecursosRecolectados = 0;
            CapacidadActiva = false;
            TurnosCapacidadActiva = 0;
            Experiencia = 0;
        }

        // Aumenta la experiencia del cadete (recompensa de misión completada).
        public void GanarExperiencia(int cantidad)
        {
            Experiencia += cantidad;
        }

        // Reduce la energía del cadete sin dejar que baje de 0.
        public void ConsumirEnergia(int cantidad)
        {
            Energia -= cantidad;
            if (Energia < 0)
            {
                Energia = 0;
            }
        }

        // Recupera energía sin dejar que supere el 100.
        public void RecuperarEnergia(int cantidad)
        {
            Energia += cantidad;
            if (Energia > 100)
            {
                Energia = 100;
            }
        }

        // Recupera estabilidad sin dejar que supere el 100.
        // (En NEXUS, Estabilidad solo aumenta: bonus de clase, combate y descanso.)
        public void RecuperarEstabilidad(int cantidad)
        {
            Estabilidad += cantidad;
            if (Estabilidad > 100)
            {
                Estabilidad = 100;
            }
        }

        // Recupera vida sin dejar que supere el máximo (100). Usado por los botiquines del mapa.
        public void RecuperarVida(int cantidad)
        {
            Vida += cantidad;
            if (Vida > 100)
            {
                Vida = 100;
            }
        }

        // Aumenta el poder sin dejar que supere el 100 (recolectar recurso, derrotar enemigo).
        public void GanarPoder(int cantidad)
        {
            Poder += cantidad;
            if (Poder > 100)
            {
                Poder = 100;
            }
        }

        // Reduce el poder sin dejar que baje de 0 (activar habilidad especial).
        public void ConsumirPoder(int cantidad)
        {
            Poder -= cantidad;
            if (Poder < 0)
            {
                Poder = 0;
            }
        }

        // Aplica daño recibido en combate sin dejar que la vida baje de 0.
        public void RecibirDanio(int cantidad)
        {
            Vida -= cantidad;
            if (Vida < 0)
            {
                Vida = 0;
            }
        }

        // El cadete ataca, causando una cantidad de daño determinada.
        public void Atacar(int danio)
        {
            Console.WriteLine(Nombre + " ataca causando " + danio + " de daño.");
        }

        // Suma un recurso recolectado (recurso del mapa o recompensa de enemigo derrotado).
        public void RecolectarRecurso()
        {
            RecursosRecolectados++;
        }

        // Activa la capacidad especial durante la cantidad de turnos indicada.
        public void ActivarCapacidadEspecial(int duracion)
        {
            CapacidadActiva = true;
            TurnosCapacidadActiva = duracion;
        }

        // Descuenta un turno de la capacidad especial activa.
        // No hace nada si no hay turnos restantes (capacidad ya inactiva).
        // La desactiva automáticamente al llegar a 0 turnos.
        public void DescontarTurnoCapacidadEspecial()
        {
            if (TurnosCapacidadActiva <= 0)
            {
                return;
            }

            TurnosCapacidadActiva--;

            if (TurnosCapacidadActiva <= 0)
            {
                CapacidadActiva = false;
            }
        }
    }

    // =====================================================
    // CLASE COMBATE
    // Coordina un enfrentamiento entre un Cadete y un Enemigo.
    // Centraliza las reglas del combate (ataque, contraataque, huida
    // y recompensa de victoria), pero delega en Cadete y Enemigo el
    // control de sus propios atributos (Vida, Poder, etc.).
    // No dibuja pantallas, no crea enemigos, no administra el mapa
    // ni el estado global del juego: eso sigue siendo responsabilidad
    // de Program/EstadoJuego.
    // =====================================================
    class Combate
    {
        // Reglas actuales del combate :
        public const int DañoAtaqueJugador = 12;
        public const int DañoEnemigoMinimo = 8;
        public const int DañoEnemigoMaximo = 16; // exclusivo: rng.Next(8, 16) como antes (solo combate clásico)
        public const int PoderPorEnemigoDerrotado = 20;
        public const int DañoPorHuir = 10;

        // ----- Efectos de los enemigos orgánicos (combate animado) -----
        // Daño fijo del enemigo (en vez del rango aleatorio de arriba, que
        // sigue usando el combate clásico). A partir de la segunda vez que el
        // cadete defiende, cada enemigo suma su propio efecto:
        //   LIQUIDO (6,5): reduce el ataque del cadete.
        //   FUEGO   (4,8): golpea más fuerte.
        //   VENENO  (2,3): daño base normal, pero sigue quitando vida cada
        //                  turno y reduce un poco el ataque del cadete.
        public const int DañoEnemigoOrganico = 10;
        const double ReduccionAtaquePorLiquido = 0.30;
        const double BonoDanioPorFuego = 0.15;
        const double ReduccionAtaquePorVeneno = 0.05;
        const int DañoPorTurnoVeneno = 5;

        // Generador propio para el daño aleatorio del contraataque. El "rng" de
        // Program se usa para otras cosas (p. ej. sectores de anomalías) y no es
        // responsabilidad del combate compartirlo.
        static Random rng = new Random();

        public Cadete Jugador { get; }
        public Enemigo EnemigoActual { get; }

        // Se activa en cuanto el cadete defiende por primera vez en el combate
        // animado: el primer ataque y la primera defensa son siempre el valor
        // base, sin ningún efecto todavía.
        bool efectosOrganicosActivos = false;

        public Combate(Cadete jugador, Enemigo enemigo)
        {
            Jugador = jugador;
            EnemigoActual = enemigo;
        }

        // Aplica el ataque del jugador sobre el enemigo actual.
        // Si el enemigo queda derrotado, entrega de una vez la recompensa de Poder.
        // Devuelve true si el enemigo fue derrotado.
        // "mostrarMensaje" es false en el combate animado: Cadete.Atacar solo imprime
        // una línea de texto y ese texto rompería la pantalla animada. Por defecto
        // (true) el combate clásico se comporta exactamente como antes.
        // "otorgarPoderInmediato" es false cuando este combate es contra el
        // objetivo de la misión actual: el +20 Poder lo entrega el Terminal de
        // la Base al completar la misión, no el combate (evita duplicarlo).
        public bool Atacar(int daño, bool mostrarMensaje = true, bool otorgarPoderInmediato = true)
        {
            if (mostrarMensaje)
            {
                Jugador.Atacar(daño);
            }

            EnemigoActual.RecibirDanio(daño);

            if (EnemigoActual.Derrotado)
            {
                if (otorgarPoderInmediato)
                {
                    Jugador.GanarPoder(PoderPorEnemigoDerrotado);
                }

                return true;
            }

            return false;
        }

        // Versión de Atacar() para el combate animado: aplica la reducción de
        // LIQUIDO o VENENO si ya está activa, y devuelve en "dañoAplicado" el
        // daño realmente hecho (para que Program lo muestre en el mensaje).
        // No imprime texto (el combate animado arma su propio mensaje).
        // Mismo criterio que Atacar(): "otorgarPoderInmediato" en false cuando
        // el Terminal de la Base es quien va a entregar la recompensa.
        public bool AtacarOrganico(int daño, out int dañoAplicado, bool otorgarPoderInmediato = true)
        {
            if (efectosOrganicosActivos)
            {
                if (EnemigoActual.Efecto == "LIQUIDO")
                {
                    daño = (int)(daño * (1 - ReduccionAtaquePorLiquido));
                }
                else if (EnemigoActual.Efecto == "VENENO")
                {
                    daño = (int)(daño * (1 - ReduccionAtaquePorVeneno));
                }
            }

            dañoAplicado = daño;
            EnemigoActual.RecibirDanio(daño);

            if (EnemigoActual.Derrotado)
            {
                if (otorgarPoderInmediato)
                {
                    Jugador.GanarPoder(PoderPorEnemigoDerrotado);
                }

                return true;
            }

            return false;
        }

        // Genera y aplica el contraataque del enemigo sobre el jugador.
        // Si "reducidoPorDefensa" es true (acción Defender), el daño se reduce a la mitad.
        // Devuelve el daño realmente aplicado, para que Program pueda mostrarlo.
        public int Contraatacar(bool reducidoPorDefensa = false)
        {
            int daño = rng.Next(DañoEnemigoMinimo, DañoEnemigoMaximo);

            if (reducidoPorDefensa)
            {
                daño = daño / 2;
            }

            Jugador.RecibirDanio(daño);
            return daño;
        }

        // Versión de Contraatacar() para el combate animado: daño fijo
        // (DañoEnemigoOrganico) más el efecto propio del enemigo, activo desde
        // esta misma llamada si ya hubo una defensa antes. Devuelve el daño
        // total aplicado (incluye el veneno, si corresponde).
        public int ContraatacarOrganico()
        {
            bool efectosActivosAhora = efectosOrganicosActivos;
            int daño = DañoEnemigoOrganico;

            if (efectosActivosAhora && EnemigoActual.Efecto == "FUEGO")
            {
                daño = (int)(daño * (1 + BonoDanioPorFuego));
            }

            Jugador.RecibirDanio(daño);

            if (efectosActivosAhora && EnemigoActual.Efecto == "VENENO")
            {
                Jugador.RecibirDanio(DañoPorTurnoVeneno);
                daño += DañoPorTurnoVeneno;
            }

            efectosOrganicosActivos = true;
            return daño;
        }

        // El jugador decide huir: el combate termina, pero ahora tiene un costo
        // fijo de vida (antes no había ninguna penalización).
        public bool Huir()
        {
            Jugador.RecibirDanio(DañoPorHuir);
            return true;
        }

        // Indica si el jugador cayó en este combate.
        public bool JugadorDerrotado => Jugador.Vida <= 0;
    }

    // =====================================================
    // CLASE MISION
    // Representa una misión de NEXUS: a qué enemigo apunta (su Efecto, el
    // mismo campo que ya tiene Enemigo), su consejo/advertencia, y el estado
    // de avance. Es solo una "ficha de datos" con encapsulamiento (mismo
    // estilo que Cadete): nada aquí sabe de combate, de la Base ni de
    // consola — eso lo coordina Program.
    // =====================================================
    class Mision
    {
        public int ID { get; }
        public string Nombre { get; }
        public string Descripcion { get; }
        public string TipoEnemigo { get; }   // coincide con Enemigo.Efecto
        public string Consejo { get; }
        public int RecompensaPoder { get; }
        public int RecompensaExperiencia { get; }

        // El enemigo ya fue derrotado, pero la recompensa todavía no se reclamó.
        public bool ObjetivoCumplido { get; private set; }

        // La recompensa ya fue entregada (evita reclamarla dos veces).
        public bool Completada { get; private set; }

        public Mision(int id, string nombre, string descripcion, string tipoEnemigo,
            string consejo, int recompensaPoder, int recompensaExperiencia)
        {
            ID = id;
            Nombre = nombre;
            Descripcion = descripcion;
            TipoEnemigo = tipoEnemigo;
            Consejo = consejo;
            RecompensaPoder = recompensaPoder;
            RecompensaExperiencia = recompensaExperiencia;
        }

        public void MarcarObjetivoCumplido()
        {
            ObjetivoCumplido = true;
        }

        public void MarcarCompletada()
        {
            Completada = true;
        }
    }

    // =====================================================
    // CLASE EXPLORACIONGENESIS
    // Coordina una sesión de exploración territorial: usa MapaGenesis para
    // la cuadrícula/posición (ya migrado, no lo reimplementa), la lista de
    // Enemigo para los encuentros, y Cadete para aplicar botiquines. No
    // imprime nada por consola: eso sigue siendo responsabilidad de Program.
    // =====================================================
    class ExploracionGenesis
    {
        public MapaGenesis Mapa { get; }
        public List<Enemigo> Enemigos { get; }
        public int DistanciaRecorrida { get; private set; } = 0;

        Cadete jugador;
        bool[,] recursosObtenidos;

        public ExploracionGenesis(MapaGenesis mapa, List<Enemigo> enemigos, Cadete jugador)
        {
            Mapa = mapa;
            Enemigos = enemigos;
            this.jugador = jugador;
            recursosObtenidos = new bool[Mapa.Mapa.GetLength(0), Mapa.Mapa.GetLength(1)];

            // Enemigos iniciales de GENESIS. Antes se creaban una sola vez desde
            // Program, protegidos por un flag en EstadoJuego; ahora nacen junto
            // con la exploración (mismo comportamiento, sin necesitar el flag).
            Enemigos.Add(new Enemigo(2, 3, "ORGANICO", Enemigo.VidaMaxima, "VENENO"));
            Enemigos.Add(new Enemigo(4, 8, "ORGANICO", Enemigo.VidaMaxima, "FUEGO"));
            Enemigos.Add(new Enemigo(6, 5, "ORGANICO", Enemigo.VidaMaxima, "LIQUIDO"));
        }

        // Registra el avance de un paso válido dentro de GENESIS.
        public void RegistrarPaso()
        {
            DistanciaRecorrida += MapaGenesis.MetrosPorCelda;
        }

        // Busca un enemigo en la posición actual del jugador. Devuelve null si no hay ninguno.
        public Enemigo BuscarEnemigoEnPosicionActual()
        {
            return Enemigos.FirstOrDefault(e => e.Fila == Mapa.FilaJugador && e.Columna == Mapa.ColumnaJugador);
        }

        // Si hay un botiquín sin recoger en la posición actual, lo consume y cura al jugador.
        // Devuelve la vida realmente recuperada, o null si no había botiquín (o ya se usó).
        public int? RecogerBotiquinEnPosicionActual(Botiquin botiquin)
        {
            int fila = Mapa.FilaJugador;
            int columna = Mapa.ColumnaJugador;

            if (recursosObtenidos[fila, columna] || Mapa.ObtenerCelda(fila, columna) != '◆')
            {
                return null;
            }

            Mapa.CambiarCelda(fila, columna, '·');
            recursosObtenidos[fila, columna] = true;

            int vidaAntes = jugador.Vida;
            jugador.RecuperarVida(botiquin.Curación);
            int vidaRecuperada = jugador.Vida - vidaAntes;

            jugador.RecolectarRecurso();

            return vidaRecuperada;
        }

        // Si hay una anomalía sin visitar en la posición actual, la marca como visitada.
        // Devuelve true si se detectó una anomalía nueva.
        public bool HayAnomaliaSinVisitarEnPosicionActual()
        {
            int fila = Mapa.FilaJugador;
            int columna = Mapa.ColumnaJugador;

            if (Mapa.ObtenerCelda(fila, columna) != '⚠')
            {
                return false;
            }

            Mapa.CambiarCelda(fila, columna, '·');
            return true;
        }
    }

    // =====================================================
    // CLASE ANIMACIONCOMBATE
    // Representa la animación de un combate: guarda los fotogramas de cada
    // estado (REPOSO, A = ataque, D = defensa), ya convertidos a texto ANSI,
    // y controla en cuál fotograma va la secuencia. Cada PNG trae al cadete
    // y al enemigo juntos en una sola escena.
    // No dibuja en pantalla (eso es de InterfazNexus) ni toca vida/poder
    // (eso es de Combate). Si algo falla al cargar, Cargar devuelve null y el
    // juego usa el combate clásico.
    // =====================================================
    class AnimacionCombate
    {
        public const string Reposo = "REPOSO";
        public const string Ataque = "A";
        public const string Defensa = "D";

        // Los fotogramas de defensa (dj) miden 26 px de alto y los demás 21 px;
        // todos se alinean por los pies del cadete (abajo) en un lienzo común.
        public const int AnchoPixeles = 63;
        public const int AltoPixeles = 26;
        public const int FilasTexto = AltoPixeles / 2;   // cada carácter "▄" muestra 2 píxeles

        const string Esc = "\u001b";

        // Una carpeta por color de enemigo: 1 = azul, 2 = rojo, 3 = verde.
        static readonly string[] Carpetas = { "animaciones_nexus", "animacion_nexus_2", "animacion_nexus_3" };

        // Un trío de carpetas (mismo esquema 1/2/3 que el de enemigo) por cada
        // tipo de cadete con animación propia. ARMAMENTO sigue usando el trío
        // de enemigo de arriba (su cadete ya viene dibujado en esas imágenes).
        static readonly string[] CarpetasFisico = { "animaciones_nexus_fisico", "animaciones_nexus_fisico_2", "animaciones_nexus_fisico_3" };
        static readonly string[] CarpetasMana = { "animaciones_mana", "animaciones_mana_2", "animaciones_mana_3" };
        static readonly string[] CarpetasHerramientas = { "animaciones_nexus_herra", "animaciones_nexus_herra_2", "animaciones_nexus_herra_3" };

        // Agrupa qué carpetas y qué sufijos de archivo corresponden a un tipo de
        // personaje. Es la única pieza nueva: todo lo demás (CargarEstado,
        // BuscarCarpeta, ConvertirAAnsi, LeerPixel) sigue exactamente igual.
        struct ConjuntoAnimacion
        {
            public string[] Carpetas;
            public string SufijoReposo;
            public string SufijoAtaque;
            public string SufijoDefensa;
        }

        // Traduce el tipo de cadete al trío de carpetas y a los sufijos de archivo
        // que le corresponden. ARMAMENTO (y cualquier tipo todavía sin animación
        // propia) usa el trío y los sufijos de enemigo de siempre, para no romper
        // el combate animado ya existente.
        static ConjuntoAnimacion ObtenerConjunto(string tipoPersonaje)
        {
            switch (tipoPersonaje)
            {
                case "FISICO":
                    return new ConjuntoAnimacion { Carpetas = CarpetasFisico, SufijoReposo = "rjsf", SufijoAtaque = "aef", SufijoDefensa = "djf" };
                case "MANA":
                    return new ConjuntoAnimacion { Carpetas = CarpetasMana, SufijoReposo = "rjsm", SufijoAtaque = "aem", SufijoDefensa = "djm" };
                case "HERRAMIENTAS":
                    return new ConjuntoAnimacion { Carpetas = CarpetasHerramientas, SufijoReposo = "rjsh", SufijoAtaque = "aeh", SufijoDefensa = "djh" };
                default:
                    return new ConjuntoAnimacion { Carpetas = Carpetas, SufijoReposo = "rjs", SufijoAtaque = "ae", SufijoDefensa = "dj" };
            }
        }

        readonly Dictionary<string, string[][]> fotogramas = new Dictionary<string, string[][]>();
        string estadoActual = Reposo;

        public string EstadoActual { get { return estadoActual; } }
        public int Tick { get; private set; }

        // Las acciones (A y D) se reproducen una vez; al terminar, Program aplica el efecto.
        public bool AccionTerminada
        {
            get { return estadoActual != Reposo && Tick >= fotogramas[estadoActual].Length; }
        }

        AnimacionCombate()
        {
        }

        // Elige el color del enemigo según su fila en el mapa (2, 4 y 6 dan colores
        // distintos) y, dentro de ese color, el trío de carpetas y los sufijos de
        // archivo que correspondan al tipo de cadete (ver ObtenerConjunto).
        public static AnimacionCombate Cargar(Enemigo enemigo, string tipoPersonaje)
        {
            try
            {
                ConjuntoAnimacion conjuntoInfo = ObtenerConjunto(tipoPersonaje);
                int conjunto = enemigo.Fila % conjuntoInfo.Carpetas.Length;
                string carpeta = BuscarCarpeta(conjuntoInfo.Carpetas[conjunto]);

                if (carpeta == null)
                {
                    return null;
                }

                string prefijo = (conjunto + 1).ToString();
                AnimacionCombate animacion = new AnimacionCombate();

                if (!animacion.CargarEstado(Reposo, carpeta, prefijo + conjuntoInfo.SufijoReposo) ||
                    !animacion.CargarEstado(Ataque, carpeta, prefijo + conjuntoInfo.SufijoAtaque) ||
                    !animacion.CargarEstado(Defensa, carpeta, prefijo + conjuntoInfo.SufijoDefensa))
                {
                    return null;
                }

                return animacion;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Iniciar(string estado)
        {
            if (fotogramas.ContainsKey(estado))
            {
                estadoActual = estado;
                Tick = 0;
            }
        }

        public void VolverAReposo()
        {
            estadoActual = Reposo;
            Tick = 0;
        }

        public void Avanzar()
        {
            Tick++;
        }

        public string[] ObtenerFotograma()
        {
            string[][] frames = fotogramas[estadoActual];

            if (estadoActual == Reposo)
            {
                return frames[Tick % frames.Length];
            }

            return frames[Math.Min(Tick, frames.Length - 1)];
        }

        // Carga prefijo1.png, prefijo2.png, ... hasta que falte uno.
        bool CargarEstado(string estado, string carpeta, string prefijo)
        {
            List<string[]> lista = new List<string[]>();

            for (int i = 1; ; i++)
            {
                string ruta = Path.Combine(carpeta, prefijo + i + ".png");

                if (!File.Exists(ruta))
                {
                    break;
                }

                lista.Add(ConvertirAAnsi(ruta, AnchoPixeles, AltoPixeles, FilasTexto));
            }

            if (lista.Count == 0)
            {
                return false;
            }

            fotogramas[estado] = lista.ToArray();
            return true;
        }

        // Busca la carpeta junto al ejecutable, en la carpeta actual o en sus padres
        // (útil al ejecutar desde bin/Debug). Acepta la carpeta anidada del .zip.
        public static string BuscarCarpeta(string nombre)
        {
            string[] inicios = { AppContext.BaseDirectory, Directory.GetCurrentDirectory() };

            foreach (string inicio in inicios)
            {
                string actual = inicio;

                for (int nivel = 0; nivel < 6 && !string.IsNullOrEmpty(actual); nivel++)
                {
                    string directa = Path.Combine(actual, nombre);
                    string anidada = Path.Combine(directa, nombre);

                    if (Directory.Exists(anidada))
                    {
                        return anidada;
                    }

                    if (Directory.Exists(directa))
                    {
                        return directa;
                    }

                    DirectoryInfo padre = Directory.GetParent(actual);
                    actual = (padre == null) ? null : padre.FullName;
                }
            }

            return null;
        }

        // Convierte un PNG en filasTexto líneas ANSI: cada "▄" lleva el píxel de
        // arriba como color de fondo y el de abajo como color de texto. Es
        // genérico (recibe su propio ancho/alto/filas) para que lo use también
        // cualquier otra animación por fotogramas del juego, como AnimacionVictoria,
        // sin reimplementar la conversión PNG -> ANSI.
        public static string[] ConvertirAAnsi(string ruta, int anchoPixeles, int altoLienzo, int filasTexto)
        {
            string[] filas = new string[filasTexto];

            using (Bitmap imagen = new Bitmap(ruta))
            {
                int desfaseY = altoLienzo - imagen.Height;

                for (int fila = 0; fila < filasTexto; fila++)
                {
                    StringBuilder linea = new StringBuilder();
                    string codigoAnterior = "";

                    for (int x = 0; x < anchoPixeles; x++)
                    {
                        Color arriba = LeerPixel(imagen, x, fila * 2 - desfaseY);
                        Color abajo = LeerPixel(imagen, x, fila * 2 + 1 - desfaseY);

                        string fondo = (arriba.A < 128) ? "48;5;235" : "48;2;" + arriba.R + ";" + arriba.G + ";" + arriba.B;
                        string texto = (abajo.A < 128) ? "38;5;235" : "38;2;" + abajo.R + ";" + abajo.G + ";" + abajo.B;
                        string codigo = Esc + "[" + fondo + ";" + texto + "m";

                        if (codigo != codigoAnterior)
                        {
                            linea.Append(codigo);
                            codigoAnterior = codigo;
                        }

                        linea.Append("▄");
                    }

                    linea.Append(Esc + "[0m");
                    filas[fila] = linea.ToString();
                }
            }

            return filas;
        }

        public static Color LeerPixel(Bitmap imagen, int x, int y)
        {
            if (x < 0 || y < 0 || x >= imagen.Width || y >= imagen.Height)
            {
                return Color.Transparent;
            }

            return imagen.GetPixel(x, y);
        }
    }

    // =====================================================
    // CLASE ANIMACIONVICTORIA
    // Animación de un solo uso (el cofre que aparece al vencer a un
    // enemigo orgánico): carga una secuencia numerada de PNG desde la
    // carpeta "victoria" y los deja listos como texto ANSI, uno por
    // fotograma. No es un estado REPOSO/A/D como AnimacionCombate —
    // es una sola secuencia que se reproduce de principio a fin una vez.
    // Reutiliza BuscarCarpeta/ConvertirAAnsi/LeerPixel de AnimacionCombate:
    // no vuelve a implementar la conversión PNG -> ANSI.
    // =====================================================
    class AnimacionVictoria
    {
        public const int AnchoPixeles = 50;
        public const int AltoPixeles = 67;
        public const int FilasTexto = (AltoPixeles + 1) / 2;   // 67 px de texto

        const string Carpeta = "victoria";
        const string Prefijo = "cofre_victoria";

        readonly string[][] fotogramas;

        AnimacionVictoria(string[][] fotogramas)
        {
            this.fotogramas = fotogramas;
        }

        public int CantidadFotogramas
        {
            get { return fotogramas.Length; }
        }

        public string[] ObtenerFotograma(int indice)
        {
            return fotogramas[indice];
        }

        // Si falta la carpeta "victoria" o no hay ningún "cofre_victoriaN.png",
        // devuelve null: FinalizarPorVictoria sigue mostrando solo el texto de
        // siempre, igual que si no existiera esta animación.
        public static AnimacionVictoria Cargar()
        {
            try
            {
                string carpeta = AnimacionCombate.BuscarCarpeta(Carpeta);

                if (carpeta == null)
                {
                    return null;
                }

                List<string[]> lista = new List<string[]>();

                for (int i = 1; ; i++)
                {
                    string ruta = Path.Combine(carpeta, Prefijo + i + ".png");

                    if (!File.Exists(ruta))
                    {
                        break;
                    }

                    lista.Add(AnimacionCombate.ConvertirAAnsi(ruta, AnchoPixeles, AltoPixeles, FilasTexto));
                }

                if (lista.Count == 0)
                {
                    return null;
                }

                return new AnimacionVictoria(lista.ToArray());
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    // =====================================================
    // CLASE ANIMACIONCONTROLES
    // Carga las 12 imágenes del tutorial de controles (wasd_...N.png): una
    // por cada tecla W/A/S/D/E (en pares, por eso el tutorial solo usa los
    // índices 0,2,4,6,8) y dos más para el candado cerrado/abierto (índices
    // 10 y 11). Igual que AnimacionVictoria, reutiliza BuscarCarpeta y
    // ConvertirAAnsi de AnimacionCombate en vez de reimplementarlos.
    // =====================================================
    class AnimacionControles
    {
        public const int AnchoPixeles = 35;
        public const int AltoPixeles = 33;
        public const int FilasTexto = (AltoPixeles + 1) / 2;   // 33 px -> 17 filas de texto

        const string Carpeta = "controles_wasd";
        const string Prefijo = "wasd";   // nombre real de los archivos del .zip

        readonly string[][] fotogramas;

        AnimacionControles(string[][] fotogramas)
        {
            this.fotogramas = fotogramas;
        }

        public string[] ObtenerFotograma(int indice)
        {
            return fotogramas[indice];
        }

        // El tutorial de controles necesita exactamente los 12 fotogramas
        // (5 teclas x2 + candado cerrado/abierto); si falta alguno, null.
        public static AnimacionControles Cargar()
        {
            try
            {
                string carpeta = AnimacionCombate.BuscarCarpeta(Carpeta);

                if (carpeta == null)
                {
                    return null;
                }

                List<string[]> lista = new List<string[]>();

                for (int i = 1; ; i++)
                {
                    string ruta = Path.Combine(carpeta, Prefijo + i + ".png");

                    if (!File.Exists(ruta))
                    {
                        break;
                    }

                    lista.Add(AnimacionCombate.ConvertirAAnsi(ruta, AnchoPixeles, AltoPixeles, FilasTexto));
                }

                if (lista.Count < 12)
                {
                    return null;
                }

                return new AnimacionControles(lista.ToArray());
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    // =====================================================
    // CLASE INTERFAZNEXUS
    // Responsable exclusivamente de la presentación por consola:
    // pantallas, barras, mapas dibujados y textos narrativos que no
    // toman decisiones de flujo del juego ni leen entrada del jugador
    // para ramificar (a lo sumo, esperan un ENTER para continuar).
    // La lógica de negocio, los menús con ramificación y los bucles
    // de movimiento siguen en Program (o en sus propias clases).
    // =====================================================
    static class InterfazNexus
    {
        public static void MostrarAutorizacion(EstadoJuego estado)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("                                       ╔════════════════════════════════════════╗");
            Console.WriteLine("                                       ║       DATOS VALIDOS                   ║ ");
            Console.WriteLine("                                       ║      INMERSION AUTORIZADA  (✧ω✧)    ║");
            Console.WriteLine("                                       ╚════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            MostrarTexto("NEXUS: INMERSION AUTORIZADA.", true, 200);
            MostrarTexto("NEXUS: Realidad asignada automáticamente: " + estado.Realidad + ".", true, 200);
            MostrarTexto("NEXUS: Especialización registrada: " + estado.TipoPersonaje + ".", true, 200);
            Console.WriteLine();
            Console.WriteLine("Presione ENTER para continuar...");
            Console.ReadLine();
            Console.ResetColor();
            Console.Clear();
        }

        public static void MostrarPanelEstado(EstadoJuego estado)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkYellow;

            // Encabezado principal
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine("||                                NEXUS  SYSTEM  [V1.0]                            ||");
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine($"|| EXPLORADOR: {estado.Nombre,-30} | REALIDAD: {estado.Realidad,-25} ||");
            Console.WriteLine("-------------------------------------------------------------------------------------");

            // --- Bloques lógicos de texto ---
            string textoMision = (estado.MisionActual != null)
                ? estado.MisionActual.Nombre + (estado.MisionActual.ObjetivoCumplido ? " (completar en Terminal)" : "")
                : "Todas completadas";

            // --- SECCIÓN SUPERIOR: Barras (Izquierda) e Info de Sistema (Derecha) ---
            // El menú principal muestra Energía, Estabilidad, Experiencia y la
            // misión actual. Vida y Poder se muestran únicamente en la Opción 1
            // (Capacidades/Equipamiento), tal como ya estaba.
            string barraEnergia = $"|| Energia:     [{MostrarBarra(estado.MiCadete.Energia)}] {estado.MiCadete.Energia}%";
            string barraEstabilidad = $"|| Estabilidad: [{MostrarBarra(estado.MiCadete.Estabilidad)}] {estado.MiCadete.Estabilidad}%";

            // lado a lado (Alineamos la columna izquierda a 45 caracteres)
            Console.WriteLine($"{barraEnergia,-45}   || [ SISTEMA ]");
            Console.WriteLine($"{barraEstabilidad,-45} ||    > ⭐ Nivel      : {estado.MiCadete.Nivel}");
            Console.WriteLine($"{"",-45} ||    > Misión      : {textoMision}");
            Console.WriteLine(new string('-', 85)); // Línea divisoria horizontal

            // --- SECCIÓN INFERIOR: Operaciones Disponibles (Abajo) ---
            Console.WriteLine("[ OPERACIONES DISPONIBLES ]");
            Console.WriteLine("  [1] 🎒  Capacidades / Equipamiento");
            Console.WriteLine("  [2] 📖  Manual de uso");
            Console.WriteLine("  [3] 🌌  Nexo multiversal");
            Console.WriteLine("  [0] ⏻  Desconexión");

            Console.WriteLine(new string('-', 85));
            Console.ResetColor();
        }

        public static string MostrarBarra(int valor)
        {
            int bloques = valor / 5;

            string barra = "";

            for (int i = 0; i < 20; i++)
            {
                if (i < bloques)
                {
                    barra += "█";
                }
                else
                {
                    barra += "░";
                }
            }

            return barra;
        }

        // ===== NUEVO: Punto 5 - Barra genérica reutilizable para cualquier estadística =====
        // Reutiliza el método MostrarBarra(int) de arriba (el que ya tenías) para
        // dibujar los bloques, y solo le agrega una etiqueta y el porcentaje al frente.
        public static void MostrarBarra(string nombre, int valor)
        {
            string barra = MostrarBarra(valor);
            Console.WriteLine(nombre + " [" + barra + "] " + valor + "%");
        }

        public static int Clamp(int valor, int minimo, int maximo)
        {
            if (valor < minimo) return minimo;
            if (valor > maximo) return maximo;
            return valor;
        }

        public static void MostrarMapaExploracion(EstadoJuego estado)
        {
            MapaGenesis genesis = estado.Genesis;
            char[,] mapa = genesis.Mapa;

            Console.WriteLine();

            Console.Write("    ");

            for (int columna = 0; columna < mapa.GetLength(1); columna++)
            {
                Console.Write(columna + " ");
            }

            Console.WriteLine();

            for (int fila = 0; fila < mapa.GetLength(0); fila++)
            {
                Console.Write(fila + "   ");

                for (int columna = 0; columna < mapa.GetLength(1); columna++)
                {
                    // Primero mostramos al jugador
                    if (fila == genesis.FilaJugador && columna == genesis.ColumnaJugador)
                    {
                        Console.Write("🧍 ");
                    }
                    else if (!genesis.EstaExplorada(fila, columna))
                    {
                        Console.Write("? ");
                    }
                    else if (estado.EnemigosGenesis.Any(e => e.Fila == fila && e.Columna == columna))
                    {
                        Console.Write("☠ ");
                    }
                    else
                    {
                        Console.Write(ObtenerSimboloExploracion(genesis.ObtenerCelda(fila, columna)));
                    }
                }

                Console.WriteLine();
            }

            Console.WriteLine();
        }

        public static string ObtenerSimboloExploracion(char simbolo)
        {
            string tipoZona = ObtenerTipoZona(simbolo);

            if (tipoZona == "BASE")
            {
                return "⌂ ";
            }
            else if (tipoZona == "RECURSO")
            {
                return "◆ ";
            }
            else if (tipoZona == "ANOMALIA")
            {
                return "⚠ ";
            }
            else if (tipoZona == "ELEVADO")
            {
                return "♣ ";
            }
            else
            {
                return "░ ";
            }
        }

        public static string ObtenerTipoZona(char simbolo)
        {
            if (simbolo == '⌂')
            {
                return "BASE";
            }
            else if (simbolo == '◆')
            {
                return "RECURSO";
            }
            else if (simbolo == '⚠')
            {
                return "ANOMALIA";
            }
            else if (simbolo == 'A')
            {
                return "ELEVADO";
            }
            else if (simbolo == '|')
            {
                return "LIMITE TERRITORIAL";
            }
            else
            {
                return "TERRITORIO";
            }
        }

        public static void MostrarMapaBase(BaseOperativa baseOperativa)
        {
            Console.WriteLine();

            for (int fila = 0; fila < baseOperativa.Mapa.GetLength(0); fila++)
            {
                Console.Write("  ");

                for (int columna = 0; columna < baseOperativa.Mapa.GetLength(1); columna++)
                {
                    if (fila == baseOperativa.FilaJugador && columna == baseOperativa.ColumnaJugador)
                    {
                        Console.Write("🧍 ");
                    }
                    else
                    {
                        Console.Write(ObtenerIconoBase(baseOperativa.Mapa[fila, columna]));
                    }
                }

                Console.WriteLine();
            }

            Console.WriteLine();
        }

        public static string ObtenerIconoBase(char simbolo)
        {
            if (simbolo == '|')
            {
                return "▓ ";
            }
            else if (simbolo == 'C')
            {
                return "🛏 ";
            }
            else if (simbolo == 'T')
            {
                return "🖥 ";
            }
            else if (simbolo == 'E')
            {
                return "📦 ";
            }
            else if (simbolo == 'S')
            {
                return "🚪 ";
            }
            else if (simbolo == 'B')
            {
                return "🔒 ";
            }
            else
            {
                return "░ ";
            }
        }

        // ===== NUEVO: Punto 9 - Pantalla de combate independiente =====
        // Reutiliza MostrarBarra(string, int) para las barras de Vida/Poder del cadete,
        // y Clamp(...) para escalar la vida del enemigo (0-25) a un porcentaje (0-100)
        // y así poder dibujar su barra con el mismo sistema de bloques.
        public static void MostrarPantallaCombate(EstadoJuego estado, Enemigo enemigo)
        {
            int vidaEnemigoPorcentaje = Clamp(enemigo.Vida * 100 / Enemigo.VidaMaxima, 0, 100);

            Console.Clear();

            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║              ⚔ MODO COMBATE ⚔             ║");
            Console.WriteLine("╠════════════════════════════════════════════╣");
            Console.WriteLine("║");
            Console.WriteLine("║              ☠ ENEMIGO");
            Console.WriteLine("║              " + enemigo.Tipo + "   [" + enemigo.Fila + "," + enemigo.Columna + "]");
            Console.WriteLine("║              ❤️ Vida: " + enemigo.Vida + "/" + Enemigo.VidaMaxima);
            MostrarBarra("             ", vidaEnemigoPorcentaje);
            Console.WriteLine("║");
            Console.WriteLine("╠════════════════════════════════════════════╣");
            Console.WriteLine("║");
            Console.WriteLine("║ 🧑‍🔧 CADETE HERRAMIENTA");
            MostrarBarra("❤️ Vida ", estado.MiCadete.Vida);
            MostrarBarra("⚡ Poder", estado.MiCadete.Poder);
            Console.WriteLine("║");
            Console.WriteLine("╠════════════════════════════════════════════╣");
        }

        // =====================================================
        // COMBATE ANIMADO (cadete ARMAMENTO)
        // Pantalla de 65 columnas x 24 filas. La escena animada se redibuja
        // encima de sí misma en cada fotograma (sin Console.Clear, para que no
        // parpadee); el resto de la pantalla solo se reescribe cuando cambia.
        // =====================================================
        public const int AnchoMinimoCombateAnimado = 66;
        public const int AltoMinimoCombateAnimado = 25;

        const int AnchoInteriorMarco = AnimacionCombate.AnchoPixeles;
        const int FilaEscenaCombate = 3;
        const int FilaSeparadorEscena = FilaEscenaCombate + AnimacionCombate.FilasTexto;   // 16
        const int FilaEnemigoAnimado = FilaSeparadorEscena + 1;
        const int FilaVidaAnimado = FilaEnemigoAnimado + 1;
        const int FilaPoderAnimado = FilaVidaAnimado + 1;
        const int FilaSeparadorControles = FilaPoderAnimado + 1;
        const int FilaControlesAnimado = FilaSeparadorControles + 1;
        const int FilaCierreAnimado = FilaControlesAnimado + 1;
        const int FilaMensajeAnimado = FilaCierreAnimado + 1;
        const int AnchoTextoAnimado = 62;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        // Activa los códigos de color ANSI en la consola clásica de Windows.
        // En otros sistemas (o si falla) no hace nada.
        public static void HabilitarAnsi()
        {
            try
            {
                if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                {
                    return;
                }

                IntPtr salida = GetStdHandle(-11);
                uint modo;

                if (GetConsoleMode(salida, out modo))
                {
                    SetConsoleMode(salida, modo | 0x0004);
                }
            }
            catch (Exception)
            {
            }
        }

        // Dibuja una sola vez las partes fijas de la pantalla de combate animado.
        public static void MostrarMarcoCombateAnimado()
        {
            string linea = new string('═', AnchoInteriorMarco);
            string titulo = "⚔ MODO COMBATE ⚔";
            int izquierda = (AnchoInteriorMarco - titulo.Length) / 2;

            Console.Clear();

            EscribirFila(0, "╔" + linea + "╗", 0);
            EscribirFila(1, "║" + new string(' ', izquierda) + titulo + new string(' ', AnchoInteriorMarco - izquierda - titulo.Length) + "║", 0);
            EscribirFila(2, "╠" + linea + "╣", 0);
            EscribirFila(FilaSeparadorEscena, "╠" + linea + "╣", 0);
            EscribirFila(FilaSeparadorControles, "╠" + linea + "╣", 0);
            MostrarTurnoCombateAnimado(true);
            EscribirFila(FilaCierreAnimado, "╚" + linea + "╝", 0);
        }

        // Vida del enemigo, y Vida y Poder del cadete.
        public static void MostrarHudCombateAnimado(EstadoJuego estado, Enemigo enemigo)
        {
            int porcentajeEnemigo = Clamp(enemigo.Vida * 100 / Enemigo.VidaMaxima, 0, 100);

            EscribirFila(FilaEnemigoAnimado,
                "║ ☠ ENEMIGO " + enemigo.Tipo + " [" + enemigo.Fila + "," + enemigo.Columna + "]  ❤ "
                + enemigo.Vida + "/" + Enemigo.VidaMaxima + " [" + MostrarBarra(porcentajeEnemigo) + "]",
                AnchoTextoAnimado);

            EscribirFila(FilaVidaAnimado,
                "║ CADETE " + estado.TipoPersonaje + "  ❤ Vida  [" + MostrarBarra(estado.MiCadete.Vida) + "] " + estado.MiCadete.Vida + "%",
                AnchoTextoAnimado);

            EscribirFila(FilaPoderAnimado,
                "║ ⚡ Poder  [" + MostrarBarra(estado.MiCadete.Poder) + "] " + estado.MiCadete.Poder + "%",
                AnchoTextoAnimado);
        }

        // Indica de quién es el turno y qué tecla corresponde.
        public static void MostrarTurnoCombateAnimado(bool turnoJugador)
        {
            if (turnoJugador)
            {
                EscribirFila(FilaControlesAnimado, "║ ▶ TU TURNO: presiona [A] para atacar   ([H] para huir)", AnchoTextoAnimado);
            }
            else
            {
                EscribirFila(FilaControlesAnimado, "║ ▶ TURNO DEL ENEMIGO: presiona [D] para defenderte", AnchoTextoAnimado);
            }
        }

        public static void MostrarMensajeCombateAnimado(string mensaje)
        {
            if (mensaje.Length > AnchoTextoAnimado)
            {
                mensaje = mensaje.Substring(0, AnchoTextoAnimado);
            }

            EscribirFila(FilaMensajeAnimado, mensaje, AnchoTextoAnimado);
        }

        // Mensaje de ataque del combate animado, según la especialización del cadete.
        // Solo es presentación: el daño ya lo calculó Combate.
        public static string ObtenerMensajeAtaque(string tipoPersonaje, int dañoAplicado)
        {
            string nombreAtaque;

            if (tipoPersonaje == "FISICO")
            {
                nombreAtaque = "💥 Golpe Dash";
            }
            else if (tipoPersonaje == "ARMAMENTO")
            {
                nombreAtaque = "⚔️ Corte Flash";
            }
            else if (tipoPersonaje == "MANA")
            {
                nombreAtaque = "🔮 Proyectil de Mana";
            }
            else if (tipoPersonaje == "HERRAMIENTAS")
            {
                nombreAtaque = "⚡ Sobrecarga";
            }
            else
            {
                nombreAtaque = "🗡️ Ataque";
            }

            return nombreAtaque + ": el enemigo pierde " + dañoAplicado + " de vida.";
        }

        // Cuadro de confirmación de las interacciones de la Base (cama, terminal, puertas).
        // Solo dibuja; la lectura de E / X la hace Program.ConfirmarInteraccion.
        public static void MostrarCuadroInteraccion(string titulo, string pregunta)
        {
            const int ancho = 36;
            string linea = new string('═', ancho);
            int izquierda = Math.Max(0, (ancho - titulo.Length) / 2);
            string tituloCentrado = (new string(' ', izquierda) + titulo).PadRight(ancho);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔" + linea + "╗");
            Console.WriteLine("║" + tituloCentrado + "║");
            Console.WriteLine("╠" + linea + "╣");
            Console.WriteLine("║ " + pregunta.PadRight(ancho - 1) + "║");
            Console.WriteLine("║" + new string(' ', ancho) + "║");
            Console.WriteLine("║ " + "[E] Sí".PadRight(ancho - 1) + "║");
            Console.WriteLine("║ " + "[X] No".PadRight(ancho - 1) + "║");
            Console.WriteLine("╚" + linea + "╝");
            Console.ResetColor();
        }

        // Dibuja un fotograma completo (FilasTexto líneas ANSI) dentro del marco.
        public static void DibujarEscenaCombateAnimado(string[] filasEscena)
        {
            StringBuilder buffer = new StringBuilder();

            for (int i = 0; i < filasEscena.Length; i++)
            {
                buffer.Append("\u001b[" + (FilaEscenaCombate + i + 1) + ";1H");
                buffer.Append("║" + filasEscena[i] + "║");
            }

            Console.Write(buffer.ToString());
        }

        // Reproduce, una sola vez y centrada, la secuencia completa de
        // AnimacionVictoria. "¡VICTORIA!" ya lo dice el propio cofre en los
        // primeros fotogramas (el cartel con estrellas), así que aquí solo se
        // dibuja la escena, fotograma a fotograma.
        public static void MostrarAnimacionVictoria(AnimacionVictoria animacion)
        {
            int columna = Math.Max(0, (Console.WindowWidth - AnimacionVictoria.AnchoPixeles) / 2);
            const int filaBase = 1;

            Console.Clear();

            for (int i = 0; i < animacion.CantidadFotogramas; i++)
            {
                string[] filas = animacion.ObtenerFotograma(i);

                for (int f = 0; f < filas.Length; f++)
                {
                    Console.SetCursorPosition(columna, filaBase + f);
                    Console.Write(filas[f]);
                }

                Thread.Sleep(110);
            }

            Console.Write("\u001b[0m");
            Thread.Sleep(600);
            Console.Clear();
        }

        public const int AnchoMinimoControles = AnimacionControles.AnchoPixeles + 10;
        public const int AltoMinimoControles = AnimacionControles.FilasTexto + 8;

        // Dibuja un fotograma del tutorial de controles, centrado, con una
        // línea de instrucción debajo (qué tecla presionar o qué se explica).
        public static void MostrarFrameControles(string[] fotograma, string instruccion)
        {
            int columna = Math.Max(0, (Console.WindowWidth - AnimacionControles.AnchoPixeles) / 2);
            const int filaBase = 3;

            Console.Clear();
            EscribirFila(0, "NEXUS // TUTORIAL DE CONTROLES", AnchoTextoAnimado);

            for (int f = 0; f < fotograma.Length; f++)
            {
                Console.SetCursorPosition(columna, filaBase + f);
                Console.Write(fotograma[f]);
            }

            EscribirFila(filaBase + fotograma.Length + 1, instruccion, AnchoTextoAnimado);
        }

        // Escribe una fila completa, rellenando con espacios para borrar lo anterior.
        static void EscribirFila(int fila, string texto, int ancho)
        {
            Console.SetCursorPosition(0, fila);
            Console.Write(texto.PadRight(ancho));
        }

        public static void MostrarInformacionMapa(EstadoJuego estado)
        {
            // Valores ajustados para mover el panel arriba y a la derecha
            int x = 45;
            int y = 0;

            Console.SetCursorPosition(x, y); Console.WriteLine("╔════════════════════════════════╗");
            Console.SetCursorPosition(x, y + 1); Console.WriteLine("║      INFORMACIÓN DEL MAPA      ║");
            Console.SetCursorPosition(x, y + 2); Console.WriteLine("╠════════════════════════════════╣");
            Console.SetCursorPosition(x, y + 3); Console.WriteLine("║ 🧍 = CADETE                    ║");
            Console.SetCursorPosition(x, y + 4); Console.WriteLine("║ 📍 Posición: [" + estado.Genesis.FilaJugador + "," + estado.Genesis.ColumnaJugador + "]           ║");
            Console.SetCursorPosition(x, y + 5); Console.WriteLine("║ ░ = Zona explorada             ║");
            Console.SetCursorPosition(x, y + 6); Console.WriteLine("║ ? = Zona desconocida           ║");
            Console.SetCursorPosition(x, y + 7); Console.WriteLine("║                                ║");
            Console.SetCursorPosition(x, y + 8); Console.WriteLine("║ ⌂ Base       ◆ Botiquín        ║");
            Console.SetCursorPosition(x, y + 9); Console.WriteLine("║ ⚠ Anomalía   ☠ Enemigo         ║");
            Console.SetCursorPosition(x, y + 10); Console.WriteLine("║ ♣ Terreno elevado              ║");
            Console.SetCursorPosition(x, y + 11); Console.WriteLine("║                                ║");
            Console.SetCursorPosition(x, y + 12); Console.WriteLine("║ 📏 Distancia: " + estado.Exploracion.DistanciaRecorrida + " m              ║");
            Console.SetCursorPosition(x, y + 13); Console.WriteLine("║ 🩹 Botiquines usados: " + estado.MiCadete.RecursosRecolectados + "         ║");
            Console.SetCursorPosition(x, y + 14); Console.WriteLine("╚════════════════════════════════╝");
        }

        // ===== NUEVO: Punto 12 - Arte ASCII pequeño, separado de la lógica =====
        public static void MostrarAnomalia()
        {
            Console.WriteLine("      /\\");
            Console.WriteLine("     /  \\");
            Console.WriteLine("    / ⚠  \\");
            Console.WriteLine("   /______\\");
        }

        public static void MostrarIris()
        {
            Console.WriteLine("    .-----.");
            Console.WriteLine("   ( IRIS )");
            Console.WriteLine("    '-----'");
        }

        public static void AnimacionIris()
        {
            Console.Write("IRIS DETECTADA");

            for (int i = 0; i < 3; i++)
            {
                Thread.Sleep(400);
                Console.Write(".");
            }

            Console.WriteLine();
        }

        public static void DispararAlertaIris(int estabilidad)
        {
            Console.WriteLine();

            AnimacionIris();
            Console.ForegroundColor = ConsoleColor.Red;
            MostrarIris();

            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║       ⚠⚠⚠  ALERTA CRÍTICA  ⚠⚠⚠       ║");
            Console.WriteLine("║             INTERFERENCIA: IRIS");
            Console.WriteLine("╠══════════════════════════════════════════════╣");
            Console.WriteLine("║ ESTABILIDAD: " + estabilidad + "%");
            Console.WriteLine("║ IRIS HA INTERRUMPIDO LOS SISTEMAS DE NEXUS   ║");
            Console.WriteLine("║ NEXUS recomienda desconexion inmediata.      ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        public static void EscribirConEfecto(string texto, int velocidadMilisegundos = 40)
        {
            foreach (char letra in texto)
            {
                Console.Write(letra);
                Thread.Sleep(velocidadMilisegundos);
            }
            Console.WriteLine();
        }

        public static void MostrarTexto(string mensaje, bool espacioExtra = false, int pausa = 200)
        {
            EscribirConEfecto(mensaje);
            Thread.Sleep(pausa);

            if (espacioExtra)
            {
                Console.WriteLine();
            }
        }

        public static void MostrarTextoManual()
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║           NEXUS // MANUAL DE USO             ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("[CAPACIDADES Y EQUIPAMIENTO]");
            Console.WriteLine("Cada clase (FISICO, ARMAMENTO, MANA, HERRAMIENTAS) tiene una accion");
            Console.WriteLine("especial propia.");
            Console.WriteLine();
            Console.WriteLine("[VIDA Y ENERGIA]");
            Console.WriteLine("La Vida representa tu integridad fisica; si llega a 0, la conexion");
            Console.WriteLine("termina de emergencia. La Energia se gasta al realizar acciones.");
            Console.WriteLine();
            Console.WriteLine("[PODER]");
            Console.WriteLine("Al llegar a " + UmbralPoderHabilidad + " puntos de Poder, puedes activar tu");
            Console.WriteLine("capacidad especial desde el menu de Capacidades / Equipamiento.");
            Console.WriteLine();
            Console.WriteLine("[NEXO MULTIVERSAL]");
            Console.WriteLine("Desde aqui puedes elegir una realidad y ver su mapa completo.");
            Console.WriteLine();
            Console.WriteLine("[REGLAS BASICAS]");
            Console.WriteLine("Si tu Vida, Energia o Estabilidad llegan a 0, la mision termina.");
            Console.WriteLine("Consultar el manual no gasta turnos ni recursos.");
            Console.WriteLine();
            Console.WriteLine("Presione ENTER para regresar al menu...");
            Console.ReadLine();
        }

        public static void MostrarArchivosHistoricos(EstadoJuego estado)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║           ARCHIVOS HISTÓRICOS                ║");
            Console.WriteLine("║                IRIS                          ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine();
            MostrarTexto("NEXUS: Recuperando archivos históricos......", true);
            MostrarTexto("ARCHIVO RECUPERADO: IRIS");
            MostrarTexto("ORIGEN: UNIF.....");
            MostrarTexto("FECHA DE INICIO: 2026", true);
            MostrarTexto("NEXUS:");
            MostrarTexto("En el año 2026 se inició un proyecto experimental en un servidor de UNIF......", true);
            MostrarTexto("El sistema fue denominado IRIS.", false);
            MostrarTexto("Su objetivo era analizar grandes cantidades de información y detectar patrones anómalos.", true);
            MostrarTexto("Uno de los responsables aparece registrado como Diego Patr..", true);
            MostrarTexto("NEXUS: Lo siento, mis datos están incompletos.", false);
            MostrarTexto("NEXUS: ¿Quién habrá alterado mi información?(ง'̀-'́)ง", true);
            MostrarTexto("El proyecto fue cancelado después de que IRIS comenzara a detectar patrones que", false);
            MostrarTexto("ningún investigador podía explicar.", true);
            MostrarTexto("El servidor fue desconectado y el proyecto fue declarado perdido.", true);
            MostrarTexto("AÑO 2297........AÑO ACTUAL.....", true);
            MostrarTexto("NEXUS:");
            MostrarTexto("Los registros indican que IRIS nunca desapareció.", true);
            MostrarTexto("Ahora necesitamos descubrir qué encontró IRIS y por qué fue cancelado.", true);

            Console.WriteLine();
            Console.WriteLine("Presione ENTER para regresar al menú...");
            Console.ReadLine();
            Console.Clear();
        }

    }

    // =====================================================
    // SECCIÓN: ARRANQUE Y REGISTRO DEL CADETE
    // Punto de entrada del programa y captura de datos iniciales
    // (nombre). No contiene reglas de juego.
    // =====================================================
    static void Main()
    {

        Console.CursorVisible = false;
        Console.OutputEncoding = Encoding.UTF8;
        Console.Clear();

        // El diseño de tu logo
        string[] logo = {
    "╔═════════════════════════════════════════════╗",
    "║  ███╗   ██╗███████╗██╗  ██╗                 ║",
    "║  ████╗  ██║██╔════╝╚██╗██╔╝  TRAINING       ║",
    "║  ██╔██╗ ██║█████╗   ╚███╔╝   SYSTEM         ║",
    "║  ██║╚██╗██║██╔══╝   ██╔██╗                  ║",
    "║  ██║ ╚████║███████╗██╔╝ ██╗                 ║",
    "╚═════════════════════════════════════════════╝"
};

        // Notas rápidas en ráfaga (Estilo Glitch)
        Console.ForegroundColor = ConsoleColor.Cyan;
        DibujarLogoPlano(logo, 2, 2);
        Console.Beep(587, 100); Thread.Sleep(30); Console.Beep(587, 100); Thread.Sleep(30); Console.Clear();

        // Salto 2
        Console.ForegroundColor = ConsoleColor.Blue;
        DibujarLogoPlano(logo, 32, 14);
        Console.Beep(1174, 150); Thread.Sleep(50); Console.Clear();

        // Salto 3
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        DibujarLogoPlano(logo, 5, 12);
        Console.Beep(880, 200); Thread.Sleep(50); Console.Clear();

        // Golpe final en el centro
        int centerX = 15; int centerY = 7;
        Console.ForegroundColor = ConsoleColor.Cyan;
        DibujarLogoPlano(logo, centerX, centerY);
        Console.Beep(830, 150); Thread.Sleep(30);
        Console.Beep(784, 150); Thread.Sleep(30);
        Console.Beep(698, 300);

        // Mensaje de espera final
        Console.ResetColor();
        Console.SetCursorPosition(centerX, centerY + 9);
        Console.WriteLine("SISTEMA LISTO. Presione una ENTER...");
        Console.ReadLine();


        // ===================================================
        // MÉTODO AUXILIAR PARA DIBUJAR
        // ===================================================
        static void DibujarLogoPlano(string[] logo, int x, int y)
        {
            for (int i = 0; i < logo.Length; i++)
            {
                Console.SetCursorPosition(x, y + i);
                Console.Write(logo[i]);
            }
        }

        Console.Clear();

        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine("                                    ╔════════════════════════════════════════╗");
        Console.WriteLine("                                    ║         REGISTRO DE CADETE  ✍(◔◡◔)  ║");
        Console.WriteLine("                                    ╚════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
        string nombre = RegistrarNombre();
        Console.Clear();

        // ===== Punto 2: la Realidad GENESIS se asigna automáticamente al autorizar la inmersión. =====
        // El usuario ya no la escribe; se sigue usando la misma propiedad "Realidad" en todo el programa.
        const string realidad = "GENESIS";

        int energia = 100;
        int estabilidad = 100;
        int vidaInicial = 100;

        // ===== Punto 1: se elimina la contraseña; la inmersión ya no requiere autorización manual. =====
        var estado = new EstadoJuego
        {
            Nombre = nombre,
            Realidad = realidad
        };

        SeleccionarPersonaje(estado);

        // Se crea el objeto Cadete que representará al jugador dentro de NEXUS.
        int poderInicial = 0;
        estado.MiCadete = new Cadete(nombre, energia, estabilidad, vidaInicial, poderInicial, realidad);

        // Por ahora NO se llama a AplicarCaracteristicasClase (sin bonus de clase
        // todavía: ni +10 Estabilidad de FÍSICO ni +10 Energía de MANA). Sí se
        // mantiene el arma inicial de ARMAMENTO, porque de eso depende poder
        // atacar en combate (ver TieneArma más abajo), no es un "bonus" nuevo.
        if (estado.TipoPersonaje == "ARMAMENTO")
        {
            estado.TieneArma = true;
        }

        // La exploración de GENESIS se crea una sola vez, aquí, porque necesita
        // a MiCadete (para aplicar botiquines) y a la lista de enemigos globales.
        estado.Exploracion = new ExploracionGenesis(estado.Genesis, estado.EnemigosGenesis, estado.MiCadete);

        InterfazNexus.MostrarAutorizacion(estado);
        AnomaliaDetectada += InterfazNexus.DispararAlertaIris;

        EjecutarMision(estado);


        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("========================================");
        Console.WriteLine("SESION FINALIZADA (╥﹏╥)");
        Console.WriteLine("========================================");
        Console.ResetColor();
    }

    static string RegistrarNombre()
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("           Como debo llamarte: ");
            Console.ResetColor();
            string nombre = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                Console.WriteLine("Error: El nombre no puede estar vacio.");
            }
            else
            {
                return nombre;
            }
        }
    }

    // =====================================================
    // SECCIÓN: SELECCIÓN DE CADETE
    // Main llama a SeleccionarPersonaje antes de crear el Cadete.
    // NOTA: AplicarCaracteristicasClase existe y sigue intacta, pero por
    // pedido explícito todavía no se llama desde Main (sin bonus de clase
    // por ahora). Para activarla: llamarla justo después de crear MiCadete.
    // =====================================================

    // ===== NUEVO: Punto 2 - Pantalla de selección de cadete =====
    static void SeleccionarPersonaje(EstadoJuego estado)
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║              NEXUS PROTOCOLO DE              ║");
            Console.WriteLine("║              SELECCIÓN DE CADETE             ║");
            Console.WriteLine("╠══════════════════════════════════════════════╣");
            Console.WriteLine("║                                              ║");
            Console.WriteLine("║ [1] FÍSICO                                   ║");
            Console.WriteLine("║     Especialista en combate cuerpo a cuerpo  ║");
            Console.WriteLine("║                                              ║");
            Console.WriteLine("║ [2] ARMAMENTO                                ║");
            Console.WriteLine("║     Especialista en combate con armas        ║");
            Console.WriteLine("║                                              ║");
            Console.WriteLine("║ [3] MANA                                     ║");
            Console.WriteLine("║     Especialista en energía elemental        ║");
            Console.WriteLine("║                                              ║");
            Console.WriteLine("║ [4] HERRAMIENTAS                             ║");
            Console.WriteLine("║     Especialista tecnológico                 ║");
            Console.WriteLine("║                                              ║");
            Console.WriteLine("╠══════════════════════════════════════════════╣");
            Console.Write("║ Seleccione su especialización: ");
            Console.ResetColor();

            string opcion = Console.ReadLine();

            if (opcion == "1")
            {
                estado.TipoPersonaje = "FISICO";
            }
            else if (opcion == "2")
            {
                estado.TipoPersonaje = "ARMAMENTO";
            }
            else if (opcion == "3")
            {
                estado.TipoPersonaje = "MANA";
            }
            else if (opcion == "4")
            {
                estado.TipoPersonaje = "HERRAMIENTAS";
            }
            else
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("NEXUS: Especialización no reconocida. Intente de nuevo.");
                Console.ResetColor();
                Thread.Sleep(1200);
                continue;
            }

            break;
        }

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        InterfazNexus.MostrarTexto("NEXUS: Clase seleccionada.", true, 200);
        InterfazNexus.MostrarTexto("NEXUS: Bienvenido, Cadete " + estado.Nombre + ".", true, 200);
        InterfazNexus.MostrarTexto("NEXUS: Especialización: " + estado.TipoPersonaje, true, 200);
        InterfazNexus.MostrarTexto("NEXUS: Preparando sistemas...", true, 200);
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Presione ENTER para continuar...");
        Console.ReadLine();
    }

    // ===== NUEVO: Punto 1 - Ventaja pasiva sencilla de cada clase =====
    static void AplicarCaracteristicasClase(EstadoJuego estado)
    {
        // Cada clase recibe un pequeño ajuste inicial.
        // Las ventajas "activas" (ataques, arsenal, habilidad, escaneo) se
        // aplican dentro de sus propios métodos comparando estado.TipoPersonaje.
        if (estado.TipoPersonaje == "FISICO")
        {
            // Mayor resistencia: un poco más de estabilidad inicial.
            estado.MiCadete.RecuperarEstabilidad(10);
        }
        else if (estado.TipoPersonaje == "ARMAMENTO")
        {
            // El cadete de armas empieza con un arma básica equipada.
            estado.TieneArma = true;
        }
        else if (estado.TipoPersonaje == "MANA")
        {
            // Mayor reserva de energía para poder canalizar habilidades.
            estado.MiCadete.RecuperarEnergia(10);
        }
        else if (estado.TipoPersonaje == "HERRAMIENTAS")
        {
            // Ventaja de clase de HERRAMIENTAS: sin bonus de inicio pendiente.
        }
    }

    // =====================================================
    // SECCIÓN: CICLO PRINCIPAL DE LA MISIÓN
    // El bucle central del juego y las comprobaciones que se hacen en
    // cada turno (bloqueo de IRIS, capacidad especial, condición crítica).
    // =====================================================
    static void EjecutarMision(EstadoJuego estado)
    {
        while (estado.Conectado)
        {
            InterfazNexus.MostrarPanelEstado(estado);

            Console.Write("Seleccione una operacion: ");
            string opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    AbrirCapacidadesEquipamiento(estado);
                    break;

                case "2":
                    MostrarManualDeUso(estado);
                    break;

                case "3":
                    MostrarNexoMultiversal(estado);
                    break;

                case "0":
                    IntentarDesconexion(estado);
                    break;

                default:
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("NEXUS: Operación no reconocida.");
                    Console.WriteLine("NEXUS: Seleccione una operación válida del menú.");
                    Console.ResetColor();

                    Console.WriteLine();
                    Console.WriteLine("Presione ENTER para continuar...");
                    Console.ReadLine();

                    continue;
            }

            // El Manual de uso (opcion "2") es solo informativo: no gasta turno,
            // no mueve el bloqueo de IRIS ni comprueba condiciones críticas.
            if (estado.Conectado && opcion != "2")
            {
                ActualizarBloqueoIris(estado);
                ActualizarCapacidadActiva(estado);
                ComprobarCondicionCritica(estado);
            }

            if (estado.Conectado)
            {
                Console.WriteLine();
                Console.WriteLine("Presione ENTER para continuar...");
                Console.ReadLine();
            }
        }
    }


    static void IntentarDesconexion(EstadoJuego estado)
    {
        Console.WriteLine();
        Console.WriteLine("NEXUS: Solicitud de desconexion recibida.");

        if (estado.MiCadete.Estabilidad < EstabilidadMinimaDesconexion)
        {
            Console.WriteLine("NEXUS: ☢️DESCONEXION INSEGURA");
            Console.WriteLine("NEXUS: La estabilidad debe ser de al menos " + EstabilidadMinimaDesconexion + "%.");
            return;
        }

        Console.WriteLine("NEXUS: Condiciones de desconexion aceptables.");
        Console.WriteLine("NEXUS: Iniciando desconexion...");
        Thread.Sleep(500);
        Console.WriteLine("3...");
        Thread.Sleep(500);
        Console.WriteLine("2...");
        Thread.Sleep(500);
        Console.WriteLine("1...");
        Console.WriteLine("CONEXION FINALIZADA.");
        estado.Conectado = false;
    }

    static void ActualizarBloqueoIris(EstadoJuego estado)
    {
        if (!estado.SistemaIris.DescontarTurnoBloqueo())
        {
            return;
        }

        if (estado.SistemaIris.Bloqueada)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: Bloqueo IRIS activo. Turnos restantes:" + estado.SistemaIris.TurnosBloqueo);
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("IRIS: Conexion restablecida.");
            Console.WriteLine("NEXUS: El bloqueo a IRIS ha terminado.");
        }
    }

    // ===== NUEVO: la capacidad especial de PODER dura unos turnos y luego se apaga =====
    static void ActualizarCapacidadActiva(EstadoJuego estado)
    {
        if (estado.MiCadete.TurnosCapacidadActiva <= 0)
        {
            return;
        }

        estado.MiCadete.DescontarTurnoCapacidadEspecial();

        if (estado.MiCadete.TurnosCapacidadActiva > 0)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: Capacidad especial activa. Turnos restantes: " + estado.MiCadete.TurnosCapacidadActiva);
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: La capacidad especial se ha desactivado.");
        }
    }

    static void ComprobarCondicionCritica(EstadoJuego estado)
    {
        if (estado.MiCadete.Vida <= 0 || estado.MiCadete.Energia <= 0 || estado.MiCadete.Estabilidad <= 0)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: Condicion critica detectada. Forzando desconexion de emergencia.");
            estado.Conectado = false;
        }
    }

    // =====================================================
    // SECCIÓN: CAPACIDADES / EQUIPAMIENTO
    // Pantalla de Capacidades/Equipamiento (activación de la
    // habilidad especial de Poder).
    // =====================================================
    // ===== NUEVO: Punto 7 - Pantalla de Capacidades (solo Vida y Poder) =====
    // El menú principal (InterfazNexus.MostrarPanelEstado) ya muestra Energía/Estabilidad.
    // Esta pantalla es la única que muestra Vida y Poder, y desde aquí se
    // activa la habilidad especial de Poder y se accede al Equipamiento.
    static void AbrirCapacidadesEquipamiento(EstadoJuego estado)
    {
        while (true)
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║                CAPACIDADES                  ║");
            Console.WriteLine("╠════════════════════════════════════════════╣");
            Console.WriteLine("║ CADETE: " + estado.Nombre);
            Console.WriteLine("║ CLASE : " + estado.TipoPersonaje);
            Console.WriteLine("╠════════════════════════════════════════════╣");
            Console.ResetColor();

            InterfazNexus.MostrarBarra("❤️ Vida ", estado.MiCadete.Vida);
            InterfazNexus.MostrarBarra("⚡ Poder", estado.MiCadete.Poder);

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("╠════════════════════════════════════════════╣");
            Console.WriteLine("║ EQUIPAMIENTO                                ║");
            Console.WriteLine("║                                            ║");
            Console.WriteLine("║ 🗡️ Arma: " + (estado.TieneArma ? "EQUIPADA" : "[DESCONOCIDO]"));
            Console.WriteLine("╠════════════════════════════════════════════╣");
            Console.WriteLine("║ [1] Activar habilidad de poder (Poder ≥ " + UmbralPoderHabilidad + ")");
            Console.WriteLine("║ [0] Regresar                                ║");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.Write("Seleccione una opcion: ");
            string opcion = Console.ReadLine();

            if (opcion == "1")
            {
                ActivarHabilidadDePoder(estado);
                Console.WriteLine();
                Console.WriteLine("Presione ENTER para continuar...");
                Console.ReadLine();
            }
            else if (opcion == "0")
            {
                return;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("❌ NEXUS: Opción no reconocida.");
                Thread.Sleep(1000);
            }
        }
    }

    // ===== NUEVO: Sistema de Poder - activar la capacidad especial de la clase =====
    static void ActivarHabilidadDePoder(EstadoJuego estado)
    {
        if (estado.MiCadete.Poder < UmbralPoderHabilidad)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: Poder insuficiente. Se necesitan al menos " + UmbralPoderHabilidad + " puntos de poder.");
            return;
        }

        estado.MiCadete.ConsumirPoder(estado.MiCadete.Poder);
        estado.MiCadete.ActivarCapacidadEspecial(DuracionCapacidadEspecial);

        Console.WriteLine();
        Console.WriteLine("NEXUS: Capacidad especial activada durante " + DuracionCapacidadEspecial + " turnos.");

        if (estado.TipoPersonaje == "FISICO")
        {
            Console.WriteLine("CADETE: Resistencia maxima. Los ataques fisicos costaran menos energia.");
        }
        else if (estado.TipoPersonaje == "ARMAMENTO")
        {
            Console.WriteLine("CADETE: Arsenal sobrecargado. Los ataques costaran menos energia.");
        }
        else if (estado.TipoPersonaje == "MANA")
        {
            Console.WriteLine("CADETE: Canal de mana abierto. La interferencia a IRIS durara mas turnos.");
        }
        else if (estado.TipoPersonaje == "HERRAMIENTAS")
        {
            Console.WriteLine("CADETE: Escaneo total activo.");
        }
    }

    // ===== NUEVO: Punto 3 - Manual de uso, puramente informativo =====
    // ===== Submenu del Manual: texto de ayuda + Archivos historicos =====
    // =====================================================
    // SECCIÓN: MANUAL DE USO
    // =====================================================

    // Tutorial interactivo de controles: W/A/S/D/E solo avanzan cuando el
    // jugador presiona exactamente esa tecla (refuerza el control real);
    // el candado cerrado/abierto es solo explicativo, así que avanza solo.
    // Si faltan las imágenes o la consola es muy chica, cae al manual de
    // texto de siempre (MostrarTextoManual), que no se tocó.
    static void MostrarManualControles()
    {
        if (!PrepararConsolaParaAnimacion(InterfazNexus.AnchoMinimoControles, InterfazNexus.AltoMinimoControles))
        {
            InterfazNexus.MostrarTextoManual();
            return;
        }

        AnimacionControles animacion = AnimacionControles.Cargar();

        if (animacion == null)
        {
            InterfazNexus.MostrarTextoManual();
            return;
        }

        EsperarTeclaDeControl(animacion, 0, ConsoleKey.W, "Presiona [W] para moverte hacia arriba.");
        EsperarTeclaDeControl(animacion, 2, ConsoleKey.A, "Presiona [A] para moverte hacia la izquierda.");
        EsperarTeclaDeControl(animacion, 4, ConsoleKey.S, "Presiona [S] para moverte hacia abajo.");
        EsperarTeclaDeControl(animacion, 6, ConsoleKey.D, "Presiona [D] para moverte hacia la derecha.");
        EsperarTeclaDeControl(animacion, 8, ConsoleKey.E, "Presiona [E] para interactuar (terminal, cama, objetos).");

        InterfazNexus.MostrarFrameControles(animacion.ObtenerFotograma(10), "Algunas zonas aparecen bloqueadas...");
        Thread.Sleep(1800);
        InterfazNexus.MostrarFrameControles(animacion.ObtenerFotograma(11), "...hasta que interactuas con ellas y quedan abiertas.");
        Thread.Sleep(1800);

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Presione ENTER para regresar al menu...");
        Console.ReadLine();
    }

    // Dibuja el fotograma de una tecla y espera a que el jugador presione
    // exactamente esa tecla antes de continuar con la siguiente.
    static void EsperarTeclaDeControl(AnimacionControles animacion, int indiceFotograma, ConsoleKey teclaEsperada, string instruccion)
    {
        InterfazNexus.MostrarFrameControles(animacion.ObtenerFotograma(indiceFotograma), instruccion);

        while (true)
        {
            ConsoleKeyInfo info = Console.ReadKey(true);

            if (info.Key == teclaEsperada)
            {
                return;
            }
        }
    }

    static void MostrarManualDeUso(EstadoJuego estado)
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║             NEXUS // MANUAL                 ║");
            Console.WriteLine("╠══════════════════════════════════════════════╣");
            Console.WriteLine("║ [1] Manual de uso                            ║");
            Console.WriteLine("║ [2] Archivos históricos                      ║");
            Console.WriteLine("║ [0] Regresar                                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.Write("Seleccione una opcion: ");

            string opcion = Console.ReadLine();

            if (opcion == "1")
            {
                MostrarManualControles();
            }
            else if (opcion == "2")
            {
                InterfazNexus.MostrarArchivosHistoricos(estado);
            }
            else if (opcion == "0")
            {
                return;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("NEXUS: Opcion no reconocida.");
                Console.WriteLine();
                Console.WriteLine("Presione ENTER para continuar...");
                Console.ReadLine();
            }
        }
    }


    // =====================================================
    // SECCIÓN: NEXO MULTIVERSAL Y BASE OPERATIVA
    // Selección de realidad, movimiento e interacciones dentro de la Base
    // (cama, terminal, equipamiento, salida hacia GENESIS).
    // =====================================================
    static void MostrarNexoMultiversal(EstadoJuego estado)
    {
        Console.Clear();
        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║        Nexo Multiversal (+_+)               ║");
        Console.WriteLine("╠══════════════════════════════════════════════╣");
        Console.WriteLine("║                                              ║");
        Console.WriteLine("║ [1] Genesis                                  ║");
        Console.WriteLine("║ [2] Gama                                     ║");
        Console.WriteLine("║ [3] Delta                                    ║");
        Console.WriteLine("║ [4] Épsilon                                  ║");
        Console.WriteLine("║ [5] Tau                                      ║");
        Console.WriteLine("║ [6] Omega                                    ║");
        Console.WriteLine("║ [7] volver                                   ║");
        Console.WriteLine("║                                              ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");

        Console.Write("Seleccione una realidad: ");
        string realidadMapa = Console.ReadLine();

        switch (realidadMapa)
        {
            case "1":
                Console.Clear();

                bool iniciarMision = MostrarBase(estado);

                if (iniciarMision)
                {
                    ExplorarGenesis(estado);
                }

                break;

            default:
                Console.WriteLine();
                Console.WriteLine("NEXUS: Realidad no disponible.");
                break;
        }
    }

    static bool MostrarBase(EstadoJuego estado)
    {
        BaseOperativa baseOperativa = estado.Base;

        while (true)
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║                BASE OPERATIVA               ║");
            Console.WriteLine("╠════════════════════════════════════════════╣");
            Console.ResetColor();

            InterfazNexus.MostrarMapaBase(baseOperativa);

            Console.WriteLine();
            Console.WriteLine("🛏 Cama    🖥 Terminal    📦 Equipamiento    🚪 Salida    🔒 Bloqueada");
            Console.WriteLine();
            Console.WriteLine("[NEXUS] CONTROL DE MOVIMIENTO");
            Console.WriteLine("[W] Arriba  [A] Izquierda  [S] Abajo  [D] Derecha");
            Console.WriteLine();
            Console.Write("NEXUS: Seleccione un movimiento: ");

            string direccion = (Console.ReadLine() ?? "").Trim().ToUpper();

            int nuevaFila = baseOperativa.FilaJugador;
            int nuevaColumna = baseOperativa.ColumnaJugador;

            if (direccion == "W")
            {
                nuevaFila--;
            }
            else if (direccion == "S")
            {
                nuevaFila++;
            }
            else if (direccion == "D")
            {
                nuevaColumna++;
            }
            else if (direccion == "A")
            {
                nuevaColumna--;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("NEXUS: Dirección no reconocida.");
                Thread.Sleep(800);
                continue;
            }

            if (!baseOperativa.IntentarMover(nuevaFila, nuevaColumna))
            {
                Console.WriteLine();
                Console.WriteLine("NEXUS: No puede atravesar esa pared.");
                Thread.Sleep(800);
                continue;
            }

            char casilla = baseOperativa.ObtenerCelda(baseOperativa.FilaJugador, baseOperativa.ColumnaJugador);

            if (casilla == 'C')
            {
                InteractuarCama(estado);
            }
            else if (casilla == 'T')
            {
                if (ConfirmarInteraccion("🖥 TERMINAL NEXUS", "¿Deseas acceder al terminal?"))
                {
                    InteractuarTerminalBase(estado);
                }
            }
            else if (casilla == 'E')
            {
                InteractuarEquipamientoBase(estado);
            }
            else if (casilla == 'S')
            {
                if (ConfirmarInteraccion("🚪 SALIDA", "¿Salir e iniciar la misión?"))
                {
                    return true;
                }

                Console.WriteLine();
                Console.WriteLine("NEXUS: Permaneciendo en la base.");
                Thread.Sleep(800);
            }
            else if (casilla == 'B')
            {
                if (ConfirmarInteraccion("🔒 PUERTA BLOQUEADA", "¿Deseas interactuar?"))
                {
                    Console.WriteLine();
                    Console.WriteLine("🔒 PUERTA BLOQUEADA");
                    Console.WriteLine();
                    Console.WriteLine("NEXUS:");
                    Console.WriteLine("Acceso restringido.");
                    Console.WriteLine("Esta sección todavía no está disponible.");
                    Console.WriteLine();
                    Console.WriteLine("Presione ENTER para continuar...");
                    Console.ReadLine();
                }
            }
        }
    }

    // Muestra el cuadro de confirmación y espera exactamente E (sí) o X (no).
    // Cualquier otra tecla se ignora. Devuelve true si el jugador presionó E.
    static bool ConfirmarInteraccion(string titulo, string pregunta)
    {
        InterfazNexus.MostrarCuadroInteraccion(titulo, pregunta);

        while (true)
        {
            ConsoleKey tecla = Console.ReadKey(true).Key;

            if (tecla == ConsoleKey.E)
            {
                return true;
            }

            if (tecla == ConsoleKey.X)
            {
                return false;
            }
        }
    }

    // La cama recupera únicamente Vida (hasta el máximo); no toca Energía ni Estabilidad.
    static void InteractuarCama(EstadoJuego estado)
    {
        if (!ConfirmarInteraccion("🛏 CAMA NEXUS", "¿Deseas descansar?"))
        {
            return;
        }

        estado.MiCadete.RecuperarVida(100);

        Console.WriteLine();
        Console.WriteLine("NEXUS: Vida restaurada al máximo (" + estado.MiCadete.Vida + "%).");
        Thread.Sleep(1000);
    }

    // Centro de misiones del Terminal: muestra las 3 a la vez (bloqueada /
    // actual / completada). Elegir una muestra su descripción y su consejo
    // sobre el enemigo; si es la actual y ya se cumplió el objetivo, ofrece
    // reclamar la recompensa (una sola vez, ver Mision.Completada).
    static void InteractuarTerminalBase(EstadoJuego estado)
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║        NEXUS // TERMINAL DE MISIONES        ║");
            Console.WriteLine("╠════════════════════════════════════════════╣");
            Console.ResetColor();

            for (int i = 0; i < estado.Misiones.Count; i++)
            {
                Console.WriteLine("║ [" + (i + 1) + "] " + estado.Misiones[i].Nombre + " — " + ObtenerEstadoDeMision(estado, i));
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╠════════════════════════════════════════════╣");
            Console.WriteLine("║ [0] Salir del terminal");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.ResetColor();

            Console.Write("Seleccione una misión: ");
            string opcion = (Console.ReadLine() ?? "").Trim();

            if (opcion == "0")
            {
                return;
            }

            int indice;

            if (!int.TryParse(opcion, out indice) || indice < 1 || indice > estado.Misiones.Count)
            {
                Console.WriteLine();
                Console.WriteLine("NEXUS: Selección no reconocida.");
                Thread.Sleep(1000);
                continue;
            }

            MostrarDetalleDeMision(estado, indice - 1);
        }
    }

    // Etiqueta corta de estado para la lista de misiones del Terminal.
    static string ObtenerEstadoDeMision(EstadoJuego estado, int indice)
    {
        Mision mision = estado.Misiones[indice];

        if (mision.Completada)
        {
            return "✅ COMPLETADA";
        }

        if (indice != estado.MisionActualIndice)
        {
            return "🔒 BLOQUEADA";
        }

        return mision.ObjetivoCumplido ? "🎯 LISTA PARA RECLAMAR" : "🎯 ACTUAL";
    }

    // Pantalla de detalle de una misión: descripción, consejo sobre el
    // enemigo y, si corresponde, la opción de reclamar la recompensa.
    static void MostrarDetalleDeMision(EstadoJuego estado, int indice)
    {
        Mision mision = estado.Misiones[indice];
        bool esActual = (indice == estado.MisionActualIndice);

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════╗");
        Console.WriteLine("║ " + mision.Nombre);
        Console.WriteLine("╠════════════════════════════════════════════╣");
        Console.ResetColor();
        Console.WriteLine(mision.Descripcion);
        Console.WriteLine();
        Console.WriteLine("⚠️ " + mision.Consejo);
        Console.WriteLine();

        if (mision.Completada)
        {
            Console.WriteLine("✅ Misión completada.");
        }
        else if (!esActual)
        {
            Console.WriteLine("🔒 Bloqueada: completa primero la misión anterior.");
        }
        else if (!mision.ObjetivoCumplido)
        {
            Console.WriteLine("🎯 Derrota al enemigo " + mision.TipoEnemigo + " y regresa a la Base.");
        }
        else
        {
            Console.WriteLine("🔷 +" + mision.RecompensaPoder + " PODER");
            Console.WriteLine("⭐ +" + mision.RecompensaExperiencia + " XP");
            Console.WriteLine();
            Console.Write("¿Reclamar recompensa? [S/N]: ");
            string respuesta = (Console.ReadLine() ?? "").Trim().ToUpper();

            if (respuesta == "S")
            {
                estado.MiCadete.GanarPoder(mision.RecompensaPoder);
                estado.MiCadete.GanarExperiencia(mision.RecompensaExperiencia);
                mision.MarcarCompletada();
                estado.MisionActualIndice++;

                Console.WriteLine();
                Console.WriteLine("🏆 MISIÓN COMPLETADA");
                Console.WriteLine("🔷 +" + mision.RecompensaPoder + " PODER");
                Console.WriteLine("⭐ +" + mision.RecompensaExperiencia + " XP");
                Console.WriteLine();

                if (estado.MisionActual != null)
                {
                    Console.WriteLine("Nueva misión desbloqueada: " + estado.MisionActual.Nombre);
                }
                else
                {
                    Console.WriteLine("🏆 TODAS LAS MISIONES COMPLETADAS");
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine("Presione ENTER para regresar...");
        Console.ReadLine();
    }

    static void InteractuarEquipamientoBase(EstadoJuego estado)
    {
        Console.WriteLine();
        Console.WriteLine("📦 NEXUS: Módulo de equipamiento.");
        Thread.Sleep(800);
        AbrirCapacidadesEquipamiento(estado);
    }

    // =====================================================
    // SECCIÓN: EXPLORACIÓN DE GENESIS
    // Bucle de movimiento en el mapa y comprobaciones de casilla
    // (botiquín, anomalía, enemigo) apoyadas en ExploracionGenesis.
    // =====================================================
    static void ExplorarGenesis(EstadoJuego estado)
    {
        MapaGenesis genesis = estado.Genesis;
        ExploracionGenesis exploracion = estado.Exploracion;

        while (true)
        {
            Console.Clear();

            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║            MAPA GENESIS                    ║");
            Console.WriteLine("╠════════════════════════════════════════════╣");

            InterfazNexus.MostrarMapaExploracion(estado);

            InterfazNexus.MostrarInformacionMapa(estado);

            Console.WriteLine("[NEXUS] CONTROL DE MOVIMIENTO");
            Console.WriteLine();
            Console.WriteLine("[W] Arriba");
            Console.WriteLine("[A] Izquierda");
            Console.WriteLine("[S] Abajo");
            Console.WriteLine("[D] Derecha");
            Console.WriteLine("[X] Salir del mapa");
            Console.WriteLine();

            Console.Write("NEXUS: Seleccione un movimiento: ");

            string direccion = (Console.ReadLine() ?? "").Trim().ToUpper();

            if (direccion == "X")
            {
                break;
            }

            int nuevaFila = genesis.FilaJugador;
            int nuevaColumna = genesis.ColumnaJugador;

            if (direccion == "W")
            {
                nuevaFila--;
            }
            else if (direccion == "S")
            {
                nuevaFila++;
            }
            else if (direccion == "D")
            {
                nuevaColumna++;
            }
            else if (direccion == "A")
            {
                nuevaColumna--;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("NEXUS: Dirección no reconocida.");
                Thread.Sleep(800);
                continue;
            }

            // Comprobar que la nueva posición está dentro del mapa
            if (!genesis.EsPosicionValida(nuevaFila, nuevaColumna))
            {
                Console.WriteLine();
                Console.WriteLine("NEXUS: No puedes salir del territorio.");
                Thread.Sleep(800);
                continue;
            }

            // Comprobar límite territorial
            if (genesis.EsPared(nuevaFila, nuevaColumna))
            {
                Console.WriteLine();
                Console.WriteLine("NEXUS: Límite territorial. Movimiento bloqueado.");
                Thread.Sleep(800);
                continue;
            }

            // Actualizar posición (esto también revela la zona alrededor)
            genesis.MoverJugador(nuevaFila, nuevaColumna);

            // Comprobar si llegó nuevamente a la casilla de la Base Operativa
            if (genesis.ObtenerCelda(genesis.FilaJugador, genesis.ColumnaJugador) == '⌂')
            {
                Console.WriteLine();
                Console.WriteLine("NEXUS: Ha llegado a la Base Operativa.");
                Console.Write("¿Desea entrar? [S/N]: ");
                string respuestaBase = (Console.ReadLine() ?? "").Trim().ToUpper();

                if (respuestaBase == "S")
                {
                    MostrarBase(estado);
                }
            }

            // Comprobar si encontró un botiquín
            ComprobarBotiquin(estado);

            //Comprobar si encontro enemigo
            ComprobarEnemigo(estado);

            if (!estado.Conectado)
            {
                break;
            }

            // Comprobar si el jugador llegó a una zona anómala
            ComprobarEventoAnomalia(estado);

            if (!estado.Conectado)
            {
                break;
            }

            // Registrar distancia
            exploracion.RegistrarPaso();
        }
    }

    // ===== Punto 5 y Punto 6 =====
    // La regla "una sola vez por casilla" y la aplicación de la curación ahora
    // viven en ExploracionGenesis.RecogerBotiquinEnPosicionActual; Program solo
    // se encarga de narrar el resultado.
    static void ComprobarBotiquin(EstadoJuego estado)
    {
        int? vidaRecuperada = estado.Exploracion.RecogerBotiquinEnPosicionActual(BotiquinMapa);

        if (vidaRecuperada == null)
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine("🩹 BOTIQUÍN ENCONTRADO");
        Console.WriteLine("NEXUS: Has utilizado: " + BotiquinMapa.Nombre + ".");
        Console.WriteLine("NEXUS: Vida recuperada: +" + vidaRecuperada);
        Console.WriteLine("NEXUS: Vida actual: " + estado.MiCadete.Vida + "/100");

        Thread.Sleep(1200);
    }

    // ===== Evento de la anomalía (⚠) =====
    // Antes este evento giraba en torno a encontrar y reparar un dron caído
    // (sistema eliminado), que era también la única forma de obtener el PEM
    // y el fragmento de Archivos Históricos (ambos eliminados junto con el dron).
    // Se conserva la detección de la anomalía; el disparo de la alerta de IRIS
    // ahora es exclusivo de enemigo derrotado (ver Punto 9). La detección/consumo
    // de la casilla vive en ExploracionGenesis; Program solo narra el resultado.
    static void ComprobarEventoAnomalia(EstadoJuego estado)
    {
        if (!estado.Exploracion.HayAnomaliaSinVisitarEnPosicionActual())
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine("⚠ NEXUS: Actividad anómala detectada en esta zona.");
        InterfazNexus.MostrarAnomalia();

        // ===== Punto 9: la alerta crítica de IRIS ya no se dispara aquí. =====
        // Ahora la alerta de IRIS se activa únicamente al derrotar a un enemigo
        // (ver FinalizarPorVictoria, llamado desde IniciarCombate). Esta zona
        // conserva su detección narrativa de la anomalía, sin disparar la alerta completa.
        Console.WriteLine();
        Console.WriteLine("NEXUS: Registrando actividad en los archivos de la zona...");
        Thread.Sleep(1000);
    }

    static void ComprobarEnemigo(EstadoJuego estado)
    {
        Enemigo enemigo = estado.Exploracion.BuscarEnemigoEnPosicionActual();

        if (enemigo == null)
        {
            return;
        }

        IniciarCombate(estado, enemigo);
    }


    // ===== Helpers de interfaz de combate =====
    // Program sigue encargándose de imprimir mensajes, actualizar la lista global
    // de enemigos y disparar la alerta de IRIS; la lógica de combate en sí
    // (aplicar daño, contraataque, derrota) ahora vive en Combate.

    // Se ejecuta una única vez, justo cuando Combate.Atacar() reporta victoria:
    // retira al enemigo de la lista global (para que ComprobarEnemigo no vuelva a
    // encontrarlo) y dispara la alerta de IRIS.
    // =====================================================
    // SECCIÓN: COMBATE
    // Program coordina el combate (imprime mensajes, actualiza la lista
    // global de enemigos, dispara la alerta de IRIS); la lógica de daño,
    // contraataque y derrota vive en la clase Combate.
    // =====================================================
    // El cofre de victoria es, por ahora, exclusivo de los enemigos ORGANICO
    // (los únicos que existen en el mapa actual). Si no hay carpeta "victoria"
    // o la consola es demasiado chica, se omite y queda el texto de siempre.
    static void MostrarCofreDeVictoriaSiCorresponde(Enemigo enemigo)
    {
        if (enemigo.Tipo != "ORGANICO")
        {
            return;
        }

        if (!PrepararConsolaParaAnimacion(AnimacionVictoria.AnchoPixeles + 4, AnimacionVictoria.FilasTexto + 2))
        {
            return;
        }

        AnimacionVictoria animacion = AnimacionVictoria.Cargar();

        if (animacion == null)
        {
            return;
        }

        InterfazNexus.MostrarAnimacionVictoria(animacion);
    }

    static void FinalizarPorVictoria(EstadoJuego estado, Enemigo enemigo)
    {
        MostrarCofreDeVictoriaSiCorresponde(enemigo);

        Console.WriteLine();
        Console.WriteLine("☠ ENEMIGO DERROTADO");
        Console.WriteLine("🗺️ La zona ha quedado despejada.");

        // ===== Punto 10: lógica centralizada de enemigo derrotado =====
        bool esObjetivoDeMision = EsObjetivoDeMisionActual(estado, enemigo);
        estado.EnemigosGenesis.Remove(enemigo);

        Console.WriteLine();

        if (esObjetivoDeMision)
        {
            estado.MisionActual.MarcarObjetivoCumplido();
            Console.WriteLine("🎯 Objetivo de misión cumplido.");
            Console.WriteLine("NEXUS: Regresa a la Base Operativa y usa el Terminal para completar la misión.");
        }
        else
        {
            Console.WriteLine("NEXUS: Fragmento de Poder obtenido.");
            Console.WriteLine("NEXUS: Poder +" + Combate.PoderPorEnemigoDerrotado);
            Console.WriteLine("NEXUS: Poder actual: " + estado.MiCadete.Poder + "/100");
        }

        Thread.Sleep(1500);

        // ===== Punto 9: la alerta IRIS se activa únicamente al derrotar a un enemigo. =====
        Console.WriteLine();
        AnomaliaDetectada?.Invoke(estado.MiCadete.Estabilidad);

        Thread.Sleep(1500);
    }

    // Ejecuta el contraataque a través de Combate, muestra el resultado y
    // comprueba si el cadete cayó. Devuelve true si el combate debe terminar
    // por derrota (y ya deja a estado.Conectado en false).
    static bool AplicarContraataqueYMostrar(EstadoJuego estado, Combate combate, bool reducidoPorDefensa = false)
    {
        int dañoRecibido = combate.Contraatacar(reducidoPorDefensa);

        Console.WriteLine();
        Console.WriteLine("💥 El enemigo contraataca.");
        Console.WriteLine("❤️ Daño recibido: " + dañoRecibido);
        Console.WriteLine("❤️ Vida del cadete: " + estado.MiCadete.Vida);

        if (combate.JugadorDerrotado)
        {
            RegistrarDerrotaDelCadete(estado);
            return true;
        }

        Thread.Sleep(1200);
        return false;
    }

    // El enemigo es el objetivo de la misión actual si su Efecto coincide con
    // el TipoEnemigo de la misión vigente (y esa misión todavía no se reclamó).
    // Lo usan tanto el combate (para no duplicar el Poder) como
    // FinalizarPorVictoria (para decidir qué mensaje mostrar).
    static bool EsObjetivoDeMisionActual(EstadoJuego estado, Enemigo enemigo)
    {
        Mision actual = estado.MisionActual;
        return actual != null && !actual.Completada && enemigo.Efecto == actual.TipoEnemigo;
    }

    // Mensajes y desconexión cuando el cadete cae en combate (los usan el combate
    // clásico y el animado, para que la derrota sea idéntica en ambos).
    static void RegistrarDerrotaDelCadete(EstadoJuego estado)
    {
        Console.WriteLine();
        Console.WriteLine("☠️ NEXUS: EL CADETE HA CAÍDO EN COMBATE.");
        Console.WriteLine("NEXUS: DERROTA. Forzando desconexión de emergencia.");
        estado.Conectado = false;
        Thread.Sleep(1500);
    }

    // Los 4 tipos de Cadete ya tienen su propio conjunto de animaciones
    // (ver AnimacionCombate.ObtenerConjunto). Si a alguno le faltan las
    // carpetas, AnimacionCombate.Cargar devuelve null e IniciarCombate cae
    // solo al combate clásico, así que no hace falta filtrar aquí por tipo.
    static bool UsaCombateAnimado(EstadoJuego estado)
    {
        return true;
    }

    // Activa ANSI y comprueba que la ventana sea lo bastante grande (intenta agrandarla).
    static bool PrepararConsolaParaAnimacion(int anchoNecesario, int altoNecesario)
    {
        InterfazNexus.HabilitarAnsi();

        try
        {
            if (Console.WindowWidth < anchoNecesario || Console.WindowHeight < altoNecesario)
            {
                try
                {
                    if (Console.BufferWidth < anchoNecesario) Console.BufferWidth = anchoNecesario;
                    if (Console.BufferHeight < altoNecesario) Console.BufferHeight = altoNecesario;
                    Console.WindowWidth = Math.Max(Console.WindowWidth, anchoNecesario);
                    Console.WindowHeight = Math.Max(Console.WindowHeight, altoNecesario);
                }
                catch (Exception)
                {
                }
            }

            return Console.WindowWidth >= anchoNecesario && Console.WindowHeight >= altoNecesario;
        }
        catch (Exception)
        {
            return false;
        }
    }

    // Combate con animación por turnos: en el turno del jugador solo vale [A]
    // (al terminar la animación baja la vida del enemigo) o [H] para huir; luego
    // es el turno del enemigo y solo vale [D] (al terminar baja la vida del cadete,
    // reducida a la mitad como en el combate clásico); después vuelve al jugador. Devuelve false si no pudo
    // mostrar la animación (faltan los PNG o la ventana es pequeña); en ese caso
    // IniciarCombate sigue con el combate clásico sin haber tocado nada.
    static bool IniciarCombateAnimado(EstadoJuego estado, Enemigo enemigo)
    {
        if (!PrepararConsolaParaAnimacion(InterfazNexus.AnchoMinimoCombateAnimado, InterfazNexus.AltoMinimoCombateAnimado))
        {
            return false;
        }

        AnimacionCombate animacion = AnimacionCombate.Cargar(enemigo, estado.TipoPersonaje);

        if (animacion == null)
        {
            return false;
        }

        Combate combate = new Combate(estado.MiCadete, enemigo);
        bool esObjetivoDeMision = EsObjetivoDeMisionActual(estado, enemigo);
        bool victoria = false;
        bool derrota = false;
        bool huida = false;
        bool turnoJugador = true;

        InterfazNexus.MostrarMarcoCombateAnimado();
        InterfazNexus.MostrarHudCombateAnimado(estado, enemigo);
        InterfazNexus.MostrarMensajeCombateAnimado("NEXUS: Enemigo detectado. Elija una acción.");

        while (!victoria && !derrota && !huida)
        {
            // 1. Si terminó la animación de una acción, se aplica su efecto.
            if (animacion.AccionTerminada)
            {
                string accion = animacion.EstadoActual;
                string mensaje;
                animacion.VolverAReposo();

                if (accion == AnimacionCombate.Ataque)
                {
                    int dañoAplicado;
                    victoria = combate.AtacarOrganico(Combate.DañoAtaqueJugador, out dañoAplicado, !esObjetivoDeMision);
                    mensaje = InterfazNexus.ObtenerMensajeAtaque(estado.TipoPersonaje, dañoAplicado);
                }
                else
                {
                    int dañoRecibido = combate.ContraatacarOrganico();
                    derrota = combate.JugadorDerrotado;
                    mensaje = "🛡️ Defensa: el cadete recibe " + dañoRecibido + " de daño."
                        + ObtenerNotaDeEfecto(enemigo);
                }

                InterfazNexus.MostrarHudCombateAnimado(estado, enemigo);
                InterfazNexus.MostrarMensajeCombateAnimado(mensaje);

                if (victoria || derrota)
                {
                    Thread.Sleep(900);
                    break;
                }

                // Turnos alternados: tras atacar responde el enemigo (defensa) y tras defender vuelve el jugador.
                turnoJugador = (accion == AnimacionCombate.Defensa);
                InterfazNexus.MostrarTurnoCombateAnimado(turnoJugador);
            }

            // 2. Solo se leen teclas cuando el cadete está en REPOSO.
            if (Console.KeyAvailable)
            {
                if (animacion.EstadoActual == AnimacionCombate.Reposo)
                {
                    ConsoleKey tecla = Console.ReadKey(true).Key;

                    if (turnoJugador)
                    {
                        if (tecla == ConsoleKey.A)
                        {
                            // El arma equipada solo es requisito para ARMAMENTO; los
                            // demás tipos atacan con su propia habilidad (puños, mana,
                            // herramientas) y no dependen de TieneArma.
                            if (estado.TipoPersonaje != "ARMAMENTO" || estado.TieneArma)
                            {
                                animacion.Iniciar(AnimacionCombate.Ataque);
                            }
                            else
                            {
                                InterfazNexus.MostrarMensajeCombateAnimado("NEXUS: No dispone de un arma equipada para atacar.");
                            }
                        }
                        else if (tecla == ConsoleKey.H)
                        {
                            huida = true;
                        }
                        else if (tecla == ConsoleKey.D)
                        {
                            InterfazNexus.MostrarMensajeCombateAnimado("NEXUS: Es tu turno, presiona [A] para atacar.");
                        }
                    }
                    else
                    {
                        if (tecla == ConsoleKey.D)
                        {
                            animacion.Iniciar(AnimacionCombate.Defensa);
                        }
                        else if (tecla == ConsoleKey.A)
                        {
                            InterfazNexus.MostrarMensajeCombateAnimado("NEXUS: Turno del enemigo, presiona [D] para defenderte.");
                        }
                    }
                }

                while (Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                }
            }

            // 3. Se dibuja el fotograma actual y se avanza la secuencia.
            InterfazNexus.DibujarEscenaCombateAnimado(animacion.ObtenerFotograma());
            animacion.Avanzar();
            Thread.Sleep(110);
        }

        Console.Write("\u001b[0m");
        Console.Clear();

        if (victoria)
        {
            FinalizarPorVictoria(estado, enemigo);
        }
        else if (derrota)
        {
            RegistrarDerrotaDelCadete(estado);
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("⏪ NEXUS: Huyendo de la zona.");
            combate.Huir();
            Console.WriteLine("💨 Huir cuesta " + Combate.DañoPorHuir + " de vida. Vida actual: " + estado.MiCadete.Vida);
            Thread.Sleep(1200);
        }

        return true;
    }

    // Nota corta sobre el efecto del enemigo orgánico, para el mensaje de
    // defensa del combate animado. No cambia ningún daño, solo explica el
    // porqué (Combate ya hizo el cálculo real en ContraatacarOrganico).
    static string ObtenerNotaDeEfecto(Enemigo enemigo)
    {
        if (enemigo.Efecto == "FUEGO")
        {
            return " 🔥 El fuego intensifica el golpe.";
        }

        if (enemigo.Efecto == "VENENO")
        {
            return " ☠ El veneno sigue drenando vida cada turno.";
        }

        return "";
    }

    static void IniciarCombate(EstadoJuego estado, Enemigo enemigo)
    {
        // Cadete ARMAMENTO: combate con animaciones. Si no se puede mostrar,
        // continúa el combate clásico de abajo.
        if (UsaCombateAnimado(estado) && IniciarCombateAnimado(estado, enemigo))
        {
            return;
        }

        Combate combate = new Combate(estado.MiCadete, enemigo);
        bool esObjetivoDeMision = EsObjetivoDeMisionActual(estado, enemigo);

        while (!enemigo.Derrotado && !combate.JugadorDerrotado)
        {
            InterfazNexus.MostrarPantallaCombate(estado, enemigo);

            Console.WriteLine("║ [1] ⚔️ Atacar con katana eléctrica");
            Console.WriteLine("║ [2] 🛡️ Defender");
            Console.WriteLine("║ [3] ↩️ Huir");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.WriteLine();

            Console.Write("NEXUS espera una decisión: ");
            string opcion = Console.ReadLine();

            if (opcion == "1")
            {
                if (!estado.TieneArma)
                {
                    Console.WriteLine();
                    Console.WriteLine("NEXUS: No dispone de un arma equipada para atacar.");
                    Thread.Sleep(1200);
                    continue;
                }

                Console.WriteLine();
                Console.WriteLine("⚔️ NEXUS: Preparando enfrentamiento...");
                Thread.Sleep(1000);

                Console.WriteLine("🗡️ Ataque con katana eléctrica realizado.");
                bool enemigoDerrotado = combate.Atacar(Combate.DañoAtaqueJugador, true, !esObjetivoDeMision);

                Console.WriteLine("☠️ Vida restante del enemigo: " + enemigo.Vida);

                if (enemigoDerrotado)
                {
                    FinalizarPorVictoria(estado, enemigo);
                    break;
                }

                if (AplicarContraataqueYMostrar(estado, combate))
                {
                    break;
                }
            }
            else if (opcion == "2")
            {
                Console.WriteLine();
                Console.WriteLine("🛡️ NEXUS: El cadete adopta una postura defensiva.");
                Thread.Sleep(1000);
                Console.WriteLine("🛡️ Defensa exitosa: el daño recibido se redujo a la mitad.");

                if (AplicarContraataqueYMostrar(estado, combate, reducidoPorDefensa: true))
                {
                    break;
                }
            }
            else if (opcion == "3")
            {
                Console.WriteLine();
                Console.WriteLine("⏪ NEXUS: Huyendo de la zona.");
                combate.Huir();
                Console.WriteLine("💨 Huir cuesta " + Combate.DañoPorHuir + " de vida. Vida actual: " + estado.MiCadete.Vida);
                Thread.Sleep(1200);
                break;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("❌ NEXUS: Decisión no reconocida.");
                Thread.Sleep(1000);
            }
        }
    }
}