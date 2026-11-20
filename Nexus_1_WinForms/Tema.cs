using System;
using System.Collections.Generic;
using System.Drawing;

namespace WinForms
{
    public enum Rol
    {
        Fondo,
        Superficie,
        SuperficieSeleccion,
        SuperficieHueco,
        Texto,
        TextoSuave,
        TextoSobreAcento,
        Acento,
        Aviso,
        Exito,
        Peligro,
        Especial,
        Velo,
        AvisoFondo,
        AlertaFondo,
        MapaMuro,
        MapaSueloBase,
        MapaSueloGenesis,
        MapaNiebla,
        MapaNieblaTexto,
        MapaTerreno,
        ObjetoCama,
        ObjetoSabana,
        ObjetoPantalla,
        ObjetoMetal,
        ObjetoMadera,
        ObjetoMaderaOscura,
        ObjetoOro,
        EnemigoVeneno,
        EnemigoFuego,
        EnemigoLiquido,
        EnemigoOjo,
        EnemigoPupila,
        ClaseFisico,
        ClaseArmamento,
        ClaseMana,
        ClaseHerramientas,
        CadetePiel,
        CadetePiernas,
        CombateCieloArriba,
        CombateCieloAbajo,
        CombateSuelo
    }

    public sealed class Tema
    {
        readonly Color[] colores;

        public string Nombre { get; }

        Tema(string nombre, Dictionary<Rol, Color> definicion)
        {
            Nombre = nombre;
            var roles = (Rol[])Enum.GetValues(typeof(Rol));
            colores = new Color[roles.Length];
            foreach (Rol rol in roles)
            {
                Color color;
                if (!definicion.TryGetValue(rol, out color))
                    throw new ArgumentException("El tema " + nombre + " no define el rol " + rol);
                colores[(int)rol] = color;
            }
        }

        public Color this[Rol rol]
        {
            get { return colores[(int)rol]; }
        }

        public Color Con(Rol rol, int alfa)
        {
            return Color.FromArgb(alfa, colores[(int)rol]);
        }

        public static readonly Tema Oscuro = new Tema("Oscuro", new Dictionary<Rol, Color>
        {
            { Rol.Fondo, Color.FromArgb(12, 14, 24) },
            { Rol.Superficie, Color.FromArgb(22, 26, 42) },
            { Rol.SuperficieSeleccion, Color.FromArgb(50, 60, 100) },
            { Rol.SuperficieHueco, Color.FromArgb(40, 44, 66) },
            { Rol.Texto, Color.FromArgb(235, 238, 248) },
            { Rol.TextoSuave, Color.FromArgb(130, 138, 160) },
            { Rol.TextoSobreAcento, Color.FromArgb(12, 14, 24) },
            { Rol.Acento, Color.FromArgb(0, 205, 225) },
            { Rol.Aviso, Color.FromArgb(245, 205, 70) },
            { Rol.Exito, Color.FromArgb(70, 210, 120) },
            { Rol.Peligro, Color.FromArgb(230, 70, 80) },
            { Rol.Especial, Color.FromArgb(190, 90, 220) },
            { Rol.Velo, Color.FromArgb(170, 0, 0, 0) },
            { Rol.AvisoFondo, Color.FromArgb(235, 10, 12, 20) },
            { Rol.AlertaFondo, Color.FromArgb(60, 10, 14) },
            { Rol.MapaMuro, Color.FromArgb(55, 60, 85) },
            { Rol.MapaSueloBase, Color.FromArgb(28, 32, 52) },
            { Rol.MapaSueloGenesis, Color.FromArgb(30, 38, 56) },
            { Rol.MapaNiebla, Color.FromArgb(18, 20, 32) },
            { Rol.MapaNieblaTexto, Color.FromArgb(70, 76, 100) },
            { Rol.MapaTerreno, Color.FromArgb(34, 139, 34) },
            { Rol.ObjetoCama, Color.FromArgb(70, 130, 180) },
            { Rol.ObjetoSabana, Color.FromArgb(255, 255, 255) },
            { Rol.ObjetoPantalla, Color.FromArgb(20, 90, 100) },
            { Rol.ObjetoMetal, Color.FromArgb(128, 128, 128) },
            { Rol.ObjetoMadera, Color.FromArgb(205, 133, 63) },
            { Rol.ObjetoMaderaOscura, Color.FromArgb(139, 69, 19) },
            { Rol.ObjetoOro, Color.FromArgb(218, 165, 32) },
            { Rol.EnemigoVeneno, Color.FromArgb(50, 205, 50) },
            { Rol.EnemigoFuego, Color.FromArgb(255, 69, 0) },
            { Rol.EnemigoLiquido, Color.FromArgb(0, 191, 255) },
            { Rol.EnemigoOjo, Color.FromArgb(255, 255, 255) },
            { Rol.EnemigoPupila, Color.FromArgb(0, 0, 0) },
            { Rol.ClaseFisico, Color.FromArgb(255, 69, 0) },
            { Rol.ClaseArmamento, Color.FromArgb(192, 192, 192) },
            { Rol.ClaseMana, Color.FromArgb(186, 85, 211) },
            { Rol.ClaseHerramientas, Color.FromArgb(173, 255, 47) },
            { Rol.CadetePiel, Color.FromArgb(235, 205, 170) },
            { Rol.CadetePiernas, Color.FromArgb(60, 70, 110) },
            { Rol.CombateCieloArriba, Color.FromArgb(30, 36, 66) },
            { Rol.CombateCieloAbajo, Color.FromArgb(14, 16, 28) },
            { Rol.CombateSuelo, Color.FromArgb(40, 46, 70) }
        });

        public static readonly Tema Claro = new Tema("Claro", new Dictionary<Rol, Color>
        {
            { Rol.Fondo, Color.FromArgb(236, 239, 246) },
            { Rol.Superficie, Color.FromArgb(252, 253, 255) },
            { Rol.SuperficieSeleccion, Color.FromArgb(205, 218, 245) },
            { Rol.SuperficieHueco, Color.FromArgb(208, 214, 228) },
            { Rol.Texto, Color.FromArgb(24, 28, 44) },
            { Rol.TextoSuave, Color.FromArgb(100, 108, 130) },
            { Rol.TextoSobreAcento, Color.FromArgb(255, 255, 255) },
            { Rol.Acento, Color.FromArgb(0, 130, 160) },
            { Rol.Aviso, Color.FromArgb(190, 120, 0) },
            { Rol.Exito, Color.FromArgb(30, 150, 80) },
            { Rol.Peligro, Color.FromArgb(200, 40, 55) },
            { Rol.Especial, Color.FromArgb(140, 60, 190) },
            { Rol.Velo, Color.FromArgb(170, 240, 242, 250) },
            { Rol.AvisoFondo, Color.FromArgb(240, 255, 255, 255) },
            { Rol.AlertaFondo, Color.FromArgb(255, 220, 224) },
            { Rol.MapaMuro, Color.FromArgb(150, 158, 185) },
            { Rol.MapaSueloBase, Color.FromArgb(214, 220, 236) },
            { Rol.MapaSueloGenesis, Color.FromArgb(205, 222, 236) },
            { Rol.MapaNiebla, Color.FromArgb(176, 182, 200) },
            { Rol.MapaNieblaTexto, Color.FromArgb(120, 128, 150) },
            { Rol.MapaTerreno, Color.FromArgb(40, 130, 60) },
            { Rol.ObjetoCama, Color.FromArgb(70, 130, 180) },
            { Rol.ObjetoSabana, Color.FromArgb(255, 255, 255) },
            { Rol.ObjetoPantalla, Color.FromArgb(30, 90, 110) },
            { Rol.ObjetoMetal, Color.FromArgb(110, 110, 120) },
            { Rol.ObjetoMadera, Color.FromArgb(190, 130, 70) },
            { Rol.ObjetoMaderaOscura, Color.FromArgb(120, 70, 30) },
            { Rol.ObjetoOro, Color.FromArgb(210, 160, 30) },
            { Rol.EnemigoVeneno, Color.FromArgb(60, 170, 60) },
            { Rol.EnemigoFuego, Color.FromArgb(235, 90, 30) },
            { Rol.EnemigoLiquido, Color.FromArgb(40, 150, 220) },
            { Rol.EnemigoOjo, Color.FromArgb(255, 255, 255) },
            { Rol.EnemigoPupila, Color.FromArgb(20, 20, 30) },
            { Rol.ClaseFisico, Color.FromArgb(220, 80, 40) },
            { Rol.ClaseArmamento, Color.FromArgb(120, 125, 140) },
            { Rol.ClaseMana, Color.FromArgb(160, 70, 190) },
            { Rol.ClaseHerramientas, Color.FromArgb(110, 170, 30) },
            { Rol.CadetePiel, Color.FromArgb(235, 205, 170) },
            { Rol.CadetePiernas, Color.FromArgb(70, 80, 130) },
            { Rol.CombateCieloArriba, Color.FromArgb(170, 200, 240) },
            { Rol.CombateCieloAbajo, Color.FromArgb(225, 235, 250) },
            { Rol.CombateSuelo, Color.FromArgb(160, 170, 195) }
        });

        public static readonly Tema Fosforo = new Tema("Fosforo", new Dictionary<Rol, Color>
        {
            { Rol.Fondo, Color.FromArgb(4, 12, 6) },
            { Rol.Superficie, Color.FromArgb(8, 24, 12) },
            { Rol.SuperficieSeleccion, Color.FromArgb(16, 56, 28) },
            { Rol.SuperficieHueco, Color.FromArgb(12, 36, 18) },
            { Rol.Texto, Color.FromArgb(150, 255, 170) },
            { Rol.TextoSuave, Color.FromArgb(70, 150, 90) },
            { Rol.TextoSobreAcento, Color.FromArgb(4, 12, 6) },
            { Rol.Acento, Color.FromArgb(80, 255, 130) },
            { Rol.Aviso, Color.FromArgb(200, 255, 90) },
            { Rol.Exito, Color.FromArgb(60, 220, 100) },
            { Rol.Peligro, Color.FromArgb(255, 110, 90) },
            { Rol.Especial, Color.FromArgb(120, 255, 200) },
            { Rol.Velo, Color.FromArgb(170, 0, 8, 2) },
            { Rol.AvisoFondo, Color.FromArgb(235, 2, 10, 4) },
            { Rol.AlertaFondo, Color.FromArgb(50, 12, 8) },
            { Rol.MapaMuro, Color.FromArgb(22, 60, 32) },
            { Rol.MapaSueloBase, Color.FromArgb(10, 30, 16) },
            { Rol.MapaSueloGenesis, Color.FromArgb(12, 36, 18) },
            { Rol.MapaNiebla, Color.FromArgb(6, 18, 10) },
            { Rol.MapaNieblaTexto, Color.FromArgb(30, 90, 48) },
            { Rol.MapaTerreno, Color.FromArgb(30, 160, 70) },
            { Rol.ObjetoCama, Color.FromArgb(40, 140, 100) },
            { Rol.ObjetoSabana, Color.FromArgb(200, 255, 210) },
            { Rol.ObjetoPantalla, Color.FromArgb(10, 70, 40) },
            { Rol.ObjetoMetal, Color.FromArgb(90, 140, 100) },
            { Rol.ObjetoMadera, Color.FromArgb(120, 170, 60) },
            { Rol.ObjetoMaderaOscura, Color.FromArgb(60, 100, 30) },
            { Rol.ObjetoOro, Color.FromArgb(190, 230, 80) },
            { Rol.EnemigoVeneno, Color.FromArgb(120, 255, 60) },
            { Rol.EnemigoFuego, Color.FromArgb(255, 160, 60) },
            { Rol.EnemigoLiquido, Color.FromArgb(60, 220, 220) },
            { Rol.EnemigoOjo, Color.FromArgb(230, 255, 230) },
            { Rol.EnemigoPupila, Color.FromArgb(4, 12, 6) },
            { Rol.ClaseFisico, Color.FromArgb(255, 150, 80) },
            { Rol.ClaseArmamento, Color.FromArgb(190, 220, 200) },
            { Rol.ClaseMana, Color.FromArgb(150, 230, 255) },
            { Rol.ClaseHerramientas, Color.FromArgb(200, 255, 90) },
            { Rol.CadetePiel, Color.FromArgb(190, 230, 170) },
            { Rol.CadetePiernas, Color.FromArgb(20, 80, 45) },
            { Rol.CombateCieloArriba, Color.FromArgb(10, 40, 24) },
            { Rol.CombateCieloAbajo, Color.FromArgb(4, 14, 8) },
            { Rol.CombateSuelo, Color.FromArgb(14, 50, 28) }
        });

        public static readonly IReadOnlyList<Tema> Todos = new Tema[] { Oscuro, Claro, Fosforo };

        public static Tema Siguiente(Tema actual)
        {
            int indice = 0;
            for (int i = 0; i < Todos.Count; i++)
            {
                if (Todos[i] == actual)
                {
                    indice = i;
                    break;
                }
            }
            return Todos[(indice + 1) % Todos.Count];
        }
    }
}