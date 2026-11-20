using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace WinForms
{
    public sealed class VistaCombate : VistaJuego
    {
        const int AlturaSuelo = 68;

        Bitmap fondo;
        Tema temaDelFondo;

        static readonly string[] SpriteSlime1 = new[]
        {
            "....OOOOOOOO....",
            "..OOLLLLLLLLOO..",
            ".OLLCCCCCCCCLLO.",
            "OLCCWWCCCCCCCCLO",
            "OLCCWWCCCCCCCCLO",
            "OLCCCCCCCCCCCCLO",
            "OLCCOOCCCCOOCCLO",
            "OLCCOOCCCCOOCCLO",
            "OLCCCCCCCCCCCCLO",
            "OCCCCCCCCCCCCCCO",
            ".OSSSSSSSSSSSSO.",
            "..OOOOOOOOOOOO..",
            "................"
        };

        static readonly string[] SpriteSlime2 = new[]
        {
            "................",
            "....OOOOOOOO....",
            "..OOLLLLLLLLOO..",
            ".OLLCCCCCCCCLLO.",
            "OLCCWWCCCCCCCCLO",
            "OLCCCCCCCCCCCCLO",
            "OLCCOOCCCCOOCCLO",
            "OLCCOOCCCCOOCCLO",
            "OLCCCCCCCCCCCCLO",
            "OCCCCCCCCCCCCCCO",
            "OSSSSSSSSSSSSSSO",
            ".OOOOOOOOOOOOOO.",
            "................"
        };

        // Ataque ARMAMENTO: Tajo de espada en media luna (10x12)
        static readonly string[] SpriteTajoLuna = new[]
        {
            "..OOOO....",
            ".OLLLLOO..",
            "OLLCCCCLO.",
            ".OOCCCCCLO",
            "...OCCCCLO",
            "....OCCCCO",
            "....OCCCCO",
            "...OCCCCLO",
            ".OOCCCCCLO",
            "OLLCCCCLO.",
            ".OLLLLOO..",
            "..OOOO...."
        };

        // Ataque MANA Fases 1, 2 y 3
        static readonly string[] SpriteRomboChico = new[]
        {
            "..O..",
            ".OCO.",
            "OCWCO",
            ".OCO.",
            "..O.."
        };

        static readonly string[] SpriteRomboMedio = new[]
        {
            "...O...",
            "..OCO..",
            ".OCCCO.",
            "OCCWCCO",
            ".OCCCO.",
            "..OCO..",
            "...O..."
        };

        static readonly string[] SpriteRomboGrande = new[]
        {
            "....O....",
            "...OLO...",
            "..OLLLO..",
            ".OLLWLLO.",
            "OLLWWWLLO",
            ".OLLWLLO.",
            "..OLLLO..",
            "...OLO...",
            "....O...."
        };

        // Defensa MANA: Barrera mágica curva (5x11)
        static readonly string[] SpriteEscudoMana = new[]
        {
            "OO...",
            "OLO..",
            ".OLO.",
            ".OLLO",
            "..OLO",
            "..OLO",
            "..OLO",
            ".OLLO",
            ".OLO.",
            "OLO..",
            "OO..."
        };

        // Ataque HERRAMIENTAS: Mini-Robot con antenas y patas (12x12) - Cuadro 1 y Cuadro 2
        static readonly string[] SpriteRobot1 = new[]
        {
            "...O....O...",
            "....O..O....",
            "...OOOOOO...",
            "..OCCCCCCOO.",
            ".OCCCCCCCCCO",
            ".OCCLLCCCCCO",
            ".OCCCCCCCCCO",
            "..OSSSSSSOO.",
            "...OOOOOO...",
            "..OO....OO..",
            ".OOO....OO..",
            "............"
        };

        static readonly string[] SpriteRobot2 = new[]
        {
            "...O....O...",
            "....O..O....",
            "...OOOOOO...",
            "..OCCCCCCOO.",
            ".OCCCCCCCCCO",
            ".OCCLLCCCCCO",
            ".OCCCCCCCCCO",
            "..OSSSSSSOO.",
            "...OOOOOO...",
            "...OO..OO...",
            "..OO....OOO.",
            "............"
        };

        // Descarga eléctrica del Robot sobre el Slime (8x6)
        static readonly string[] SpriteChispaRobot = new[]
        {
            "OO......",
            "WLOO....",
            ".WWLOO..",
            "..OOWLO.",
            "....OWLO",
            "......OO"
        };

        // Defensa HERRAMIENTAS: Escudo deflector triangular (7x11)
        static readonly string[] SpriteEscudoTecnico = new[]
        {
            "OO.....",
            "OLO....",
            "OLLO...",
            "OCLLO..",
            "OCCLLO.",
            "OCCCWLO",
            "OCCLLO.",
            "OCLLO..",
            "OLLO...",
            "OLO....",
            "OO....."
        };

        // Contraataque Enemigo: Esfera de energía (8x8)
        static readonly string[] SpriteOrbe = new[]
        {
            "..OOOO..",
            ".OLLLLOO",
            "OLWWWWCO",
            "OLWWCCCO",
            "OLCCCCCO",
            "OCCCCSSO",
            ".OSSSSO.",
            "..OOOO.."
        };

        // Ataque FISICO: Onda de choque (10x10)
        static readonly string[] SpriteGolpe = new[]
        {
            "....OO....",
            ".OO.OO.OO.",
            ".OLLOLLOO.",
            "..OCCCCO..",
            "OOCCWWCCOO",
            "OOCCWWCCOO",
            "..OCCCCO..",
            ".OLLOLLOO.",
            ".OO.OO.OO.",
            "....OO...."
        };

        // Cofre de Victoria (16x12)
        static readonly string[] SpriteCofre = new[]
        {
            "..OOOOOOOOOOOO..",
            ".OLLLLLLLLLLLLO.",
            "OLLLLLLLLLLLLLLO",
            "BBBBBBBBBBBBBBBB",
            "BMMMMMBBBMBMMMMB",
            "BMMMMMBCCMBMMMMB",
            "BBBBBBBCCBBBBBBB",
            "BMMMMMMMMMMMMMMB",
            "BMMMMMMMMMMMMMMB",
            "BMMMMMMMMMMMMMMB",
            "BBBBBBBBBBBBBBBB",
            ".BBBBBBBBBBBBBB."
        };

        public VistaCombate(Juego juego, EstiloVisual estilo) : base(juego, estilo)
        {
        }

        protected override bool Animada
        {
            get { return true; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && fondo != null)
            {
                fondo.Dispose();
                fondo = null;
            }
            base.Dispose(disposing);
        }

        void AsegurarFondo(Size tamano)
        {
            if (fondo != null && fondo.Size == tamano && temaDelFondo == estilo.Tema) return;

            if (fondo != null) fondo.Dispose();
            fondo = new Bitmap(tamano.Width, tamano.Height);
            temaDelFondo = estilo.Tema;

            Color cieloArriba = C(Rol.CombateCieloArriba);
            Color cieloAbajo = C(Rol.CombateCieloAbajo);
            Color sueloColor = C(Rol.CombateSuelo);
            Color sueloBorde = ControlPaint.Light(sueloColor, 0.25f);
            Color sueloSombra = ControlPaint.Dark(sueloColor, 0.25f);

            using (Graphics lienzo = Graphics.FromImage(fondo))
            {
                lienzo.SmoothingMode = SmoothingMode.None;
                int altoCielo = tamano.Height - AlturaSuelo;

                int franjas = 4;
                int altoFranja = Math.Max(1, altoCielo / franjas);
                for (int i = 0; i < franjas; i++)
                {
                    float t = i / (float)(franjas - 1);
                    Color cFranja = Color.FromArgb(
                        (int)(cieloArriba.R + (cieloAbajo.R - cieloArriba.R) * t),
                        (int)(cieloArriba.G + (cieloAbajo.G - cieloArriba.G) * t),
                        (int)(cieloArriba.B + (cieloAbajo.B - cieloArriba.B) * t)
                    );
                    using (var b = new SolidBrush(cFranja))
                        lienzo.FillRectangle(b, 0, i * altoFranja, tamano.Width, altoFranja + 4);
                }

                using (var bEstrella = new SolidBrush(Color.FromArgb(120, 255, 255, 255)))
                {
                    for (int x = 24; x < tamano.Width; x += 64)
                    {
                        int y = ((x * 37) % Math.Max(20, altoCielo - 20)) + 10;
                        lienzo.FillRectangle(bEstrella, x, y, 4, 4);
                    }
                }

                int ySuelo = tamano.Height - AlturaSuelo;
                using (var bSuelo = new SolidBrush(sueloColor))
                using (var bBorde = new SolidBrush(sueloBorde))
                using (var bSombra = new SolidBrush(sueloSombra))
                {
                    lienzo.FillRectangle(bSuelo, 0, ySuelo, tamano.Width, AlturaSuelo);
                    lienzo.FillRectangle(bBorde, 0, ySuelo, tamano.Width, 4);
                    lienzo.FillRectangle(bSombra, 0, ySuelo + 4, tamano.Width, 4);

                    for (int x = 0; x < tamano.Width; x += 32)
                    {
                        lienzo.FillRectangle(bSombra, x, ySuelo + 16, 4, 4);
                        lienzo.FillRectangle(bSombra, x + 16, ySuelo + 36, 4, 4);
                        lienzo.FillRectangle(bBorde, x + 8, ySuelo + 52, 8, 4);
                    }
                }
            }
        }

        protected override void Dibujar(Graphics g)
        {
            Enemigo enemigo = juego.Combate.Enemigo;
            Color colorEnemigo = ColorEnemigo(enemigo);

            var escena = new Rectangle(40, 30, ClientSize.Width - 80, 330);
            Marco(g, escena, "MODO COMBATE", Rol.Peligro);

            var interior = new Rectangle(escena.X + 6, escena.Y + 32, escena.Width - 12, escena.Height - 38);
            AsegurarFondo(interior.Size);
            g.DrawImage(fondo, interior);

            DibujarLuchadores(g, escena, enemigo, colorEnemigo);
            Texto(g, "ENEMIGO " + enemigo.Efecto + " [" + enemigo.Fila + "," + enemigo.Columna + "]", estilo.Negrita, colorEnemigo, escena.Right - 310, escena.Y + 42);
            DibujarPanel(g, enemigo, colorEnemigo);

            var mensaje = new Rectangle(40, 548, ClientSize.Width - 80, 70);
            estilo.DibujarCajaPixel(g, mensaje, C(Rol.Superficie), C(Rol.TextoSuave), C(Rol.Acento), 2);
            TextoCaja(g, juego.MensajeCombate, estilo.Normal, Rol.Texto, new RectangleF(mensaje.X + 14, mensaje.Y + 10, mensaje.Width - 28, mensaje.Height - 16));
            if (juego.Fase == FaseCombate.Victoria) DibujarVictoria(g);
        }
        void DibujarLuchadores(Graphics g, Rectangle escena, Enemigo enemigo, Color colorEnemigo)
        {
            FaseCombate fase = juego.Fase;
            int anim = juego.AnimTick;
            int duracion = Math.Max(1, Juego.DuracionAnimacion);
            float progreso = Math.Max(0f, Math.Min(1f, anim / (float)duracion));
            string clase = juego.TipoPersonaje;
            float baseX = escena.X + 120;
            float baseY = escena.Y + 195;
            float enemigoX = escena.Right - 260;
            float distancia = enemigoX - baseX - 85;
            int vaiven = (int)(Math.Sin(juego.Tick * 0.3) * 3);
            float sacudida = (fase == FaseCombate.AnimDefensa && progreso > 0.6f) ? (float)Math.Sin(anim * 2.8) * 10 : 0;
            float impactoEnemigo = (fase == FaseCombate.AnimAtaque && progreso > 0.55f) ? (float)Math.Sin(anim * 3.2) * 12 : 0;
            if (fase == FaseCombate.AnimAtaque && clase == "FISICO")
            {
                Color colorDash = Color.FromArgb(110, 145, 195);
                if (progreso < 0.25f)
                {
                    float t = progreso / 0.25f;
                    DibujarSiluetaDash(g, baseX + t * distancia, baseY, 1.5f, colorDash, true);
                }
                else if (progreso < 0.72f)
                {
                    if (progreso < 0.42f || progreso > 0.60f)
                    {
                        DibujarLineasViento(g, baseX + distancia * 0.45f, baseY + 25);
                    }
                    DibujarCadete(g, baseX + distancia, baseY, 1.5f, ColorJugador, 0, true);
                    if (!enemigo.Derrotado && progreso > 0.35f && progreso < 0.65f)
                    {
                        DibujarSpriteConContorno(g, SpriteGolpe, enemigoX - 10, baseY + 12, 6, C(Rol.ClaseFisico));
                    }
                }
                else
                {
                    float t = (progreso - 0.72f) / 0.28f;
                    DibujarSiluetaDash(g, baseX + (1f - t) * distancia, baseY, 1.5f, colorDash, false);
                }
            }
            else
            {
                float avance = 0;
                bool poseEspecial = false;
                if (fase == FaseCombate.AnimAtaque)
                {
                    poseEspecial = (clase == "ARMAMENTO");
                    if (clase == "ARMAMENTO")
                        avance = (float)Math.Sin(progreso * Math.PI) * 90f;
                }
                else if (fase == FaseCombate.AnimDefensa && clase == "HERRAMIENTAS")
                {
                    // El ingeniero activa su visor iluminado y extiende el brazo para proyectar el escudo
                    poseEspecial = true;
                }
                DibujarCadete(g, baseX + avance - sacudida, baseY, 1.5f, ColorJugador, vaiven, poseEspecial);
                if (fase == FaseCombate.AnimAtaque && !enemigo.Derrotado)
                {
                    DibujarAtaquePorClase(g, clase, baseX + avance + 75, baseY + 20, enemigoX, progreso);
                }
            }
            if (enemigo.Derrotado) return;
            float ex = enemigoX + impactoEnemigo;
            float ey = escena.Y + 165 + (float)Math.Sin(juego.Tick * 0.25) * 5;
            DibujarSlimePixel(g, ex, ey, 9, colorEnemigo);
            // Contraataque del Slime + Escudos especiales según clase
            if (fase == FaseCombate.AnimDefensa)
            {
                if (clase == "MANA" && progreso > 0.35f)
                {
                    DibujarSpriteConContorno(g, SpriteEscudoMana, baseX + 68, baseY + 14, 6, Color.FromArgb(65, 220, 175));
                }
                else if (clase == "HERRAMIENTAS" && progreso > 0.30f)
                {
                    // Escudo deflector triangular rojo/granate frente al brazo del ingeniero
                    DibujarSpriteConContorno(g, SpriteEscudoTecnico, baseX + 82, baseY + 14, 6, Color.FromArgb(145, 55, 65));
                }
                float tBala = Math.Min(1f, progreso / 0.75f);
                float bx = ex - tBala * (ex - (baseX + 75));
                float by = ey + 45 - (float)Math.Sin(tBala * Math.PI) * 25;
                DibujarSpriteConContorno(g, SpriteOrbe, bx, by, 6, colorEnemigo);
            }
        }
        void DibujarLineasViento(Graphics g, float x, float y)
        {
            using (var bViento = new SolidBrush(Color.FromArgb(180, 90, 95, 110)))
            {
                for (int linea = 0; linea < 3; linea++)
                {
                    float ly = y + linea * 20;
                    for (int seg = 0; seg < 6; seg++)
                    {
                        float offset = (seg % 2 == 0) ? 0 : -4;
                        g.FillRectangle(bViento, x + seg * 16, ly + offset, 12, 4);
                    }
                }
            }
        }
        void DibujarAtaquePorClase(Graphics g, string clase, float origenX, float origenY, float enemigoX, float progreso)
        {
            if (clase == "ARMAMENTO")
            {
                float x = origenX + progreso * (enemigoX + 30 - origenX);
                DibujarSpriteConContorno(g, SpriteTajoLuna, x, origenY - 22, 7, Color.FromArgb(90, 210, 255));
            }
            else if (clase == "MANA")
            {
                Color colorCarga = Color.FromArgb(165, 175, 185);
                Color colorMagia = Color.FromArgb(65, 220, 175);
                if (progreso < 0.15f)
                    DibujarSpriteConContorno(g, SpriteRomboChico, origenX + 10, origenY + 8, 6, colorCarga);
                else if (progreso < 0.30f)
                    DibujarSpriteConContorno(g, SpriteRomboMedio, origenX + 6, origenY + 2, 6, colorCarga);
                else if (progreso < 0.45f)
                    DibujarSpriteConContorno(g, SpriteRomboGrande, origenX, origenY - 4, 6, colorMagia);
                else
                {
                    float tVuelo = (progreso - 0.45f) / 0.55f;
                    float x = origenX + tVuelo * (enemigoX + 30 - origenX);
                    DibujarSpriteConContorno(g, SpriteRomboGrande, x, origenY - 4, 6, colorMagia);
                }
            }
            else if (clase == "HERRAMIENTAS")
            {
                // Animación exacta de tu referencia:
                // 1) El mini-robot camina desde el ingeniero hasta el slime (0% a 35%)
                // 2) Descarga una onda eléctrica sobre el slime (35% a 65%)
                // 3) Regresa caminando hasta el ingeniero (65% a 100%)
                float destinoRobot = enemigoX - 75;
                float robotX;
                if (progreso < 0.35f)
                {
                    float t = progreso / 0.35f;
                    robotX = origenX + t * (destinoRobot - origenX);
                }
                else if (progreso < 0.65f)
                {
                    robotX = destinoRobot;
                }
                else
                {
                    float t = (progreso - 0.65f) / 0.35f;
                    robotX = destinoRobot - t * (destinoRobot - origenX);
                }
                string[] spriteRobot = ((juego.Tick / 4) % 2 == 0) ? SpriteRobot1 : SpriteRobot2;
                Color colorRobot = Color.FromArgb(75, 55, 85); // Cuerpo oscuro tecnológico como en tu captura
                DibujarSpriteConContorno(g, spriteRobot, robotX, origenY + 12, 6, colorRobot);
                // Descarga eléctrica sobre el slime cuando el robot está frente a él
                if (progreso >= 0.38f && progreso <= 0.62f)
                {
                    DibujarSpriteConContorno(g, SpriteChispaRobot, robotX + 56, origenY + 6, 6, Color.FromArgb(75, 225, 185));
                }
            }
        }
        void DibujarSpriteConContorno(Graphics g, string[] sprite, float x, float y, int px, Color colorBase)
        {
            Color contorno = Color.FromArgb(20, 18, 24);
            Color luz = ControlPaint.Light(colorBase, 0.4f);
            Color sombra = ControlPaint.Dark(colorBase, 0.3f);
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
                        case 'L': colorPixel = luz; break;
                        case 'S': colorPixel = sombra; break;
                        case 'W': colorPixel = Color.White; break;
                        default: colorPixel = colorBase; break;
                    }
                    using (var brocha = new SolidBrush(colorPixel))
                        g.FillRectangle(brocha, x + col * px, y + fila * px, px, px);
                }
            }
        }
        void DibujarSlimePixel(Graphics g, float x, float y, int px, Color colorBase)
        {
            string[] sprite = ((juego.Tick / 12) % 2 == 0) ? SpriteSlime1 : SpriteSlime2;
            Color contorno = Color.FromArgb(20, 18, 24);
            Color brillo = ControlPaint.Light(colorBase, 0.35f);
            Color sombra = ControlPaint.Dark(colorBase, 0.35f);
            using (var bSombra = new SolidBrush(Color.FromArgb(110, 0, 0, 0)))
            {
                g.FillRectangle(bSombra, x + px * 2, y + 11 * px + 4, px * 12, px * 2);
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
                        case 'L': colorPixel = brillo; break;
                        case 'S': colorPixel = sombra; break;
                        case 'W': colorPixel = Color.White; break;
                        default: colorPixel = colorBase; break;
                    }
                    using (var brocha = new SolidBrush(colorPixel))
                        g.FillRectangle(brocha, x + col * px, y + fila * px, px, px);
                }
            }
        }
        void DibujarCofrePixel(Graphics g, float x, float y, int px)
        {
            Color oro = C(Rol.ObjetoOro);
            Color oroBrillo = ControlPaint.Light(oro, 0.4f);
            Color madera = C(Rol.ObjetoMaderaOscura);
            Color cerrojo = C(Rol.Aviso);
            for (int fila = 0; fila < SpriteCofre.Length; fila++)
            {
                string linea = SpriteCofre[fila];
                for (int col = 0; col < linea.Length; col++)
                {
                    char c = linea[col];
                    if (c == '.') continue;
                    Color colorPixel;
                    switch (c)
                    {
                        case 'L': colorPixel = oroBrillo; break;
                        case 'O': colorPixel = oro; break;
                        case 'M': colorPixel = madera; break;
                        case 'C': colorPixel = cerrojo; break;
                        default: colorPixel = Color.FromArgb(30, 30, 35); break;
                    }
                    using (var brocha = new SolidBrush(colorPixel))
                        g.FillRectangle(brocha, x + col * px, y + fila * px, px, px);
                }
            }
        }
        void DibujarPanel(Graphics g, Enemigo enemigo, Color colorEnemigo)
        {
            Cadete cadete = juego.Cadete;
            var panel = new Rectangle(40, 375, ClientSize.Width - 80, 160);
            Marco(g, panel, "", Rol.Acento);
            int porcentajeEnemigo = enemigo.Vida * 100 / Enemigo.VidaMaxima;
            Texto(g, "☠ ENEMIGO " + enemigo.Efecto + "  " + enemigo.Vida + "/" + Enemigo.VidaMaxima, estilo.Normal, Rol.Texto, panel.X + 16, panel.Y + 12);
            Barra(g, panel.X + 16, panel.Y + 38, panel.Width - 32, 20, porcentajeEnemigo, colorEnemigo, "Enemigo");
            Texto(g, "CADETE " + juego.TipoPersonaje, estilo.Normal, Rol.Texto, panel.X + 16, panel.Y + 66);
            Barra(g, panel.X + 16, panel.Y + 90, panel.Width / 2 - 24, 20, cadete.Vida, Rol.Peligro, "Vida ");
            Barra(g, panel.X + panel.Width / 2 + 8, panel.Y + 90, panel.Width / 2 - 24, 20, cadete.Poder, Rol.Aviso, "Poder");
            string instruccion;
            Rol rolInstruccion = Rol.Aviso;
            switch (juego.Fase)
            {
                case FaseCombate.Jugador:
                    instruccion = "TU TURNO: [A] atacar   [H] huir";
                    break;
                case FaseCombate.Enemigo:
                    instruccion = "TURNO DEL ENEMIGO: presiona [D] para defenderte";
                    break;
                case FaseCombate.Victoria:
                    instruccion = "ENEMIGO DERROTADO · ENTER para continuar";
                    rolInstruccion = Rol.Exito;
                    break;
                case FaseCombate.Derrota:
                    instruccion = "EL CADETE HA CAÍDO · ENTER para continuar";
                    rolInstruccion = Rol.Peligro;
                    break;
                default:
                    instruccion = "...";
                    break;
            }
            Texto(g, instruccion, estilo.Negrita, rolInstruccion, panel.X + 16, panel.Y + 122);
        }
        void DibujarVictoria(Graphics g)
        {
            Cadete cadete = juego.Cadete;
            using (var velo = new SolidBrush(C(Rol.Velo)))
                g.FillRectangle(velo, ClientRectangle);
            var r = new Rectangle(ClientSize.Width / 2 - 300, 70, 600, 500);
            Marco(g, r, "¡VICTORIA!", Rol.Exito);
            float rebote = (float)Math.Abs(Math.Sin(juego.Tick * 0.16)) * 14;
            int cx = r.X + r.Width / 2;
            DibujarCofrePixel(g, cx - 64, r.Y + 72 - rebote, 8);
            Texto(g, "ENEMIGO DERROTADO · La zona ha quedado despejada.", estilo.Normal, Rol.Texto, r.X + 30, r.Y + 190);
            if (juego.ObjetivoMision)
            {
                Texto(g, "Objetivo de misión cumplido.", estilo.Negrita, Rol.Exito, r.X + 30, r.Y + 225);
                Texto(g, "Regresa a la Base y usa el Terminal para reclamar la recompensa.", estilo.Pequena, Rol.Texto, r.X + 30, r.Y + 252);
            }
            else
            {
                Texto(g, "Fragmento de Poder obtenido: +" + Combate.PoderPorEnemigo + "  (Poder " + cadete.Poder + "/100)", estilo.Negrita, Rol.Aviso, r.X + 30, r.Y + 225);
            }
            var alerta = new Rectangle(r.X + 30, r.Y + 290, r.Width - 60, 150);
            estilo.DibujarCajaPixel(g, alerta, C(Rol.AlertaFondo), C(Rol.Peligro), C(Rol.Aviso), 3);
            TextoCentro(g, "⚠ ALERTA CRÍTICA ⚠", estilo.Grande, Rol.Peligro, new RectangleF(alerta.X, alerta.Y + 8, alerta.Width, 32));
            TextoCentro(g, "INTERFERENCIA: IRIS", estilo.Negrita, Rol.Peligro, new RectangleF(alerta.X, alerta.Y + 42, alerta.Width, 26));
            TextoCentro(g, "ESTABILIDAD: " + cadete.Estabilidad + "%", estilo.Normal, Rol.Texto, new RectangleF(alerta.X, alerta.Y + 72, alerta.Width, 26));
            TextoCentro(g, "IRIS HA INTERRUMPIDO LOS SISTEMAS DE NEXUS", estilo.Pequena, Rol.Texto, new RectangleF(alerta.X, alerta.Y + 100, alerta.Width, 20));
            TextoCentro(g, "NEXUS recomienda desconexión inmediata.", estilo.Pequena, Rol.Texto, new RectangleF(alerta.X, alerta.Y + 120, alerta.Width, 20));
            TextoCentro(g, "ENTER para continuar", estilo.Normal, Rol.TextoSuave, new RectangleF(r.X, r.Bottom - 40, r.Width, 28));
        }
    }
}