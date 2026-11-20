using System;
using System.Drawing;
using System.Linq;

namespace WinForms
{
    public sealed class VistaGenesis : VistaJuego
    {
        const float Suavizado = 0.58f;
        const float Tolerancia = 0.01f;
        const int Celda = 52;

        float visX;
        float visY;

        // Sprites Pixel Art de 12x12 para las celdas del mapa
        static readonly string[] SpriteBase = new[]
        {
            ".....AA.....",
            "....AAAA....",
            "...AAAAAA...",
            "..AAAAAAAA..",
            ".AAAAAAAAAA.",
            "BBBBBBBBBBBB",
            ".BMMMMMMMMB.",
            ".BMMPPPPMMB.",
            ".BMMPPPPMMB.",
            ".BMMPPPPMMB.",
            ".BMMMMMMMMB.",
            "BBBBBBBBBBBB"
        };

        static readonly string[] SpriteArbol = new[]
        {
            ".....LL.....",
            "....LLLL....",
            "...LLLLLL...",
            "..LLCCCCLL..",
            "...CCCCCC...",
            "..CCCCCCCC..",
            ".CCCCCCCCCC.",
            "CCCCCCCCCCCC",
            ".CCCCCCCCCC.",
            "....TTTT....",
            "....TTTT....",
            "...TTTTTT..."
        };

        static readonly string[] SpriteBotiquin = new[]
        {
            "............",
            "..BBBBBBBB..",
            ".BWWWWWWWWB.",
            ".BWWECCEWBB.",
            ".BWWECCEWBB.",
            ".BWCCCCCCWB.",
            ".BWCCCCCCWB.",
            ".BWWECCEWBB.",
            ".BWWECCEWBB.",
            ".BWWWWWWWWB.",
            "..BBBBBBBB..",
            "............"
        };

        static readonly string[] SpriteAnomalia = new[]
        {
            ".....LL.....",
            "....LCCL....",
            "...LCCCCL...",
            "..LCCCCCCL..",
            ".LCCCPPPCCL.",
            "LCCCCPPPCCCL",
            "LCCCCPPPCCCL",
            ".CCCCCCCCCL.",
            "..CCCPPPCC..",
            "...CCPPPCC..",
            "....CCCCC...",
            ".....CCC...."
        };

        static readonly string[] SpriteMiniEnemigo = new[]
        {
            "............",
            "...CCCCCC...",
            "..CLLLLLLC..",
            ".CCCCCCCCCC.",
            ".CCOOCCOOCC.",
            ".CCOOCCOOCC.",
            ".CCCCCCCCCC.",
            "..CCSSSSCC..",
            "...CCCCCC...",
            "..CC.CC.CC..",
            ".CC..CC..CC.",
            "............"
        };

        public VistaGenesis(Juego juego, EstiloVisual estilo) : base(juego, estilo)
        {
        }

        protected override bool Animada
        {
            get
            {
                return Math.Abs(juego.GCol - visX) > Tolerancia || Math.Abs(juego.GFila - visY) > Tolerancia;
            }
        }

        public override void AlMostrar()
        {
            visX = juego.GCol;
            visY = juego.GFila;
            base.AlMostrar();
        }

        public override void Avanzar()
        {
            visX = Acercar(visX, juego.GCol);
            visY = Acercar(visY, juego.GFila);
            base.Avanzar();
        }

        static float Acercar(float actual, int destino)
        {
            float siguiente = actual + (destino - actual) * Suavizado;
            return Math.Abs(destino - siguiente) < Tolerancia ? destino : siguiente;
        }

        protected override void Dibujar(Graphics g)
        {
            char[,] mapa = juego.MapaGenesis;
            int filas = mapa.GetLength(0);
            int columnas = mapa.GetLength(1);
            int ox = 30;
            int oy = 60;
            Marco(g, new Rectangle(ox - 10, oy - 40, columnas * Celda + 20, filas * Celda + 100), "MAPA GENESIS", Rol.Acento);

            for (int f = 0; f < filas; f++)
                for (int c = 0; c < columnas; c++)
                    DibujarCelda(g, f, c, ox + c * Celda, oy + f * Celda, Celda);

            // Si el personaje se está moviendo, activa el vaivén de pasos
            int vaiven = Animada ? (int)(Math.Sin(juego.Tick * 0.8) * 3) : 0;
            DibujarCadete(g, ox + visX * Celda + 9, oy + visY * Celda + 2, 0.55f, ColorJugador, vaiven);
            Texto(g, "[W A S D] mover   [Esc] salir del mapa", estilo.Pequena, Rol.TextoSuave, ox, oy + filas * Celda + 10);

            DibujarInformacion(g, ox + columnas * Celda + 30);
        }

        void DibujarInformacion(Graphics g, int x)
        {
            Cadete cadete = juego.Cadete;
            int ancho = ClientSize.Width - x - 30;

            var info = new Rectangle(x, 20, ancho, 330);
            Marco(g, info, "INFORMACIÓN DEL MAPA", Rol.Aviso);
            Texto(g, "Posición: [" + juego.GFila + "," + juego.GCol + "]", estilo.Normal, Rol.Texto, info.X + 14, info.Y + 44);
            Texto(g, "Distancia: " + juego.Distancia + " m", estilo.Normal, Rol.Texto, info.X + 14, info.Y + 74);
            Texto(g, "Botiquines: " + cadete.Recursos, estilo.Normal, Rol.Texto, info.X + 14, info.Y + 104);
            Texto(g, "◆ Botiquín", estilo.Pequena, Rol.Exito, info.X + 14, info.Y + 146);
            Texto(g, "⚠ Anomalía", estilo.Pequena, Rol.Aviso, info.X + 14, info.Y + 170);
            Texto(g, "☠ Enemigo", estilo.Pequena, Rol.Peligro, info.X + 14, info.Y + 194);
            Texto(g, "⌂ Base", estilo.Pequena, Rol.Acento, info.X + 14, info.Y + 218);
            Texto(g, "♣ Terreno elevado", estilo.Pequena, Rol.MapaTerreno, info.X + 14, info.Y + 242);
            Texto(g, "? Desconocido", estilo.Pequena, Rol.TextoSuave, info.X + 14, info.Y + 266);

            var estado = new Rectangle(x, 370, ancho, 220);
            Marco(g, estado, "CADETE", Rol.Aviso);
            Barra(g, estado.X + 14, estado.Y + 48, estado.Width - 28, 24, cadete.Vida, Rol.Peligro, "Vida ");
            Barra(g, estado.X + 14, estado.Y + 84, estado.Width - 28, 24, cadete.Poder, Rol.Aviso, "Poder");
            Texto(g, "Clase: " + juego.TipoPersonaje, estilo.Pequena, Rol.Texto, estado.X + 14, estado.Y + 126);
            Mision mision = juego.MisionActual;
            Texto(g, "Misión:", estilo.Pequena, Rol.TextoSuave, estado.X + 14, estado.Y + 152);
            TextoCaja(g, mision != null ? mision.Descripcion : "Todas completadas", estilo.Pequena, Rol.Texto, new RectangleF(estado.X + 14, estado.Y + 172, estado.Width - 28, 40));
        }

        void DibujarCelda(Graphics g, int f, int c, int x, int y, int t)
        {
            var area = new Rectangle(x, y, t - 2, t - 2);
            char simbolo = juego.MapaGenesis[f, c];

            // 1. Muro con textura de bloques pixel art
            if (simbolo == '|')
            {
                Color muroBase = C(Rol.MapaMuro);
                Color muroLuz = ControlPaint.Light(muroBase, 0.2f);
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
                    // Línea central de ladrillo
                    g.FillRectangle(bSombra, x + 4, y + t / 2 - 2, t - 10, 3);
                }
                return;
            }

            // 2. Niebla de guerra pixelada
            if (!juego.Explorada[f, c])
            {
                Color nieblaColor = C(Rol.MapaNiebla);
                using (var niebla = new SolidBrush(nieblaColor))
                using (var borde = new SolidBrush(ControlPaint.Dark(nieblaColor, 0.15f)))
                {
                    g.FillRectangle(niebla, area);
                    g.FillRectangle(borde, x, y + t - 5, t - 2, 3);
                }
                TextoCentro(g, "?", estilo.Negrita, Rol.MapaNieblaTexto, area);
                return;
            }

            // 3. Suelo explorado con relieve de baldosa retro
            Color sueloColor = C(Rol.MapaSueloGenesis);
            using (var suelo = new SolidBrush(sueloColor))
            using (var sombraSuelo = new SolidBrush(ControlPaint.Dark(sueloColor, 0.12f)))
            using (var puntoSuelo = new SolidBrush(ControlPaint.Light(sueloColor, 0.15f)))
            {
                g.FillRectangle(suelo, area);
                g.FillRectangle(sombraSuelo, x, y + t - 5, t - 2, 3);
                // Pequeño detalle de textura según coordenadas
                if ((f + c) % 2 == 0)
                    g.FillRectangle(puntoSuelo, x + 10, y + 10, 4, 4);
                else
                    g.FillRectangle(puntoSuelo, x + t - 16, y + t - 16, 4, 4);
            }

            // Centrar sprite de 12x12 a escala px = 4 (48x48 píxeles dentro de la celda de 50x50)
            int px = 4;
            int sx = x + 1;
            int sy = y + 1;

            Enemigo enemigo = juego.Enemigos.FirstOrDefault(en => en.Fila == f && en.Columna == c);
            if (enemigo != null)
            {
                Color cEnemigo = ColorEnemigo(enemigo);
                DibujarTilePixel(g, SpriteMiniEnemigo, sx, sy, px, cEnemigo, ControlPaint.Light(cEnemigo, 0.35f), ControlPaint.Dark(cEnemigo, 0.35f), C(Rol.EnemigoOjo));
            }
            else if (simbolo == '⌂')
            {
                Color acento = C(Rol.Acento);
                DibujarTilePixel(g, SpriteBase, sx, sy, px, C(Rol.Superficie), acento, ControlPaint.Dark(acento, 0.3f), C(Rol.Aviso));
            }
            else if (simbolo == '◆')
            {
                Color exito = C(Rol.Exito);
                DibujarTilePixel(g, SpriteBotiquin, sx, sy, px, exito, ControlPaint.Light(exito, 0.3f), ControlPaint.Dark(exito, 0.4f), Color.White);
            }
            else if (simbolo == '⚠')
            {
                Color aviso = C(Rol.Aviso);
                DibujarTilePixel(g, SpriteAnomalia, sx, sy, px, aviso, ControlPaint.Light(aviso, 0.4f), ControlPaint.Dark(aviso, 0.3f), C(Rol.Peligro));
            }
            else if (simbolo == 'A')
            {
                Color terreno = C(Rol.MapaTerreno);
                DibujarTilePixel(g, SpriteArbol, sx, sy, px, terreno, ControlPaint.Light(terreno, 0.3f), C(Rol.ObjetoMaderaOscura), terreno);
            }
        }
        void DibujarTilePixel(Graphics g, string[] sprite, int x, int y, int px, Color principal, Color luz, Color oscuro, Color detalle)
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
                        case 'L': colorPixel = luz; break;
                        case 'B':
                        case 'S':
                        case 'T': colorPixel = oscuro; break;
                        case 'P':
                        case 'O':
                        case 'W': colorPixel = detalle; break;
                        case 'A':
                        case 'M': colorPixel = (c == 'A') ? luz : principal; break;
                        default: colorPixel = principal; break;
                    }
                    using (var brocha = new SolidBrush(colorPixel))
                    {
                        g.FillRectangle(brocha, x + col * px, y + fila * px, px, px);
                    }
                }
            }
        }
    }
}