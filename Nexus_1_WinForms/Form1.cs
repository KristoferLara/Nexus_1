using System;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace WinForms
{
    public partial class Form1 : Form
    {
        readonly Juego juego = new Juego();
        readonly EstiloVisual estilo;
        readonly VistaMenus vistaMenus;
        readonly VistaBase vistaBase;
        readonly VistaGenesis vistaGenesis;
        readonly VistaCombate vistaCombate;
        VistaJuego vistaActiva;
        Pantalla pantalla = Pantalla.Titulo;
        Pantalla pantallaAnterior = Pantalla.Menu;

        public Form1()
        {
            InitializeComponent();

            estilo = new EstiloVisual(Tema.Oscuro);
            components.Add(estilo);
            estilo.TemaCambiado += AlCambiarTema;

            vistaMenus = new VistaMenus(juego, estilo);
            vistaBase = new VistaBase(juego, estilo);
            vistaGenesis = new VistaGenesis(juego, estilo);
            vistaCombate = new VistaCombate(juego, estilo);
            Controls.AddRange(new Control[] { vistaMenus, vistaBase, vistaGenesis, vistaCombate });

            BackColor = estilo.Tema[Rol.Fondo];
            CambiarPantalla(Pantalla.Titulo);
            timerJuego.Start();
        }

        void AlCambiarTema(object sender, EventArgs e)
        {
            BackColor = estilo.Tema[Rol.Fondo];
            juego.Avisar("TEMA: " + estilo.Tema.Nombre.ToUpper());
            Refrescar();
        }

        void Refrescar()
        {
            if (!IsDisposed) vistaActiva.Invalidate();
        }

        void CambiarPantalla(Pantalla destino)
        {
            pantalla = destino;
            VistaJuego nueva = VistaPara(destino);

            if (nueva == vistaMenus) vistaMenus.Modo = destino;

            if (nueva != vistaActiva)
            {
                if (vistaActiva != null) vistaActiva.Visible = false;
                vistaActiva = nueva;
                vistaActiva.Visible = true;
            }
            vistaActiva.AlMostrar();
        }

        VistaJuego VistaPara(Pantalla destino)
        {
            switch (destino)
            {
                case Pantalla.Base: return vistaBase;
                case Pantalla.Genesis: return vistaGenesis;
                case Pantalla.Combate: return vistaCombate;
                default: return vistaMenus;
            }
        }

        void timerJuego_Tick(object sender, EventArgs e)
        {
            juego.Avanzar();
            if (pantalla == Pantalla.Combate) juego.AvanzarCombate();
            vistaActiva.Avanzar();
        }

        void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (pantalla != Pantalla.Registro) return;

            juego.EscribirCaracter(e.KeyChar);
            e.Handled = true;
            Refrescar();
        }

        void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Keys k = e.KeyCode;
            int n = NumeroTecla(k);

            if (k == Keys.C)
            {
                estilo.AplicarTema(Tema.Siguiente(estilo.Tema));
                return;
            }

            if (juego.Dialogo != null)
            {
                if (k == Keys.E || k == Keys.Enter) juego.ResolverDialogo(true);
                else if (k == Keys.X || k == Keys.Escape) juego.ResolverDialogo(false);
                Refrescar();
                return;
            }

            switch (pantalla)
            {
                case Pantalla.Titulo:
                    if (k == Keys.Enter) CambiarPantalla(Pantalla.Registro);
                    break;

                case Pantalla.Registro:
                    if (k == Keys.Enter && juego.ConfirmarNombre()) CambiarPantalla(Pantalla.Seleccion);
                    break;

                case Pantalla.Seleccion:
                    TeclaSeleccion(k, n);
                    break;

                case Pantalla.Menu:
                    TeclaMenu(k, n);
                    break;

                case Pantalla.Capacidades:
                    if (n == 1) juego.ActivarHabilidad();
                    else if (n == 0 || k == Keys.Escape) CambiarPantalla(pantallaAnterior);
                    break;

                case Pantalla.Manual:
                    TeclaManual(k, n);
                    break;

                case Pantalla.Nexo:
                    TeclaNexo(k, n);
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
                    TeclaTerminal(k, n);
                    break;

                case Pantalla.DetalleMision:
                    TeclaDetalleMision(k);
                    break;

                case Pantalla.Fin:
                    if (k == Keys.Enter || k == Keys.Escape) Close();
                    break;
            }

            Refrescar();
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

        void IniciarPartida()
        {
            juego.IniciarPartida();
            CambiarPantalla(Pantalla.Menu);
        }

        void TeclaSeleccion(Keys k, int n)
        {
            if (k == Keys.Up) juego.MoverSeleccionClase(-1);
            else if (k == Keys.Down) juego.MoverSeleccionClase(1);
            else if (n >= 1 && n <= Juego.Clases.Length)
            {
                juego.ElegirClase(n - 1);
                IniciarPartida();
            }
            else if (k == Keys.Enter) IniciarPartida();
            else if (k == Keys.Escape) CambiarPantalla(Pantalla.Registro);
        }

        void TeclaMenu(Keys k, int n)
        {
            if (k == Keys.Up) juego.MoverSeleccionMenu(-1);
            else if (k == Keys.Down) juego.MoverSeleccionMenu(1);
            else if (k == Keys.Enter) n = Juego.TeclasMenu[juego.SeleccionMenu];

            if (n == 1)
            {
                pantallaAnterior = Pantalla.Menu;
                CambiarPantalla(Pantalla.Capacidades);
            }
            else if (n == 2)
            {
                juego.AbrirManual();
                CambiarPantalla(Pantalla.Manual);
            }
            else if (n == 3) CambiarPantalla(Pantalla.Nexo);
            else if (n == 0 && juego.IntentarDesconectar()) CambiarPantalla(Pantalla.Fin);
        }

        void TeclaManual(Keys k, int n)
        {
            if (juego.ManualSub == 0)
            {
                if (n == 1 || n == 2) juego.ElegirManual(n);
                else if (n == 0 || k == Keys.Escape) CambiarPantalla(Pantalla.Menu);
            }
            else if (k == Keys.Enter || k == Keys.Escape)
            {
                juego.ElegirManual(0);
            }
        }

        void TeclaNexo(Keys k, int n)
        {
            if (n == 1) CambiarPantalla(Pantalla.Base);
            else if (n >= 2 && n <= 6) juego.Avisar("NEXUS: Realidad no disponible.");
            else if (n == 7 || n == 0 || k == Keys.Escape) CambiarPantalla(Pantalla.Menu);
        }

        void TeclaBase(Keys k)
        {
            if (k == Keys.Escape)
            {
                CambiarPantalla(Pantalla.Nexo);
                return;
            }

            int df, dc;
            if (!Direccion(k, out df, out dc)) return;

            switch (juego.MoverBase(df, dc))
            {
                case Interaccion.Cama:
                    juego.Confirmar("CAMA NEXUS", "¿Deseas descansar?", juego.Descansar);
                    break;
                case Interaccion.Terminal:
                    juego.Confirmar("TERMINAL NEXUS", "¿Deseas acceder al terminal?", () => CambiarPantalla(Pantalla.Terminal));
                    break;
                case Interaccion.Equipamiento:
                    pantallaAnterior = Pantalla.Base;
                    CambiarPantalla(Pantalla.Capacidades);
                    break;
                case Interaccion.Salida:
                    juego.Confirmar("SALIDA", "¿Salir e iniciar la misión?", () => CambiarPantalla(Pantalla.Genesis));
                    break;
                case Interaccion.Puerta:
                    juego.Confirmar("PUERTA BLOQUEADA", "¿Deseas interactuar?", () => juego.Avisar("Acceso restringido. Esta sección todavía no está disponible."));
                    break;
            }
        }

        void TeclaGenesis(Keys k)
        {
            if (k == Keys.Escape)
            {
                CambiarPantalla(Pantalla.Menu);
                return;
            }

            int df, dc;
            if (!Direccion(k, out df, out dc)) return;

            switch (juego.MoverGenesis(df, dc))
            {
                case Encuentro.Base:
                    juego.Confirmar("BASE OPERATIVA", "¿Deseas entrar a la base?", () => CambiarPantalla(Pantalla.Base));
                    break;
                case Encuentro.Combate:
                    CambiarPantalla(Pantalla.Combate);
                    break;
            }
        }

        void TeclaCombate(Keys k)
        {
            switch (juego.Fase)
            {
                case FaseCombate.Jugador:
                case FaseCombate.Enemigo:
                    if (k == Keys.A) juego.PedirAtaque();
                    else if (k == Keys.D) juego.PedirDefensa();
                    else if (k == Keys.H && juego.PedirHuida()) CambiarPantalla(Pantalla.Genesis);
                    break;

                case FaseCombate.Victoria:
                    if (k == Keys.Enter) CambiarPantalla(Pantalla.Genesis);
                    break;

                case FaseCombate.Derrota:
                    if (k == Keys.Enter)
                    {
                        juego.TerminarPorDerrota();
                        CambiarPantalla(Pantalla.Fin);
                    }
                    break;
            }
        }

        void TeclaTerminal(Keys k, int n)
        {
            if (n >= 1 && n <= juego.Misiones.Count)
            {
                juego.SeleccionarMision(n - 1);
                CambiarPantalla(Pantalla.DetalleMision);
            }
            else if (n == 0 || k == Keys.Escape)
            {
                CambiarPantalla(Pantalla.Base);
            }
        }

        void TeclaDetalleMision(Keys k)
        {
            if (k == Keys.Escape || k == Keys.Back) CambiarPantalla(Pantalla.Terminal);
            else if (k == Keys.S && juego.ReclamarRecompensa()) CambiarPantalla(Pantalla.Terminal);
        }
    }
}