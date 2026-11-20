using System;
using System.Collections.Generic;
using System.Linq;

namespace WinForms
{
    public enum Interaccion { Ninguna, Cama, Terminal, Equipamiento, Salida, Puerta }

    public enum Encuentro { Ninguno, Base, Combate }

    public sealed class Dialogo
    {
        public string Titulo { get; }
        public string Pregunta { get; }
        public Action Accion { get; }

        public Dialogo(string titulo, string pregunta, Action accion)
        {
            Titulo = titulo;
            Pregunta = pregunta;
            Accion = accion;
        }
    }

    public sealed class Juego
    {
        public const int DuracionAnimacion = 13;
        public const int DuracionAviso = 110;
        public const int TicksPorLineaArchivo = 14;
        public const int UmbralPoder = 100;
        public const int DuracionCapacidad = 3;
        public const int EstabilidadMinima = 15;
        public const int MetrosPorCelda = 5;
        public const int LimiteNombre = 20;

        public static readonly string[] Clases = { "FISICO", "ARMAMENTO", "MANA", "HERRAMIENTAS" };

        public static readonly string[] DescClases =
        {
            "Especialista en combate cuerpo a cuerpo",
            "Especialista en combate con armas",
            "Especialista en energía elemental",
            "Especialista tecnológico"
        };

        public static readonly string[] TextosMenu =
        {
            "[1]  Capacidades / Equipamiento",
            "[2]  Manual de uso",
            "[3]  Nexo multiversal",
            "[0]  Desconexión"
        };

        public static readonly int[] TeclasMenu = { 1, 2, 3, 0 };

        public static readonly string[] Reinos = { "Genesis", "Gama", "Delta", "Épsilon", "Tau", "Omega", "Volver" };

        public static readonly string[] Archivos =
        {
            "NEXUS: Recuperando archivos históricos......",
            "ARCHIVO RECUPERADO: IRIS",
            "ORIGEN: UNIF.....",
            "FECHA DE INICIO: 2026",
            "",
            "En el año 2026 se inició un proyecto experimental en un servidor de UNIF......",
            "El sistema fue denominado IRIS.",
            "Su objetivo era analizar grandes cantidades de información y detectar patrones anómalos.",
            "Uno de los responsables aparece registrado como Diego Patr..",
            "NEXUS: Lo siento, mis datos están incompletos.",
            "NEXUS: ¿Quién habrá alterado mi información?",
            "",
            "El proyecto fue cancelado después de que IRIS comenzara a detectar patrones",
            "que ningún investigador podía explicar.",
            "El servidor fue desconectado y el proyecto fue declarado perdido.",
            "",
            "AÑO 2297........AÑO ACTUAL.....",
            "Los registros indican que IRIS nunca desapareció.",
            "Ahora necesitamos descubrir qué encontró IRIS y por qué fue cancelado."
        };

        static readonly string[] FilasBase =
        {
            "||||||||||",
            "|··C···T·|",
            "|········|",
            "|··E·····|",
            "|····S··B|",
            "||||||||||"
        };

        static readonly string[] FilasGenesis =
        {
            "|||||||||||",
            "|⌂···◆····|",
            "|······◆··|",
            "|···⚠··A··|",
            "|·◆···A···|",
            "|····◆····|",
            "|········◆|",
            "|||||||||||"
        };

        readonly List<Mision> misiones = new List<Mision>();
        readonly List<Enemigo> enemigos = new List<Enemigo>();
        string nombreEntrada = "";
        bool[,] recogido;

        public int Tick { get; private set; }
        public string Aviso { get; private set; } = "";
        public int AvisoTicks { get; private set; }
        public Dialogo Dialogo { get; private set; }

        public string NombreEntrada { get { return nombreEntrada; } }
        public int SeleccionClase { get; private set; }
        public int SeleccionMenu { get; private set; }
        public int ManualSub { get; private set; }
        public int ArchivoTick { get; private set; }
        public int MisionDetalle { get; private set; }
        public string TipoPersonaje { get; private set; } = "";
        public string MensajeFin { get; private set; } = "";

        public Cadete Cadete { get; private set; }
        public bool TieneArma { get; private set; }
        public int MisionIndice { get; private set; }

        public char[,] MapaBase { get; private set; }
        public int BFila { get; private set; } = 2;
        public int BCol { get; private set; } = 2;

        public char[,] MapaGenesis { get; private set; }
        public bool[,] Explorada { get; private set; }
        public int GFila { get; private set; }
        public int GCol { get; private set; }
        public int Distancia { get; private set; }

        public Combate Combate { get; private set; }
        public FaseCombate Fase { get; private set; }
        public int AnimTick { get; private set; }
        public string MensajeCombate { get; private set; } = "";
        public bool ObjetivoMision { get; private set; }

        public IReadOnlyList<Mision> Misiones { get { return misiones; } }
        public IReadOnlyList<Enemigo> Enemigos { get { return enemigos; } }

        public Mision MisionActual
        {
            get { return MisionIndice < misiones.Count ? misiones[MisionIndice] : null; }
        }

        public void Avanzar()
        {
            Tick++;
            if (AvisoTicks > 0) AvisoTicks--;
        }

        public void Avisar(string texto)
        {
            Aviso = texto;
            AvisoTicks = DuracionAviso;
        }

        public void Confirmar(string titulo, string pregunta, Action accion)
        {
            Dialogo = new Dialogo(titulo, pregunta, accion);
        }

        public void ResolverDialogo(bool aceptado)
        {
            Dialogo dialogo = Dialogo;
            Dialogo = null;
            if (aceptado && dialogo != null && dialogo.Accion != null) dialogo.Accion();
        }

        public void EscribirCaracter(char c)
        {
            if (c == '\b')
            {
                if (nombreEntrada.Length > 0) nombreEntrada = nombreEntrada.Substring(0, nombreEntrada.Length - 1);
            }
            else if (!char.IsControl(c) && nombreEntrada.Length < LimiteNombre)
            {
                nombreEntrada += c;
            }
        }

        public bool ConfirmarNombre()
        {
            if (string.IsNullOrWhiteSpace(nombreEntrada))
            {
                Avisar("Error: el nombre no puede estar vacío.");
                return false;
            }
            return true;
        }

        public void MoverSeleccionClase(int delta)
        {
            SeleccionClase = (SeleccionClase + delta + Clases.Length) % Clases.Length;
        }

        public void ElegirClase(int indice)
        {
            SeleccionClase = indice;
        }

        public void IniciarPartida()
        {
            TipoPersonaje = Clases[SeleccionClase];
            Cadete = new Cadete(nombreEntrada.Trim());
            TieneArma = TipoPersonaje == "ARMAMENTO";
            MisionIndice = 0;
            Distancia = 0;

            misiones.Clear();
            misiones.Add(new Mision("EL PRIMER OBJETIVO", "Derrota al enemigo LIQUIDO (6,5).", "LIQUIDO", "Este enemigo reduce el daño de tus ataques. Ataca rápido antes de que te desgaste.", 20, 50));
            misiones.Add(new Mision("LA PRUEBA DE FUEGO", "Derrota al enemigo FUEGO (4,8).", "FUEGO", "Este enemigo golpea cada vez más fuerte. Cuida tu vida y no alargues el combate.", 20, 50));
            misiones.Add(new Mision("EL ÚLTIMO VENENO", "Derrota al enemigo VENENO (2,3).", "VENENO", "Este enemigo te quita vida en cada turno. Termina el combate lo antes posible.", 20, 50));

            MapaBase = ConvertirMapa(FilasBase);
            MapaGenesis = ConvertirMapa(FilasGenesis);
            Explorada = new bool[MapaGenesis.GetLength(0), MapaGenesis.GetLength(1)];
            recogido = new bool[MapaGenesis.GetLength(0), MapaGenesis.GetLength(1)];
            GFila = MapaGenesis.GetLength(0) / 2;
            GCol = MapaGenesis.GetLength(1) / 2;
            RevelarZona();

            enemigos.Clear();
            enemigos.Add(new Enemigo(2, 3, "VENENO"));
            enemigos.Add(new Enemigo(4, 8, "FUEGO"));
            enemigos.Add(new Enemigo(6, 5, "LIQUIDO"));

            BFila = 2;
            BCol = 2;
            SeleccionMenu = 0;
            Dialogo = null;
            Combate = null;
            Fase = FaseCombate.Jugador;
            Avisar("INMERSIÓN AUTORIZADA · Realidad GENESIS · " + TipoPersonaje);
        }

        public void MoverSeleccionMenu(int delta)
        {
            SeleccionMenu = (SeleccionMenu + delta + TextosMenu.Length) % TextosMenu.Length;
        }

        public bool IntentarDesconectar()
        {
            if (Cadete.Estabilidad < EstabilidadMinima)
            {
                Avisar("DESCONEXIÓN INSEGURA: la estabilidad debe ser al menos " + EstabilidadMinima + "%.");
                return false;
            }
            MensajeFin = "CONEXIÓN FINALIZADA.";
            return true;
        }

        public void ActivarHabilidad()
        {
            if (Cadete.Poder < UmbralPoder)
            {
                Avisar("NEXUS: Poder insuficiente. Se necesitan " + UmbralPoder + " puntos.");
                return;
            }
            Cadete.ConsumirPoder(Cadete.Poder);
            Cadete.ActivarCapacidad(DuracionCapacidad);
            Avisar("Capacidad especial activada durante " + DuracionCapacidad + " turnos.");
        }

        public void AbrirManual()
        {
            ManualSub = 0;
        }

        public void ElegirManual(int sub)
        {
            ManualSub = sub;
            if (sub == 2) ArchivoTick = Tick;
        }

        public Interaccion MoverBase(int df, int dc)
        {
            int nf = BFila + df;
            int nc = BCol + dc;
            if (MapaBase[nf, nc] == '|') return Interaccion.Ninguna;

            BFila = nf;
            BCol = nc;
            PasoDeTurno();

            switch (MapaBase[BFila, BCol])
            {
                case 'C': return Interaccion.Cama;
                case 'T': return Interaccion.Terminal;
                case 'E': return Interaccion.Equipamiento;
                case 'S': return Interaccion.Salida;
                case 'B': return Interaccion.Puerta;
                default: return Interaccion.Ninguna;
            }
        }

        public void Descansar()
        {
            Cadete.RecuperarVida(100);
            Avisar("NEXUS: Vida restaurada al máximo (" + Cadete.Vida + "%).");
        }

        public Encuentro MoverGenesis(int df, int dc)
        {
            int nf = GFila + df;
            int nc = GCol + dc;
            if (nf < 0 || nf >= MapaGenesis.GetLength(0) || nc < 0 || nc >= MapaGenesis.GetLength(1)) return Encuentro.Ninguno;
            if (MapaGenesis[nf, nc] == '|')
            {
                Avisar("NEXUS: Límite territorial. Movimiento bloqueado.");
                return Encuentro.Ninguno;
            }

            GFila = nf;
            GCol = nc;
            RevelarZona();
            Distancia += MetrosPorCelda;
            PasoDeTurno();

            char celda = MapaGenesis[GFila, GCol];
            if (celda == '⌂') return Encuentro.Base;

            if (celda == '◆' && !recogido[GFila, GCol])
            {
                recogido[GFila, GCol] = true;
                MapaGenesis[GFila, GCol] = '·';
                int antes = Cadete.Vida;
                Cadete.RecuperarVida(25);
                Cadete.RecolectarRecurso();
                Avisar("BOTIQUÍN ENCONTRADO · Vida +" + (Cadete.Vida - antes) + " · Vida actual: " + Cadete.Vida + "/100");
            }
            else if (celda == '⚠')
            {
                MapaGenesis[GFila, GCol] = '·';
                Avisar("NEXUS: Actividad anómala detectada en esta zona.");
            }

            int fila = GFila;
            int col = GCol;
            Enemigo enemigo = enemigos.FirstOrDefault(en => en.Fila == fila && en.Columna == col);
            if (enemigo == null) return Encuentro.Ninguno;

            IniciarCombate(enemigo);
            return Encuentro.Combate;
        }

        public void AvanzarCombate()
        {
            if (Combate == null) return;

            if (Fase == FaseCombate.AnimAtaque)
            {
                AnimTick++;
                if (AnimTick < DuracionAnimacion) return;

                AnimTick = 0;
                int aplicado;
                bool victoria = Combate.Atacar(out aplicado, !ObjetivoMision);
                MensajeCombate = NombreAtaque() + ": el enemigo pierde " + aplicado + " de vida.";
                if (victoria)
                {
                    FinalizarVictoria();
                    Fase = FaseCombate.Victoria;
                }
                else
                {
                    Fase = FaseCombate.Enemigo;
                }
            }
            else if (Fase == FaseCombate.AnimDefensa)
            {
                AnimTick++;
                if (AnimTick < DuracionAnimacion) return;

                AnimTick = 0;
                int recibido = Combate.Contraatacar();
                MensajeCombate = "Defensa: el cadete recibe " + recibido + " de daño." + NotaEfecto();
                Fase = Combate.JugadorDerrotado ? FaseCombate.Derrota : FaseCombate.Jugador;
            }
        }

        public void PedirAtaque()
        {
            if (Fase == FaseCombate.Jugador)
            {
                if (TipoPersonaje == "ARMAMENTO" && !TieneArma)
                {
                    MensajeCombate = "NEXUS: No dispone de un arma equipada para atacar.";
                    return;
                }
                Fase = FaseCombate.AnimAtaque;
                AnimTick = 0;
            }
            else if (Fase == FaseCombate.Enemigo)
            {
                MensajeCombate = "NEXUS: Turno del enemigo, presiona [D] para defenderte.";
            }
        }

        public void PedirDefensa()
        {
            if (Fase == FaseCombate.Jugador)
            {
                MensajeCombate = "NEXUS: Es tu turno, presiona [A] para atacar.";
            }
            else if (Fase == FaseCombate.Enemigo)
            {
                Fase = FaseCombate.AnimDefensa;
                AnimTick = 0;
            }
        }

        public bool PedirHuida()
        {
            if (Fase != FaseCombate.Jugador) return false;

            Combate.Huir();
            if (Combate.JugadorDerrotado)
            {
                Fase = FaseCombate.Derrota;
                return false;
            }
            Avisar("Huyendo de la zona. Huir cuesta " + Combate.DanioPorHuir + " de vida. Vida actual: " + Cadete.Vida);
            return true;
        }

        public void TerminarPorDerrota()
        {
            MensajeFin = "EL CADETE HA CAÍDO EN COMBATE. Desconexión de emergencia.";
        }

        public void SeleccionarMision(int indice)
        {
            MisionDetalle = indice;
        }

        public bool ReclamarRecompensa()
        {
            Mision m = misiones[MisionDetalle];
            if (MisionDetalle != MisionIndice || !m.ObjetivoCumplido || m.Completada) return false;

            Cadete.GanarPoder(m.RecompensaPoder);
            Cadete.GanarExperiencia(m.RecompensaXp);
            m.MarcarCompletada();
            MisionIndice++;
            Avisar("MISIÓN COMPLETADA · +" + m.RecompensaPoder + " PODER · +" + m.RecompensaXp + " XP");
            return true;
        }

        public string EstadoMision(int i)
        {
            Mision m = misiones[i];
            if (m.Completada) return "COMPLETADA";
            if (i != MisionIndice) return "BLOQUEADA";
            return m.ObjetivoCumplido ? "LISTA PARA RECLAMAR" : "ACTUAL";
        }

        void IniciarCombate(Enemigo enemigo)
        {
            Combate = new Combate(Cadete, enemigo);
            Mision actual = MisionActual;
            ObjetivoMision = actual != null && !actual.Completada && enemigo.Efecto == actual.TipoEnemigo;
            Fase = FaseCombate.Jugador;
            AnimTick = 0;
            MensajeCombate = "NEXUS: Enemigo detectado. Elija una acción.";
        }

        void FinalizarVictoria()
        {
            enemigos.Remove(Combate.Enemigo);
            if (ObjetivoMision) MisionActual.MarcarObjetivo();
        }

        string NombreAtaque()
        {
            switch (TipoPersonaje)
            {
                case "FISICO": return "Golpe Dash";
                case "ARMAMENTO": return "Corte Flash";
                case "MANA": return "Proyectil de Mana";
                default: return "Sobrecarga";
            }
        }

        string NotaEfecto()
        {
            switch (Combate.Enemigo.Efecto)
            {
                case "FUEGO": return " El fuego intensifica el golpe.";
                case "VENENO": return " El veneno sigue drenando vida.";
                default: return "";
            }
        }

        void PasoDeTurno()
        {
            if (Cadete.DescontarTurnoCapacidad()) Avisar("NEXUS: La capacidad especial se ha desactivado.");
        }

        void RevelarZona()
        {
            for (int df = -1; df <= 1; df++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    int f = GFila + df;
                    int c = GCol + dc;
                    if (f >= 0 && f < Explorada.GetLength(0) && c >= 0 && c < Explorada.GetLength(1))
                        Explorada[f, c] = true;
                }
            }
        }

        static char[,] ConvertirMapa(string[] filas)
        {
            var mapa = new char[filas.Length, filas[0].Length];
            for (int f = 0; f < filas.Length; f++)
                for (int c = 0; c < filas[f].Length; c++)
                    mapa[f, c] = filas[f][c];
            return mapa;
        }
    }
}