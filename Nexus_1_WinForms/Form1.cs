using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace WinForms
{
    public partial class Form1 : Form
    {
        static readonly Color Fondo = Color.FromArgb(12, 14, 24);
        static readonly Color Panel = Color.FromArgb(22, 26, 42);
        static readonly Color Cian = Color.FromArgb(0, 205, 225);
        static readonly Color Amarillo = Color.FromArgb(245, 205, 70);
        static readonly Color Verde = Color.FromArgb(70, 210, 120);
        static readonly Color Rojo = Color.FromArgb(230, 70, 80);
        static readonly Color Magenta = Color.FromArgb(190, 90, 220);
        static readonly Color Gris = Color.FromArgb(130, 138, 160);
        static readonly Color Blanco = Color.FromArgb(235, 238, 248);

        readonly Font fTitulo = new Font("Consolas", 26, FontStyle.Bold);
        readonly Font fGrande = new Font("Consolas", 16, FontStyle.Bold);
        readonly Font fNormal = new Font("Consolas", 12);
        readonly Font fNegrita = new Font("Consolas", 12, FontStyle.Bold);
        readonly Font fPeq = new Font("Consolas", 10);

        const int UmbralPoder = 100;
        const int DuracionCapacidad = 3;
        const int EstabilidadMinima = 15;
        const int MetrosPorCelda = 5;

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

        static readonly string[] Clases = { "FISICO", "ARMAMENTO", "MANA", "HERRAMIENTAS" };
        static readonly string[] DescClases =
        {
            "Especialista en combate cuerpo a cuerpo",
            "Especialista en combate con armas",
            "Especialista en energía elemental",
            "Especialista tecnológico"
        };

        static readonly string[] Archivos =
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

        Pantalla pantalla = Pantalla.Titulo;
        Pantalla pantallaAnterior = Pantalla.Menu;
        int tick;
        string nombreEntrada = "";
        int seleccionClase;
        int seleccionMenu;
        int manualSub;
        int archivoTick;
        int misionDetalle;
        string tipoPersonaje = "";
        string mensajeFin = "";
        string toast = "";
        int toastTicks;

        Cadete cadete;
        bool tieneArma;
        List<Mision> misiones;
        int misionIndice;

        char[,] mapaBase;
        int bFila = 2, bCol = 2;
        char[,] mapaGenesis;
        bool[,] explorada;
        bool[,] recogido;
        int gFila, gCol;
        int distancia;
        List<Enemigo> enemigos;

        float visX, visY;

        bool confirmando;
        string confTitulo = "";
        string confPregunta = "";
        Action confAccion;

        Combate combate;
        FaseCombate fase;
        int animTick;
        string mensajeCombate = "";
        bool objetivoMision;
        bool alertaIris;

        public Form1()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            timerJuego.Start();
        }

        void Form1_Load(object sender, EventArgs e)
        {
        }

        void IniciarPartida()
        {
            tipoPersonaje = Clases[seleccionClase];
            cadete = new Cadete(nombreEntrada.Trim());
            tieneArma = tipoPersonaje == "ARMAMENTO";
            misionIndice = 0;
            distancia = 0;
            misiones = new List<Mision>
            {
                new Mision("EL PRIMER OBJETIVO", "Derrota al enemigo LIQUIDO (6,5).", "LIQUIDO", "Este enemigo reduce el daño de tus ataques. Ataca rápido antes de que te desgaste.", 20, 50),
                new Mision("LA PRUEBA DE FUEGO", "Derrota al enemigo FUEGO (4,8).", "FUEGO", "Este enemigo golpea cada vez más fuerte. Cuida tu vida y no alargues el combate.", 20, 50),
                new Mision("EL ÚLTIMO VENENO", "Derrota al enemigo VENENO (2,3).", "VENENO", "Este enemigo te quita vida en cada turno. Termina el combate lo antes posible.", 20, 50)
            };
            mapaBase = ConvertirMapa(FilasBase);
            mapaGenesis = ConvertirMapa(FilasGenesis);
            explorada = new bool[mapaGenesis.GetLength(0), mapaGenesis.GetLength(1)];
            recogido = new bool[mapaGenesis.GetLength(0), mapaGenesis.GetLength(1)];
            gFila = mapaGenesis.GetLength(0) / 2;
            gCol = mapaGenesis.GetLength(1) / 2;
            RevelarZona();
            enemigos = new List<Enemigo>
            {
                new Enemigo(2, 3, "VENENO"),
                new Enemigo(4, 8, "FUEGO"),
                new Enemigo(6, 5, "LIQUIDO")
            };
            bFila = 2;
            bCol = 2;
            seleccionMenu = 0;
            pantalla = Pantalla.Menu;
            Avisar("INMERSIÓN AUTORIZADA · Realidad GENESIS · " + tipoPersonaje);
        }

        static char[,] ConvertirMapa(string[] filas)
        {
            var m = new char[filas.Length, filas[0].Length];
            for (int f = 0; f < filas.Length; f++)
                for (int c = 0; c < filas[f].Length; c++)
                    m[f, c] = filas[f][c];
            return m;
        }

        Mision MisionActual => misionIndice < misiones.Count ? misiones[misionIndice] : null;

        void Avisar(string texto)
        {
            toast = texto;
            toastTicks = 220;
        }

        void RevelarZona()
        {
            for (int df = -1; df <= 1; df++)
                for (int dc = -1; dc <= 1; dc++)
                {
                    int f = gFila + df, c = gCol + dc;
                    if (f >= 0 && f < explorada.GetLength(0) && c >= 0 && c < explorada.GetLength(1))
                        explorada[f, c] = true;
                }
        }

        void timerJuego_Tick(object sender, EventArgs e)
        {
            tick++;
            if (toastTicks > 0) toastTicks--;

            float objX = 0, objY = 0;
            int tam = 0;
            if (pantalla == Pantalla.Base) { objX = bCol; objY = bFila; tam = 1; }
            else if (pantalla == Pantalla.Genesis) { objX = gCol; objY = gFila; tam = 1; }
            if (tam == 1)
            {
                visX += (objX - visX) * 0.35f;
                visY += (objY - visY) * 0.35f;
            }

            if (pantalla == Pantalla.Combate) ActualizarCombate();
            Invalidate();
        }

        void ActualizarCombate()
        {
            if (fase == FaseCombate.AnimAtaque)
            {
                animTick++;
                if (animTick >= 26)
                {
                    animTick = 0;
                    int aplicado;
                    bool victoria = combate.Atacar(out aplicado, !objetivoMision);
                    mensajeCombate = NombreAtaque() + ": el enemigo pierde " + aplicado + " de vida.";
                    if (victoria)
                    {
                        FinalizarVictoria();
                        fase = FaseCombate.Victoria;
                    }
                    else fase = FaseCombate.Enemigo;
                }
            }
            else if (fase == FaseCombate.AnimDefensa)
            {
                animTick++;
                if (animTick >= 26)
                {
                    animTick = 0;
                    int recibido = combate.Contraatacar();
                    mensajeCombate = "Defensa: el cadete recibe " + recibido + " de daño." + NotaEfecto();
                    fase = combate.JugadorDerrotado ? FaseCombate.Derrota : FaseCombate.Jugador;
                }
            }
        }

        string NombreAtaque()
        {
            switch (tipoPersonaje)
            {
                case "FISICO": return "Golpe Dash";
                case "ARMAMENTO": return "Corte Flash";
                case "MANA": return "Proyectil de Mana";
                default: return "Sobrecarga";
            }
        }

        string NotaEfecto()
        {
            switch (combate.Enemigo.Efecto)
            {
                case "FUEGO": return " El fuego intensifica el golpe.";
                case "VENENO": return " El veneno sigue drenando vida.";
                default: return "";
            }
        }

        void FinalizarVictoria()
        {
            enemigos.Remove(combate.Enemigo);
            if (objetivoMision) MisionActual.MarcarObjetivo();
            alertaIris = true;
        }

        void IniciarCombate(Enemigo enemigo)
        {
            combate = new Combate(cadete, enemigo);
            var actual = MisionActual;
            objetivoMision = actual != null && !actual.Completada && enemigo.Efecto == actual.TipoEnemigo;
            fase = FaseCombate.Jugador;
            animTick = 0;
            alertaIris = false;
            mensajeCombate = "NEXUS: Enemigo detectado. Elija una acción.";
            pantalla = Pantalla.Combate;
        }

        void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (pantalla != Pantalla.Registro) return;
            if (e.KeyChar == (char)Keys.Back)
            {
                if (nombreEntrada.Length > 0) nombreEntrada = nombreEntrada.Substring(0, nombreEntrada.Length - 1);
            }
            else if (!char.IsControl(e.KeyChar) && nombreEntrada.Length < 20)
            {
                nombreEntrada += e.KeyChar;
            }
            e.Handled = true;
        }

        static int NumeroTecla(Keys k)
        {
            if (k >= Keys.D0 && k <= Keys.D9) return k - Keys.D0;
            if (k >= Keys.NumPad0 && k <= Keys.NumPad9) return k - Keys.NumPad0;
            return -1;
        }

        static bool Direccion(Keys k, out int df, out int dc)
        {
            df = 0;
            dc = 0;
            if (k == Keys.W || k == Keys.Up) df = -1;
            else if (k == Keys.S || k == Keys.Down) df = 1;
            else if (k == Keys.A || k == Keys.Left) dc = -1;
            else if (k == Keys.D || k == Keys.Right) dc = 1;
            else return false;
            return true;
        }

        void Confirmar(string titulo, string pregunta, Action accion)
        {
            confTitulo = titulo;
            confPregunta = pregunta;
            confAccion = accion;
            confirmando = true;
        }

        void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Keys k = e.KeyCode;
            int n = NumeroTecla(k);

            if (confirmando)
            {
                if (k == Keys.E || k == Keys.Enter)
                {
                    confirmando = false;
                    confAccion?.Invoke();
                }
                else if (k == Keys.X || k == Keys.Escape)
                {
                    confirmando = false;
                }
                return;
            }

            switch (pantalla)
            {
                case Pantalla.Titulo:
                    if (k == Keys.Enter) pantalla = Pantalla.Registro;
                    break;

                case Pantalla.Registro:
                    if (k == Keys.Enter)
                    {
                        if (string.IsNullOrWhiteSpace(nombreEntrada)) Avisar("Error: el nombre no puede estar vacío.");
                        else pantalla = Pantalla.Seleccion;
                    }
                    e.SuppressKeyPress = false;
                    break;

                case Pantalla.Seleccion:
                    if (k == Keys.Up) seleccionClase = (seleccionClase + 3) % 4;
                    else if (k == Keys.Down) seleccionClase = (seleccionClase + 1) % 4;
                    else if (n >= 1 && n <= 4) { seleccionClase = n - 1; IniciarPartida(); }
                    else if (k == Keys.Enter) IniciarPartida();
                    else if (k == Keys.Escape) pantalla = Pantalla.Registro;
                    break;

                case Pantalla.Menu:
                    TeclaMenu(k, n);
                    break;

                case Pantalla.Capacidades:
                    if (n == 1) ActivarHabilidad();
                    else if (n == 0 || k == Keys.Escape) pantalla = pantallaAnterior;
                    break;

                case Pantalla.Manual:
                    TeclaManual(k, n);
                    break;

                case Pantalla.Nexo:
                    if (n == 1) EntrarBase();
                    else if (n >= 2 && n <= 6) Avisar("NEXUS: Realidad no disponible.");
                    else if (n == 7 || n == 0 || k == Keys.Escape) pantalla = Pantalla.Menu;
                    break;

                case Pantalla.Base:
                    TeclaBase(k);
                    break;

                case Pantalla.Genesis:
                    TeclaGenesis(k);
                    break;

                case Pantalla.Combate:
                    TeclaCombate(k);
                    break;

                case Pantalla.Terminal:
                    if (n >= 1 && n <= misiones.Count) { misionDetalle = n - 1; pantalla = Pantalla.DetalleMision; }
                    else if (n == 0 || k == Keys.Escape) pantalla = Pantalla.Base;
                    break;

                case Pantalla.DetalleMision:
                    TeclaDetalleMision(k);
                    break;

                case Pantalla.Fin:
                    if (k == Keys.Enter || k == Keys.Escape) Close();
                    break;
            }
        }

        void TeclaMenu(Keys k, int n)
        {
            int[] orden = { 1, 2, 3, 0 };
            if (k == Keys.Up) seleccionMenu = (seleccionMenu + 3) % 4;
            else if (k == Keys.Down) seleccionMenu = (seleccionMenu + 1) % 4;
            else if (k == Keys.Enter) n = orden[seleccionMenu];

            if (n == 1) { pantallaAnterior = Pantalla.Menu; pantalla = Pantalla.Capacidades; }
            else if (n == 2) { manualSub = 0; pantalla = Pantalla.Manual; }
            else if (n == 3) pantalla = Pantalla.Nexo;
            else if (n == 0 && (k == Keys.Enter || k == Keys.D0 || k == Keys.NumPad0)) Desconectar();
        }

        void Desconectar()
        {
            if (cadete.Estabilidad < EstabilidadMinima)
            {
                Avisar("DESCONEXIÓN INSEGURA: la estabilidad debe ser al menos " + EstabilidadMinima + "%.");
                return;
            }
            mensajeFin = "CONEXIÓN FINALIZADA.";
            pantalla = Pantalla.Fin;
        }

        void ActivarHabilidad()
        {
            if (cadete.Poder < UmbralPoder)
            {
                Avisar("NEXUS: Poder insuficiente. Se necesitan " + UmbralPoder + " puntos.");
                return;
            }
            cadete.ConsumirPoder(cadete.Poder);
            cadete.ActivarCapacidad(DuracionCapacidad);
            Avisar("Capacidad especial activada durante " + DuracionCapacidad + " turnos.");
        }

        void TeclaManual(Keys k, int n)
        {
            if (manualSub == 0)
            {
                if (n == 1) manualSub = 1;
                else if (n == 2) { manualSub = 2; archivoTick = tick; }
                else if (n == 0 || k == Keys.Escape) pantalla = Pantalla.Menu;
            }
            else if (k == Keys.Enter || k == Keys.Escape) manualSub = 0;
        }

        void EntrarBase()
        {
            visX = bCol;
            visY = bFila;
            pantalla = Pantalla.Base;
        }

        void PasoDeTurno()
        {
            if (cadete.DescontarTurnoCapacidad()) Avisar("NEXUS: La capacidad especial se ha desactivado.");
        }

        void TeclaBase(Keys k)
        {
            if (k == Keys.Escape) { pantalla = Pantalla.Nexo; return; }
            int df, dc;
            if (!Direccion(k, out df, out dc)) return;

            int nf = bFila + df, nc = bCol + dc;
            if (mapaBase[nf, nc] == '|') return;
            bFila = nf;
            bCol = nc;
            PasoDeTurno();

            switch (mapaBase[bFila, bCol])
            {
                case 'C':
                    Confirmar("CAMA NEXUS", "¿Deseas descansar?", () =>
                    {
                        cadete.RecuperarVida(100);
                        Avisar("NEXUS: Vida restaurada al máximo (" + cadete.Vida + "%).");
                    });
                    break;
                case 'T':
                    Confirmar("TERMINAL NEXUS", "¿Deseas acceder al terminal?", () => pantalla = Pantalla.Terminal);
                    break;
                case 'E':
                    pantallaAnterior = Pantalla.Base;
                    pantalla = Pantalla.Capacidades;
                    break;
                case 'S':
                    Confirmar("SALIDA", "¿Salir e iniciar la misión?", () =>
                    {
                        visX = gCol;
                        visY = gFila;
                        pantalla = Pantalla.Genesis;
                    });
                    break;
                case 'B':
                    Confirmar("PUERTA BLOQUEADA", "¿Deseas interactuar?", () => Avisar("Acceso restringido. Esta sección todavía no está disponible."));
                    break;
            }
        }

        void TeclaGenesis(Keys k)
        {
            if (k == Keys.Escape) { pantalla = Pantalla.Menu; return; }
            int df, dc;
            if (!Direccion(k, out df, out dc)) return;

            int nf = gFila + df, nc = gCol + dc;
            if (nf < 0 || nf >= mapaGenesis.GetLength(0) || nc < 0 || nc >= mapaGenesis.GetLength(1)) return;
            if (mapaGenesis[nf, nc] == '|')
            {
                Avisar("NEXUS: Límite territorial. Movimiento bloqueado.");
                return;
            }

            gFila = nf;
            gCol = nc;
            RevelarZona();
            distancia += MetrosPorCelda;
            PasoDeTurno();

            char celda = mapaGenesis[gFila, gCol];

            if (celda == '⌂')
            {
                Confirmar("BASE OPERATIVA", "¿Deseas entrar a la base?", () => EntrarBase());
                return;
            }

            if (celda == '◆' && !recogido[gFila, gCol])
            {
                recogido[gFila, gCol] = true;
                mapaGenesis[gFila, gCol] = '·';
                int antes = cadete.Vida;
                cadete.RecuperarVida(25);
                cadete.RecolectarRecurso();
                Avisar("BOTIQUÍN ENCONTRADO · Vida +" + (cadete.Vida - antes) + " · Vida actual: " + cadete.Vida + "/100");
            }
            else if (celda == '⚠')
            {
                mapaGenesis[gFila, gCol] = '·';
                Avisar("NEXUS: Actividad anómala detectada en esta zona.");
            }

            var enemigo = enemigos.FirstOrDefault(en => en.Fila == gFila && en.Columna == gCol);
            if (enemigo != null) IniciarCombate(enemigo);
        }

        void TeclaCombate(Keys k)
        {
            if (fase == FaseCombate.Jugador)
            {
                if (k == Keys.A)
                {
                    if (tipoPersonaje == "ARMAMENTO" && !tieneArma)
                    {
                        mensajeCombate = "NEXUS: No dispone de un arma equipada para atacar.";
                        return;
                    }
                    fase = FaseCombate.AnimAtaque;
                    animTick = 0;
                }
                else if (k == Keys.H)
                {
                    combate.Huir();
                    if (combate.JugadorDerrotado) { fase = FaseCombate.Derrota; return; }
                    pantalla = Pantalla.Genesis;
                    Avisar("Huyendo de la zona. Huir cuesta " + Combate.DanioPorHuir + " de vida. Vida actual: " + cadete.Vida);
                }
                else if (k == Keys.D) mensajeCombate = "NEXUS: Es tu turno, presiona [A] para atacar.";
            }
            else if (fase == FaseCombate.Enemigo)
            {
                if (k == Keys.D) { fase = FaseCombate.AnimDefensa; animTick = 0; }
                else if (k == Keys.A) mensajeCombate = "NEXUS: Turno del enemigo, presiona [D] para defenderte.";
            }
            else if (fase == FaseCombate.Victoria && k == Keys.Enter)
            {
                pantalla = Pantalla.Genesis;
            }
            else if (fase == FaseCombate.Derrota && k == Keys.Enter)
            {
                mensajeFin = "EL CADETE HA CAÍDO EN COMBATE. Desconexión de emergencia.";
                pantalla = Pantalla.Fin;
            }
        }

        void TeclaDetalleMision(Keys k)
        {
            if (k == Keys.Escape || k == Keys.Back) { pantalla = Pantalla.Terminal; return; }
            var m = misiones[misionDetalle];
            if (k == Keys.S && misionDetalle == misionIndice && m.ObjetivoCumplido && !m.Completada)
            {
                cadete.GanarPoder(m.RecompensaPoder);
                cadete.GanarExperiencia(m.RecompensaXp);
                m.MarcarCompletada();
                misionIndice++;
                Avisar("MISIÓN COMPLETADA · +" + m.RecompensaPoder + " PODER · +" + m.RecompensaXp + " XP");
                pantalla = Pantalla.Terminal;
            }
        }

        string EstadoMision(int i)
        {
            var m = misiones[i];
            if (m.Completada) return "COMPLETADA";
            if (i != misionIndice) return "BLOQUEADA";
            return m.ObjetivoCumplido ? "LISTA PARA RECLAMAR" : "ACTUAL";
        }

        void Texto(Graphics g, string t, Font f, Color c, float x, float y)
        {
            using (var b = new SolidBrush(c)) g.DrawString(t, f, b, x, y);
        }

        void TextoCentro(Graphics g, string t, Font f, Color c, RectangleF r)
        {
            using (var b = new SolidBrush(c))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(t, f, b, r, sf);
        }

        void Marco(Graphics g, Rectangle r, string titulo, Color c)
        {
            using (var b = new SolidBrush(Panel)) g.FillRectangle(b, r);
            using (var p = new Pen(c, 2)) g.DrawRectangle(p, r);
            if (!string.IsNullOrEmpty(titulo))
            {
                using (var b = new SolidBrush(c)) g.FillRectangle(b, r.X, r.Y, r.Width, 30);
                TextoCentro(g, titulo, fNegrita, Fondo, new RectangleF(r.X, r.Y, r.Width, 30));
            }
        }

        void Barra(Graphics g, int x, int y, int w, int h, int pct, Color c, string etiqueta)
        {
            pct = Math.Max(0, Math.Min(100, pct));
            using (var b = new SolidBrush(Color.FromArgb(40, 44, 66))) g.FillRectangle(b, x, y, w, h);
            using (var b = new SolidBrush(c)) g.FillRectangle(b, x, y, w * pct / 100, h);
            using (var p = new Pen(Gris)) g.DrawRectangle(p, x, y, w, h);
            Texto(g, etiqueta + " " + pct + "%", fPeq, Blanco, x + 6, y + (h - 14) / 2f - 1);
        }

        void Form1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(Fondo);

            switch (pantalla)
            {
                case Pantalla.Titulo: DibujarTitulo(g); break;
                case Pantalla.Registro: DibujarRegistro(g); break;
                case Pantalla.Seleccion: DibujarSeleccion(g); break;
                case Pantalla.Menu: DibujarMenu(g); break;
                case Pantalla.Capacidades: DibujarCapacidades(g); break;
                case Pantalla.Manual: DibujarManual(g); break;
                case Pantalla.Nexo: DibujarNexo(g); break;
                case Pantalla.Base: DibujarBase(g); break;
                case Pantalla.Genesis: DibujarGenesis(g); break;
                case Pantalla.Combate: DibujarCombate(g); break;
                case Pantalla.Terminal: DibujarTerminal(g); break;
                case Pantalla.DetalleMision: DibujarDetalleMision(g); break;
                case Pantalla.Fin: DibujarFin(g); break;
            }

            if (confirmando) DibujarConfirmacion(g);
            if (toastTicks > 0 && toast.Length > 0) DibujarToast(g);
        }

        void DibujarToast(Graphics g)
        {
            var r = new Rectangle(80, ClientSize.Height - 54, ClientSize.Width - 160, 38);
            using (var b = new SolidBrush(Color.FromArgb(235, 10, 12, 20))) g.FillRectangle(b, r);
            using (var p = new Pen(Amarillo, 2)) g.DrawRectangle(p, r);
            TextoCentro(g, toast, fNormal, Amarillo, r);
        }

        void DibujarConfirmacion(Graphics g)
        {
            using (var b = new SolidBrush(Color.FromArgb(150, 0, 0, 0))) g.FillRectangle(b, ClientRectangle);
            var r = new Rectangle(ClientSize.Width / 2 - 200, ClientSize.Height / 2 - 100, 400, 200);
            Marco(g, r, confTitulo, Cian);
            TextoCentro(g, confPregunta, fNormal, Blanco, new RectangleF(r.X, r.Y + 45, r.Width, 40));
            TextoCentro(g, "[E] Sí", fNegrita, Verde, new RectangleF(r.X, r.Y + 100, r.Width, 30));
            TextoCentro(g, "[X] No", fNegrita, Rojo, new RectangleF(r.X, r.Y + 135, r.Width, 30));
        }

        void DibujarTitulo(Graphics g)
        {
            int desfase = (int)(Math.Sin(tick * 0.15) * 4);
            var area = new RectangleF(0, 150 + desfase, ClientSize.Width, 80);
            TextoCentro(g, "N E X", new Font("Consolas", 64, FontStyle.Bold), Cian, area);
            TextoCentro(g, "TRAINING SYSTEM", fGrande, Blanco, new RectangleF(0, 250, ClientSize.Width, 30));
            using (var p = new Pen(Cian, 2)) g.DrawRectangle(p, ClientSize.Width / 2 - 230, 120, 460, 190);
            if ((tick / 30) % 2 == 0)
                TextoCentro(g, "SISTEMA LISTO. Presione ENTER...", fNormal, Amarillo, new RectangleF(0, 380, ClientSize.Width, 30));
        }

        void DibujarRegistro(Graphics g)
        {
            var r = new Rectangle(ClientSize.Width / 2 - 260, 170, 520, 230);
            Marco(g, r, "REGISTRO DE CADETE", Magenta);
            TextoCentro(g, "¿Cómo debo llamarte?", fGrande, Cian, new RectangleF(r.X, r.Y + 50, r.Width, 40));
            var caja = new Rectangle(r.X + 60, r.Y + 110, r.Width - 120, 44);
            using (var b = new SolidBrush(Fondo)) g.FillRectangle(b, caja);
            using (var p = new Pen(Cian, 2)) g.DrawRectangle(p, caja);
            string cursor = (tick / 25) % 2 == 0 ? "_" : " ";
            Texto(g, nombreEntrada + cursor, fGrande, Blanco, caja.X + 10, caja.Y + 8);
            TextoCentro(g, "ENTER para continuar", fPeq, Gris, new RectangleF(r.X, r.Y + 175, r.Width, 30));
        }

        void DibujarSeleccion(Graphics g)
        {
            var r = new Rectangle(ClientSize.Width / 2 - 330, 60, 660, 520);
            Marco(g, r, "NEXUS PROTOCOLO DE SELECCIÓN DE CADETE", Magenta);
            Color[] colores = { Color.OrangeRed, Color.Silver, Color.MediumOrchid, Color.GreenYellow };
            for (int i = 0; i < 4; i++)
            {
                var fila = new Rectangle(r.X + 30, r.Y + 55 + i * 110, r.Width - 60, 95);
                bool sel = i == seleccionClase;
                using (var b = new SolidBrush(sel ? Color.FromArgb(50, 60, 100) : Fondo)) g.FillRectangle(b, fila);
                using (var p = new Pen(sel ? Amarillo : Gris, sel ? 3 : 1)) g.DrawRectangle(p, fila);
                DibujarCadete(g, fila.X + 20, fila.Y + 18, 0.8f, colores[i], 0);
                Texto(g, "[" + (i + 1) + "] " + Clases[i], fGrande, sel ? Amarillo : Blanco, fila.X + 110, fila.Y + 18);
                Texto(g, DescClases[i], fNormal, Gris, fila.X + 110, fila.Y + 52);
            }
            TextoCentro(g, "↑ ↓ o 1-4 · ENTER para confirmar", fPeq, Gris, new RectangleF(r.X, r.Bottom - 34, r.Width, 24));
        }

        Color ColorClase()
        {
            switch (tipoPersonaje)
            {
                case "FISICO": return Color.OrangeRed;
                case "ARMAMENTO": return Color.Silver;
                case "MANA": return Color.MediumOrchid;
                default: return Color.GreenYellow;
            }
        }

        void DibujarMenu(Graphics g)
        {
            var cab = new Rectangle(30, 25, ClientSize.Width - 60, 80);
            Marco(g, cab, "", Amarillo);
            TextoCentro(g, "NEXUS  SYSTEM  [V1.0]", fGrande, Amarillo, new RectangleF(cab.X, cab.Y + 6, cab.Width, 34));
            TextoCentro(g, "EXPLORADOR: " + cadete.Nombre + "   |   REALIDAD: GENESIS", fNormal, Blanco, new RectangleF(cab.X, cab.Y + 44, cab.Width, 28));

            var izq = new Rectangle(30, 125, 560, 170);
            Marco(g, izq, "ESTADO", Amarillo);
            Barra(g, izq.X + 20, izq.Y + 50, izq.Width - 40, 26, cadete.Energia, Cian, "Energía");
            Barra(g, izq.X + 20, izq.Y + 90, izq.Width - 40, 26, cadete.Estabilidad, Verde, "Estabilidad");
            Texto(g, "Clase: " + tipoPersonaje, fNormal, Blanco, izq.X + 20, izq.Y + 130);

            var der = new Rectangle(610, 125, 320, 170);
            Marco(g, der, "SISTEMA", Amarillo);
            string mision = MisionActual != null ? MisionActual.Nombre + (MisionActual.ObjetivoCumplido ? " (completar en Terminal)" : "") : "Todas completadas";
            Texto(g, "Nivel: " + cadete.Nivel + "   XP: " + cadete.Experiencia, fNormal, Blanco, der.X + 16, der.Y + 44);
            Texto(g, "Misión:", fNormal, Gris, der.X + 16, der.Y + 78);
            g.DrawString(mision, fPeq, Brushes.White, new RectangleF(der.X + 16, der.Y + 102, der.Width - 32, 60));

            var ops = new Rectangle(30, 315, ClientSize.Width - 60, 270);
            Marco(g, ops, "OPERACIONES DISPONIBLES", Cian);
            string[] textos = { "[1]  Capacidades / Equipamiento", "[2]  Manual de uso", "[3]  Nexo multiversal", "[0]  Desconexión" };
            for (int i = 0; i < 4; i++)
            {
                var fila = new Rectangle(ops.X + 40, ops.Y + 50 + i * 52, ops.Width - 80, 42);
                bool sel = i == seleccionMenu;
                if (sel)
                {
                    using (var b = new SolidBrush(Color.FromArgb(50, 60, 100))) g.FillRectangle(b, fila);
                    using (var p = new Pen(Amarillo, 2)) g.DrawRectangle(p, fila);
                }
                Texto(g, textos[i], fGrande, sel ? Amarillo : Blanco, fila.X + 20, fila.Y + 8);
            }
        }

        void DibujarCapacidades(Graphics g)
        {
            var r = new Rectangle(ClientSize.Width / 2 - 320, 80, 640, 470);
            Marco(g, r, "CAPACIDADES", Cian);
            Texto(g, "CADETE: " + cadete.Nombre, fNormal, Blanco, r.X + 30, r.Y + 50);
            Texto(g, "CLASE : " + tipoPersonaje, fNormal, Blanco, r.X + 30, r.Y + 78);
            DibujarCadete(g, r.Right - 130, r.Y + 50, 1f, ColorClase(), 0);
            Barra(g, r.X + 30, r.Y + 125, r.Width - 60, 28, cadete.Vida, Rojo, "Vida ");
            Barra(g, r.X + 30, r.Y + 165, r.Width - 60, 28, cadete.Poder, Amarillo, "Poder");
            Texto(g, "EQUIPAMIENTO", fNegrita, Cian, r.X + 30, r.Y + 220);
            Texto(g, "Arma: " + (tieneArma ? "EQUIPADA" : "[DESCONOCIDO]"), fNormal, Blanco, r.X + 30, r.Y + 250);
            string estadoCap = cadete.CapacidadActiva ? "ACTIVA · turnos restantes: " + cadete.TurnosCapacidad : "inactiva";
            Texto(g, "Capacidad especial: " + estadoCap, fNormal, cadete.CapacidadActiva ? Verde : Gris, r.X + 30, r.Y + 280);
            Texto(g, "[1] Activar habilidad de poder (Poder ≥ " + UmbralPoder + ")", fNormal, Amarillo, r.X + 30, r.Y + 345);
            Texto(g, "[0] Regresar", fNormal, Amarillo, r.X + 30, r.Y + 375);
        }

        void DibujarManual(Graphics g)
        {
            var r = new Rectangle(60, 50, ClientSize.Width - 120, 540);
            if (manualSub == 0)
            {
                r = new Rectangle(ClientSize.Width / 2 - 260, 150, 520, 280);
                Marco(g, r, "NEXUS // MANUAL", Cian);
                Texto(g, "[1] Manual de uso", fGrande, Blanco, r.X + 50, r.Y + 60);
                Texto(g, "[2] Archivos históricos", fGrande, Blanco, r.X + 50, r.Y + 110);
                Texto(g, "[0] Regresar", fGrande, Blanco, r.X + 50, r.Y + 160);
            }
            else if (manualSub == 1)
            {
                Marco(g, r, "NEXUS // MANUAL DE USO", Cian);
                string[] lineas =
                {
                    "[CONTROLES]",
                    "W A S D / Flechas: moverte por la Base y el mapa.",
                    "E: confirmar interacciones (cama, terminal, salida).   X: cancelar.",
                    "Esc: volver al menú.",
                    "",
                    "[COMBATE]",
                    "[A] atacar en tu turno, [H] huir (cuesta " + Combate.DanioPorHuir + " de vida).",
                    "[D] defenderte en el turno del enemigo.",
                    "",
                    "[VIDA Y ENERGÍA]",
                    "Si tu Vida llega a 0, la conexión termina de emergencia.",
                    "",
                    "[PODER]",
                    "Al llegar a " + UmbralPoder + " de Poder puedes activar tu capacidad especial",
                    "desde Capacidades / Equipamiento.",
                    "",
                    "[MISIONES]",
                    "Derrota a los enemigos en orden y reclama la recompensa en el Terminal.",
                    "",
                    "ENTER para regresar"
                };
                for (int i = 0; i < lineas.Length; i++)
                {
                    bool cabecera = lineas[i].StartsWith("[");
                    Texto(g, lineas[i], cabecera ? fNegrita : fNormal, cabecera ? Amarillo : Blanco, r.X + 30, r.Y + 45 + i * 24);
                }
            }
            else
            {
                Marco(g, r, "ARCHIVOS HISTÓRICOS · IRIS", Amarillo);
                int visibles = Math.Min(Archivos.Length, (tick - archivoTick) / 28 + 1);
                for (int i = 0; i < visibles; i++)
                {
                    Color c = Archivos[i].StartsWith("NEXUS") ? Cian : Blanco;
                    Texto(g, Archivos[i], fNormal, c, r.X + 30, r.Y + 45 + i * 24);
                }
                if (visibles >= Archivos.Length)
                    Texto(g, "ENTER para regresar", fNormal, Gris, r.X + 30, r.Bottom - 34);
            }
        }

        void DibujarNexo(Graphics g)
        {
            var r = new Rectangle(ClientSize.Width / 2 - 260, 90, 520, 440);
            Marco(g, r, "NEXO MULTIVERSAL", Cian);
            string[] reinos = { "Genesis", "Gama", "Delta", "Épsilon", "Tau", "Omega", "Volver" };
            for (int i = 0; i < reinos.Length; i++)
            {
                Color c = i == 0 || i == 6 ? Blanco : Gris;
                Texto(g, "[" + (i + 1) + "] " + reinos[i], fGrande, c, r.X + 60, r.Y + 55 + i * 50);
            }
        }

        void DibujarBase(Graphics g)
        {
            int tam = 70;
            int filas = mapaBase.GetLength(0), cols = mapaBase.GetLength(1);
            int ox = (ClientSize.Width - cols * tam) / 2;
            int oy = 90;
            Marco(g, new Rectangle(ox - 20, oy - 55, cols * tam + 40, filas * tam + 160), "BASE OPERATIVA", Cian);

            for (int f = 0; f < filas; f++)
                for (int c = 0; c < cols; c++)
                    DibujarCeldaBase(g, mapaBase[f, c], ox + c * tam, oy + f * tam, tam);

            DibujarCadete(g, ox + visX * tam + 14, oy + visY * tam + 6, 0.8f, ColorClase(), (int)(Math.Sin(tick * 0.2) * 2));

            Texto(g, "Cama · Terminal · Equipamiento · Salida · Bloqueada", fPeq, Gris, ox, oy + filas * tam + 20);
            Texto(g, "[W A S D] mover   [Esc] volver", fPeq, Gris, ox, oy + filas * tam + 44);
        }

        void DibujarCeldaBase(Graphics g, char s, int x, int y, int t)
        {
            var r = new Rectangle(x, y, t - 2, t - 2);
            if (s == '|')
            {
                using (var b = new SolidBrush(Color.FromArgb(55, 60, 85))) g.FillRectangle(b, r);
                return;
            }
            using (var b = new SolidBrush(Color.FromArgb(28, 32, 52))) g.FillRectangle(b, r);
            if (s == 'C')
            {
                using (var b = new SolidBrush(Color.SteelBlue)) g.FillRectangle(b, x + 8, y + 18, t - 18, t - 36);
                using (var b = new SolidBrush(Color.White)) g.FillRectangle(b, x + 10, y + 21, 18, t - 42);
            }
            else if (s == 'T')
            {
                using (var b = new SolidBrush(Color.FromArgb(20, 90, 100))) g.FillRectangle(b, x + 10, y + 10, t - 22, t - 34);
                using (var b = new SolidBrush(Cian)) g.FillRectangle(b, x + 14, y + 14, t - 30, t - 42);
                using (var b = new SolidBrush(Color.Gray)) g.FillRectangle(b, x + 24, y + t - 24, 20, 10);
            }
            else if (s == 'E')
            {
                using (var b = new SolidBrush(Color.Peru)) g.FillRectangle(b, x + 10, y + 14, t - 22, t - 28);
                using (var p = new Pen(Color.SaddleBrown, 3)) g.DrawRectangle(p, x + 10, y + 14, t - 22, t - 28);
            }
            else if (s == 'S' || s == 'B')
            {
                Color c = s == 'S' ? Verde : Rojo;
                using (var b = new SolidBrush(c)) g.FillRectangle(b, x + 12, y + 6, t - 26, t - 14);
                if (s == 'B')
                {
                    using (var b = new SolidBrush(Amarillo)) g.FillRectangle(b, x + t / 2 - 10, y + t / 2 - 4, 16, 14);
                    using (var p = new Pen(Amarillo, 3)) g.DrawArc(p, x + t / 2 - 8, y + t / 2 - 18, 12, 18, 180, 180);
                }
            }
        }

        void DibujarGenesis(Graphics g)
        {
            int tam = 52;
            int filas = mapaGenesis.GetLength(0), cols = mapaGenesis.GetLength(1);
            int ox = 30, oy = 60;
            Marco(g, new Rectangle(ox - 10, oy - 40, cols * tam + 20, filas * tam + 100), "MAPA GENESIS", Cian);

            for (int f = 0; f < filas; f++)
                for (int c = 0; c < cols; c++)
                    DibujarCeldaGenesis(g, f, c, ox + c * tam, oy + f * tam, tam);

            DibujarCadete(g, ox + visX * tam + 9, oy + visY * tam + 2, 0.55f, ColorClase(), 0);
            Texto(g, "[W A S D] mover   [Esc] salir del mapa", fPeq, Gris, ox, oy + filas * tam + 10);

            var info = new Rectangle(ox + cols * tam + 30, 20, ClientSize.Width - (ox + cols * tam + 60), 330);
            Marco(g, info, "INFORMACIÓN DEL MAPA", Amarillo);
            Texto(g, "Posición: [" + gFila + "," + gCol + "]", fNormal, Blanco, info.X + 14, info.Y + 44);
            Texto(g, "Distancia: " + distancia + " m", fNormal, Blanco, info.X + 14, info.Y + 74);
            Texto(g, "Botiquines: " + cadete.Recursos, fNormal, Blanco, info.X + 14, info.Y + 104);
            Texto(g, "◆ Botiquín", fPeq, Verde, info.X + 14, info.Y + 146);
            Texto(g, "⚠ Anomalía", fPeq, Amarillo, info.X + 14, info.Y + 170);
            Texto(g, "☠ Enemigo", fPeq, Rojo, info.X + 14, info.Y + 194);
            Texto(g, "⌂ Base", fPeq, Cian, info.X + 14, info.Y + 218);
            Texto(g, "♣ Terreno elevado", fPeq, Color.ForestGreen, info.X + 14, info.Y + 242);
            Texto(g, "? Desconocido", fPeq, Gris, info.X + 14, info.Y + 266);

            var est = new Rectangle(info.X, 370, info.Width, 220);
            Marco(g, est, "CADETE", Amarillo);
            Barra(g, est.X + 14, est.Y + 48, est.Width - 28, 24, cadete.Vida, Rojo, "Vida ");
            Barra(g, est.X + 14, est.Y + 84, est.Width - 28, 24, cadete.Poder, Amarillo, "Poder");
            Texto(g, "Clase: " + tipoPersonaje, fPeq, Blanco, est.X + 14, est.Y + 126);
            var m = MisionActual;
            Texto(g, "Misión:", fPeq, Gris, est.X + 14, est.Y + 152);
            g.DrawString(m != null ? m.Descripcion : "Todas completadas", fPeq, Brushes.White, new RectangleF(est.X + 14, est.Y + 172, est.Width - 28, 40));
        }

        void DibujarCeldaGenesis(Graphics g, int f, int c, int x, int y, int t)
        {
            var r = new Rectangle(x, y, t - 2, t - 2);
            char s = mapaGenesis[f, c];

            if (s == '|')
            {
                using (var b = new SolidBrush(Color.FromArgb(55, 60, 85))) g.FillRectangle(b, r);
                return;
            }
            if (!explorada[f, c])
            {
                using (var b = new SolidBrush(Color.FromArgb(18, 20, 32))) g.FillRectangle(b, r);
                TextoCentro(g, "?", fNegrita, Color.FromArgb(70, 76, 100), r);
                return;
            }

            using (var b = new SolidBrush(Color.FromArgb(30, 38, 56))) g.FillRectangle(b, r);

            var enemigo = enemigos.FirstOrDefault(en => en.Fila == f && en.Columna == c);
            if (enemigo != null)
            {
                using (var b = new SolidBrush(ColorEnemigo(enemigo))) g.FillEllipse(b, x + 8, y + 8, t - 18, t - 18);
                TextoCentro(g, "☠", fNegrita, Fondo, r);
            }
            else if (s == '⌂')
            {
                using (var b = new SolidBrush(Cian)) g.FillPolygon(b, new[] { new Point(x + 8, y + 24), new Point(x + t / 2 - 1, y + 6), new Point(x + t - 10, y + 24) });
                using (var b = new SolidBrush(Cian)) g.FillRectangle(b, x + 12, y + 24, t - 26, 18);
            }
            else if (s == '◆')
            {
                using (var b = new SolidBrush(Verde)) g.FillRectangle(b, x + 18, y + 8, 12, t - 20);
                using (var b = new SolidBrush(Verde)) g.FillRectangle(b, x + 8, y + 18, t - 20, 12);
            }
            else if (s == '⚠')
            {
                using (var b = new SolidBrush(Amarillo)) g.FillPolygon(b, new[] { new Point(x + 8, y + t - 12), new Point(x + t / 2 - 1, y + 8), new Point(x + t - 10, y + t - 12) });
                TextoCentro(g, "!", fNegrita, Fondo, new RectangleF(x, y + 8, t - 2, t - 2));
            }
            else if (s == 'A')
            {
                using (var b = new SolidBrush(Color.ForestGreen)) g.FillPolygon(b, new[] { new Point(x + 8, y + t - 14), new Point(x + t / 2 - 1, y + 6), new Point(x + t - 10, y + t - 14) });
                using (var b = new SolidBrush(Color.SaddleBrown)) g.FillRectangle(b, x + t / 2 - 5, y + t - 16, 8, 10);
            }
        }

        Color ColorEnemigo(Enemigo e)
        {
            switch (e.Efecto)
            {
                case "VENENO": return Color.LimeGreen;
                case "FUEGO": return Color.OrangeRed;
                default: return Color.DeepSkyBlue;
            }
        }

        void DibujarCadete(Graphics g, float x, float y, float escala, Color c, int bob)
        {
            float cx = x + 20 * escala, top = y + bob;
            using (var b = new SolidBrush(Color.FromArgb(235, 205, 170)))
                g.FillEllipse(b, cx - 12 * escala, top, 24 * escala, 24 * escala);
            using (var b = new SolidBrush(c))
                g.FillRectangle(b, cx - 14 * escala, top + 26 * escala, 28 * escala, 34 * escala);
            using (var b = new SolidBrush(Color.FromArgb(60, 70, 110)))
            {
                g.FillRectangle(b, cx - 12 * escala, top + 60 * escala, 10 * escala, 20 * escala);
                g.FillRectangle(b, cx + 2 * escala, top + 60 * escala, 10 * escala, 20 * escala);
            }
        }

        void DibujarCombate(Graphics g)
        {
            var en = combate.Enemigo;
            var escena = new Rectangle(40, 30, ClientSize.Width - 80, 330);
            Marco(g, escena, "MODO COMBATE", Rojo);

            using (var br = new LinearGradientBrush(new Rectangle(escena.X + 2, escena.Y + 30, escena.Width - 3, escena.Height - 31), Color.FromArgb(30, 36, 66), Color.FromArgb(14, 16, 28), 90f))
                g.FillRectangle(br, escena.X + 2, escena.Y + 30, escena.Width - 3, escena.Height - 31);
            using (var b = new SolidBrush(Color.FromArgb(40, 46, 70)))
                g.FillRectangle(b, escena.X + 2, escena.Bottom - 70, escena.Width - 3, 69);

            float lunge = 0;
            if (fase == FaseCombate.AnimAtaque) lunge = (float)Math.Sin(animTick / 26.0 * Math.PI) * 330;
            float sacudida = fase == FaseCombate.AnimDefensa ? (float)Math.Sin(animTick * 1.2) * 12 : 0;
            float recibe = fase == FaseCombate.AnimAtaque && animTick > 13 ? (float)Math.Sin(animTick * 1.5) * 10 : 0;

            DibujarCadete(g, escena.X + 120 + lunge - sacudida, escena.Y + 170, 1.4f, ColorClase(), (int)(Math.Sin(tick * 0.15) * 3));
            if (fase == FaseCombate.AnimAtaque && animTick > 6 && animTick < 20)
            {
                using (var p = new Pen(Color.White, 4)) g.DrawLine(p, escena.X + 200 + lunge, escena.Y + 200, escena.X + 260 + lunge, escena.Y + 160);
            }

            if (!en.Derrotado)
            {
                float ex = escena.Right - 270 + recibe;
                float ey = escena.Y + 150 + (float)Math.Sin(tick * 0.1) * 6;
                using (var b = new SolidBrush(ColorEnemigo(en))) g.FillEllipse(b, ex, ey, 160, 140);
                using (var b = new SolidBrush(Color.White)) { g.FillEllipse(b, ex + 38, ey + 40, 28, 28); g.FillEllipse(b, ex + 94, ey + 40, 28, 28); }
                using (var b = new SolidBrush(Color.Black)) { g.FillEllipse(b, ex + 48, ey + 50, 12, 12); g.FillEllipse(b, ex + 100, ey + 50, 12, 12); }
                using (var p = new Pen(Color.Black, 4)) g.DrawArc(p, ex + 50, ey + 85, 60, 30, 0, -180);
            }

            Texto(g, "ENEMIGO " + en.Efecto + " [" + en.Fila + "," + en.Columna + "]", fNegrita, ColorEnemigo(en), escena.Right - 300, escena.Y + 42);

            var hud = new Rectangle(40, 375, ClientSize.Width - 80, 160);
            Marco(g, hud, "", Cian);
            int pctEnemigo = en.Vida * 100 / Enemigo.VidaMaxima;
            Texto(g, "☠ ENEMIGO " + en.Efecto + "  " + en.Vida + "/" + Enemigo.VidaMaxima, fNormal, Blanco, hud.X + 16, hud.Y + 10);
            Barra(g, hud.X + 16, hud.Y + 36, hud.Width - 32, 20, pctEnemigo, ColorEnemigo(en), "Enemigo");
            Texto(g, "CADETE " + tipoPersonaje, fNormal, Blanco, hud.X + 16, hud.Y + 64);
            Barra(g, hud.X + 16, hud.Y + 88, hud.Width / 2 - 24, 20, cadete.Vida, Rojo, "Vida ");
            Barra(g, hud.X + hud.Width / 2 + 8, hud.Y + 88, hud.Width / 2 - 24, 20, cadete.Poder, Amarillo, "Poder");

            string instruccion;
            Color ci = Amarillo;
            switch (fase)
            {
                case FaseCombate.Jugador: instruccion = "TU TURNO: [A] atacar   [H] huir"; break;
                case FaseCombate.Enemigo: instruccion = "TURNO DEL ENEMIGO: presiona [D] para defenderte"; break;
                case FaseCombate.Victoria: instruccion = "ENEMIGO DERROTADO · ENTER para continuar"; ci = Verde; break;
                case FaseCombate.Derrota: instruccion = "EL CADETE HA CAÍDO · ENTER para continuar"; ci = Rojo; break;
                default: instruccion = "..."; break;
            }
            Texto(g, instruccion, fNegrita, ci, hud.X + 16, hud.Y + 120);

            var msg = new Rectangle(40, 548, ClientSize.Width - 80, 70);
            using (var b = new SolidBrush(Panel)) g.FillRectangle(b, msg);
            using (var p = new Pen(Gris)) g.DrawRectangle(p, msg);
            g.DrawString(mensajeCombate, fNormal, Brushes.White, new RectangleF(msg.X + 10, msg.Y + 8, msg.Width - 20, msg.Height - 12));

            if (fase == FaseCombate.Victoria) DibujarVictoria(g);
        }

        void DibujarVictoria(Graphics g)
        {
            using (var b = new SolidBrush(Color.FromArgb(190, 0, 0, 0))) g.FillRectangle(b, ClientRectangle);
            var r = new Rectangle(ClientSize.Width / 2 - 300, 70, 600, 500);
            Marco(g, r, "¡VICTORIA!", Verde);

            float rebote = (float)Math.Abs(Math.Sin(tick * 0.08)) * 14;
            int cx = r.X + r.Width / 2;
            using (var b = new SolidBrush(Color.SaddleBrown)) g.FillRectangle(b, cx - 60, r.Y + 90 - rebote, 120, 70);
            using (var b = new SolidBrush(Color.Goldenrod)) g.FillRectangle(b, cx - 66, r.Y + 70 - rebote, 132, 30);
            using (var b = new SolidBrush(Amarillo)) g.FillRectangle(b, cx - 10, r.Y + 96 - rebote, 20, 24);

            Texto(g, "ENEMIGO DERROTADO · La zona ha quedado despejada.", fNormal, Blanco, r.X + 30, r.Y + 190);
            if (objetivoMision)
            {
                Texto(g, "Objetivo de misión cumplido.", fNegrita, Verde, r.X + 30, r.Y + 225);
                Texto(g, "Regresa a la Base y usa el Terminal para reclamar la recompensa.", fPeq, Blanco, r.X + 30, r.Y + 252);
            }
            else
            {
                Texto(g, "Fragmento de Poder obtenido: +" + Combate.PoderPorEnemigo + "  (Poder " + cadete.Poder + "/100)", fNegrita, Amarillo, r.X + 30, r.Y + 225);
            }

            var iris = new Rectangle(r.X + 30, r.Y + 290, r.Width - 60, 150);
            using (var b = new SolidBrush(Color.FromArgb(60, 10, 14))) g.FillRectangle(b, iris);
            using (var p = new Pen(Rojo, 2)) g.DrawRectangle(p, iris);
            TextoCentro(g, "⚠ ALERTA CRÍTICA ⚠", fGrande, Rojo, new RectangleF(iris.X, iris.Y + 6, iris.Width, 32));
            TextoCentro(g, "INTERFERENCIA: IRIS", fNegrita, Rojo, new RectangleF(iris.X, iris.Y + 40, iris.Width, 26));
            TextoCentro(g, "ESTABILIDAD: " + cadete.Estabilidad + "%", fNormal, Blanco, new RectangleF(iris.X, iris.Y + 72, iris.Width, 26));
            TextoCentro(g, "IRIS HA INTERRUMPIDO LOS SISTEMAS DE NEXUS", fPeq, Blanco, new RectangleF(iris.X, iris.Y + 100, iris.Width, 20));
            TextoCentro(g, "NEXUS recomienda desconexión inmediata.", fPeq, Blanco, new RectangleF(iris.X, iris.Y + 120, iris.Width, 20));
            TextoCentro(g, "ENTER para continuar", fNormal, Gris, new RectangleF(r.X, r.Bottom - 40, r.Width, 28));
        }

        void DibujarTerminal(Graphics g)
        {
            var r = new Rectangle(ClientSize.Width / 2 - 340, 110, 680, 380);
            Marco(g, r, "NEXUS // TERMINAL DE MISIONES", Cian);
            for (int i = 0; i < misiones.Count; i++)
            {
                string estado = EstadoMision(i);
                Color c = estado == "COMPLETADA" ? Verde : estado == "BLOQUEADA" ? Gris : estado == "ACTUAL" ? Amarillo : Cian;
                var fila = new Rectangle(r.X + 24, r.Y + 55 + i * 80, r.Width - 48, 68);
                using (var b = new SolidBrush(Fondo)) g.FillRectangle(b, fila);
                using (var p = new Pen(c, 2)) g.DrawRectangle(p, fila);
                Texto(g, "[" + (i + 1) + "] " + misiones[i].Nombre, fNegrita, Blanco, fila.X + 14, fila.Y + 10);
                Texto(g, estado, fNormal, c, fila.X + 14, fila.Y + 38);
            }
            Texto(g, "[0] Salir del terminal", fNormal, Amarillo, r.X + 24, r.Bottom - 34);
        }

        void DibujarDetalleMision(Graphics g)
        {
            var m = misiones[misionDetalle];
            bool esActual = misionDetalle == misionIndice;
            var r = new Rectangle(ClientSize.Width / 2 - 340, 110, 680, 380);
            Marco(g, r, m.Nombre, Cian);
            Texto(g, m.Descripcion, fNormal, Blanco, r.X + 30, r.Y + 55);
            g.DrawString("⚠ " + m.Consejo, fNormal, new SolidBrush(Amarillo), new RectangleF(r.X + 30, r.Y + 100, r.Width - 60, 70));

            if (m.Completada)
                Texto(g, "Misión completada.", fNegrita, Verde, r.X + 30, r.Y + 210);
            else if (!esActual)
                Texto(g, "Bloqueada: completa primero la misión anterior.", fNormal, Gris, r.X + 30, r.Y + 210);
            else if (!m.ObjetivoCumplido)
                Texto(g, "Derrota al enemigo " + m.TipoEnemigo + " y regresa a la Base.", fNormal, Cian, r.X + 30, r.Y + 210);
            else
            {
                Texto(g, "+" + m.RecompensaPoder + " PODER", fNegrita, Cian, r.X + 30, r.Y + 210);
                Texto(g, "+" + m.RecompensaXp + " XP", fNegrita, Amarillo, r.X + 30, r.Y + 240);
                Texto(g, "[S] Reclamar recompensa", fGrande, Verde, r.X + 30, r.Y + 290);
            }
            Texto(g, "[Esc] Regresar", fPeq, Gris, r.X + 30, r.Bottom - 30);
        }

        void DibujarFin(Graphics g)
        {
            TextoCentro(g, "SESIÓN FINALIZADA", fTitulo, Cian, new RectangleF(0, 190, ClientSize.Width, 60));
            TextoCentro(g, mensajeFin, fNormal, Blanco, new RectangleF(0, 270, ClientSize.Width, 40));
            TextoCentro(g, "Cadete " + (cadete != null ? cadete.Nombre : "") + " · Nivel " + (cadete != null ? cadete.Nivel : 1), fNormal, Amarillo, new RectangleF(0, 320, ClientSize.Width, 30));
            TextoCentro(g, "ENTER para cerrar", fPeq, Gris, new RectangleF(0, 400, ClientSize.Width, 30));
        }
    }
}