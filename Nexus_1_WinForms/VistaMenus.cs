using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinForms
{
    public sealed class VistaMenus : VistaJuego
    {
        Pantalla modo = Pantalla.Titulo;
        int glitchPaso;
        bool glitchEnCurso;
        bool glitchPendiente;
        int secuenciaGlitchId;

        static readonly string[] LogoNexPixel = new[]
        {
            "NN...N.EEEEE.X...X",
            "NNN..N.E......X.X.",
            "N.N..N.EEEE....X..",
            "N..NNN.E......X.X.",
            "N...NN.EEEEE.X...X"
        };

        static readonly string[] SpriteEmblemaNexus = new[]
        {
            "....AAAA....",
            "..AALLLLAA..",
            ".ALCCCCCCLA.",
            "ALCCPPPPCCLA",
            "ACCPWWWWPCCA",
            "ACCPWWWWPCCA",
            "ACCPWWWWPCCA",
            "ACCPWWWWPCCA",
            "ALCCPPPPCCLA",
            ".ALCCCCCCLA.",
            "..AALLLLAA..",
            "....AAAA...."
        };

        sealed class Nota
        {
            public readonly int Frecuencia;
            public readonly int Duracion;
            public readonly int Pausa;

            public Nota(int frecuencia, int duracion, int pausa)
            {
                Frecuencia = frecuencia;
                Duracion = duracion;
                Pausa = pausa;
            }
        }

        static readonly Nota[][] SecuenciaGlitch =
        {
            new[] { new Nota(587, 100, 30), new Nota(587, 100, 30) },
            new[] { new Nota(1174, 150, 50) },
            new[] { new Nota(880, 200, 50) },
            new[] { new Nota(830, 150, 30), new Nota(784, 150, 30), new Nota(698, 300, 0) }
        };

        public VistaMenus(Juego juego, EstiloVisual estilo) : base(juego, estilo)
        {
        }

        public new Pantalla Modo
        {
            get { return modo; }
            set
            {
                modo = value;
                Invalidate();
            }
        }

        public override void AlMostrar()
        {
            if (modo == Pantalla.Titulo) IniciarGlitch();
            base.AlMostrar();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!glitchPendiente) return;

            glitchPendiente = false;
            int id = secuenciaGlitchId;
            BeginInvoke(new Action(() => EjecutarGlitchAsync(id)));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) secuenciaGlitchId++;
            base.Dispose(disposing);
        }

        void IniciarGlitch()
        {
            secuenciaGlitchId++;
            glitchPaso = 1;
            glitchEnCurso = true;

            if (!IsHandleCreated)
            {
                glitchPendiente = true;
                return;
            }
            EjecutarGlitchAsync(secuenciaGlitchId);
        }

        bool GlitchVigente(int id)
        {
            return id == secuenciaGlitchId && modo == Pantalla.Titulo && !IsDisposed;
        }

        async void EjecutarGlitchAsync(int id)
        {
            for (int i = 0; i < SecuenciaGlitch.Length; i++)
            {
                glitchPaso = i + 1;
                glitchEnCurso = glitchPaso < SecuenciaGlitch.Length;
                Invalidate();
                Update();

                foreach (Nota nota in SecuenciaGlitch[i])
                {
                    await ReproducirBeep(nota.Frecuencia, nota.Duracion);
                    await Task.Delay(nota.Pausa);
                    if (!GlitchVigente(id)) return;
                }
            }
        }

        static Task ReproducirBeep(int frecuencia, int duracion)
        {
            return Task.Run(() =>
            {
                try { Console.Beep(frecuencia, duracion); }
                catch (Exception) { }
            });
        }

        protected override bool Animada
        {
            get
            {
                if (modo == Pantalla.Titulo || modo == Pantalla.Registro || modo == Pantalla.Seleccion || modo == Pantalla.Menu) return true;
                return modo == Pantalla.Manual && juego.ManualSub == 2 && LineasVisibles() < Juego.Archivos.Length;
            }
        }

        int LineasVisibles()
        {
            int transcurridas = (juego.Tick - juego.ArchivoTick) / Juego.TicksPorLineaArchivo;
            return Math.Min(Juego.Archivos.Length, transcurridas + 1);
        }

        protected override void Dibujar(Graphics g)
        {
            DibujarFondoRetro(g);

            switch (modo)
            {
                case Pantalla.Titulo: DibujarTitulo(g); break;
                case Pantalla.Registro: DibujarRegistro(g); break;
                case Pantalla.Seleccion: DibujarSeleccion(g); break;
                case Pantalla.Menu: DibujarMenu(g); break;
                case Pantalla.Capacidades: DibujarCapacidades(g); break;
                case Pantalla.Manual: DibujarManual(g); break;
                case Pantalla.Nexo: DibujarNexo(g); break;
                case Pantalla.Terminal: DibujarTerminal(g); break;
                case Pantalla.DetalleMision: DibujarDetalleMision(g); break;
                case Pantalla.Fin: DibujarFin(g); break;
            }
        }

        void DibujarFondoRetro(Graphics g)
        {
            Color punto = ControlPaint.Light(C(Rol.Fondo), 0.12f);
            using (var bPunto = new SolidBrush(punto))
            {
                for (int x = 24; x < ClientSize.Width; x += 48)
                {
                    for (int y = 24; y < ClientSize.Height; y += 48)
                    {
                        g.FillRectangle(bPunto, x, y, 3, 3);
                    }
                }
            }
        }

        void DibujarEmblema(Graphics g, int x, int y, int px)
        {
            Color acento = C(Rol.Acento);
            Color luz = ControlPaint.Light(acento, 0.35f);
            Color aviso = C(Rol.Aviso);

            for (int fila = 0; fila < SpriteEmblemaNexus.Length; fila++)
            {
                string linea = SpriteEmblemaNexus[fila];
                for (int col = 0; col < linea.Length; col++)
                {
                    char c = linea[col];
                    if (c == '.') continue;

                    Color colorPixel;
                    switch (c)
                    {
                        case 'L': colorPixel = luz; break;
                        case 'P': colorPixel = aviso; break;
                        case 'W': colorPixel = Color.White; break;
                        case 'C': colorPixel = C(Rol.Superficie); break;
                        default: colorPixel = acento; break;
                    }

                    using (var brocha = new SolidBrush(colorPixel))
                        g.FillRectangle(brocha, x + col * px, y + fila * px, px, px);
                }
            }
        }

        void DibujarBloquesNex(Graphics g, int x, int y, int px, Color colorPrincipal)
        {
            Color sombra = Color.FromArgb(15, 15, 22);
            Color brillo = ControlPaint.Light(colorPrincipal, 0.35f);

            for (int fila = 0; fila < LogoNexPixel.Length; fila++)
            {
                string linea = LogoNexPixel[fila];
                for (int col = 0; col < linea.Length; col++)
                {
                    char c = linea[col];
                    if (c == '.') continue;

                    int bx = x + col * px;
                    int by = y + fila * px;

                    using (var bSombra = new SolidBrush(sombra))
                        g.FillRectangle(bSombra, bx + 4, by + 4, px, px);

                    using (var bMain = new SolidBrush(colorPrincipal))
                        g.FillRectangle(bMain, bx, by, px, px);

                    using (var bLuz = new SolidBrush(brillo))
                        g.FillRectangle(bLuz, bx, by, px, Math.Max(2, px / 4));
                }
            }
        }

        void DibujarTitulo(Graphics g)
        {
            int boxW = 620;
            int boxH = 210;
            int centroX = (ClientSize.Width - boxW) / 2;
            int centroY = 145;

            int posX = centroX;
            int posY = centroY;
            Color colorMarco = C(Rol.Acento);
            Color colorLogo = C(Rol.Acento);

            switch (glitchPaso)
            {
                case 1:
                    posX = 25;
                    posY = 25;
                    colorMarco = Color.Cyan;
                    colorLogo = Color.Cyan;
                    break;
                case 2:
                    posX = Math.Min(ClientSize.Width - boxW - 25, 260);
                    posY = 160;
                    colorMarco = Color.DodgerBlue;
                    colorLogo = Color.DeepSkyBlue;
                    break;
                case 3:
                    posX = 55;
                    posY = 135;
                    colorMarco = Color.DarkCyan;
                    colorLogo = Color.MediumTurquoise;
                    break;
                default:
                    posY = centroY + (int)(Math.Sin(juego.Tick * 0.2) * 3);
                    break;
            }

            if (glitchEnCurso)
            {
                using (var bGlitch = new SolidBrush(Color.FromArgb(140, colorLogo)))
                {
                    g.FillRectangle(bGlitch, posX - 35, posY + 35, boxW + 70, 4);
                    g.FillRectangle(bGlitch, posX - 20, posY + boxH - 40, boxW + 40, 4);
                }
            }

            var cajaTitulo = new Rectangle(posX, posY, boxW, boxH);
            estilo.DibujarCajaPixel(g, cajaTitulo, C(Rol.Superficie), colorMarco, C(Rol.Aviso), 4);

            DibujarEmblema(g, posX + 24, posY + 22, 3);
            DibujarBloquesNex(g, posX + 75, posY + 62, 16, colorLogo);
            Texto(g, "TRAINING", estilo.Titulo, Rol.Texto, posX + 405, posY + 68);
            Texto(g, "SYSTEM", estilo.Titulo, colorLogo, posX + 405, posY + 106);

            if (!glitchEnCurso && (juego.Tick / 15) % 2 == 0)
            {
                TextoCentro(g, "▶ SISTEMA LISTO. Presione ENTER... ◀", estilo.Negrita, Rol.Aviso, new RectangleF(0, centroY + boxH + 55, ClientSize.Width, 32));
            }
        }

        void DibujarRegistro(Graphics g)
        {
            var r = new Rectangle(ClientSize.Width / 2 - 260, 170, 520, 230);
            Marco(g, r, "REGISTRO DE CADETE", Rol.Especial);
            TextoCentro(g, "¿Cómo debo llamarte?", estilo.Grande, Rol.Acento, new RectangleF(r.X, r.Y + 50, r.Width, 40));

            var caja = new Rectangle(r.X + 60, r.Y + 108, r.Width - 120, 48);
            estilo.DibujarCajaPixel(g, caja, C(Rol.Fondo), C(Rol.Acento), C(Rol.TextoSuave), 2);

            string cursor = (juego.Tick / 12) % 2 == 0 ? "█" : " ";
            Texto(g, juego.NombreEntrada + cursor, estilo.Grande, Rol.Texto, caja.X + 14, caja.Y + 10);
            TextoCentro(g, "ENTER para continuar", estilo.Pequena, Rol.TextoSuave, new RectangleF(r.X, r.Y + 180, r.Width, 30));
        }

        void DibujarSeleccion(Graphics g)
        {
            var r = new Rectangle(ClientSize.Width / 2 - 340, 50, 680, 540);
            Marco(g, r, "NEXUS // PROTOCOLO DE SELECCIÓN DE CADETE", Rol.Especial);
            int pulso = (juego.Tick / 10) % 2 == 0 ? 4 : 0;
            for (int i = 0; i < Juego.Clases.Length; i++)
            {
                var fila = new Rectangle(r.X + 28, r.Y + 52 + i * 112, r.Width - 56, 98);
                bool seleccionada = i == juego.SeleccionClase;
                estilo.DibujarCajaPixel(
                g,
                fila,
                C(seleccionada ? Rol.SuperficieSeleccion : Rol.Fondo),
                C(seleccionada ? Rol.Aviso : Rol.TextoSuave),
                C(seleccionada ? Rol.Acento : Rol.Superficie),
                seleccionada ? 3 : 2
                );
                DibujarCadete(g, fila.X + 22, fila.Y + 18, 0.8f, ColorClase(Juego.Clases[i]), seleccionada ? pulso / 2 : 0, false, Juego.Clases[i]);
                string prefijo = seleccionada ? "▶ [" + (i + 1) + "] " : "  [" + (i + 1) + "] ";
                Texto(g, prefijo + Juego.Clases[i], estilo.Grande, seleccionada ? Rol.Aviso : Rol.Texto, fila.X + 105, fila.Y + 18);
                Texto(g, Juego.DescClases[i], estilo.Normal, Rol.TextoSuave, fila.X + 115, fila.Y + 54);
            }
            TextoCentro(g, "↑ ↓ o 1-4 · ENTER para confirmar", estilo.Pequena, Rol.TextoSuave, new RectangleF(r.X, r.Bottom - 32, r.Width, 24));
        }
        void DibujarMenu(Graphics g)
        {
            Cadete cadete = juego.Cadete;
            var cabecera = new Rectangle(30, 25, ClientSize.Width - 60, 82);
            Marco(g, cabecera, "", Rol.Aviso);
            TextoCentro(g, "NEXUS  SYSTEM  [V1.0]", estilo.Grande, Rol.Aviso, new RectangleF(cabecera.X, cabecera.Y + 8, cabecera.Width, 34));
            TextoCentro(g, "EXPLORADOR: " + cadete.Nombre + "   |   REALIDAD: GENESIS", estilo.Normal, Rol.Texto, new RectangleF(cabecera.X, cabecera.Y + 46, cabecera.Width, 28));
            var estado = new Rectangle(30, 125, 560, 170);
            Marco(g, estado, "ESTADO", Rol.Aviso);
            Barra(g, estado.X + 20, estado.Y + 50, estado.Width - 40, 26, cadete.Energia, Rol.Acento, "Energía");
            Barra(g, estado.X + 20, estado.Y + 90, estado.Width - 40, 26, cadete.Estabilidad, Rol.Exito, "Estabilidad");
            Texto(g, "Clase: " + juego.TipoPersonaje, estilo.Normal, Rol.Texto, estado.X + 20, estado.Y + 130);
            var sistema = new Rectangle(610, 125, 320, 170);
            Marco(g, sistema, "SISTEMA", Rol.Aviso);
            Mision actual = juego.MisionActual;
            string mision = actual != null
            ? actual.Nombre + (actual.ObjetivoCumplido ? " (completar en Terminal)" : "")
            : "Todas completadas";
            Texto(g, "Nivel: " + cadete.Nivel + "   XP: " + cadete.Experiencia, estilo.Normal, Rol.Texto, sistema.X + 16, sistema.Y + 44);
            Texto(g, "Misión:", estilo.Normal, Rol.TextoSuave, sistema.X + 16, sistema.Y + 78);
            TextoCaja(g, mision, estilo.Pequena, Rol.Texto, new RectangleF(sistema.X + 16, sistema.Y + 102, sistema.Width - 32, 60));
            var operaciones = new Rectangle(30, 315, ClientSize.Width - 60, 275);
            Marco(g, operaciones, "OPERACIONES DISPONIBLES", Rol.Acento);
            for (int i = 0; i < Juego.TextosMenu.Length; i++)
            {
                var fila = new Rectangle(operaciones.X + 36, operaciones.Y + 48 + i * 54, operaciones.Width - 72, 46);
                bool seleccionada = i == juego.SeleccionMenu;
                if (seleccionada)
                {
                    estilo.DibujarCajaPixel(g, fila, C(Rol.SuperficieSeleccion), C(Rol.Aviso), C(Rol.Acento), 2);
                }
                string cursor = seleccionada ? "▶ " : "  ";
                Texto(g, cursor + Juego.TextosMenu[i], estilo.Grande, seleccionada ? Rol.Aviso : Rol.Texto, fila.X + 18, fila.Y + 10);
            }
        }
        void DibujarCapacidades(Graphics g)
        {
            Cadete cadete = juego.Cadete;
            var r = new Rectangle(ClientSize.Width / 2 - 320, 80, 640, 470);
            Marco(g, r, "CAPACIDADES", Rol.Acento);
            Texto(g, "CADETE: " + cadete.Nombre, estilo.Normal, Rol.Texto, r.X + 30, r.Y + 50);
            Texto(g, "CLASE : " + juego.TipoPersonaje, estilo.Normal, Rol.Texto, r.X + 30, r.Y + 78);
            DibujarCadete(g, r.Right - 130, r.Y + 50, 1f, ColorJugador, 0);
            Barra(g, r.X + 30, r.Y + 125, r.Width - 60, 28, cadete.Vida, Rol.Peligro, "Vida ");
            Barra(g, r.X + 30, r.Y + 165, r.Width - 60, 28, cadete.Poder, Rol.Aviso, "Poder");
            Texto(g, "EQUIPAMIENTO", estilo.Negrita, Rol.Acento, r.X + 30, r.Y + 220);
            Texto(g, "Arma: " + (juego.TieneArma ? "EQUIPADA" : "[DESCONOCIDO]"), estilo.Normal, Rol.Texto, r.X + 30, r.Y + 250);
            string estadoCapacidad = cadete.CapacidadActiva ? "ACTIVA · turnos restantes: " + cadete.TurnosCapacidad : "inactiva";
            Texto(g, "Capacidad especial: " + estadoCapacidad, estilo.Normal, cadete.CapacidadActiva ? Rol.Exito : Rol.TextoSuave, r.X + 30, r.Y + 280);
            Texto(g, "[1] Activar habilidad de poder (Poder ≥ " + Juego.UmbralPoder + ")", estilo.Normal, Rol.Aviso, r.X + 30, r.Y + 345);
            Texto(g, "[0] Regresar", estilo.Normal, Rol.Aviso, r.X + 30, r.Y + 375);
        }
        void DibujarManual(Graphics g)
        {
            if (juego.ManualSub == 0)
            {
                var menu = new Rectangle(ClientSize.Width / 2 - 260, 150, 520, 280);
                Marco(g, menu, "NEXUS // MANUAL", Rol.Acento);
                Texto(g, "[1] Manual de uso", estilo.Grande, Rol.Texto, menu.X + 50, menu.Y + 60);
                Texto(g, "[2] Archivos históricos", estilo.Grande, Rol.Texto, menu.X + 50, menu.Y + 110);
                Texto(g, "[0] Regresar", estilo.Grande, Rol.Texto, menu.X + 50, menu.Y + 160);
                return;
            }
            var r = new Rectangle(60, 50, ClientSize.Width - 120, 540);
            if (juego.ManualSub == 1)
            {
                Marco(g, r, "NEXUS // MANUAL DE USO", Rol.Acento);
                string[] lineas =
                {
"[CONTROLES]",
"W A S D / Flechas: moverte por la Base y el mapa.",
"E: confirmar interacciones (cama, terminal, salida).   X: cancelar.",
"Esc: volver al menú.   F2: cambiar el tema visual.",
"",
"[COMBATE]",
"[A] atacar en tu turno, [H] huir (cuesta " + Combate.DanioPorHuir + " de vida).",
"[D] defenderte en el turno del enemigo.",
"",
"[VIDA Y ENERGÍA]",
"Si tu Vida llega a 0, la conexión termina de emergencia.",
"",
"[PODER]",
"Al llegar a " + Juego.UmbralPoder + " de Poder puedes activar tu capacidad especial",
"desde Capacidades / Equipamiento.",
"",
"[MISIONES]",
"Derrota a los enemigos en orden y reclama la recompensa en el Terminal.",
"",
"ENTER para regresar"
};
                for (int i = 0; i < lineas.Length; i++)
                {
                    bool encabezado = lineas[i].StartsWith("[");
                    Texto(g, lineas[i], encabezado ? estilo.Negrita : estilo.Normal, encabezado ? Rol.Aviso : Rol.Texto, r.X + 30, r.Y + 45 + i * 24);
                }
                return;
            }
            Marco(g, r, "ARCHIVOS HISTÓRICOS · IRIS", Rol.Aviso);
            int visibles = LineasVisibles();
            for (int i = 0; i < visibles; i++)
            {
                bool deNexus = Juego.Archivos[i].StartsWith("NEXUS");
                Texto(g, Juego.Archivos[i], estilo.Normal, deNexus ? Rol.Acento : Rol.Texto, r.X + 30, r.Y + 45 + i * 24);
            }
            if (visibles >= Juego.Archivos.Length)
                Texto(g, "ENTER para regresar", estilo.Normal, Rol.TextoSuave, r.X + 30, r.Bottom - 34);
        }
        void DibujarNexo(Graphics g)
        {
            var r = new Rectangle(ClientSize.Width / 2 - 260, 90, 520, 440);
            Marco(g, r, "NEXO MULTIVERSAL", Rol.Acento);
            for (int i = 0; i < Juego.Reinos.Length; i++)
            {
                bool disponible = i == 0 || i == Juego.Reinos.Length - 1;
                string marca = disponible ? "◆ " : "  ";
                Texto(g, marca + "[" + (i + 1) + "] " + Juego.Reinos[i], estilo.Grande, disponible ? Rol.Texto : Rol.TextoSuave, r.X + 50, r.Y + 55 + i * 50);
            }
        }
        void DibujarTerminal(Graphics g)
        {
            var r = new Rectangle(ClientSize.Width / 2 - 340, 110, 680, 380);
            Marco(g, r, "NEXUS // TERMINAL DE MISIONES", Rol.Acento);
            for (int i = 0; i < juego.Misiones.Count; i++)
            {
                string estado = juego.EstadoMision(i);
                Rol rolEstado = Rol.Acento;
                if (estado == "COMPLETADA") rolEstado = Rol.Exito;
                else if (estado == "BLOQUEADA") rolEstado = Rol.TextoSuave;
                else if (estado == "ACTUAL") rolEstado = Rol.Aviso;
                var fila = new Rectangle(r.X + 24, r.Y + 55 + i * 80, r.Width - 48, 68);
                estilo.DibujarCajaPixel(g, fila, C(Rol.Fondo), C(rolEstado), C(Rol.TextoSuave), 2);
                Texto(g, "[" + (i + 1) + "] " + juego.Misiones[i].Nombre, estilo.Negrita, Rol.Texto, fila.X + 14, fila.Y + 10);
                Texto(g, estado, estilo.Normal, rolEstado, fila.X + 14, fila.Y + 38);
            }
            Texto(g, "[0] Salir del terminal", estilo.Normal, Rol.Aviso, r.X + 24, r.Bottom - 34);
        }
        void DibujarDetalleMision(Graphics g)
        {
            Mision m = juego.Misiones[juego.MisionDetalle];
            bool esActual = juego.MisionDetalle == juego.MisionIndice;
            var r = new Rectangle(ClientSize.Width / 2 - 340, 110, 680, 380);
            Marco(g, r, m.Nombre, Rol.Acento);
            Texto(g, m.Descripcion, estilo.Normal, Rol.Texto, r.X + 30, r.Y + 55);
            TextoCaja(g, "⚠ " + m.Consejo, estilo.Normal, Rol.Aviso, new RectangleF(r.X + 30, r.Y + 100, r.Width - 60, 70));
            if (m.Completada)
            {
                Texto(g, "Misión completada.", estilo.Negrita, Rol.Exito, r.X + 30, r.Y + 210);
            }
            else if (!esActual)
            {
                Texto(g, "Bloqueada: completa primero la misión anterior.", estilo.Normal, Rol.TextoSuave, r.X + 30, r.Y + 210);
            }
            else if (!m.ObjetivoCumplido)
            {
                Texto(g, "Derrota al enemigo " + m.TipoEnemigo + " y regresa a la Base.", estilo.Normal, Rol.Acento, r.X + 30, r.Y + 210);
            }
            else
            {
                Texto(g, "+" + m.RecompensaPoder + " PODER", estilo.Negrita, Rol.Acento, r.X + 30, r.Y + 210);
                Texto(g, "+" + m.RecompensaXp + " XP", estilo.Negrita, Rol.Aviso, r.X + 30, r.Y + 240);
                Texto(g, "[S] Reclamar recompensa", estilo.Grande, Rol.Exito, r.X + 30, r.Y + 290);
            }
            Texto(g, "[Esc] Regresar", estilo.Pequena, Rol.TextoSuave, r.X + 30, r.Bottom - 30);
        }
        void DibujarFin(Graphics g)
        {
            Cadete cadete = juego.Cadete;
            string nombre = cadete != null ? cadete.Nombre : "";
            int nivel = cadete != null ? cadete.Nivel : 1;
            TextoCentro(g, "SESIÓN FINALIZADA", estilo.Titulo, Rol.Acento, new RectangleF(0, 190, ClientSize.Width, 60));
            TextoCentro(g, juego.MensajeFin, estilo.Normal, Rol.Texto, new RectangleF(0, 270, ClientSize.Width, 40));
            TextoCentro(g, "Cadete " + nombre + " · Nivel " + nivel, estilo.Normal, Rol.Aviso, new RectangleF(0, 320, ClientSize.Width, 30));
            TextoCentro(g, "ENTER para cerrar", estilo.Pequena, Rol.TextoSuave, new RectangleF(0, 400, ClientSize.Width, 30));
        }
    }
}