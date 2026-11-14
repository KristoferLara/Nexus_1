using System;

namespace WinForms
{
    public enum Pantalla { Titulo, Registro, Seleccion, Menu, Capacidades, Manual, Nexo, Base, Genesis, Combate, Terminal, DetalleMision, Fin }

    public enum FaseCombate { Jugador, AnimAtaque, Enemigo, AnimDefensa, Victoria, Derrota }

    public class Cadete
    {
        public string Nombre { get; }
        public int Energia { get; private set; } = 100;
        public int Estabilidad { get; private set; } = 100;
        public int Vida { get; private set; } = 100;
        public int Poder { get; private set; }
        public int Experiencia { get; private set; }
        public int Recursos { get; private set; }
        public int TurnosCapacidad { get; private set; }
        public int Nivel => Experiencia / 100 + 1;
        public bool CapacidadActiva => TurnosCapacidad > 0;

        public Cadete(string nombre)
        {
            Nombre = nombre;
        }

        public void RecuperarVida(int c) { Vida = Math.Min(100, Vida + c); }
        public void RecibirDanio(int c) { Vida = Math.Max(0, Vida - c); }
        public void GanarPoder(int c) { Poder = Math.Min(100, Poder + c); }
        public void ConsumirPoder(int c) { Poder = Math.Max(0, Poder - c); }
        public void GanarExperiencia(int c) { Experiencia += c; }
        public void RecolectarRecurso() { Recursos++; }
        public void ActivarCapacidad(int turnos) { TurnosCapacidad = turnos; }

        public bool DescontarTurnoCapacidad()
        {
            if (TurnosCapacidad <= 0) return false;
            TurnosCapacidad--;
            return TurnosCapacidad == 0;
        }
    }

    public class Enemigo
    {
        public const int VidaMaxima = 25;
        public int Fila { get; }
        public int Columna { get; }
        public string Efecto { get; }
        public int Vida { get; private set; } = VidaMaxima;
        public bool Derrotado => Vida <= 0;

        public Enemigo(int fila, int columna, string efecto)
        {
            Fila = fila;
            Columna = columna;
            Efecto = efecto;
        }

        public void RecibirDanio(int d) { Vida = Math.Max(0, Vida - d); }
    }

    public class Mision
    {
        public string Nombre { get; }
        public string Descripcion { get; }
        public string TipoEnemigo { get; }
        public string Consejo { get; }
        public int RecompensaPoder { get; }
        public int RecompensaXp { get; }
        public bool ObjetivoCumplido { get; private set; }
        public bool Completada { get; private set; }

        public Mision(string nombre, string descripcion, string tipo, string consejo, int poder, int xp)
        {
            Nombre = nombre;
            Descripcion = descripcion;
            TipoEnemigo = tipo;
            Consejo = consejo;
            RecompensaPoder = poder;
            RecompensaXp = xp;
        }

        public void MarcarObjetivo() { ObjetivoCumplido = true; }
        public void MarcarCompletada() { Completada = true; }
    }

    public class Combate
    {
        public const int DanioJugador = 12;
        public const int DanioEnemigo = 10;
        public const int PoderPorEnemigo = 20;
        public const int DanioPorHuir = 10;

        public Cadete Jugador { get; }
        public Enemigo Enemigo { get; }
        bool efectosActivos;

        public Combate(Cadete jugador, Enemigo enemigo)
        {
            Jugador = jugador;
            Enemigo = enemigo;
        }

        public bool Atacar(out int aplicado, bool otorgarPoder)
        {
            int d = DanioJugador;
            if (efectosActivos && Enemigo.Efecto == "LIQUIDO") d = (int)(d * 0.70);
            else if (efectosActivos && Enemigo.Efecto == "VENENO") d = (int)(d * 0.95);
            aplicado = d;
            Enemigo.RecibirDanio(d);
            if (!Enemigo.Derrotado) return false;
            if (otorgarPoder) Jugador.GanarPoder(PoderPorEnemigo);
            return true;
        }

        public int Contraatacar()
        {
            int d = DanioEnemigo;
            if (efectosActivos && Enemigo.Efecto == "FUEGO") d = (int)(d * 1.15);
            Jugador.RecibirDanio(d);
            if (efectosActivos && Enemigo.Efecto == "VENENO")
            {
                Jugador.RecibirDanio(5);
                d += 5;
            }
            efectosActivos = true;
            return d;
        }

        public void Huir() { Jugador.RecibirDanio(DanioPorHuir); }
        public bool JugadorDerrotado => Jugador.Vida <= 0;
    }
}