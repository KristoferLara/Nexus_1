using System.Threading;
using System.Timers;

class Program
{
    static Random rng = new Random();
    static int turnosBloqueoIris = 0;
    static bool anomaliaLocalizada = false;
    static string ubicacionAnomalia = "Desconocida";
    static bool tieneDetector = true;
    static bool tieneDron = false;
    static bool tienePEM = false;
    static bool tieneEnergia = true;
    static bool tieneDesconectar = true;
    static void EscribirConEfecto(string texto, int velocidadMilisegundos = 50)
    {
        foreach (char letra in texto)
        {
            Console.Write(letra);
            Thread.Sleep(velocidadMilisegundos);
        }
        Console.WriteLine();
    }
    static void MostrarTexto(string mensaje, bool espacioExtra =false, int pausa = 800)
    {
        EscribirConEfecto(mensaje);
        Thread.Sleep(pausa);

        if (espacioExtra)
        {
            Console.WriteLine();
        }
    }
        
    static event Action<int> AnomaliaDetectada;
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

        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║       ⚠⚠⚠  ALERTA CRÍTICA  ⚠⚠⚠       ║");
        Console.WriteLine("║             INTERFERENCIA: IRIS"               );
        Console.WriteLine("╠══════════════════════════════════════════════╣");
        Console.WriteLine("║ ESTABILIDAD: " + estabilidad + "%");
        Console.WriteLine("║ IRIS HA INTERRUMPIDO LOS SISTEMAS DE NEXUS   ║");
        Console.WriteLine("║ NEXUS recomienda desconexion inmediata.      ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");
        Console.ResetColor();
    }
    static void MostrarBarra(string nombre, int valor)
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

        Console.WriteLine("║ " + nombre + ": [" + barra + "] " + valor + "%");
    }

    static bool EvaluarRiesgoIris(int energia, int estabilidad)
    {
        if (turnosBloqueoIris > 0)
        {
            return false;
        }
        int probabilidad = 10;

        if (energia < 30)
        {
            probabilidad += 20;
        }

        if (estabilidad < 30)
        {
            probabilidad += 25;
        }
        if (anomaliaLocalizada)
        {
            probabilidad += 15;
        }
        return rng.Next(1, 101) <= probabilidad;
    }

    static int Clamp(int valor, int minimo, int maximo)
    {
        if (valor < minimo) return minimo;
        if (valor > maximo) return maximo;
        return valor;
    }

    static void MostrarLogo()
    {
        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        Console.Write(@"
                       ╔═════════════════════════════════════════════╗
                       ║  ███╗   ██╗███████╗██╗  ██╗                 ║
                       ║  ████╗  ██║██╔════╝╚██╗██╔╝  TRAINING       ║
                       ║  ██╔██╗ ██║█████╗   ╚███╔╝   SYSTEM         ║
                       ║  ██║╚██╗██║██╔══╝   ██╔██╗                  ║
                       ║  ██║ ╚████║███████╗██╔╝ ██╗                 ║
                       ║  ╚═╝  ╚═══╝╚══════╝╚═╝  ╚═╝                 ║
                       ╚═════════════════════════════════════════════╝
    ");
        Console.ResetColor();        
        }

    static void Main()
    {

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════╗");
        Console.WriteLine("║         NEXUS   SYSTEM          ║");
        Console.WriteLine("╚════════════════════════════════════════╝");

        Console.WriteLine();
        Console.WriteLine("           AÑO 2297");
        Console.WriteLine();


        MostrarTexto("NEXUS: Recuperando archivos historicos......", espacioExtra: true);

        MostrarTexto("ARCHIVO RECUPERADO: IRIS");
        MostrarTexto("ORIGEN: UNIFRANZ");
        MostrarTexto("FECHA DE INICIO: 2026", espacioExtra: true);

        MostrarTexto("NEXUS:");
        MostrarTexto("En el año 2026 se inicio un proyecto experimental en un servidor de UNIFRANZ", espacioExtra: true);

        MostrarTexto("El sistema fue denominado IRIS Su objetivo era analizar grandes cantidades");
        MostrarTexto("de informacion y detectar patrones anomalos.", espacioExtra: true);

        MostrarTexto("El proyecto fue cancelado despues de que IRIS comenzara a detectar patrones que");
        MostrarTexto("ningun investigador podia explicar.", espacioExtra: true);

        MostrarTexto("El servidor fue desconectado y el proyecto fue declarado perdido.", espacioExtra: true);

        MostrarTexto("AÑO 2297........AÑO ACTUAL.....", espacioExtra: true);

        MostrarTexto("NEXUS:");
        MostrarTexto("Los registros indican que IRIS nunca desaparecio", espacioExtra: true);

        MostrarTexto("Ahora necesitamos decubrir que encontro IRIS y porque se cancelo.", espacioExtra: true);

        Console.WriteLine("Presiona ENTER y descubramoslo juntos...");
        Console.ReadLine();

        Console.Clear();

        Console.ResetColor();

        string nombre;
        while (true)
        {
            MostrarLogo();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("           Ingrese su nombre: ");
            Console.ResetColor();
            nombre = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(nombre))
            {
                Console.WriteLine("Error: El nombre no puede estar vacio.");
            }
            else
            {
                break;
            }
        }

        int edad;
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("           Ingrese su edad: ");
            Console.ResetColor();
            bool edadValida = int.TryParse(Console.ReadLine(), out edad);

            if (!edadValida)
            {
                Console.WriteLine("Error: Debe ingresar un numero.");
            }
            else if (edad <= 0 || edad > 120)
            {
                Console.WriteLine("Error: La edad ingresada no es valida.");
            }
            else
            {
                break;
            }
        }

        string realidad;
        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("           Ingrese la realidad asignada: ");
            Console.ResetColor();
            realidad = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(realidad))
            {
                Console.WriteLine("Error: La realidad no puede estar vacia.");
            }
            else
            {
                break;
            }
        }

        int energia;

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("           Ingrese el nivel de energia:  ");
            Console.ResetColor();

            bool energiaValida = int.TryParse(Console.ReadLine(), out energia);

            if (!energiaValida)
            {
                Console.WriteLine("Error: Debe ingresar un numero");
            }
            else if (energia < 0 || energia > 100)
            {
                Console.WriteLine("Error: La energia debe estar entre 0 y 100.");
            }
            else
            {
                Console.WriteLine("         Nivel de energia registrado correctamente.");
                break;
            }
        }

        int estabilidad;

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("           Ingrese el nivel de estabilidad:  ");
            Console.ResetColor();

            bool estabilidadValida = int.TryParse(Console.ReadLine(), out estabilidad);

            if (!estabilidadValida)
            {
                Console.WriteLine("Error: Debe ingresar un numero");
            }
            else if (estabilidad < 0 || estabilidad > 100)
            {
                Console.WriteLine("Error: La estabilidad debe estar entre 0 y 100.");
            }
            else
            {
                Console.WriteLine("         Nivel de estabilidad registrado correctamente.");
                break;
            }
        }

        bool autorizado;
        if (edad >= 18 && energia >= 40 && estabilidad >= 50)
        {
            autorizado = true;
        }
        else
        {
            autorizado = false;
        }
        Console.WriteLine();

        Console.WriteLine("========================================");
        Console.WriteLine("EVALUACION DE NEXUS");
        Console.WriteLine("========================================");
        if (autorizado)
        {
            Console.WriteLine("ESTADO DE INMERSION AUTORIZADA");
            Console.WriteLine("NEXUS: Todos los parametros son aceptables.");
        }
        else
        {
            Console.WriteLine("ESTADO DE INMERSION RECHAZADA");

            if (edad < 18)
            {
                Console.WriteLine("Motivo: Edad no autorizada.");
            }

            if (energia < 40)
            {
                Console.WriteLine("Motivo: Nivel de energia insuficiente.");
            }

            if (estabilidad < 50)
            {
                Console.WriteLine("Motivo: Nivel de estabilidad insuficiente.");
            }
        }

        if (autorizado)
        {
            Console.WriteLine();

            Console.WriteLine("========================================");
            Console.WriteLine("ACCESO A NEXUS AUTORIZADO");
            Console.WriteLine("========================================");
            Console.WriteLine("Presione ENTER para iniciar la simulacion...");
            Console.ReadLine();

            AnomaliaDetectada += DispararAlertaIris;

            string[] fragmentosIris =
            {
                "IRIS parece originarse en la capa mas profunda de la realidad simulada.",
                "Se detectan patrones repetitivos que no corresponden al entorno base.",
                "IRIS podria estar reaccionando a la presencia del Explorador.",
                "Los registros historicos muestran incidentes similares en otras realidades."
            };

            bool conectado = true;
            bool tieneDron = false;
            bool tienePulso = false;
            bool pulsoActivo = false;
            int turnosPulso = 0;

            while (conectado)
            {
                Console.Clear();

                Console.WriteLine("╔══════════════════════════════════════════════╗");
                Console.WriteLine("║             N E X U S   S Y S T E M          ║");
                Console.WriteLine("╠══════════════════════════════════════════════╣");
                Console.WriteLine("║ EXPLORADOR:  " + nombre);
                Console.WriteLine("║ REALIDAD:    " + realidad);
                Console.WriteLine("╠══════════════════════════════════════════════╣");
                MostrarBarra("Energia", energia);
                MostrarBarra("Estabilidad", estabilidad);
                Console.WriteLine();
                Console.WriteLine("EQUIPAMIENTO:");

                Console.WriteLine("Detector: " + (tieneDetector ? "DISPONIBLE" : "NO DISPONIBLE"));
                Console.WriteLine("DRON: " + (tieneDron ? "DISPONIBLE" : "NO DISPONIBLE"));
                Console.WriteLine("PEM:" + (tienePEM ? "DISPONIBLE" : "NO DISPONIBLE"));
                Console.WriteLine("ENERGIA:" + (tieneEnergia ? "DISPONIBLE" : "NO DISPONIBLE"));


                if (anomaliaLocalizada)
                {
                    Console.WriteLine("║ ANOMALIA: LOCALIZADA - " + ubicacionAnomalia);
                }  
                else
                {
                   Console.WriteLine("║ ANOMALIA: NO LOCALIZADA");
                }
                if (turnosBloqueoIris > 0)
                {
                   Console.WriteLine("║ IRIS: BLOQUEADA - " + turnosBloqueoIris + "TURNOS");
                }
                else
                {
                   Console.WriteLine("║ IRIS: ACTIVA");  
                }
                Console.WriteLine("╠══════════════════════════════════════════════╣");
                Console.WriteLine("║                                              ║");
                Console.WriteLine("║  [1] ▶Iniciar deteccion                      ║");
                Console.WriteLine("║  [2] ◉Desplegar dron                         ║");
                Console.WriteLine("║  [3]⚡Emitir pulso electromagnetico          ║");
                Console.WriteLine("║  [4] +Protocolo de recuperacion              ║");
                Console.WriteLine("║  [5] ⏻Intentar desconexion                  ║");
                Console.WriteLine("║                                              ║");
                Console.WriteLine("╚══════════════════════════════════════════════╝");

                Console.Write("Seleccione una operacion: ");
                string opcion = Console.ReadLine();
                switch (opcion)
                {
                    case "1":
                        if (pulsoActivo)
                        {
                            Console.WriteLine();
                            Console.WriteLine("NEXUS: El pulso electromagnetico esta activo.");
                            Console.WriteLine("Nexus: Los sistemas de exploracion estan bloqueados.");
                            break;
                        }
                        if (energia <= 15)
                        {
                            Console.WriteLine("NEXUS: ⚡Energia insuficiente para explorar la realidad.");
                        }
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("NEXUS: Iniciando exploracion");
                            int costoExploracion = rng.Next(10, 21);
                            energia = Clamp(energia - costoExploracion, 0, 100);
                            Thread.Sleep(500);
                            Console.WriteLine("NEXUS: Escaneando entorno...");
                            Thread.Sleep(500);
                            Console.WriteLine("NEXUS: Analizando fluctuaciones...");
                            int tiradaExploracion = rng.Next(1, 101);
                            if (anomaliaLocalizada) 
                            {
                                Console.WriteLine();
                                Console.WriteLine("NEXUS: Coordenadas recibidas del dron.");
                                Console.WriteLine("NEXUS: Dirigiendose a" + ubicacionAnomalia + "...");
                                Thread.Sleep(700);
                                Console.WriteLine("NEXUS: ☢️ Anomalia localizada en " + ubicacionAnomalia);
                                int impactoanomalia = rng.Next(5, 11);
                                    estabilidad = Clamp(estabilidad - impactoanomalia, 0, 100);
                                    Console.WriteLine("NEXUS: La presencia de la anomalia afecta la estabilidad");
                                Console.WriteLine("NEXUS: Estabilidad -" + impactoanomalia);
                            }
                            else 
                            {
                                if (tiradaExploracion <= 40)
                                {
                                    Console.WriteLine("NEXUS: ☢️Anomalia detectada.");
                                }
                                else
                                {
                                    Console.WriteLine("NEXUS: No se detectaron anomalias.");
                                }   
                            }
                                if (tiradaExploracion > estabilidad)
                            {
                                int perdida = rng.Next(5, 16);
                                estabilidad = Clamp(estabilidad - perdida, 0, 100);
                                Console.WriteLine("La exploracion sufrio turbulencias. Estabilidad -" + perdida + ".");
                            }
                            else
                            {
                                Console.WriteLine("Exploracion completada sin incidentes.");
                            }
                        }
                            
                        break;

                    case "2":
                        if (pulsoActivo)
                        {
                            Console.WriteLine();
                            Console.WriteLine("NEXUS: El pulso electromagnetico esta activo.");
                            Console.WriteLine("NEXUS: Los sistemas del dron no responden.");
                            break;
                        }

                        if (!tieneDron)
                        {
                            Console.WriteLine();
                            Console.WriteLine("NEXUS: Señal de tecnologia desconocida detectada.");
                            Console.WriteLine("NEXUS: Explorador, investigue la ubicacion.");

                            tieneDron = true;

                            Console.WriteLine();
                            Console.WriteLine("OBJETO RECUPERADO: DRON DE RECONOCIMIENTO");
                            Console.WriteLine("NEXUS: El dispositivo todavia parece funcional");

                            Console.WriteLine();
                            Console.WriteLine("NEXUS: Se ha detectado modulo electromagnetico");
                            Console.WriteLine("NUEVA CAPACIDAD: PULSO ELECTROMAGNETICO");

                            tienePulso = true;
                        }
                        else if (energia < 5)
                        {
                            Console.WriteLine();
                            Console.WriteLine("NEXUS: Energia insuficiente para desplegar el dron.");
                        }
                        else
                        {
                            energia = Clamp(energia - 5, 0, 100);
                            Console.WriteLine();
                            Console.WriteLine("NEXUS: Desplegando dron de reconocimiento...");
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
                                ubicacionAnomalia = sectores[sectorEncontrado];
                                anomaliaLocalizada = true;
                                Console.WriteLine();
                                Console.WriteLine("DRON: ☢SEÑAL DETECTADA.");
                                Console.WriteLine("DRON: Anomalia detectada.");
                                Console.WriteLine("DRON: Ubicacion: " + ubicacionAnomalia);
                            }
                            else
                            {
                                Console.WriteLine();
                                Console.WriteLine("DRON: No se detectaron señales");
                            }
                        }
                        break;

                    case "3":
                        if (!tienePEM)
                        {
                            Console.WriteLine();
                            Console.WriteLine("NEXUS: Generador PEM no disponible.");
                            Console.WriteLine("NEXUS: Debes encontrar tecnologia adicional.");
                            break;
                        }

                        if (pulsoActivo)
                        {
                            Console.WriteLine();
                            Console.WriteLine("NEXUS: El pulso electromagnrtico ya esta activo.");
                            break;
                        }

                        if(energia < 15)
                        {
                            Console.WriteLine();
                            Console.WriteLine("NEXUS: Energia insuficiente para emitir el pulso.");
                            break;
                        }         
                        else 
                        {
                                energia = Clamp(energia - 15, 0, 100);
                                Console.WriteLine();
                                Console.WriteLine("NEXUS: Cargando pulso electromagnetico...");
                                Thread.Sleep(500);
                                Console.WriteLine("NEXUS: Potencia al 30%...");
                                Thread.Sleep(500);
                                Console.WriteLine("NEXUS: Potencia al 70%...");
                                Thread.Sleep(500);
                                Console.WriteLine("NEXUS: ⚡PULSO ELECTROMAGNETICO EMITIDO.");

                                turnosBloqueoIris = 3;
                                turnosPulso = 3;
                                pulsoActivo = true;

                                Console.WriteLine("IRIS: ERROR DE SEÑAL");
                                Console.WriteLine("NEXUS: IRIS Ha perdido temporalmente la conexion.");
                                Console.WriteLine("NEXUS:Bloqueo activo durante 3 turnos");
                        }
                        break;

                    case "4":
                        if (energia >= 100)
                        {
                            Console.WriteLine("NEXUS: El nivel de energia ya esta al maximo.");
                        }
                        else
                        {
                            int recuperado = rng.Next(15, 26);
                            energia = Clamp(energia + recuperado, 0, 100);

                            int desgaste = rng.Next(0, 4);
                            estabilidad = Clamp(estabilidad - desgaste, 0, 100);
                            Console.WriteLine("NEXUS: Iniciando protocolo de recuperacion...");
                            Console.WriteLine("NEXUS:Energia restaurada en " + recuperado + ". Estabilidad ajustada en -" + desgaste + ".");
                        }
                        break;

                    case "5":
                        Console.WriteLine();
                        Console.WriteLine("NEXUS: Solicitud de desconexion recibida.");
                        if (estabilidad >= 30)
                        {
                            Console.WriteLine("NEXUS: Condiciones de desconexion aceptables.");
                            Console.WriteLine("NEXUS: Iniciando desconexion...");
                            Thread.Sleep(500);
                            Console.WriteLine("3...");
                            Thread.Sleep(500);
                            Console.WriteLine("2...");
                            Thread.Sleep(500);
                            Console.WriteLine("1...");
                            Console.WriteLine("CONEXION FINALIZADA.");
                            conectado = false;
                        }
                        else
                        {  
                            Console.WriteLine("NEXUS: ☢️DESCONEXION INSEGURA");
                            Console.WriteLine("NEXUS: La estabilidad debe ser superior a 30%.");
                        }                       
                        break;
                }

                if (conectado && opcion != "3")
                {
                    if (turnosBloqueoIris > 0)
                    {
                        turnosBloqueoIris--;
                        if (turnosBloqueoIris > 0)
                        {
                            Console.WriteLine();
                            Console.WriteLine("NEXUS: Bloqueo IRIS activo. Turnos restantes:" +turnosBloqueoIris);
                        }
                        else
                        {
                            Console.WriteLine();
                            Console.WriteLine("IRIS: Conexion restablecida.");
                            Console.WriteLine("NEXUS: El bloqueo electromagnetico ha terminado.");

                            pulsoActivo = false;
                        }
                    }
                    if (EvaluarRiesgoIris(energia, estabilidad))
                    {
                        AnomaliaDetectada.Invoke(estabilidad);
                    }

                    if (energia <= 0 || estabilidad <= 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("NEXUS: Condicion critica detectada. Forzando desconexion de emergencia.");
                        conectado = false;
                    }
                }

                if (conectado)
                {
                    Console.WriteLine();
                    Console.WriteLine("Presione ENTER para continuar...");
                    Console.ReadLine();
                }
            }

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("SESION FINALIZADA");
            Console.WriteLine("========================================");
        }
    }
}