using System.Threading;

class Program
{
    static Random rng = new Random();
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

        Console.WriteLine("╔══════════════════════════════════════════════╗");
        Console.WriteLine("║       ⚠ ALERTA DE INTERFERENCIA IRIS        ║");
        Console.WriteLine("╠══════════════════════════════════════════════╣");
        Console.WriteLine("║ ESTABILIDAD: " + estabilidad + "%");
        Console.WriteLine("║                                              ║");
        Console.WriteLine("║ NEXUS recomienda desconexion inmediata.      ║");
        Console.WriteLine("╚══════════════════════════════════════════════╝");
    }

    static bool EvaluarRiesgoIris(int energia, int estabilidad)
    {
        int probabilidad = 5;

        if (energia < 30)
        {
            probabilidad += 20;
        }

        if (estabilidad < 30)
        {
            probabilidad += 25;
        }

        if (energia < 30 && estabilidad < 30)
        {
            probabilidad += 30;
        }

        int tirada = rng.Next(1, 101);
        return tirada <= probabilidad;
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
        MostrarLogo();

        static void MostrarBarra(string nombre, int valor)
        {
            int bloques = valor / 5;

            Console.Write(nombre + " [");

            for (int i = 0; i < 20; i++)
            {
                if (i < bloques)
                {
                    Console.Write("█");
                }
                else
                {
                    Console.Write("░");
                }
            }

            Console.WriteLine("] " + valor + "%");
        }
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("           AÑO 2297.");
        Console.WriteLine("           Bienvenido, Cadete de Explorador.");
        Console.WriteLine();
        Console.ResetColor();

        string nombre;
        while (true)
        {
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

            while (conectado)
            {
                Console.Clear();

                Console.WriteLine("╔══════════════════════════════════════════════╗");
                Console.WriteLine("║             N E X U S   S Y S T E M          ║");
                Console.WriteLine("╠══════════════════════════════════════════════╣");
                Console.WriteLine("║ EXPLORADOR:  " + nombre);
                Console.WriteLine("║ REALIDAD:    " + realidad);
                Console.WriteLine("╠══════════════════════════════════════════════╣");
                Console.WriteLine("║ ENERGIA:     " + energia + "%");
                Console.WriteLine("║ ESTABILIDAD: " + estabilidad + "%");
                Console.WriteLine("╠══════════════════════════════════════════════╣");
                Console.WriteLine("║                                              ║");
                Console.WriteLine("║  [1] Explorar realidad                       ║");
                Console.WriteLine("║  [2] Analizar anomalia                       ║");
                Console.WriteLine("║  [3] Consultar estado                        ║");
                Console.WriteLine("║  [4] Recuperar energia                       ║");
                Console.WriteLine("║  [5] Intentar desconexion                    ║");
                Console.WriteLine("║                                              ║");
                Console.WriteLine("╚══════════════════════════════════════════════╝");

                Console.Write("Seleccione una operacion: ");
                string opcion = Console.ReadLine();
                switch (opcion)
                {
                    case "1":
                        if (energia < 15)
                        {
                            Console.WriteLine("NEXUS: Energia insuficiente para explorar la realidad.");
                        }
                        else
                        {
                            int costoExploracion = rng.Next(10, 21);
                            energia = Clamp(energia - costoExploracion, 0, 100);

                            int tiradaExploracion = rng.Next(1, 101);
                            if (tiradaExploracion > estabilidad)
                            {
                                int perdida = rng.Next(5, 16);
                                estabilidad = Clamp(estabilidad - perdida, 0, 100);
                                Console.WriteLine("La exploracion sufre turbulencias. Estabilidad reducida en " + perdida + ".");
                            }
                            else
                            {
                                int ganancia = rng.Next(0, 6);
                                estabilidad = Clamp(estabilidad + ganancia, 0, 100);
                                Console.WriteLine("Exploracion completada sin incidentes.");
                            }
                        }
                        break;

                    case "2":
                        if (energia < 10)
                        {
                            Console.WriteLine("NEXUS: Energia insuficiente para analizar la anomalia.");
                        }
                        else
                        {
                            energia = Clamp(energia - 10, 0, 100);

                            int hallazgo = rng.Next(1, 101);
                            if (hallazgo <= 50)
                            {
                                int indice = rng.Next(0, fragmentosIris.Length);
                                Console.WriteLine("NEXUS: " + fragmentosIris[indice]);
                            }
                            else
                            {
                                Console.WriteLine("NEXUS: No se detectaron patrones significativos.");
                            }
                        }
                        break;

                    case "3":
                        Console.WriteLine("EXPLORADOR: " + nombre);
                        Console.WriteLine("REALIDAD: " + realidad);
                        Console.WriteLine("ENERGIA: " + energia);
                        Console.WriteLine("ESTABILIDAD: " + estabilidad);
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

                            Console.WriteLine("Energia recuperada en " + recuperado + ". Estabilidad ajustada en -" + desgaste + ".");
                        }
                        break;

                    case "5":
                        Console.WriteLine("Has seleccionado desconexion.");
                        conectado = false;
                        break;

                    default:
                        Console.WriteLine("ERROR: Operacion no valida.");
                        break;
                }

                if (conectado && opcion != "3")
                {
                    if (EvaluarRiesgoIris(energia, estabilidad))
                    {
                        AnomaliaDetectada?.Invoke(estabilidad);
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