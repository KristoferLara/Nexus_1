using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace WinForms
{
    public sealed class EstiloVisual : Component
    {
        public Tema Tema { get; private set; }
        public Font Gigante { get; }
        public Font Titulo { get; }
        public Font Grande { get; }
        public Font Normal { get; }
        public Font Negrita { get; }
        public Font Pequena { get; }

        public event EventHandler TemaCambiado;

        public EstiloVisual(Tema tema)
        {
            Tema = tema;
            Gigante = new Font("Consolas", 64, FontStyle.Bold);
            Titulo = new Font("Consolas", 26, FontStyle.Bold);
            Grande = new Font("Consolas", 16, FontStyle.Bold);
            Normal = new Font("Consolas", 12);
            Negrita = new Font("Consolas", 12, FontStyle.Bold);
            Pequena = new Font("Consolas", 10);
        }

        public void AplicarTema(Tema tema)
        {
            if (tema == null || tema == Tema) return;
            Tema = tema;
            TemaCambiado?.Invoke(this, EventArgs.Empty);
        }

        // --- HERRAMIENTAS DE DIBUJO PIXEL ART ---

        /// <summary>
        /// Dibuja un panel estilo ventana RPG de 16 bits con sombra dura y doble borde pixelado.
        /// </summary>
        public void DibujarCajaPixel(Graphics g, Rectangle rect, Color fondo, Color bordePrincipal, Color bordeSecundario, int escala = 3)
        {
            if (rect.Width <= escala * 4 || rect.Height <= escala * 4) return;

            // 1. Sombra dura desplazada (abajo y derecha)
            using (var brochaSombra = new SolidBrush(Color.FromArgb(140, 0, 0, 0)))
            {
                g.FillRectangle(brochaSombra, rect.X + escala * 2, rect.Y + escala * 2, rect.Width, rect.Height);
            }

            // 2. Fondo principal recortado en las esquinas
            using (var brochaFondo = new SolidBrush(fondo))
            {
                g.FillRectangle(brochaFondo, rect.X + escala, rect.Y + escala, rect.Width - escala * 2, rect.Height - escala * 2);
            }

            // 3. Borde exterior pixelado (sin esquinas para efecto retro)
            using (var brochaExt = new SolidBrush(bordePrincipal))
            {
                g.FillRectangle(brochaExt, rect.X + escala, rect.Y, rect.Width - escala * 2, escala); // Arriba
                g.FillRectangle(brochaExt, rect.X + escala, rect.Bottom - escala, rect.Width - escala * 2, escala); // Abajo
                g.FillRectangle(brochaExt, rect.X, rect.Y + escala, escala, rect.Height - escala * 2); // Izquierda
                g.FillRectangle(brochaExt, rect.Right - escala, rect.Y + escala, escala, rect.Height - escala * 2); // Derecha
            }

            // 4. Borde interior de contraste + píxeles de esquina
            using (var brochaInt = new SolidBrush(bordeSecundario))
            {
                g.FillRectangle(brochaInt, rect.X + escala * 2, rect.Y + escala, rect.Width - escala * 4, escala);
                g.FillRectangle(brochaInt, rect.X + escala * 2, rect.Bottom - escala * 2, rect.Width - escala * 4, escala);
                g.FillRectangle(brochaInt, rect.X + escala, rect.Y + escala * 2, escala, rect.Height - escala * 4);
                g.FillRectangle(brochaInt, rect.Right - escala * 2, rect.Y + escala * 2, escala, rect.Height - escala * 4);

                // Remaches en las 4 esquinas interiores
                g.FillRectangle(brochaInt, rect.X + escala, rect.Y + escala, escala, escala);
                g.FillRectangle(brochaInt, rect.Right - escala * 2, rect.Y + escala, escala, escala);
                g.FillRectangle(brochaInt, rect.X + escala, rect.Bottom - escala * 2, escala, escala);
                g.FillRectangle(brochaInt, rect.Right - escala * 2, rect.Bottom - escala * 2, escala, escala);
            }
        }

        /// <summary>
        /// Dibuja una barra de vida o energía dividida en bloques retro con brillo superior.
        /// </summary>
        public void DibujarBarraPixel(Graphics g, Rectangle rect, int actual, int maximo, Color colorLleno, Color colorVacio, Color borde)
        {
            if (maximo <= 0) maximo = 1;
            float porcentaje = Math.Max(0f, Math.Min(1f, (float)actual / maximo));

            using (var bBorde = new SolidBrush(borde))
            using (var bVacio = new SolidBrush(colorVacio))
            using (var bLleno = new SolidBrush(colorLleno))
            using (var bBrillo = new SolidBrush(Color.FromArgb(90, 255, 255, 255)))
            {
                // Marco exterior
                g.FillRectangle(bBorde, rect);
                Rectangle interior = new Rectangle(rect.X + 2, rect.Y + 2, Math.Max(0, rect.Width - 4), Math.Max(0, rect.Height - 4));
                g.FillRectangle(bVacio, interior);

                // Relleno proporcional
                int anchoLleno = (int)(interior.Width * porcentaje);
                if (anchoLleno > 0)
                {
                    g.FillRectangle(bLleno, interior.X, interior.Y, anchoLleno, interior.Height);
                    // Línea de brillo pixel art en la parte superior
                    g.FillRectangle(bBrillo, interior.X, interior.Y, anchoLleno, Math.Max(2, interior.Height / 4));
                }

                // Divisiones verticales estilo arcade cada 10 píxeles
                for (int x = interior.X + 10; x < interior.Right; x += 10)
                {
                    g.FillRectangle(bBorde, x, interior.Y, 2, interior.Height);
                }
            }
        }

        /// <summary>
        /// Convierte una matriz de texto en un Bitmap pixel art nítido ('.' = transparente).
        /// </summary>
        public static Bitmap CrearSprite(string[] filas, Dictionary<char, Color> paleta, int escala = 3)
        {
            int alto = filas.Length;
            int ancho = filas[0].Length;
            var bmp = new Bitmap(ancho * escala, alto * escala);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.Clear(Color.Transparent);

                for (int y = 0; y < alto; y++)
                {
                    string fila = filas[y];
                    for (int x = 0; x < fila.Length && x < ancho; x++)
                    {
                        char c = fila[x];
                        if (c == '.' || c == ' ') continue;
                        if (paleta.TryGetValue(c, out Color color))
                        {
                            using (var brocha = new SolidBrush(color))
                            {
                                g.FillRectangle(brocha, x * escala, y * escala, escala, escala);
                            }
                        }
                    }
                }
            }
            return bmp;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                TemaCambiado = null;
                Gigante.Dispose();
                Titulo.Dispose();
                Grande.Dispose();
                Normal.Dispose();
                Negrita.Dispose();
                Pequena.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
