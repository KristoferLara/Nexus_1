using System;
using System.Text;
using System.Threading;

class Program
{
    const int UmbralBajo = 30;

    const int CostoDron = 5;
    const int EnergiaMinimaDron = 5;

    const int CostoPulso = 15;
    const int EnergiaMinimaPulso = 15;
    const int DuracionBloqueoIris = 3;

    const int EstabilidadMinimaDesconexion = 15;

    // ===== SISTEMA DE CLASES DE CADETE =====
    // Costos de las acciones especiales por clase
    const int CostoAtaqueFisico = 10;
    const int CostoAtaqueFuerte = 25;
    const int CostoHabilidadMana = 20;

    // El cadete de HERRAMIENTAS usa el dron y el PEM más barato (ventaja de clase)
    const int CostoDronHerramientas = 3;
    const int CostoPulsoHerramientas = 10;

    // ===== SISTEMA DE PODER =====
    const int UmbralPoderHabilidad = 50;       // Poder necesario para activar la capacidad especial
    const int DuracionCapacidadEspecial = 3;   // Turnos que dura el efecto activo

    static Random rng = new Random();
    static event Action<int> AnomaliaDetectada;

    class EstadoJuego
    {
        public string Nombre;
        public string Realidad;
        public int Energia;
        public int Estabilidad;
        public bool TieneDetector = true;
        public bool TieneDron = false;
        public bool TienePEM = false;
        public bool PulsoActivo = false;
        public bool Conectado = true;
        public int TurnosBloqueoIris = 0;
        public bool AnomaliaLocalizada = false;
        public string UbicacionAnomalia = "Desconocida";

        // ===== Sistema de clases =====
        public string TipoPersonaje = "";   // "FISICO", "ARMAMENTO", "MANA" o "HERRAMIENTAS"
        public bool TieneArma = false;      // Solo el cadete de ARMAS empieza con esto en true

        // ===== Vida y Poder =====
        public int Vida = 100;
        public int Poder = 0;
        public int RecursosRecolectados = 0;
        public bool CapacidadActiva = false;
        public int TurnosCapacidadActiva = 0;

        // ===== Exploración territorial =====
        public int FilaJugador;
        public int ColumnaJugador;
        public int DistanciaRecorrida = 0;
        public int MetrosPorCasilla = 10;
        public int FilaDron = -1;
        public int ColumnaDron = -1;
        public bool DronDesplegado = false;

        // ===== MAPA GENESIS =====
        public char[,] MapaGenesis =
        {
            { '|', '|', '|', '|', '|', '|', '|', '|', '|', '|', '|' },
            { '|', '⌂', '·', '·', '·', '◆', '·', '·', '·', '·', '|' },
            { '|', '·', '·', '·', '·', '·', '·', '◆', '·', '·', '|' },
            { '|', '·', '·', '·', '⚠', '·', '·', '·', '·', '·', '|' },
            { '|', '·', '◆', '·', '·', '·', '·', '·', '·', '·', '|' },
            { '|', '·', '·', '·', '·', '◆', '·', '·', '·', '·', '|' },
            { '|', '·', '·', '·', '·', '·', '·', '·', '·', '·', '|' },
            { '|', '|', '|', '|', '|', '|', '|', '|', '|', '|', '|' }
        };

        public bool[,] ZonasExploradasGenesis = new bool[8, 11];

        public bool PosicionGenesisInicializada = false;

        // ===== ENEMIGOS GENESIS =====
        public bool[,] EnemigosGenesis = new bool[8, 11];

        public int[,] VidaEnemigosGenesis = new int[8, 11];
    }

    static void Main()
    {

        Console.CursorVisible = false;
        Console.OutputEncoding = Encoding.UTF8;
        Console.Clear();

        // El diseño de tu logo
        string[] logo = {
    "╔═════════════════════════════════════════════╗",
    "║  ███╗   ██╗███████╗██╗  ██╗                 ║",
    "║  ████╗  ██║██╔════╝╚██╗██╔╝  TRAINING       ║",
    "║  ██╔██╗ ██║█████╗   ╚███╔╝   SYSTEM         ║",
    "║  ██║╚██╗██║██╔══╝   ██╔██╗                  ║",
    "║  ██║ ╚████║███████╗██╔╝ ██╗                 ║",
    "╚═════════════════════════════════════════════╝"
};

        // Notas rápidas en ráfaga (Estilo Glitch)
        Console.ForegroundColor = ConsoleColor.Cyan;
        DibujarLogoPlano(logo, 2, 2);
        Console.Beep(587, 100); Thread.Sleep(30); Console.Beep(587, 100); Thread.Sleep(30); Console.Clear();

        // Salto 2
        Console.ForegroundColor = ConsoleColor.Blue;
        DibujarLogoPlano(logo, 32, 14);
        Console.Beep(1174, 150); Thread.Sleep(50); Console.Clear();

        // Salto 3
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        DibujarLogoPlano(logo, 5, 12);
        Console.Beep(880, 200); Thread.Sleep(50); Console.Clear();

        // Golpe final en el centro
        int centerX = 15; int centerY = 7;
        Console.ForegroundColor = ConsoleColor.Cyan;
        DibujarLogoPlano(logo, centerX, centerY);
        Console.Beep(830, 150); Thread.Sleep(30);
        Console.Beep(784, 150); Thread.Sleep(30);
        Console.Beep(698, 300);

        // Mensaje de espera final
        Console.ResetColor();
        Console.SetCursorPosition(centerX, centerY + 9);
        Console.WriteLine("SISTEMA LISTO. Presione una ENTER...");
        Console.ReadLine();


        // ===================================================
        // MÉTODO AUXILIAR PARA DIBUJAR
        // ===================================================
        static void DibujarLogoPlano(string[] logo, int x, int y)
        {
            for (int i = 0; i < logo.Length; i++)
            {
                Console.SetCursorPosition(x, y + i);
                Console.Write(logo[i]);
            }
        }

        Console.Clear();

        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.WriteLine("                                    ╔════════════════════════════════════════╗");
        Console.WriteLine("                                    ║         REGISTRO DE CADETE  ✍(◔◡◔)  ║");
        Console.WriteLine("                                    ╚════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
        string nombre = RegistrarNombre();
        Console.Clear();

        string contraseña = RegistrarContraseña();
        Console.Clear();

        string realidad = RegistrarRealidad();
        Console.Clear();

        int energia = 100;
        int estabilidad = 100;

        bool autorizado = contraseña == "k-dete";

        if (autorizado)
        {
            var estado = new EstadoJuego
            {
                Nombre = nombre,
                Realidad = realidad,
                Energia = energia,
                Estabilidad = estabilidad
            };

            estado.TipoPersonaje = "HERRAMIENTAS";
            estado.TieneArma = true;

            MostrarAutorizacion(estado);
            AnomaliaDetectada += DispararAlertaIris;
            EjecutarMision(estado);
        }


        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("========================================");
        Console.WriteLine("SESION FINALIZADA (╥﹏╥)");
        Console.WriteLine("========================================");
        Console.ResetColor();
    }

    static string RegistrarNombre()
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("           Como debo llamarte: ");
            Console.ResetColor();
            string nombre = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                Console.WriteLine("Error: El nombre no puede estar vacio.");
            }
            else
            {
                return nombre;
            }
        }
    }

    static string RegistrarContraseña()
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine();
            Console.WriteLine("               Contraseña de acceso:");
            Console.ResetColor();
            string contraseña = Console.ReadLine();
            if (contraseña == "k-dete")
            {
                return contraseña;
            }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("NEXUS: Contraseña incorrecta");
            Console.ResetColor();
        }
    }

    static string RegistrarRealidad()
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine();
            Console.WriteLine("                         ╔══════════════════════════════════════╗");
            Console.WriteLine("                         ║         REALIDADES DISPONIBLES  🔮  ║");
            Console.WriteLine("                         ╚══════════════════════════════════════╝");
            Console.WriteLine("                         ║              GENESIS                 ║");
            Console.WriteLine("                         ╚══════════════════════════════════════╝");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("           Ingresa una realidad asignada(ˆ-ˆ): ");
            Console.ResetColor();

            string realidad = (Console.ReadLine() ?? "").Trim().ToUpper();

            if (realidad == "GENESIS")
            {
                Console.WriteLine("NEXUS: Realidad reconocida.");
                return realidad;
            }

            Console.WriteLine();
            Console.WriteLine("NEXUS: ERROR - Realidad no reconocida.");
            Console.WriteLine("NEXUS: Introduzca una realidad valida.");
        }
    }
    // Pendiente:
    //static int RegistrarEnergiaInicial()
    //{
    // int energia = LeerEntero("║    Ingrese el nivel de energia:  ",40, 100,"Error: La energia debe estar entre 40 y 100.");
    // Console.WriteLine("         Nivel de energia registrado correctamente.");
    // return energia;
    //}

    //static int RegistrarEstabilidadInicial()
    //{
    //   int estabilidad = LeerEntero("║   Ingrese el nivel de estabilidad:  ",50, 100,"Error: La estabilidad debe estar entre 50 y 100.");
    //   Console.WriteLine("         Nivel de estabilidad registrado correctamente.");
    //   return estabilidad;
    //}

    static int LeerEntero(string prompt, int minimo, int maximo, string mensajeFueraDeRango)
    {
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(prompt);
            Console.ResetColor();

            bool esValido = int.TryParse(Console.ReadLine(), out int valor);

            if (!esValido)
            {
                Console.WriteLine("Error: Debe ingresar un numero.");
            }
            else if (valor < minimo || valor > maximo)
            {
                Console.WriteLine(mensajeFueraDeRango);
            }
            else
            {
                return valor;
            }
        }
    }

    // ===== NUEVO: Punto 2 - Pantalla de selección de cadete =====
    static void SeleccionarPersonaje(EstadoJuego estado)
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║              NEXUS PROTOCOLO DE              ║");
            Console.WriteLine("║              SELECCIÓN DE CADETE             ║");
            Console.WriteLine("╠══════════════════════════════════════════════╣");
            Console.WriteLine("║                                              ║");
            Console.WriteLine("║ [1] FÍSICO                                   ║");
            Console.WriteLine("║     Especialista en combate cuerpo a cuerpo  ║");
            Console.WriteLine("║                                              ║");
            Console.WriteLine("║ [2] ARMAMENTO                                ║");
            Console.WriteLine("║     Especialista en combate con armas        ║");
            Console.WriteLine("║                                              ║");
            Console.WriteLine("║ [3] MANA                                     ║");
            Console.WriteLine("║     Especialista en energía elemental        ║");
            Console.WriteLine("║                                              ║");
            Console.WriteLine("║ [4] HERRAMIENTAS                             ║");
            Console.WriteLine("║     Especialista tecnológico                 ║");
            Console.WriteLine("║                                              ║");
            Console.WriteLine("╠══════════════════════════════════════════════╣");
            Console.Write("║ Seleccione su especialización: ");
            Console.ResetColor();

            string opcion = Console.ReadLine();

            if (opcion == "1")
            {
                estado.TipoPersonaje = "FISICO";
            }
            else if (opcion == "2")
            {
                estado.TipoPersonaje = "ARMAMENTO";
            }
            else if (opcion == "3")
            {
                estado.TipoPersonaje = "MANA";
            }
            else if (opcion == "4")
            {
                estado.TipoPersonaje = "HERRAMIENTAS";
            }
            else
            {
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("NEXUS: Especialización no reconocida. Intente de nuevo.");
                Console.ResetColor();
                Thread.Sleep(1200);
                continue;
            }

            break;
        }

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        MostrarTexto("NEXUS: Clase seleccionada.", true, 200);
        MostrarTexto("NEXUS: Bienvenido, Cadete " + estado.Nombre + ".", true, 200);
        MostrarTexto("NEXUS: Especialización: " + estado.TipoPersonaje, true, 200);
        MostrarTexto("NEXUS: Preparando sistemas...", true, 200);
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Presione ENTER para continuar...");
        Console.ReadLine();
    }

    // ===== NUEVO: Punto 1 - Ventaja pasiva sencilla de cada clase =====
    static void AplicarCaracteristicasClase(EstadoJuego estado)
    {
        // Cada clase recibe un pequeño ajuste inicial.
        // Las ventajas "activas" (ataques, arsenal, habilidad, escaneo) se
        // aplican dentro de sus propios métodos comparando estado.TipoPersonaje.
        if (estado.TipoPersonaje == "FISICO")
        {
            // Mayor resistencia: un poco más de estabilidad inicial.
            estado.Estabilidad = Clamp(estado.Estabilidad + 10, 0, 100);
        }
        else if (estado.TipoPersonaje == "ARMAMENTO")
        {
            // El cadete de armas empieza con un arma básica equipada.
            estado.TieneArma = true;
        }
        else if (estado.TipoPersonaje == "MANA")
        {
            // Mayor reserva de energía para poder canalizar habilidades.
            estado.Energia = Clamp(estado.Energia + 10, 0, 100);
        }
        else if (estado.TipoPersonaje == "HERRAMIENTAS")
        {
            // El detector ya viene disponible por defecto (TieneDetector = true).
            // Su ventaja real (dron y PEM más baratos) se aplica en
            // DesplegarDron() y EmitirPulsoElectromagnetico().
        }
    }

    static void MostrarAutorizacion(EstadoJuego estado)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("                                       ╔════════════════════════════════════════╗");
        Console.WriteLine("                                       ║       DATOS VALIDOS                   ║ ");
        Console.WriteLine("                                       ║      INMERSION AUTORIZADA  (✧ω✧)    ║");
        Console.WriteLine("                                       ╚════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        MostrarTexto("Nexus: Bienvenido a Nexus, Cadete " + estado.Nombre + ".", true, 200);
        MostrarTexto("NEXUS: Todos sus parametros han sido validos.", true, 200);
        MostrarTexto("NEXUS: Preparando enlace con la realidad " + estado.Realidad + "...", true, 200);
        MostrarTexto("NEXUS: Especialización registrada: " + estado.TipoPersonaje + ".", true, 200);
        Console.WriteLine();
        Console.WriteLine("Presione ENTER para continuar...");
        Console.ReadLine();
        Console.ResetColor();
        Console.Clear();

        Console.ForegroundColor = ConsoleColor.Yellow;
        MostrarTexto("NEXUS: Hola, Cadete " + estado.Nombre + ".", true, 200);
        MostrarTexto("NEXUS: Yo soy NEXUS (>‿<)✌️ y te guiare durante esta mision.", true, 200);
        MostrarTexto("NEXUS: Los archivos históricos de IRIS están disponibles.", true, 200);
        MostrarTexto("NEXUS: puedes consultarlos desde el menú cuando lo desees.", true, 200);
        Console.ResetColor();
        Console.WriteLine();
    }
    //Pendiente:
    //static void MostrarRechazo(string contraseña, int energia, int estabilidad)
    // {
    // Console.Clear();
    // Console.ForegroundColor = ConsoleColor.Red;
    // Console.WriteLine("╔════════════════════════════════════════╗");
    // Console.WriteLine("║        INMERSION DENEGADA   (╥﹏╥)     ║");
    //Console.WriteLine("╚════════════════════════════════════════╝");
    // Console.ResetColor();
    // Console.WriteLine();
    // Console.WriteLine("NEXUS: Los parametros minimos no fueron alcanzados.");
    // Console.WriteLine("NEXUS: Se requiere contraseña == " + contraseña + ".");
    //  Console.WriteLine("NEXUS: Valores recibidos  contraseña != " + contraseña + ".");
    //}

    static void EjecutarMision(EstadoJuego estado)
    {
        while (estado.Conectado)
        {
            MostrarPanelEstado(estado);

            Console.Write("Seleccione una operacion: ");
            string opcion = Console.ReadLine();

            switch (opcion)
            {
                case "1":
                    AbrirCapacidadesEquipamiento(estado);
                    break;

                case "2":
                    MostrarManualDeUso();
                    break;

                case "3":
                    MostrarNexoMultiversal(estado);
                    break;

                case "4":
                    AbrirSistemasDeAccion(estado);
                    break;

                case "0":
                    IntentarDesconexion(estado);
                    break;

                default:
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("NEXUS: Operación no reconocida.");
                    Console.WriteLine("NEXUS: Seleccione una operación válida del menú.");
                    Console.ResetColor();

                    Console.WriteLine();
                    Console.WriteLine("Presione ENTER para continuar...");
                    Console.ReadLine();

                    continue;
            }

            // El Manual de uso (opcion "2") es solo informativo: no gasta turno,
            // no mueve el bloqueo de IRIS ni comprueba condiciones críticas.
            if (estado.Conectado && opcion != "2")
            {
                ActualizarBloqueoIris(estado);
                ActualizarCapacidadActiva(estado);
                ComprobarRiesgoIris(estado);
                ComprobarCondicionCritica(estado);
            }

            if (estado.Conectado)
            {
                Console.WriteLine();
                Console.WriteLine("Presione ENTER para continuar...");
                Console.ReadLine();
            }
        }
    }

    static void MostrarPanelEstado(EstadoJuego estado)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkYellow;

        // Encabezado principal
        Console.WriteLine("-------------------------------------------------------------------------------------");
        Console.WriteLine("||                                NEXUS  SYSTEM  [V1.0]                            ||");
        Console.WriteLine("-------------------------------------------------------------------------------------");
        Console.WriteLine($"|| EXPLORADOR: {estado.Nombre,-30} | REALIDAD: {estado.Realidad,-25} ||");
        Console.WriteLine("-------------------------------------------------------------------------------------");

        // --- Bloques lógicos de texto ---
        string anomalia = estado.AnomaliaLocalizada ? $"LOCALIZADA - {estado.UbicacionAnomalia}" : "NO LOCALIZADA";
        string iris = estado.TurnosBloqueoIris > 0 ? $"BLOQUEADA ({estado.TurnosBloqueoIris} T)" : "ACTIVA";
        string capacidad = estado.CapacidadActiva ? $"ACTIVA ({estado.TurnosCapacidadActiva} T)" : "INACTIVA";

        // --- SECCIÓN SUPERIOR: Barras (Izquierda) e Info de Sistema (Derecha) ---
        // El menú principal SOLO muestra Energía y Estabilidad.
        // Vida y Poder se muestran únicamente en la Opción 2 (Capacidades/Equipamiento).
        string barraEnergia = $"|| Energia:     [{MostrarBarra(estado.Energia)}] {estado.Energia}%";
        string barraEstabilidad = $"|| Estabilidad: [{MostrarBarra(estado.Estabilidad)}] {estado.Estabilidad}%";

        // lado a lado (Alineamos la columna izquierda a 45 caracteres)
        Console.WriteLine($"{barraEnergia,-45}   || [ SISTEMA ]");
        Console.WriteLine($"{barraEstabilidad,-45} ||    > Clase     : {estado.TipoPersonaje}");
        Console.WriteLine($"{"",-45} ||    > Anomalía  : {anomalia}");
        Console.WriteLine($"{"",-45} ||    > Iris      : {iris}");
        Console.WriteLine($"{"",-45} ||    > Capacidad : {capacidad}");
        Console.WriteLine(new string('-', 85)); // Línea divisoria horizontal

        // --- SECCIÓN INFERIOR: Operaciones Disponibles (Abajo) ---
        Console.WriteLine("[ OPERACIONES DISPONIBLES ]");
        Console.WriteLine("  [1] 🎒  Capacidades / Equipamiento");
        Console.WriteLine("  [2] 📖  Manual de uso");
        Console.WriteLine("  [3] 🌌  Nexo multiversal");
        Console.WriteLine("  [4] ⚙️  Sistemas de acción");
        Console.WriteLine("  [0] ⏻  Desconexión");

        Console.WriteLine(new string('-', 85));
        Console.ResetColor();
    }

    static void DesplegarDron(EstadoJuego estado, char[,] mapa, bool[,] zonaExploradas)
    {
        if (estado.PulsoActivo)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: El pulso electromagnetico esta activo.");
            Console.WriteLine("NEXUS: Los sistemas del dron no responden.");
            return;
        }

        if (!estado.TieneDron)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: Señal de tecnologia desconocida detectada.");
            Console.WriteLine("NEXUS: Explorador, investigue la ubicacion.");

            estado.TieneDron = true;

            Console.WriteLine();
            Console.WriteLine("OBJETO RECUPERADO: DRON DE RECONOCIMIENTO");
            Console.WriteLine("NEXUS: El dispositivo todavia parece funcional");

            Console.WriteLine();
            Console.WriteLine("NEXUS: Se ha detectado modulo electromagnetico");
            Console.WriteLine("NUEVA CAPACIDAD: PULSO ELECTROMAGNETICO");

            estado.TienePEM = true;
            return;
        }

        if (estado.Energia < EnergiaMinimaDron)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: Energia insuficiente para desplegar el dron.");
            return;
        }

        // El cadete de HERRAMIENTAS despliega el dron gastando menos energía.
        int costoDronReal = estado.TipoPersonaje == "HERRAMIENTAS" ? CostoDronHerramientas : CostoDron;

        // Si la capacidad especial de PODER esta activa, el costo baja aun mas.
        if (estado.CapacidadActiva)
        {
            costoDronReal = costoDronReal / 2;
        }

        estado.Energia = Clamp(estado.Energia - costoDronReal, 0, 100);
        Console.WriteLine();
        Console.WriteLine("NEXUS: Desplegando dron de reconocimiento...");
        MostrarDron();
        Thread.Sleep(500);
        Console.WriteLine("DRON: Sistemas iniciados.");
        Thread.Sleep(500);
        Console.WriteLine("DRON: Escaneando frecuencias");
        int señal = rng.Next(1, 101);

        if (señal <= 65)
        {
            string[] sectores =
            {
                "Sector Delta",
                "Zona de Ruinas",
                "Corredor Omega",
                "Zona de interferencia"
            };
            int sectorEncontrado = rng.Next(sectores.Length);
            estado.UbicacionAnomalia = sectores[sectorEncontrado];
            estado.AnomaliaLocalizada = true;
            Console.WriteLine();
            Console.WriteLine("DRON: ☢ SEÑAL DETECTADA.");
            Console.WriteLine("DRON: Anomalia detectada.");
            Console.WriteLine("DRON: Ubicacion: " + estado.UbicacionAnomalia);
            MostrarAnomalia();
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("DRON: No se detectaron señales");
        }
    }

    static void EmitirPulsoElectromagnetico(EstadoJuego estado)
    {
        if (!estado.TienePEM)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: Generador PEM no disponible.");
            Console.WriteLine("NEXUS: Debes encontrar tecnologia adicional.");
            return;
        }

        if (estado.PulsoActivo)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: El pulso electromagnetico ya esta activo.");
            return;
        }

        if (estado.Energia < EnergiaMinimaPulso)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: Energia insuficiente para emitir el pulso.");
            return;
        }

        // El cadete de HERRAMIENTAS emite el pulso gastando menos energía.
        int costoPulsoReal = estado.TipoPersonaje == "HERRAMIENTAS" ? CostoPulsoHerramientas : CostoPulso;

        // Si la capacidad especial de PODER esta activa, el costo baja aun mas.
        if (estado.CapacidadActiva)
        {
            costoPulsoReal = costoPulsoReal / 2;
        }

        estado.Energia = Clamp(estado.Energia - costoPulsoReal, 0, 100);
        Console.WriteLine();
        Console.WriteLine("NEXUS: Cargando pulso electromagnetico...");
        MostrarPEM();
        Thread.Sleep(500);
        Console.WriteLine("NEXUS: Potencia al 30%...");
        Thread.Sleep(500);
        Console.WriteLine("NEXUS: Potencia al 70%...");
        Thread.Sleep(500);
        Console.WriteLine("NEXUS: ⚡PULSO ELECTROMAGNETICO EMITIDO.");

        estado.TurnosBloqueoIris = DuracionBloqueoIris;
        estado.PulsoActivo = true;

        Console.WriteLine("IRIS: ERROR DE SEÑAL");
        Console.WriteLine("NEXUS: IRIS Ha perdido temporalmente la conexion.");
        Console.WriteLine("NEXUS: Bloqueo activo durante " + DuracionBloqueoIris + " turnos");
    }

    static void IntentarDesconexion(EstadoJuego estado)
    {
        Console.WriteLine();
        Console.WriteLine("NEXUS: Solicitud de desconexion recibida.");

        if (estado.Estabilidad < EstabilidadMinimaDesconexion)
        {
            Console.WriteLine("NEXUS: ☢️DESCONEXION INSEGURA");
            Console.WriteLine("NEXUS: La estabilidad debe ser de al menos " + EstabilidadMinimaDesconexion + "%.");
            return;
        }

        Console.WriteLine("NEXUS: Condiciones de desconexion aceptables.");
        Console.WriteLine("NEXUS: Iniciando desconexion...");
        Thread.Sleep(500);
        Console.WriteLine("3...");
        Thread.Sleep(500);
        Console.WriteLine("2...");
        Thread.Sleep(500);
        Console.WriteLine("1...");
        Console.WriteLine("CONEXION FINALIZADA.");
        estado.Conectado = false;
    }

    static void ActualizarBloqueoIris(EstadoJuego estado)
    {
        if (estado.TurnosBloqueoIris <= 0)
        {
            return;
        }

        estado.TurnosBloqueoIris--;

        if (estado.TurnosBloqueoIris > 0)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: Bloqueo IRIS activo. Turnos restantes:" + estado.TurnosBloqueoIris);
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("IRIS: Conexion restablecida.");
            Console.WriteLine("NEXUS: El bloqueo electromagnetico ha terminado.");
            estado.PulsoActivo = false;
        }
    }

    static void ComprobarRiesgoIris(EstadoJuego estado)
    {
        if (EvaluarRiesgoIris(estado))
        {
            AnomaliaDetectada?.Invoke(estado.Estabilidad);
        }
    }

    static bool EvaluarRiesgoIris(EstadoJuego estado)
    {
        if (estado.TurnosBloqueoIris > 0)
        {
            return false;
        }

        int probabilidad = 10;

        if (estado.Energia < UmbralBajo)
        {
            probabilidad += 20;
        }

        if (estado.Estabilidad < UmbralBajo)
        {
            probabilidad += 25;
        }

        if (estado.AnomaliaLocalizada)
        {
            probabilidad += 15;
        }

        return rng.Next(1, 101) <= probabilidad;
    }

    // ===== NUEVO: la capacidad especial de PODER dura unos turnos y luego se apaga =====
    static void ActualizarCapacidadActiva(EstadoJuego estado)
    {
        if (estado.TurnosCapacidadActiva <= 0)
        {
            return;
        }

        estado.TurnosCapacidadActiva--;

        if (estado.TurnosCapacidadActiva > 0)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: Capacidad especial activa. Turnos restantes: " + estado.TurnosCapacidadActiva);
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: La capacidad especial se ha desactivado.");
            estado.CapacidadActiva = false;
        }
    }

    static void ComprobarCondicionCritica(EstadoJuego estado)
    {
        if (estado.Vida <= 0 || estado.Energia <= 0 || estado.Estabilidad <= 0)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: Condicion critica detectada. Forzando desconexion de emergencia.");
            estado.Conectado = false;
        }
    }

    // ===== NUEVO: Punto 3 - Un solo despachador según estado.TipoPersonaje =====
    // Esto evita crear 4 programas distintos: usamos if / else if para decidir
    // qué método ejecutar, dependiendo de la clase que eligió el cadete.
    static void AccionEspecialDeClase(EstadoJuego estado)
    {
        if (estado.TipoPersonaje == "FISICO")
        {
            Console.WriteLine();
            Console.WriteLine("=== CAPACIDADES FISICAS ===");
            AtaqueFisico(estado);
        }
        else if (estado.TipoPersonaje == "ARMAMENTO")
        {
            Console.WriteLine();
            Console.WriteLine("=== ARSENAL ===");
            AbrirArsenal(estado);
        }
        else if (estado.TipoPersonaje == "MANA")
        {
            Console.WriteLine();
            Console.WriteLine("=== HABILIDADES DE MANA ===");
            UsarHabilidadDeMana(estado);
        }
        else if (estado.TipoPersonaje == "HERRAMIENTAS")
        {
            Console.WriteLine();
            Console.WriteLine("=== HERRAMIENTAS ===");
            EscaneoAvanzado(estado);
        }
    }

    // ----- FÍSICO: ataque físico simple, consume poca energía -----
    static void AtaqueFisico(EstadoJuego estado)
    {
        // Si la capacidad especial esta activa, el ataque cuesta la mitad.
        int costoReal = estado.CapacidadActiva ? CostoAtaqueFisico / 2 : CostoAtaqueFisico;

        if (estado.Energia < costoReal)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: Energia insuficiente para el ataque fisico.");
            return;
        }

        estado.Energia = Clamp(estado.Energia - costoReal, 0, 100);

        Console.WriteLine();
        Console.WriteLine("NEXUS: Ejecutando ataque fisico...");
        Thread.Sleep(300);
        Console.WriteLine("CADETE: Impacto directo.");

        // Ventaja de FISICO: recupera un poco de estabilidad al golpear.
        estado.Estabilidad = Clamp(estado.Estabilidad + 3, 0, 100);
        Console.WriteLine("NEXUS: Estabilidad +3 (resistencia fisica).");
    }

    // ----- ARMAS: submenú con ataque básico y ataque fuerte -----
    static void AbrirArsenal(EstadoJuego estado)
    {
        if (!estado.TieneArma)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: No hay ningun arma equipada.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║              ARSENAL                     ║");
        Console.WriteLine("╠══════════════════════════════════════════╣");
        Console.WriteLine("║ [1] Ataque basico   (-" + CostoAtaqueFisico + " energia)     ║");
        Console.WriteLine("║ [2] Ataque fuerte   (-" + CostoAtaqueFuerte + " energia)     ║");
        Console.WriteLine("║ [3] Cancelar                             ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.Write("Seleccione un ataque: ");

        string opcion = Console.ReadLine();

        // Si la capacidad especial esta activa, ambos ataques cuestan menos.
        int costoBasicoReal = estado.CapacidadActiva ? CostoAtaqueFisico / 2 : CostoAtaqueFisico;
        int costoFuerteReal = estado.CapacidadActiva ? CostoAtaqueFuerte / 2 : CostoAtaqueFuerte;

        if (opcion == "1")
        {
            if (estado.Energia < costoBasicoReal)
            {
                Console.WriteLine("NEXUS: Energia insuficiente.");
                return;
            }

            estado.Energia = Clamp(estado.Energia - costoBasicoReal, 0, 100);
            Console.WriteLine("NEXUS: Ataque basico ejecutado.");
        }
        else if (opcion == "2")
        {
            if (estado.Energia < costoFuerteReal)
            {
                Console.WriteLine("NEXUS: Energia insuficiente para un ataque fuerte.");
                return;
            }

            estado.Energia = Clamp(estado.Energia - costoFuerteReal, 0, 100);
            Console.WriteLine("NEXUS: ¡Ataque fuerte ejecutado!");
        }
        else
        {
            Console.WriteLine("NEXUS: Arsenal cerrado.");
        }
    }

    // ----- MANA: habilidad especial que también interfiere a IRIS -----
    static void UsarHabilidadDeMana(EstadoJuego estado)
    {
        if (estado.Energia < CostoHabilidadMana)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: Energia insuficiente para canalizar mana.");
            return;
        }

        estado.Energia = Clamp(estado.Energia - CostoHabilidadMana, 0, 100);

        Console.WriteLine();
        Console.WriteLine("NEXUS: Canalizando energia de mana...");
        Thread.Sleep(400);
        Console.WriteLine("CADETE: La señal de IRIS se distorsiona brevemente.");

        // Ventaja de MANA: puede interferir a IRIS si no esta ya bloqueada.
        // Con la capacidad especial activa, la interferencia dura mas turnos.
        if (estado.TurnosBloqueoIris <= 0)
        {
            estado.TurnosBloqueoIris = estado.CapacidadActiva ? 2 : 1;
            Console.WriteLine("NEXUS: IRIS ha sido interferida temporalmente.");
        }
    }

    // ----- HERRAMIENTAS: escaneo de información sin gastar energía -----
    static void EscaneoAvanzado(EstadoJuego estado)
    {
        if (!estado.TieneDetector)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: El detector no esta disponible.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine("NEXUS: Iniciando escaneo avanzado (especialidad HERRAMIENTAS)...");
        Thread.Sleep(400);

        Console.WriteLine("NEXUS: Energia restante: " + estado.Energia + "%");
        Console.WriteLine("NEXUS: Estabilidad restante: " + estado.Estabilidad + "%");

        if (estado.TurnosBloqueoIris > 0)
        {
            Console.WriteLine("NEXUS: Bloqueo IRIS: " + estado.TurnosBloqueoIris + " turnos");
        }
        else
        {
            Console.WriteLine("NEXUS: Bloqueo IRIS: sin bloqueo");
        }

        if (estado.AnomaliaLocalizada)
        {
            Console.WriteLine("NEXUS: Ultima anomalia registrada en " + estado.UbicacionAnomalia + ".");
        }
        else if (estado.CapacidadActiva)
        {
            // Ventaja de la capacidad especial: revela una anomalia automaticamente.
            string[] sectores = { "Sector Delta", "Zona de Ruinas", "Corredor Omega", "Zona de interferencia" };
            estado.UbicacionAnomalia = sectores[rng.Next(sectores.Length)];
            estado.AnomaliaLocalizada = true;
            Console.WriteLine("NEXUS: Escaneo total activo. Anomalia revelada en " + estado.UbicacionAnomalia + ".");
        }
        else
        {
            Console.WriteLine("NEXUS: No hay anomalias registradas por el momento.");
        }
    }

    // ===== NUEVO: Punto 2 - Menú unificado de Capacidades y Equipamiento =====

    static void AbrirSistemasDeAccion(EstadoJuego estado)
    {
        Console.Clear();

        Console.ForegroundColor = ConsoleColor.DarkCyan;

        Console.WriteLine("╔════════════════════════════════════════════╗");
        Console.WriteLine("║          NEXUS // SISTEMAS DE ACCIÓN       ║");
        Console.WriteLine("╠════════════════════════════════════════════╣");
        Console.WriteLine("║ [1] 🔎 Usar detector                       ║");
        Console.WriteLine("║ [2] 🤖 Desplegar dron                      ║");
        Console.WriteLine("║ [3] ⚡ Emitir PEM                           ║");
        Console.WriteLine("║ [4] 🗡️ Katana eléctrica                    ║");
        Console.WriteLine("║ [0] ↩ Regresar                             ║");
        Console.WriteLine("╚════════════════════════════════════════════╝");

        Console.ResetColor();

        Console.Write("Seleccione una acción: ");
        string opcion = Console.ReadLine();
        if (opcion == "2")
        {
            if (estado.DronDesplegado)
            {
                Console.WriteLine();
                Console.WriteLine("⚠️ NEXUS: Ya existe un dron desplegado.");
                Thread.Sleep(1200);
                return;
            }

            estado.FilaDron = estado.FilaJugador;
            estado.ColumnaDron = estado.ColumnaJugador;
            estado.DronDesplegado = true;

            Console.WriteLine();
            Console.WriteLine("🤖 NEXUS: Dron de reconocimiento desplegado.");
            Console.WriteLine("📍 Posición del dron: [" +
                estado.FilaDron + "," + estado.ColumnaDron + "]");
            Thread.Sleep(1500);

            return;
        }
        if (opcion == "0")
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine("⚠️ Sistema seleccionado.");
        Console.WriteLine("NEXUS: Esta función será integrada próximamente.");
        Thread.Sleep(1200);
    }
    static void AbrirCapacidadesEquipamiento(EstadoJuego estado)
    {
        Console.Clear();

        Console.ForegroundColor = ConsoleColor.DarkCyan;

        Console.WriteLine("╔════════════════════════════════════════════╗");
        Console.WriteLine("║       NEXUS // CAPACIDADES Y EQUIPO        ║");
        Console.WriteLine("╠════════════════════════════════════════════╣");
        Console.WriteLine("║ ESPECIALIZACIÓN: HERRAMIENTAS              ║");
        Console.WriteLine("╠════════════════════════════════════════════╣");

        Console.ResetColor();

        MostrarBarra("VIDA  ", estado.Vida);
        MostrarBarra("PODER ", estado.Poder);

        Console.ForegroundColor = ConsoleColor.DarkCyan;

        Console.WriteLine("╠════════════════════════════════════════════╣");
        Console.WriteLine("║ 🛠️ CAPACIDADES                             ║");
        Console.WriteLine("║                                            ║");
        Console.WriteLine("║ 🔎 Detector                                ║");
        Console.WriteLine("║ 🤖 Dron de reconocimiento                  ║");
        Console.WriteLine("║ ⚡ Pulso electromagnético                  ║");
        Console.WriteLine("║                                            ║");
        Console.WriteLine("║ ⚔️ ARMAMENTO                               ║");
        Console.WriteLine("║                                            ║");
        Console.WriteLine("║ 🗡️ Katana eléctrica                       ║");
        Console.WriteLine("║ 🔫 Pistola de plasma                      ║");
        Console.WriteLine("║ 💣 Granada                                ║");
        Console.WriteLine("╠════════════════════════════════════════════╣");
        Console.WriteLine("║ [1] Regresar                               ║");
        Console.WriteLine("╚════════════════════════════════════════════╝");

        Console.ResetColor();

        Console.Write("Seleccione una opcion: ");
        string opcion = Console.ReadLine();

        if (opcion == "1")
        {
            return;
        }

        Console.WriteLine();
        Console.WriteLine("❌ NEXUS: Opción no reconocida.");
        Thread.Sleep(1000);
    }

    // ----- Etiqueta del menu de "Ver capacidades/equipo", segun la clase -----
    static string ObtenerEtiquetaVerCapacidades(string tipoPersonaje)
    {
        if (tipoPersonaje == "ARMAMENTO")
        {
            return "Ver armamento                          ║";
        }
        else if (tipoPersonaje == "HERRAMIENTAS")
        {
            return "Ver equipo (detector/dron/PEM/arma)    ║";
        }
        else
        {
            return "Ver capacidades                        ║";
        }
    }

    // ----- Pantalla de capacidades/equipo, distinta segun la clase del cadete -----
    static void MostrarCapacidadesClase(EstadoJuego estado)
    {
        if (estado.TipoPersonaje == "FISICO")
        {
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║          NEXUS // FÍSICO                    ║");
            Console.WriteLine("╠══════════════════════════════════════════════╣");
            Console.WriteLine("║ ❤️ Vida: " + estado.Vida + "%");
            Console.WriteLine("║ 🔷 Poder: " + estado.Poder + "%");
            Console.WriteLine("╠══════════════════════════════════════════════╣");
            Console.WriteLine("║ CAPACIDADES");
            Console.WriteLine("║");
            Console.WriteLine("║ • golpe simple");
            Console.WriteLine("║ • golpe cruzado");
            Console.WriteLine("║ • proteccion");
            Console.WriteLine("║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
        }
        else if (estado.TipoPersonaje == "ARMAMENTO")
        {
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║           NEXUS // ARMAMENTO                ║");
            Console.WriteLine("╠══════════════════════════════════════════════╣");
            Console.WriteLine("║ ❤️ Vida: " + estado.Vida + "%");
            Console.WriteLine("║ 🔷 Poder: " + estado.Poder + "%");
            Console.WriteLine("╠══════════════════════════════════════════════╣");
            Console.WriteLine("║ ARMAMENTO");
            Console.WriteLine("║");
            Console.WriteLine("║ • pistola");
            Console.WriteLine("║ • combo");
            Console.WriteLine("║ • escudo");
            Console.WriteLine("║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
        }
        else if (estado.TipoPersonaje == "MANA")
        {
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║             NEXUS // MANA                  ║");
            Console.WriteLine("╠══════════════════════════════════════════════╣");
            Console.WriteLine("║ ❤️ Vida: " + estado.Vida + "%");
            Console.WriteLine("║ 🔷 Poder: " + estado.Poder + "%");
            Console.WriteLine("╠══════════════════════════════════════════════╣");
            Console.WriteLine("║ CAPACIDADES");
            Console.WriteLine("║");
            Console.WriteLine("║ • bola de energia");
            Console.WriteLine("║ • Manipulación de energia");
            Console.WriteLine("║ • escudo");
            Console.WriteLine("║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
        }
        else if (estado.TipoPersonaje == "HERRAMIENTAS")
        {
            // El cadete de HERRAMIENTAS es el unico que tiene detector, dron y PEM.
            VerEquipo(estado);
        }
    }

    // ----- Ver estado del equipo y, desde ahi, usar dron o PEM -----
    // Solo se llama para el cadete de HERRAMIENTAS.
    static void VerEquipo(EstadoJuego estado)
    {
        Console.WriteLine();
        Console.WriteLine("DETECTOR: " + (estado.TieneDetector ? "DISPONIBLE" : "[BLOQUEADO]"));
        Console.WriteLine("DRON DE RECONOCIMIENTO: " + (estado.TieneDron ? "DISPONIBLE" : "[DESCONOCIDO]"));
        Console.WriteLine("PULSO ELECTROMAGNETICO (PEM): " + (estado.TienePEM ? "DISPONIBLE" : "[DESCONOCIDO]"));
        Console.WriteLine("ARMA: " + (estado.TieneArma ? "EQUIPADA" : "[DESCONOCIDO]"));
        Console.WriteLine();
        Console.WriteLine("[1] Desplegar dron");
        Console.WriteLine("[2] Emitir PEM");
        Console.WriteLine("[3] No usar nada, regresar");
        Console.Write("Seleccione una opcion: ");

        string opcion = Console.ReadLine();

        if (opcion == "1")
        {
            DesplegarDron(estado);
        }
        else if (opcion == "2")
        {
            EmitirPulsoElectromagnetico(estado);
        }
    }

    // ===== NUEVO: Sistema de Poder - activar la capacidad especial de la clase =====
    static void ActivarHabilidadDePoder(EstadoJuego estado)
    {
        if (estado.Poder < UmbralPoderHabilidad)
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: Poder insuficiente. Se necesitan al menos " + UmbralPoderHabilidad + " puntos de poder.");
            return;
        }

        estado.Poder = 0;
        estado.CapacidadActiva = true;
        estado.TurnosCapacidadActiva = DuracionCapacidadEspecial;

        Console.WriteLine();
        Console.WriteLine("NEXUS: Capacidad especial activada durante " + DuracionCapacidadEspecial + " turnos.");

        if (estado.TipoPersonaje == "FISICO")
        {
            Console.WriteLine("CADETE: Resistencia maxima. Los ataques fisicos costaran menos energia.");
        }
        else if (estado.TipoPersonaje == "ARMAMENTO")
        {
            Console.WriteLine("CADETE: Arsenal sobrecargado. Los ataques costaran menos energia.");
        }
        else if (estado.TipoPersonaje == "MANA")
        {
            Console.WriteLine("CADETE: Canal de mana abierto. La interferencia a IRIS durara mas turnos.");
        }
        else if (estado.TipoPersonaje == "HERRAMIENTAS")
        {
            Console.WriteLine("CADETE: Escaneo total activo. El dron y el PEM costaran menos energia.");
        }
    }

    // ===== NUEVO: Punto 3 - Manual de uso, puramente informativo =====
    // ===== Submenu del Manual: texto de ayuda + Archivos historicos =====
    static void MostrarManualDeUso()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔══════════════════════════════════════════════╗");
            Console.WriteLine("║             NEXUS // MANUAL                 ║");
            Console.WriteLine("╠══════════════════════════════════════════════╣");
            Console.WriteLine("║ [1] Manual de uso                            ║");
            Console.WriteLine("║ [2] Archivos históricos                      ║");
            Console.WriteLine("║ [0] Regresar                                 ║");
            Console.WriteLine("╚══════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.Write("Seleccione una opcion: ");

            string opcion = Console.ReadLine();

            if (opcion == "1")
            {
                MostrarTextoManual();
            }
            else if (opcion == "2")
            {
                MostrarArchivosHistoricos();
            }
            else if (opcion == "0")
            {
                return;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("NEXUS: Opcion no reconocida.");
                Console.WriteLine();
                Console.WriteLine("Presione ENTER para continuar...");
                Console.ReadLine();
            }
        }
    }

    static void MostrarTextoManual()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║           NEXUS // MANUAL DE USO             ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("[CAPACIDADES Y EQUIPAMIENTO]");
        Console.WriteLine("Cada clase (FISICO, ARMAMENTO, MANA, HERRAMIENTAS) tiene una accion");
        Console.WriteLine("especial propia. Solo el cadete de HERRAMIENTAS cuenta con detector,");
        Console.WriteLine("dron y PEM.");
        Console.WriteLine();
        Console.WriteLine("[VIDA Y ENERGIA]");
        Console.WriteLine("La Vida representa tu integridad fisica; si llega a 0, la conexion");
        Console.WriteLine("termina de emergencia. La Energia se gasta al realizar acciones.");
        Console.WriteLine();
        Console.WriteLine("[PODER]");
        Console.WriteLine("Al llegar a " + UmbralPoderHabilidad + " puntos de Poder, puedes activar tu");
        Console.WriteLine("capacidad especial desde el menu de Capacidades / Equipamiento.");
        Console.WriteLine();
        Console.WriteLine("[NEXO MULTIVERSAL]");
        Console.WriteLine("Desde aqui puedes elegir una realidad y ver su mapa completo.");
        Console.WriteLine();
        Console.WriteLine("[REGLAS BASICAS]");
        Console.WriteLine("Si tu Vida, Energia o Estabilidad llegan a 0, la mision termina.");
        Console.WriteLine("Consultar el manual no gasta turnos ni recursos.");
        Console.WriteLine();
        Console.WriteLine("Presione ENTER para regresar al menu...");
        Console.ReadLine();
    }

    static void MostrarNexoMultiversal(EstadoJuego estado)
    {
        Console.Clear();
        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║        Nexo Multiversal (+_+)               ║");
        Console.WriteLine("╠══════════════════════════════════════════════╣");
        Console.WriteLine("║                                              ║");
        Console.WriteLine("║ [1] Genesis                                  ║");
        Console.WriteLine("║ [2] Gama                                     ║");
        Console.WriteLine("║ [3] Delta                                    ║");
        Console.WriteLine("║ [4] Épsilon                                  ║");
        Console.WriteLine("║ [5] Tau                                      ║");
        Console.WriteLine("║ [6] Omega                                    ║");
        Console.WriteLine("║ [7] volver                                   ║");
        Console.WriteLine("║                                              ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");

        Console.Write("Seleccione una realidad: ");
        string realidadMapa = Console.ReadLine();

        switch (realidadMapa)
        {
            case "1":
                Console.Clear();
                MapaGenesis(estado);
                break;

            default:
                Console.WriteLine();
                Console.WriteLine("NEXUS: Realidad no disponible.");
                break;
        }
    }

    static void EscribirConEfecto(string texto, int velocidadMilisegundos = 40)
    {
        foreach (char letra in texto)
        {
            Console.Write(letra);
            Thread.Sleep(velocidadMilisegundos);
        }
        Console.WriteLine();
    }

    static void MostrarTexto(string mensaje, bool espacioExtra = false, int pausa = 200)
    {
        EscribirConEfecto(mensaje);
        Thread.Sleep(pausa);

        if (espacioExtra)
        {
            Console.WriteLine();
        }
    }

    // ===== NUEVO: Punto 12 - Arte ASCII pequeño, separado de la lógica =====
    static void MostrarDron()
    {
        Console.WriteLine("     __/\\__");
        Console.WriteLine("    [ DRON ]");
        Console.WriteLine("     \\____/");
    }

    static void MostrarPEM()
    {
        Console.WriteLine("    ((( * )))");
        Console.WriteLine("     [ PEM ]");
    }

    static void MostrarAnomalia()
    {
        Console.WriteLine("      /\\");
        Console.WriteLine("     /  \\");
        Console.WriteLine("    / ⚠  \\");
        Console.WriteLine("   /______\\");
    }

    static void MostrarIris()
    {
        Console.WriteLine("    .-----.");
        Console.WriteLine("   ( IRIS )");
        Console.WriteLine("    '-----'");
    }

    static void AnimacionIris()
    {
        Console.Write("IRIS DETECTADA");

        for (int i = 0; i < 3; i++)
        {
            Thread.Sleep(400);
            Console.Write(".");
        }

        Console.WriteLine();
    }

    static void DispararAlertaIris(int estabilidad)
    {
        Console.WriteLine();

        AnimacionIris();
        Console.ForegroundColor = ConsoleColor.Red;
        MostrarIris();

        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║       ⚠⚠⚠  ALERTA CRÍTICA  ⚠⚠⚠       ║");
        Console.WriteLine("║             INTERFERENCIA: IRIS");
        Console.WriteLine("╠══════════════════════════════════════════════╣");
        Console.WriteLine("║ ESTABILIDAD: " + estabilidad + "%");
        Console.WriteLine("║ IRIS HA INTERRUMPIDO LOS SISTEMAS DE NEXUS   ║");
        Console.WriteLine("║ NEXUS recomienda desconexion inmediata.      ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");
        Console.ResetColor();
    }

    static string MostrarBarra(int valor)
    {
        int bloques = valor / 5;

        string barra = "";

        for (int i = 0; i < 20; i++)
        {
            if (i < bloques)
            {
                barra += "█";
            }
            else
            {
                barra += "░";
            }
        }

        return barra;
    }

    // ===== NUEVO: Punto 5 - Barra genérica reutilizable para cualquier estadística =====
    // Reutiliza el método MostrarBarra(int) de arriba (el que ya tenías) para
    // dibujar los bloques, y solo le agrega una etiqueta y el porcentaje al frente.
    static void MostrarBarra(string nombre, int valor)
    {
        string barra = MostrarBarra(valor);
        Console.WriteLine(nombre + " [" + barra + "] " + valor + "%");
    }

    static int Clamp(int valor, int minimo, int maximo)
    {
        if (valor < minimo) return minimo;
        if (valor > maximo) return maximo;
        return valor;
    }

    static void MostrarMapa(char[,] mapa)
    {
        Console.WriteLine();

        Console.Write("    ");

        for (int columna = 0; columna < mapa.GetLength(1); columna++)
        {
            Console.Write(columna + " ");
        }

        Console.WriteLine();

        for (int fila = 0; fila < mapa.GetLength(0); fila++)
        {
            Console.Write(fila + "   ");

            for (int columna = 0; columna < mapa.GetLength(1); columna++)
            {
                char simbolo = mapa[fila, columna];

                string tipoZona = ObtenerTipoZona(simbolo);

                if (tipoZona == "BASE")
                {
                    Console.Write("⌂ ");
                }
                else if (tipoZona == "RECURSO")
                {
                    Console.Write("◆ ");
                }
                else if (tipoZona == "ANOMALIA")
                {
                    Console.Write("⚠ ");
                }
                else if (tipoZona == "LIMITE TERRITORIAL")
                {
                    Console.Write("| ");
                }
                else
                {
                    Console.Write("· ");
                }
            }

            Console.WriteLine();
        }

        Console.WriteLine();
    }
    static void MostrarMapaExploracion(char[,] mapa, bool[,] zonasExploradas, EstadoJuego estado)
    {
        Console.WriteLine();

        Console.Write("    ");

        for (int columna = 0; columna < mapa.GetLength(1); columna++)
        {
            Console.Write(columna + " ");
        }

        Console.WriteLine();

        for (int fila = 0; fila < mapa.GetLength(0); fila++)
        {
            Console.Write(fila + "   ");

            for (int columna = 0; columna < mapa.GetLength(1); columna++)
            {
                // Primero mostramos al jugador
                if (fila == estado.FilaJugador && columna == estado.ColumnaJugador)
                {
                    Console.Write("🧍 ");
                }
                else if (estado.DronDesplegado &&
                         fila == estado.FilaDron &&
                         columna == estado.ColumnaDron)
                {
                    Console.Write("🤖 ");
                }
                else if (!zonasExploradas[fila, columna])
                {
                    Console.Write("? ");
                }
                else if (estado.EnemigosGenesis[fila, columna])
                {
                    Console.Write("☠ ");
                }
                else
                {
                    Console.Write(ObtenerSimboloExploracion(mapa[fila, columna]));
                }
            }

            Console.WriteLine();
        }

        Console.WriteLine();
    }

    static string ObtenerSimboloExploracion(char simbolo)
    {
        string tipoZona = ObtenerTipoZona(simbolo);

        if (tipoZona == "BASE")
        {
            return "⌂ ";
        }
        else if (tipoZona == "RECURSO")
        {
            return "◆ ";
        }
        else if (tipoZona == "ANOMALIA")
        {
            return "⚠ ";
        }
        else
        {
            return "░ ";
        }
    }

    static void RevelarZonaAlrededor(bool[,] zonasExploradas, int filaCentro, int columnaCentro)
    {
        for (int deltaFila = -1; deltaFila <= 1; deltaFila++)
        {
            for (int deltaColumna = -1; deltaColumna <= 1; deltaColumna++)
            {
                int fila = filaCentro + deltaFila;
                int columna = columnaCentro + deltaColumna;

                if (fila >= 0 && fila < zonasExploradas.GetLength(0) &&
                    columna >= 0 && columna < zonasExploradas.GetLength(1))
                {
                    zonasExploradas[fila, columna] = true;
                }
            }
        }
    }

    static void MapaGenesis(EstadoJuego estado)
    {
        char[,] mapa = estado.MapaGenesis;
        bool[,] zonasExploradas = estado.ZonasExploradasGenesis;

        // Solo colocamos al cadete la primera vez que entra
        if (!estado.PosicionGenesisInicializada)
        {
            estado.FilaJugador = mapa.GetLength(0) / 2;
            estado.ColumnaJugador = mapa.GetLength(1) / 2;

            RevelarZonaAlrededor(
                zonasExploradas,
                estado.FilaJugador,
                estado.ColumnaJugador
            );

            // Pocision enemigos
            estado.EnemigosGenesis[2, 3] = true;
            estado.EnemigosGenesis[4, 8] = true;
            estado.EnemigosGenesis[6, 5] = true;

            estado.VidaEnemigosGenesis[2, 3] = 25;
            estado.VidaEnemigosGenesis[4, 8] = 25;
            estado.VidaEnemigosGenesis[6, 5] = 25;


            estado.PosicionGenesisInicializada = true;
        }

        while (true)
        {
            Console.Clear();

            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("║            MAPA GENESIS                    ║");
            Console.WriteLine("╠════════════════════════════════════════════╣");

            MostrarMapaExploracion(mapa, zonasExploradas, estado);

            MostrarInformacionMapa(estado);
    
            Console.WriteLine("[NEXUS] CONTROL DE MOVIMIENTO");
            Console.WriteLine();
            Console.WriteLine("[W] Arriba");
            Console.WriteLine("[A] Izquierda");
            Console.WriteLine("[S] Abajo");
            Console.WriteLine("[D] Derecha");
            Console.WriteLine("[X] Salir del mapa");
            Console.WriteLine();

            Console.Write("NEXUS: Seleccione un movimiento: ");

            string direccion = (Console.ReadLine() ?? "").Trim().ToUpper();

            if (direccion == "X")
            {
                break;
            }

            int nuevaFila = estado.FilaJugador;
            int nuevaColumna = estado.ColumnaJugador;

            if (direccion == "W")
            {
                nuevaFila--;
            }
            else if (direccion == "S")
            {
                nuevaFila++;
            }
            else if (direccion == "D")
            {
                nuevaColumna++;
            }
            else if (direccion == "A")
            {
                nuevaColumna--;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("NEXUS: Dirección no reconocida.");
                Thread.Sleep(800);
                continue;
            }

            // Comprobar que la nueva posición está dentro del mapa
            if (nuevaFila < 0 ||
                nuevaFila >= mapa.GetLength(0) ||
                nuevaColumna < 0 ||
                nuevaColumna >= mapa.GetLength(1))
            {
                Console.WriteLine();
                Console.WriteLine("NEXUS: No puedes salir del territorio.");
                Thread.Sleep(800);
                continue;
            }

            // Comprobar límite territorial
            if (mapa[nuevaFila, nuevaColumna] == '|')
            {
                Console.WriteLine();
                Console.WriteLine("NEXUS: Límite territorial. Movimiento bloqueado.");
                Thread.Sleep(800);
                continue;
            }

            // Actualizar posición
            estado.FilaJugador = nuevaFila;
            estado.ColumnaJugador = nuevaColumna;

            // Se revela la zona alrededor de la nueva posición del cadete
            RevelarZonaAlrededor(zonasExploradas, estado.FilaJugador, estado.ColumnaJugador);

            // Comprobar si encontró un recurso
            ComprobarRecurso(estado, mapa);

            //Comprobar si encontro enemigo
            ComprobarEnemigo(estado);

            // Registrar distancia
            estado.DistanciaRecorrida =
                estado.DistanciaRecorrida + estado.MetrosPorCasilla;
        }
    }
    static void MostrarInformacionMapa(EstadoJuego estado)
    {
        // Valores ajustados para mover el panel arriba y a la derecha
        int x = 45;
        int y = 0;

        Console.SetCursorPosition(x, y); Console.WriteLine("╔════════════════════════════════╗");
        Console.SetCursorPosition(x, y + 1); Console.WriteLine("║      INFORMACIÓN DEL MAPA      ║");
        Console.SetCursorPosition(x, y + 2); Console.WriteLine("╠════════════════════════════════╣");
        Console.SetCursorPosition(x, y + 3); Console.WriteLine("║ 🧍 = CADETE                    ║");
        Console.SetCursorPosition(x, y + 4); Console.WriteLine("║ 📍 Posición: [" + estado.FilaJugador + "," + estado.ColumnaJugador + "]           ║");
        Console.SetCursorPosition(x, y + 5); Console.WriteLine("║ ░ = Zona explorada             ║");
        Console.SetCursorPosition(x, y + 6); Console.WriteLine("║ ? = Zona desconocida           ║");
        Console.SetCursorPosition(x, y + 7); Console.WriteLine("║                                ║");
        Console.SetCursorPosition(x, y + 8); Console.WriteLine("║ ⌂ Base       ◆ Recurso         ║");
        Console.SetCursorPosition(x, y + 9); Console.WriteLine("║ ⚠ Anomalía   ☠ Enemigo         ║");
        Console.SetCursorPosition(x, y + 10); Console.WriteLine("║                                ║");
        Console.SetCursorPosition(x, y + 11); Console.WriteLine("║ 📏 Distancia: " + estado.DistanciaRecorrida + " m              ║");
        Console.SetCursorPosition(x, y + 12); Console.WriteLine("║ ◆ Recursos: " + estado.RecursosRecolectados + "                 ║");
        Console.SetCursorPosition(x, y + 13); Console.WriteLine("╚════════════════════════════════╝");
    }

    static void ComprobarRecurso(EstadoJuego estado, char[,] mapa)
    {
        int fila = estado.FilaJugador;
        int columna = estado.ColumnaJugador;

        if (mapa[fila, columna] == '◆')
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: ◆ Recurso detectado.");
            Console.WriteLine("NEXUS: Recolectando recurso...");

            mapa[fila, columna] = '·';

            estado.RecursosRecolectados++;

            estado.Poder = Math.Min(100, estado.Poder + 10);

            Console.WriteLine("NEXUS: Recurso recolectado.");
            Console.WriteLine("NEXUS: Poder +10");
            Console.WriteLine("NEXUS: Recursos recolectados: " +
                              estado.RecursosRecolectados);

            Thread.Sleep(1200);
        }
    }
    static void ComprobarEnemigo(EstadoJuego estado)
    {
        int fila = estado.FilaJugador;
        int columna = estado.ColumnaJugador;

        if (estado.EnemigosGenesis[fila, columna])
        {
            int vidaEnemigo = estado.VidaEnemigosGenesis[fila, columna];

            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════════╗");
            Console.WriteLine("        ⚠ NEXUS: ENEMIGO DETECTADO");
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.WriteLine("📍 Coordenada: [" + fila + "," + columna + "]");
            Console.WriteLine("☠️ Vida del enemigo: " + vidaEnemigo);
            Console.WriteLine();
            Console.WriteLine("[1] Atacar");
            Console.WriteLine("[2] Retroceder");
            Console.WriteLine();

            Console.Write("NEXUS espera una decisión: ");
            string opcion = Console.ReadLine();

            if (opcion == "1")
            {
                int daño = 15;

                Console.WriteLine();
                Console.WriteLine("⚔️ NEXUS: Preparando enfrentamiento...");
                Thread.Sleep(1000);

                vidaEnemigo = vidaEnemigo - daño;

                estado.VidaEnemigosGenesis[fila, columna] = vidaEnemigo;

                Console.WriteLine("💥 Ataque realizado.");
                Console.WriteLine("⚔️ Daño causado: " + daño);
                Console.WriteLine("☠️ Vida restante: " + vidaEnemigo);

                if (vidaEnemigo <= 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("✅ ENEMIGO DERROTADO.");
                    Console.WriteLine("🗺️ La zona ha quedado despejada.");

                    estado.EnemigosGenesis[fila, columna] = false;
                    estado.VidaEnemigosGenesis[fila, columna] = 0;

                    Thread.Sleep(1500);
                }
                else
                {
                    Thread.Sleep(1200);
                }
            }
            else if (opcion == "2")
            {
                Console.WriteLine();
                Console.WriteLine("⏪ NEXUS: Retrocediendo de la zona.");
                Thread.Sleep(1000);
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("❌ NEXUS: Decisión no reconocida.");
                Thread.Sleep(1000);
            }
        }
    }
    static string ObtenerTipoZona(char simbolo)
    {
        if (simbolo == '⌂')
        {
            return "BASE";
        }
        else if (simbolo == '◆')
        {
            return "RECURSO";
        }
        else if (simbolo == '⚠')
        {
            return "ANOMALIA";
        }
        else if (simbolo == '|')
        {
            return "LIMITE TERRITORIAL";
        }
        else
        {
            return "TERRITORIO";
        }
    }
    static void AnalizarCoordenada(char[,] mapa, bool[,] zonasExploradas)
    {
        int fila = LeerEntero("Ingrese la fila:", 0, mapa.GetLength(0) - 1, "Nexus: Fila fuera del territorio.");

        int columna = LeerEntero("Ingrese la columna:", 0, mapa.GetLength(1) - 1, "Nexus: Columna fuera del territorio.");

        if (fila >= 0 && fila < mapa.GetLength(0) &&
            columna >= 0 && columna < mapa.GetLength(1))
        {
            zonasExploradas[fila, columna] = true;
            char simbolo = mapa[fila, columna];
            string tipoZona = ObtenerTipoZona(simbolo);

            Console.WriteLine();
            Console.WriteLine("NEXUS: Coordenada [" + fila + "," + columna + "]");
            Console.WriteLine("NEXUS: Tipo de zona: " + tipoZona);

            if (tipoZona == "BASE")
            {
                Console.WriteLine("NEXUS: ⌂ Base operativa localizada.");
            }
            else if (tipoZona == "RECURSO")
            {
                Console.WriteLine("NEXUS: ◆ Recurso disponible para recolección.");
                Console.Write("¿Desea recolectar este recurso? [S/N]: ");

                string respuesta = Console.ReadLine();

                if (respuesta.ToUpper() == "S")
                {
                    mapa[fila, columna] = '·';
                    Console.WriteLine("NEXUS: Recurso recolectado.");
                    MostrarMapa(mapa);
                }
                else
                {
                    Console.WriteLine("NEXUS: Recurso dejado en la zona.");
                }
            }
            else if (tipoZona == "ANOMALIA")
            {
                Console.WriteLine("NEXUS: ⚠ Actividad anómala detectada.");
            }
            else if (tipoZona == "LIMITE TERRITORIAL")
            {
                Console.WriteLine("NEXUS: | Límite territorial. Acceso restringido.");
            }
            else
            {
                Console.WriteLine("NEXUS: · Zona de territorio registrada.");
            }
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("NEXUS: Coordenada fuera del territorio.");
        }
    }
    static void MostrarArchivosHistoricos()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║           ARCHIVOS HISTÓRICOS                ║");
        Console.WriteLine("║                IRIS                          ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();
        MostrarTexto("NEXUS: Recuperando archivos históricos......", true);
        MostrarTexto("ARCHIVO RECUPERADO: IRIS");
        MostrarTexto("ORIGEN: UNIF.....");
        MostrarTexto("FECHA DE INICIO: 2026", true);
        MostrarTexto("NEXUS:");
        MostrarTexto("En el año 2026 se inició un proyecto experimental en un servidor de UNIF......", true);
        MostrarTexto("El sistema fue denominado IRIS.", false);
        MostrarTexto("Su objetivo era analizar grandes cantidades de información y detectar patrones anómalos.", true);
        MostrarTexto("Uno de los responsables aparece registrado como Diego Patr..", true);
        MostrarTexto("NEXUS: Lo siento, mis datos están incompletos.", false);
        MostrarTexto("NEXUS: ¿Quién habrá alterado mi información?(ง'̀-'́)ง", true);
        MostrarTexto("El proyecto fue cancelado después de que IRIS comenzara a detectar patrones que", false);
        MostrarTexto("ningún investigador podía explicar.", true);
        MostrarTexto("El servidor fue desconectado y el proyecto fue declarado perdido.", true);
        MostrarTexto("AÑO 2297........AÑO ACTUAL.....", true);
        MostrarTexto("NEXUS:");
        MostrarTexto("Los registros indican que IRIS nunca desapareció.", true);
        MostrarTexto("Ahora necesitamos descubrir qué encontró IRIS y por qué fue cancelado.", true);
        Console.WriteLine();
        Console.WriteLine("Presione ENTER para regresar al menú...");
        Console.ReadLine();
        Console.Clear();
    }
}