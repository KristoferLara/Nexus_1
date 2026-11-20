using System;
using System.Drawing;

namespace WinForms
{
    public sealed class VistaBase : VistaJuego
    {
        const float Suavizado = 0.58f;
        const int Celda = 70;

        float visX;
        float visY;

        // Sprites Pixel Art de 16x16 para el interior de la Base
        static readonly string[] SpriteCama = new[]
        {
            "................",
            "................",
            ".MM..........MM.",
            ".MMMMMMMMMMMMMM.",
            ".MWWWCCCCCCCCCM.",
            ".MWWWCCCCCCCCCM.",
            ".MWWWCCCCCCCCCM.",
            ".MWWWCCCCCCCCCM.",
            ".MWWWCCCCCCCCCM.",
            ".MWWWCCCCCCCCCM.",
            ".MMMMMMMMMMMMMM.",
            ".MM..........MM.",
            ".SS..........SS.",
            "................",
            "................",
            "................"
        };

        static readonly string[] SpriteTerminal = new[]
        {
            "................",
            "..MMMMMMMMMMMM..",
            ".MLLLLLLLLLLLLM.",
            ".MLPPPPPPPPPPLM.",
            ".MLPWWPPPPPPPLM.",
            ".MLPPPPWWWWPPLM.",
            ".MLPWWPPPPPPPLM.",
            ".MLPPPPPPPPPPLM.",
            ".MLLLLLLLLLLLLM.",
            "..MMMMMMMMMMMM..",
            ".....MMMMMM.....",
            "...MMMMMMMMMM...",
            "..MLLLLLLLLLLM..",
            "..MMMMMMMMMMMM..",
            "................",
            "................"
        };

        static readonly string[] SpriteEquipamiento = new[]
        {
            "................",
            "................",
            "..BBBBBBBBBBBB..",
            ".BMMMMMMMMMMMMB.",
            ".BMLLLLLLLLLLMB.",
            ".BMLLLLLLLLLLMB.",
            ".BBBBBBCCBBBBBB.",
            ".BMMMMBCCBMMMMB.",
            ".BMLLLLLLLLLLMB.",
            ".BMLLLLLLLLLLMB.",
            ".BMLLLLLLLLLLMB.",
            ".BMMMMMMMMMMMMB.",
            "..BBBBBBBBBBBB..",
            "................",
            "................",
            "................"
        };

        static readonly string[] SpritePuerta = new[]
        {
            "..MMMMMMMMMMMM..",
            ".MLLLLLLLLLLLLM.",
            ".MLCCCCCCCCCCLM.",
            ".MLCCCCCCCCCCLM.",
            ".MLCCCCCCCCCCLM.",
            ".MLCCCCCCCCCCLM.",
            ".MLCCCCCCCCCCLM.",
            ".MLCCCCCCCCCCLM.",
            ".MLCCCCCCCCCCLM.",
            ".MLCCCCCCCCCCLM.",
            ".MLCCCCCCCCCCLM.",
            ".MLCCCCCCCCCCLM.",
            ".MLCCCCCCCCCCLM.",
            ".MLCCCCCCCCCCLM.",
            ".MLLLLLLLLLLLLM.",
            "MMMMMMMMMMMMMMMM"
        };

        static readonly string[] SpriteCandado = new[]
        {
            "......AAAA......",
            ".....A....A.....",
            ".....A....A.....",
            "....AAAAAAAA....",
            "....AAAKKAAA....",
            "....AAAKKAAA....",
            "....AAAAAAAA....",
            "................"
        };

        public VistaBase(Juego juego, EstiloVisual estilo) : base(juego, estilo)
        {
        }

        protected override bool Animada
        {
            get { return true; }
        }

        public override void AlMostrar()
        {
            visX = juego.BCol;
            visY = juego.BFila;
            base.AlMostrar();
        }

        public override void Avanzar()
        {
            visX += (juego.BCol - visX) * Suavizado;
            visY += (juego.BFila - visY) * Suavizado;
            base.Avanzar();
        }

        protected override void Dibujar(Graphics g)
        {
            char[,] mapa = juego.MapaBase;
            int filas = mapa.GetLength(0);
            int columnas = mapa.GetLength(1);
            int ox = (ClientSize.Width - columnas * Celda) / 2;
            int oy = 90;
            Marco(g, new Rectangle(ox - 20, oy - 55, columnas * Celda + 40, filas * Celda + 160), "BASE OPERATIVA", Rol.Acento);

            for (int f = 0; f < filas; f++)
                for (int c = 0; c < columnas; c++)
                    DibujarCelda(g, mapa[f, c], ox + c * Celda, oy + f * Celda, Celda);

            int vaiven = (int)(Math.Sin(juego.Tick * 0.4) * 2);
            DibujarCadete(g, ox + visX * Celda + 14, oy + visY * Celda + 6, 0.8f, ColorJugador, vaiven);

            Texto(g, "Cama · Terminal · Equipamiento · Salida · Bloqueada", estilo.Pequena, Rol.TextoSuave, ox, oy + filas * Celda + 20);
            Texto(g, "[W A S D] mover   [Esc] volver", estilo.Pequena, Rol.TextoSuave, ox, oy + filas * Celda + 44);
        }

        void DibujarCelda(Graphics g, char simbolo, int x, int y, int t)
        {
            var area = new Rectangle(x, y, t - 2, t - 2);

            // 1. Muros metálicos de la estación con paneles y remaches
            if (simbolo == '|')
            {
                Color muroBase = C(Rol.MapaMuro);
                Color muroLuz = ControlPaint.Light(muroBase, 0.25f);
                Color muroSombra = ControlPaint.Dark(muroBase, 0.3f);

                using (var bBase = new SolidBrush(muroBase))
                using (var bLuz = new SolidBrush(muroLuz))
                using (var bSombra = new SolidBrush(muroSombra))
                {
                    g.FillRectangle(bBase, area);
                    g.FillRectangle(bLuz, x, y, t - 2, 4);
                    g.FillRectangle(bLuz, x, y, 4, t - 2);
                    g.FillRectangle(bSombra, x, y + t - 6, t - 2, 4);
                    g.FillRectangle(bSombra, x + t - 6, y, 4, t - 2);

                    // Remaches pixel art en las 4 esquinas del muro
                    g.FillRectangle(bSombra, x + 8, y + 8, 4, 4);
                    g.FillRectangle(bSombra, x + t - 14, y + 8, 4, 4);
                    g.FillRectangle(bSombra, x + 8, y + t - 14, 4, 4);
                    g.FillRectangle(bSombra, x + t - 14, y + t - 14, 4, 4);
                }
                return;
            }

            // 2. Suelo de la base con baldosas tecnológicas
            Color sueloColor = C(Rol.MapaSueloBase);
            using (var suelo = new SolidBrush(sueloColor))
            using (var bordeSuelo = new SolidBrush(ControlPaint.Dark(sueloColor, 0.14f)))
            using (var brilloSuelo = new SolidBrush(ControlPaint.Light(sueloColor, 0.12f)))
            {
                g.FillRectangle(suelo, area);
                g.FillRectangle(bordeSuelo, x, y + t - 5, t - 2, 3);
                g.FillRectangle(brilloSuelo, x + 4, y + 4, 4, 4);
            }

            // Centrar sprite de 16x16 a escala px = 4 (64x64 píxeles dentro de la celda de 68x68)
            int px = 4;
            int sx = x + 2;
            int sy = y + 2;

            if (simbolo == 'C')
            {
                DibujarObjetoBase(g, SpriteCama, sx, sy, px, C(Rol.ObjetoMetal), C(Rol.ObjetoCama), C(Rol.ObjetoSabana), Color.Black);
            }
            else if (simbolo == 'T')
            {
                // Parpadeo sutil de las líneas de código en el monitor del Terminal
                Color textoMonitor = (juego.Tick / 16) % 2 == 0 ? Color.White : C(Rol.Aviso);
                DibujarObjetoBase(g, SpriteTerminal, sx, sy, px, C(Rol.ObjetoMetal), C(Rol.ObjetoPantalla), C(Rol.Acento), textoMonitor);
            }
            else if (simbolo == 'E')
            {
                DibujarObjetoBase(g, SpriteEquipamiento, sx, sy, px, C(Rol.ObjetoMaderaOscura), C(Rol.ObjetoMadera), C(Rol.ObjetoOro), C(Rol.ObjetoMetal));
            }
            else if (simbolo == 'S' || simbolo == 'B')
            {
                Color colorLuzPuerta = simbolo == 'S' ? C(Rol.Exito) : C(Rol.Peligro);
                DibujarObjetoBase(g, SpritePuerta, sx, sy, px, C(Rol.ObjetoMetal), ControlPaint.Dark(C(Rol.ObjetoMetal), 0.25f), colorLuzPuerta, Color.White);

                if (simbolo == 'B')
                {
                    // Dibuja el candado pixel art centrado sobre la puerta bloqueada
                    DibujarCandado(g, sx, sy + 20, px, C(Rol.Aviso));
                }
            }
        }

        void DibujarObjetoBase(Graphics g, string[] sprite, int x, int y, int px, Color metal, Color principal, Color detalle, Color acento)
        {
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
                        case 'M': colorPixel = metal; break;
                        case 'L':
                        case 'B': colorPixel = principal; break;
                        case 'C':
                        case 'P': colorPixel = detalle; break;
                        case 'W': colorPixel = acento; break;
                        case 'S': colorPixel = Color.FromArgb(110, 0, 0, 0); break;
                        default: colorPixel = principal; break;
                    }

                    using (var brocha = new SolidBrush(colorPixel))
                        g.FillRectangle(brocha, x + col * px, y + fila * px, px, px);
                }
            }
        }

        void DibujarCandado(Graphics g, int x, int y, int px, Color colorCandado)
        {
            for (int fila = 0; fila < SpriteCandado.Length; fila++)
            {
                string linea = SpriteCandado[fila];
                for (int col = 0; col < linea.Length; col++)
                {
                    char c = linea[col];
                    if (c == '.') continue;

                    Color colorPixel = (c == 'K') ? Color.FromArgb(30, 30, 35) : colorCandado;
                    using (var brocha = new SolidBrush(colorPixel))
                        g.FillRectangle(brocha, x + col * px, y + fila * px, px, px);
                }
            }
        }
    }
}
