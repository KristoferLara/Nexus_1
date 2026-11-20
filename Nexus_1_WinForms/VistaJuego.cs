using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace WinForms
{
    public abstract class VistaJuego : Control
    {
        protected readonly Juego juego;
        protected readonly EstiloVisual estilo;
        bool animabaAntes;
        bool teniaAviso;

        // 1. FISICO: Guantes blancos de pelea en ambos puños (15x16)
        static readonly string[] SpriteFisicoNormal = new[]
        {
            "....OOOOOO.....",
            "...OHHHHHHO....",
            "..OHHHHHHHHO...",
            "..OHPPPPPPPO...",
            "..OHP.PP.PPO...",
            "..OHPPPPPPPO...",
            "...OPPPPPPO....",
            "..ORRRRRRRRO...",
            ".OWWRRRRRRWWO..",
            ".OWWRCCCCRWWO..",
            "..OORRRRRROO...",
            "...ORRRRRRO....",
            "...OLO..OLO....",
            "...OLO..OLO....",
            "...OBO..OBO....",
            "..OBBO..OBBO..."
        };

        static readonly string[] SpriteFisicoAtaque = new[]
        {
            ".....OOOOOO....",
            "....OHHHHHHO...",
            "...OHHHHHHHHO..",
            "...OHPPPPPPPO..",
            "...OHP.PP.PPO..",
            "...OHPPPPPPPO..",
            "....OPPPPPPO...",
            "...ORRRRRRRRO..",
            "..OPRRRRRRRROOO",
            "..OPRRCCCCROWWW",
            "...OORRRRRROOOO",
            "....ORRRRRRO...",
            "...OLO...OBO...",
            "..OBBO....OBBO.",
            "...............",
            "..............."
        };

        // 2. ARMAMENTO: Espada y escudo (15x16)
        static readonly string[] SpriteArmamentoNormal = new[]
        {
            "....OOOOOO.....",
            "...OHHHHHHO....",
            "..OHHHHHHHHO...",
            "..OHPPPPPPPO...",
            "..OHP.PP.PPO...",
            "..OHPPPPPPPO...",
            "...OPPPPPPO....",
            "..ORRRRRRRRO.O.",
            ".OPRRRRRRRRPOEO",
            ".OPRRCCCCRRPOEO",
            ".OPRROOOORROEO.",
            "..ORROEEEOO.O..",
            "...OLOEEEO.....",
            "...OLOEEEO.....",
            "...OBO.OBO.....",
            "..OBBO.OBBO...."
        };

        static readonly string[] SpriteArmamentoAtaque = new[]
        {
            ".....OOOOOO.....",
            "....OHHHHHHO....",
            "...OHHHHHHHHO...",
            "...OHPPPPPPPO...",
            "...OHP.PP.PPO...",
            "...OHPPPPPPPO...",
            "....OPPPPPPO....",
            "...ORRRRRRRRO...",
            "..OPRRRRRRRRPO..",
            "..OPRRCCCCRROOOO",
            "..OPRROOOORREEEE",
            "...ORROEEEO.OOOO",
            "....OLOEEEO.....",
            "...OLO.OBO......",
            "..OBBO..OBBO....",
            "................"
        };

        // 3. MANA: Báculo con gema verde/cian en la punta (16x16)
        static readonly string[] SpriteManaNormal = new[]
        {
            "....OOOOOO...O..",
            "...OHHHHHHO.OCO.",
            "..OHHHHHHHHOCCCO",
            "..OHPPPPPPPOOCO.",
            "..OHP.PP.PPO.O..",
            "..OHPPPPPPPO.M..",
            "...OPPPPPPO..M..",
            "..ORRRRRRRRO.M..",
            ".OWWRRRRRRWOOM..",
            ".OWWRCCCCRWOOM..",
            "..OORRRRRROO.M..",
            "...ORRRRRRO..M..",
            "...OLO..OLO..M..",
            "...OLO..OLO..M..",
            "...OBO..OBO..M..",
            "..OBBO..OBBO.O.."
        };

        // 4. HERRAMIENTAS: Ingeniero con mochila/gadget (15x16)
        static readonly string[] SpriteHerramientasNormal = new[]
        {
            "....OOOOOO.....",
            "...OHHHHHHO....",
            "..OHHHHHHHHO...",
            "..OHPPPPPPPO...",
            "..OHP.PP.PPO...",
            "..OHPPPPPPPO...",
            "...OPPPPPPO....",
            ".OOORRRRRRRO...",
            "OMMORRRRRRRPO..",
            "OMMORRCCCCRPO..",
            ".OOORRRRRRRO...",
            "...ORRRRRRO....",
            "...OLO..OLO....",
            "...OLO..OLO....",
            "...OBO..OBO....",
            "..OBBO..OBBO..."
        };

        // HERRAMIENTAS en defensa: Visor táctico iluminado y brazo extendido (16x16)
        static readonly string[] SpriteHerramientasDefensa = new[]
        {
            "....OOOOOO......",
            "...OHHHHHHO.....",
            "..OHHHHHHHHO....",
            "..OEEEEEEEEO....",
            "..OEWEEEEWEO....",
            "..OEEEEEEEEO....",
            "...OPPPPPPO.....",
            ".OOORRRRRRRO....",
            "OMMORRRRRRROOO..",
            "OMMORRCCCCROEEO.",
            ".OOORRRRRRROOO..",
            "...ORRRRRRO.....",
            "...OLO..OLO.....",
            "...OLO..OLO.....",
            "...OBO..OBO.....",
            "..OBBO..OBBO...."
        };

        protected VistaJuego(Juego juego, EstiloVisual estilo)
        {
            this.juego = juego;
            this.estilo = estilo;
            DoubleBuffered = true;
            ResizeRedraw = true;
            TabStop = false;
            Dock = DockStyle.Fill;
            Visible = false;
            SetStyle(ControlStyles.Selectable, false);
        }

        protected virtual bool Animada
        {
            get { return false; }
        }

        protected Color ColorJugador
        {
            get { return ColorClase(juego.TipoPersonaje); }
        }

        public virtual void AlMostrar()
        {
            Invalidate();
        }

        public virtual void Avanzar()
        {
            bool anima = Animada;
            bool hayAviso = juego.AvisoTicks > 0;
            if (anima || animabaAntes || hayAviso || teniaAviso) Invalidate();
            animabaAntes = anima;
            teniaAviso = hayAviso;
        }

        protected abstract void Dibujar(Graphics g);

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

            g.Clear(C(Rol.Fondo));
            Dibujar(g);
            DibujarDialogo(g);
            DibujarAviso(g);
            base.OnPaint(e);
        }

        protected Color C(Rol rol)
        {
            return estilo.Tema[rol];
        }

        protected Color ColorClase(string tipo)
        {
            switch (tipo)
            {
                case "FISICO": return C(Rol.ClaseFisico);
                case "ARMAMENTO": return C(Rol.ClaseArmamento);
                case "MANA": return C(Rol.ClaseMana);
                default: return C(Rol.ClaseHerramientas);
            }
        }

        protected Color ColorEnemigo(Enemigo enemigo)
        {
            switch (enemigo.Efecto)
            {
                case "VENENO": return C(Rol.EnemigoVeneno);
                case "FUEGO": return C(Rol.EnemigoFuego);
                default: return C(Rol.EnemigoLiquido);
            }
        }

        protected void Texto(Graphics g, string texto, Font fuente, Color color, float x, float y)
        {
            using (var pincel = new SolidBrush(color))
                g.DrawString(texto, fuente, pincel, x, y);
        }

        protected void Texto(Graphics g, string texto, Font fuente, Rol rol, float x, float y)
        {
            Texto(g, texto, fuente, C(rol), x, y);
        }

        protected void TextoCentro(Graphics g, string texto, Font fuente, Color color, RectangleF area)
        {
            using (var pincel = new SolidBrush(color))
            using (var formato = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(texto, fuente, pincel, area, formato);
        }

        protected void TextoCentro(Graphics g, string texto, Font fuente, Rol rol, RectangleF area)
        {
            TextoCentro(g, texto, fuente, C(rol), area);
        }

        protected void TextoCaja(Graphics g, string texto, Font fuente, Color color, RectangleF area)
        {
            using (var pincel = new SolidBrush(color))
                g.DrawString(texto, fuente, pincel, area);
        }

        protected void TextoCaja(Graphics g, string texto, Font fuente, Rol rol, RectangleF area)
        {
            TextoCaja(g, texto, fuente, C(rol), area);
        }

        protected void Marco(Graphics g, Rectangle r, string titulo, Color borde)
        {
            estilo.DibujarCajaPixel(g, r, C(Rol.Superficie), borde, C(Rol.TextoSuave), 3);

            if (string.IsNullOrEmpty(titulo)) return;

            using (var barra = new SolidBrush(borde))
                g.FillRectangle(barra, r.X + 6, r.Y + 6, r.Width - 12, 26);
            TextoCentro(g, titulo, estilo.Negrita, Rol.TextoSobreAcento, new RectangleF(r.X + 6, r.Y + 6, r.Width - 12, 26));
        }

        protected void Marco(Graphics g, Rectangle r, string titulo, Rol borde)
        {
            Marco(g, r, titulo, C(borde));
        }

        protected void Barra(Graphics g, int x, int y, int w, int h, int porcentaje, Color relleno, string etiqueta)
        {
            porcentaje = Math.Max(0, Math.Min(100, porcentaje));
            var rect = new Rectangle(x, y, w, h);

            estilo.DibujarBarraPixel(g, rect, porcentaje, 100, relleno, C(Rol.SuperficieHueco), C(Rol.TextoSuave));

            Texto(g, etiqueta + " " + porcentaje + "%", estilo.Pequena, Color.Black, x + 7, y + (h - 14) / 2f);
            Texto(g, etiqueta + " " + porcentaje + "%", estilo.Pequena, Rol.Texto, x + 6, y + (h - 14) / 2f - 1);
        }

        protected void Barra(Graphics g, int x, int y, int w, int h, int porcentaje, Rol relleno, string etiqueta)
        {
            Barra(g, x, y, w, h, porcentaje, C(relleno), etiqueta);
        }

        string[] ObtenerSpriteCadete(string clase, bool poseEspecial)
        {
            switch (clase)
            {
                case "FISICO": return poseEspecial ? SpriteFisicoAtaque : SpriteFisicoNormal;
                case "ARMAMENTO": return poseEspecial ? SpriteArmamentoAtaque : SpriteArmamentoNormal;
                case "MANA": return SpriteManaNormal;
                default: return poseEspecial ? SpriteHerramientasDefensa : SpriteHerramientasNormal;
            }
        }
        protected void DibujarCadete(Graphics g, float x, float y, float escala, Color ropa, int vaiven, bool poseEspecial = false, string claseOverride = null)
        {
            int px = Math.Max(2, (int)Math.Round(4 * escala));
            float inicioX = x + (20 * escala) - (7 * px);
            float inicioY = y + vaiven;
            string claseActual = string.IsNullOrEmpty(claseOverride) ? juego.TipoPersonaje : claseOverride;
            string[] sprite = ObtenerSpriteCadete(claseActual, poseEspecial);
            Color contorno = Color.FromArgb(20, 18, 24);
            Color piel = C(Rol.CadetePiel);
            Color piernas = C(Rol.CadetePiernas);
            Color casco = ControlPaint.Dark(ropa, 0.25f);
            Color cinturon = C(Rol.Acento);
            Color botas = ControlPaint.Dark(piernas, 0.35f);
            Color acero = (claseActual == "HERRAMIENTAS" && poseEspecial)
            ? Color.FromArgb(145, 130, 175) // Visor táctico iluminado en defensa
            : Color.FromArgb(200, 215, 230);
            Color baston = Color.FromArgb(75, 50, 125);
            Color gemaMana = Color.FromArgb(65, 220, 175);
            using (var bSombra = new SolidBrush(Color.FromArgb(110, 0, 0, 0)))
            {
                g.FillRectangle(bSombra, inicioX + px * 3, y + 15 * px, px * 9, px * 2);
            }
            for (int fila = 0; fila < sprite.Length; fila++)
            {
                string linea = sprite[fila];
                for (int col = 0; col < linea.Length; col++)
                {
                    char c = linea[col];
                    if (c == '.') continue;
                    Color colorPixel;
                    switch (c)
                    {
                        case 'O': colorPixel = contorno; break;
                        case 'H': colorPixel = casco; break;
                        case 'P': colorPixel = piel; break;
                        case 'R': colorPixel = ropa; break;
                        case 'C': colorPixel = (claseActual == "MANA") ? gemaMana : cinturon; break;
                        case 'L': colorPixel = piernas; break;
                        case 'B': colorPixel = botas; break;
                        case 'E': colorPixel = acero; break;
                        case 'W': colorPixel = Color.White; break;
                        case 'M': colorPixel = baston; break;
                        default: colorPixel = piel; break;
                    }
                    float offsetY = 0;
                    if (!poseEspecial && vaiven != 0 && (c == 'L' || c == 'B'))
                    {
                        offsetY = (col < 7) ? -px * 0.5f : px * 0.5f;
                    }
                    using (var brocha = new SolidBrush(colorPixel))
                    {
                        g.FillRectangle(brocha, inicioX + col * px, inicioY + fila * px + offsetY, px, px);
                    }
                }
            }
        }
        protected void DibujarSiluetaDash(Graphics g, float x, float y, float escala, Color colorSilueta, bool haciaDerecha)
        {
            int px = Math.Max(2, (int)Math.Round(4 * escala));
            float inicioX = x + (20 * escala) - (7 * px);
            string[] sprite = SpriteFisicoAtaque;
            using (var brocha = new SolidBrush(Color.FromArgb(190, colorSilueta)))
            {
                for (int fila = 0; fila < sprite.Length; fila++)
                {
                    string linea = sprite[fila];
                    for (int col = 0; col < linea.Length; col++)
                    {
                        if (linea[col] == '.') continue;
                        g.FillRectangle(brocha, inicioX + col * px, y + fila * px, px, px);
                    }
                }
                int dir = haciaDerecha ? -1 : 1;
                float estelaX = haciaDerecha ? inicioX - px * 5 : inicioX + px * 15;
                g.FillRectangle(brocha, estelaX, y + px * 3, px * 6, px);
                g.FillRectangle(brocha, estelaX + dir * px * 2, y + px * 7, px * 7, px);
                g.FillRectangle(brocha, estelaX, y + px * 11, px * 6, px);
            }
        }
        void DibujarDialogo(Graphics g)
        {
            Dialogo dialogo = juego.Dialogo;
            if (dialogo == null) return;
            using (var velo = new SolidBrush(C(Rol.Velo)))
                g.FillRectangle(velo, ClientRectangle);
            var r = new Rectangle(ClientSize.Width / 2 - 200, ClientSize.Height / 2 - 100, 400, 200);
            Marco(g, r, dialogo.Titulo, Rol.Acento);
            TextoCentro(g, dialogo.Pregunta, estilo.Normal, Rol.Texto, new RectangleF(r.X + 10, r.Y + 45, r.Width - 20, 45));
            TextoCentro(g, "[E] Sí", estilo.Negrita, Rol.Exito, new RectangleF(r.X, r.Y + 105, r.Width, 30));
            TextoCentro(g, "[X] No", estilo.Negrita, Rol.Peligro, new RectangleF(r.X, r.Y + 140, r.Width, 30));
        }
        void DibujarAviso(Graphics g)
        {
            if (juego.AvisoTicks <= 0 || juego.Aviso.Length == 0) return;
            var r = new Rectangle(80, ClientSize.Height - 54, ClientSize.Width - 160, 38);
            estilo.DibujarCajaPixel(g, r, C(Rol.AvisoFondo), C(Rol.Aviso), C(Rol.TextoSuave), 2);
            TextoCentro(g, juego.Aviso, estilo.Normal, Rol.Aviso, r);
        }
    }
}