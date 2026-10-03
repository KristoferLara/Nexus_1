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

    // ===== SISTEMA DE CLASES DE CADETE =====
    // Costos de las acciones especiales por clase
    const int CostoAtaqueFisico = 10;
    const int CostoAtaqueFuerte = 25;
    const int CostoHabilidadMana = 20;

    // ===== SISTEMA DE PODER =====
    const int UmbralPoderHabilidad = 100;       // Poder necesario para activar la capacidad especial
    const int DuracionCapacidadEspecial = 3;   // Turnos que dura el efecto activo
    // PoderPorEnemigoDerrotado se mudó a Combate: es una recompensa que pertenece
    // exclusivamente a la lógica de combate (ver clase Combate).

    // ===== SISTEMA DE BOTIQUINES =====
    const int CuracionBotiquin = 25;   // Punto 5: vida recuperada al usar un botiquín del mapa

    static Random rng = new Random();
    static event Action<int> AnomaliaDetectada;

    class EstadoJuego
    {
        public string Nombre;
        public string Realidad;
        // Energia ya no se guarda aquí: ahora vive únicamente en MiCadete.Energia (POO).
        // Estabilidad ya no se guarda aquí: ahora vive únicamente en MiCadete.Estabilidad (POO).
        public bool Conectado = true;
        public bool BienvenidaNexusMostrada = false;

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
                { '|', '·', '·', '·', '·', '·', '·', '·', '·', '·', '|' },
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
                { '|', '·', '·', '·', '·', 'S', '·', '·', '·', '|' },
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

        public bool Derrotado => Vida <= 0;

        public Enemigo(int fila, int columna, string tipo, int vidaInicial)
        {
            Fila = fila;
            Columna = columna;
            Tipo = tipo;
            Vida = vidaInicial;
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
        public string Posicion { get; private set; }
        public string Realidad { get; private set; }
        public int RecursosRecolectados { get; private set; }
        public bool CapacidadActiva { get; private set; }
        public int TurnosCapacidadActiva { get; private set; }

        public Cadete(string nombre, int energia, int estabilidad, int vida, int poder, string realidad)
        {
            Nombre = nombre;
            Energia = energia;
            Estabilidad = estabilidad;
            Vida = vida;
            Poder = poder;
            Realidad = realidad;
            Posicion = "Base";
            RecursosRecolectados = 0;
            CapacidadActiva = false;
            TurnosCapacidadActiva = 0;
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

        public void ActualizarPosicion(string nuevaPosicion)
        {
            Posicion = nuevaPosicion;
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
        public const int DañoAtaqueJugador = 15;
        public const int DañoEnemigoMinimo = 8;
        public const int DañoEnemigoMaximo = 16; // exclusivo: rng.Next(8, 16) como antes
        public const int PoderPorEnemigoDerrotado = 20;

        // Generador propio para el daño aleatorio del contraataque. El "rng" de
        // Program se usa para otras cosas (p. ej. sectores de anomalías) y no es
        // responsabilidad del combate compartirlo.
        static Random rng = new Random();

        public Cadete Jugador { get; }
        public Enemigo EnemigoActual { get; }

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
        public bool Atacar(int daño, bool mostrarMensaje = true)
        {
            if (mostrarMensaje)
            {
                Jugador.Atacar(daño);
            }

            EnemigoActual.RecibirDanio(daño);

            if (EnemigoActual.Derrotado)
            {
                Jugador.GanarPoder(PoderPorEnemigoDerrotado);
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

        // El jugador decide huir: el combate simplemente termina, sin penalización
        // (el comportamiento actual del juego no contempla ninguna).
        public bool Huir()
        {
            return true;
        }

        // Indica si el jugador cayó en este combate.
        public bool JugadorDerrotado => Jugador.Vida <= 0;
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
            Enemigos.Add(new Enemigo(2, 3, "ORGANICO", Enemigo.VidaMaxima));
            Enemigos.Add(new Enemigo(4, 8, "ORGANICO", Enemigo.VidaMaxima));
            Enemigos.Add(new Enemigo(6, 5, "ORGANICO", Enemigo.VidaMaxima));
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
        public int? RecogerBotiquinEnPosicionActual(int curacion)
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
            jugador.RecuperarVida(curacion);
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
        public const int AltoPixeles = 75;
        public const int FilasTexto = (AltoPixeles + 1) / 2;   // 75 px -> 38 filas de texto

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
            string anomalia = estado.SistemaIris.AnomaliaLocalizada ? $"LOCALIZADA - {estado.SistemaIris.UbicacionAnomalia}" : "NO LOCALIZADA";
            string iris = estado.SistemaIris.Bloqueada ? $"BLOQUEADA ({estado.SistemaIris.TurnosBloqueo} T)" : "ACTIVA";
            string capacidad = estado.MiCadete.CapacidadActiva ? $"ACTIVA ({estado.MiCadete.TurnosCapacidadActiva} T)" : "INACTIVA";

            // --- SECCIÓN SUPERIOR: Barras (Izquierda) e Info de Sistema (Derecha) ---
            // El menú principal SOLO muestra Energía y Estabilidad.
            // Vida y Poder se muestran únicamente en la Opción 2 (Capacidades/Equipamiento).
            string barraEnergia = $"|| Energia:     [{MostrarBarra(estado.MiCadete.Energia)}] {estado.MiCadete.Energia}%";
            string barraEstabilidad = $"|| Estabilidad: [{MostrarBarra(estado.MiCadete.Estabilidad)}] {estado.MiCadete.Estabilidad}%";

            // lado a lado (Alineamos la columna izquierda a 45 caracteres)
            Console.WriteLine($"{barraEnergia,-45}   || [ SISTEMA ]");
            Console.WriteLine($"{barraEstabilidad,-45} ||    > Clase     : {estado.TipoPersonaje}");
            Console.WriteLine($"{"",-45} ||    > Anomalía  : {anomalia}");
            Console.WriteLine($"{"",-45} ||    > Iris      : {iris}");
            Console.WriteLine($"{"",-45} ||    > Capacidad : {capacidad}");
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

    // NOTA (Fase 8): actualmente sin uso en el flujo del juego. Queda disponible
    // como utilidad genérica de lectura numérica validada.
    static int LeerEntero(string prompt, int minimo, int maximo, string mensajeFueraDeRango)
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(prompt);
            Console.ResetColor();

            bool esValido = int.TryParse(Console.ReadLine(), out int valor);

            if (!esValido)
            {
                Console.WriteLine("Error: Debe ingresar un numero.");
            }
            else if (valor < minimo || valor > maximo)
            {
                Console.WriteLine(mensajeFueraDeRango);
            }
            else
            {
                return valor;
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

    // ===== NUEVO: Punto 3 - Un solo despachador según estado.TipoPersonaje =====
    // Esto evita crear 4 programas distintos: usamos if / else if para decidir
    // qué método ejecutar, dependiendo de la clase que eligió el cadete.
    // =====================================================
    // SECCIÓN: SISTEMA DE CLASES Y CAPACIDADES/EQUIPAMIENTO
    // Acción especial de cada clase de Cadete, el menú de Sistemas de
    // Acción y la pantalla de Capacidades/Equipamiento (activación de la
    // habilidad especial de Poder).
    // =====================================================
    static void AccionEspecialDeClase(EstadoJuego estado)
    {
        if (estado.TipoPersonaje == "FISICO")
        {
            Console.WriteLine();
            Console.WriteLine("=== CAPACIDADES FISICAS ===");
            AtaqueFisico(estado);
        }
        else if (estado.TipoPersonaje == "ARMAMENTO")
        {
            Console.WriteLine();
            Console.WriteLine("=== ARSENAL ===");
            AbrirArsenal(estado);
        }
        else if (estado.TipoPersonaje == "MANA")
        {
            Console.WriteLine();
            Console.WriteLine("=== HABILIDADES DE MANA ===");
            UsarHabilidadDeMana(estado);
        }
        else if (estado.TipoPersonaje == "HERRAMIENTAS")
        {
            Console.WriteLine();
            Console.WriteLine("=== HERRAMIENTAS ===");
            EscaneoAvanzado(estado);
        }
    }

    // ----- FÍSICO: ataque físico simple, consume poca energía -----
    static void AtaqueFisico(EstadoJuego estado)
    {
        // Si la capacidad especial esta activa, el ataque cuesta la mitad.
        int costoReal = estado.MiCadete.CapacidadActiva ? CostoAtaqueFisico / 2 : CostoAtaqueFisico;

        if (estado.MiCadete.Energia < costoReal)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: Energia insuficiente para el ataque fisico.");
            return;
        }

        estado.MiCadete.ConsumirEnergia(costoReal);

        Console.WriteLine();
        Console.WriteLine("NEXUS: Ejecutando ataque fisico...");
        Thread.Sleep(300);
        Console.WriteLine("CADETE: Impacto directo.");

        // Ventaja de FISICO: recupera un poco de estabilidad al golpear.
        estado.MiCadete.RecuperarEstabilidad(3);
        Console.WriteLine("NEXUS: Estabilidad +3 (resistencia fisica).");
    }

    // ----- ARMAS: submenú con ataque básico y ataque fuerte -----
    static void AbrirArsenal(EstadoJuego estado)
    {
        if (!estado.TieneArma)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: No hay ningun arma equipada.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║              ARSENAL                     ║");
        Console.WriteLine("╠══════════════════════════════════════════╣");
        Console.WriteLine("║ [1] Ataque basico   (-" + CostoAtaqueFisico + " energia)     ║");
        Console.WriteLine("║ [2] Ataque fuerte   (-" + CostoAtaqueFuerte + " energia)     ║");
        Console.WriteLine("║ [3] Cancelar                             ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.Write("Seleccione un ataque: ");

        string opcion = Console.ReadLine();

        // Si la capacidad especial esta activa, ambos ataques cuestan menos.
        int costoBasicoReal = estado.MiCadete.CapacidadActiva ? CostoAtaqueFisico / 2 : CostoAtaqueFisico;
        int costoFuerteReal = estado.MiCadete.CapacidadActiva ? CostoAtaqueFuerte / 2 : CostoAtaqueFuerte;

        if (opcion == "1")
        {
            if (estado.MiCadete.Energia < costoBasicoReal)
            {
                Console.WriteLine("NEXUS: Energia insuficiente.");
                return;
            }

            estado.MiCadete.ConsumirEnergia(costoBasicoReal);
            Console.WriteLine("NEXUS: Ataque basico ejecutado.");
        }
        else if (opcion == "2")
        {
            if (estado.MiCadete.Energia < costoFuerteReal)
            {
                Console.WriteLine("NEXUS: Energia insuficiente para un ataque fuerte.");
                return;
            }

            estado.MiCadete.ConsumirEnergia(costoFuerteReal);
            Console.WriteLine("NEXUS: ¡Ataque fuerte ejecutado!");
        }
        else
        {
            Console.WriteLine("NEXUS: Arsenal cerrado.");
        }
    }

    // ----- MANA: habilidad especial que también interfiere a IRIS -----
    static void UsarHabilidadDeMana(EstadoJuego estado)
    {
        if (estado.MiCadete.Energia < CostoHabilidadMana)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: Energia insuficiente para canalizar mana.");
            return;
        }

        estado.MiCadete.ConsumirEnergia(CostoHabilidadMana);

        Console.WriteLine();
        Console.WriteLine("NEXUS: Canalizando energia de mana...");
        Thread.Sleep(400);
        Console.WriteLine("CADETE: La señal de IRIS se distorsiona brevemente.");

        // Ventaja de MANA: puede interferir a IRIS si no esta ya bloqueada.
        // Con la capacidad especial activa, la interferencia dura mas turnos.
        int turnosInterferencia = estado.MiCadete.CapacidadActiva ? 2 : 1;

        if (estado.SistemaIris.Interferir(turnosInterferencia))
        {
            Console.WriteLine("NEXUS: IRIS ha sido interferida temporalmente.");
        }
    }

    // ----- HERRAMIENTAS: escaneo de información sin gastar energía -----
    static void EscaneoAvanzado(EstadoJuego estado)
    {
        Console.WriteLine();
        Console.WriteLine("NEXUS: Iniciando escaneo avanzado (especialidad HERRAMIENTAS)...");
        Thread.Sleep(400);

        Console.WriteLine("NEXUS: Energia restante: " + estado.MiCadete.Energia + "%");
        Console.WriteLine("NEXUS: Estabilidad restante: " + estado.MiCadete.Estabilidad + "%");

        if (estado.SistemaIris.Bloqueada)
        {
            Console.WriteLine("NEXUS: Bloqueo IRIS: " + estado.SistemaIris.TurnosBloqueo + " turnos");
        }
        else
        {
            Console.WriteLine("NEXUS: Bloqueo IRIS: sin bloqueo");
        }

        if (estado.SistemaIris.AnomaliaLocalizada)
        {
            Console.WriteLine("NEXUS: Ultima anomalia registrada en " + estado.SistemaIris.UbicacionAnomalia + ".");
        }
        else if (estado.MiCadete.CapacidadActiva)
        {
            // Ventaja de la capacidad especial: revela una anomalia automaticamente.
            string[] sectores = { "Sector Delta", "Zona de Ruinas", "Corredor Omega", "Zona de interferencia" };
            estado.SistemaIris.RegistrarAnomalia(sectores[rng.Next(sectores.Length)]);
            Console.WriteLine("NEXUS: Escaneo total activo. Anomalia revelada en " + estado.SistemaIris.UbicacionAnomalia + ".");
        }
        else
        {
            Console.WriteLine("NEXUS: No hay anomalias registradas por el momento.");
        }
    }

    // ===== NUEVO: Punto 2 - Menú unificado de Capacidades y Equipamiento =====

    static void AbrirSistemasDeAccion(EstadoJuego estado)
    {
        Console.Clear();

        Console.ForegroundColor = ConsoleColor.DarkCyan;

        Console.WriteLine("╔════════════════════════════════════════════╗");
        Console.WriteLine("║          NEXUS // SISTEMAS DE ACCIÓN       ║");
        Console.WriteLine("╠════════════════════════════════════════════╣");
        Console.WriteLine("║ [1] " + ObtenerEtiquetaAccionEspecial(estado.TipoPersonaje));
        Console.WriteLine("║ [0] ↩ Regresar                             ║");
        Console.WriteLine("╚════════════════════════════════════════════╝");

        Console.ResetColor();

        Console.Write("Seleccione una acción: ");
        string opcion = Console.ReadLine();
        if (opcion == "1")
        {
            AccionEspecialDeClase(estado);
            Thread.Sleep(1200);
            return;
        }
        if (opcion == "0")
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine("⚠️ Sistema seleccionado.");
        Console.WriteLine("NEXUS: Esta función será integrada próximamente.");
        Thread.Sleep(1200);
    }

    // ----- Etiqueta del menú de "Sistemas de acción", según la clase -----
    static string ObtenerEtiquetaAccionEspecial(string tipoPersonaje)
    {
        if (tipoPersonaje == "FISICO")
        {
            return "👊 Ataque físico                      ║";
        }
        else if (tipoPersonaje == "ARMAMENTO")
        {
            return "🔫 Arsenal                             ║";
        }
        else if (tipoPersonaje == "MANA")
        {
            return "🔮 Habilidad de mana                   ║";
        }
        else
        {
            return "🖥️ Escaneo avanzado                    ║";
        }
    }

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
                InterfazNexus.MostrarTextoManual();
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
            Console.WriteLine("🛏 Cama    🖥 Terminal    📦 Equipamiento    🚪 Salida");
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
                InteractuarTerminalBase(estado);
            }
            else if (casilla == 'E')
            {
                InteractuarEquipamientoBase(estado);
            }
            else if (casilla == 'S')
            {
                Console.WriteLine();
                Console.WriteLine("🚪 NEXUS: Ha llegado a la salida de la base.");
                Console.Write("¿Desea iniciar la misión y abandonar la base? [S/N]: ");
                string respuesta = (Console.ReadLine() ?? "").Trim().ToUpper();

                if (respuesta == "S")
                {
                    return true;
                }

                Console.WriteLine("NEXUS: Permaneciendo en la base.");
                Thread.Sleep(800);
            }
        }
    }

    static void InteractuarCama(EstadoJuego estado)
    {
        Console.WriteLine();
        Console.WriteLine("🛏 NEXUS: Esta es su litera de descanso.");
        Console.Write("¿Desea descansar para recuperar Energía y Estabilidad? [S/N]: ");
        string respuesta = (Console.ReadLine() ?? "").Trim().ToUpper();

        if (respuesta == "S")
        {
            estado.MiCadete.RecuperarEnergia(100);
            estado.MiCadete.RecuperarEstabilidad(100);
            Console.WriteLine("NEXUS: Energía y Estabilidad restauradas al 100%.");
            Thread.Sleep(1000);
        }
    }

    static void InteractuarTerminalBase(EstadoJuego estado)
    {
        Console.WriteLine();
        Console.WriteLine("🖥 NEXUS: Terminal de mando central.");

        if (!estado.BienvenidaNexusMostrada)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            InterfazNexus.MostrarTexto("NEXUS: Bienvenido, Cadete " + estado.Nombre + ".", true, 200);
            InterfazNexus.MostrarTexto("NEXUS: La conexión con NEXUS ha sido establecida.", true, 200);
            InterfazNexus.MostrarTexto("NEXUS: Tu inmersión ha sido autorizada.", true, 200);
            InterfazNexus.MostrarTexto("NEXUS: Realidad asignada: " + estado.Realidad + ".", true, 200);
            InterfazNexus.MostrarTexto("NEXUS: Los sistemas están sincronizados.", true, 200);
            InterfazNexus.MostrarTexto("NEXUS: Yo soy NEXUS (>‿<)✌️ y te guiaré durante esta misión.", true, 200);
            InterfazNexus.MostrarTexto("NEXUS: Los archivos históricos de IRIS están disponibles.", true, 200);
            InterfazNexus.MostrarTexto("NEXUS: puedes consultarlos desde el menú cuando lo desees.", true, 200);
            Console.ResetColor();

            estado.BienvenidaNexusMostrada = true;
        }
        else
        {
            Console.WriteLine("NEXUS: Explorador " + estado.Nombre + ", bienvenido de vuelta.");
            Console.WriteLine("NEXUS: Realidad asignada: " + estado.Realidad);
        }

        Thread.Sleep(1500);
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
        int? vidaRecuperada = estado.Exploracion.RecogerBotiquinEnPosicionActual(CuracionBotiquin);

        if (vidaRecuperada == null)
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine("🩹 BOTIQUÍN ENCONTRADO");
        Console.WriteLine("NEXUS: Has utilizado un botiquín.");
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
        estado.EnemigosGenesis.Remove(enemigo);

        Console.WriteLine();
        Console.WriteLine("NEXUS: Fragmento de Poder obtenido.");
        Console.WriteLine("NEXUS: Poder +" + Combate.PoderPorEnemigoDerrotado);
        Console.WriteLine("NEXUS: Poder actual: " + estado.MiCadete.Poder + "/100");

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
                    victoria = combate.Atacar(Combate.DañoAtaqueJugador, false);
                    mensaje = "🗡️ Ataque con katana: el enemigo pierde " + Combate.DañoAtaqueJugador + " de vida.";
                }
                else
                {
                    int dañoRecibido = combate.Contraatacar(true);
                    derrota = combate.JugadorDerrotado;
                    mensaje = "🛡️ Defensa: el cadete recibe " + dañoRecibido + " de daño.";
                }

                InterfazNexus.MostrarHudCombateAnimado(estado, enemigo);
                InterfazNexus.MostrarMensajeCombateAnimado(mensaje);

                if (victoria || derrota)
                {
                    Thread.Sleep(1200);
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
            Thread.Sleep(150);
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
            Thread.Sleep(1000);
            combate.Huir();
        }

        return true;
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
                bool enemigoDerrotado = combate.Atacar(Combate.DañoAtaqueJugador);

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
                Thread.Sleep(1000);
                combate.Huir();
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