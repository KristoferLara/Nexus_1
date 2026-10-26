# Arquitectura actual de NEXUS (H3)

## Base de inspección

Inspección del árbol de la rama `master` del repositorio [KristoferLara/Nexus_1](https://github.com/KristoferLara/Nexus_1/tree/master), cuyo commit inspeccionado fue `c6f886d` (19-10-2026). Las observaciones describen el contenido visible de esa revisión, no una ejecución local.

## Estructura real

- `Nexus_1.slnx`: solución de .NET.
- `Nexus_1/Nexus_1.csproj`: proyecto SDK de consola, `OutputType=Exe`, `TargetFramework=net10.0`, nullable y usings implícitos habilitados. Declara `LibVLCSharp 3.10.1`, `System.Drawing.Common 10.0.11` y `VideoLAN.LibVLC.Windows 3.0.23.1`.
- `Nexus_1/Program .cs`: punto de entrada, tipos del juego, estado, coordinación y presentación están agrupados en un archivo grande (aprox. 140 KB en el árbol consultado).
- `docs/manual-tecnico.md` y `docs/diagrams/architecture.mmd`: documentación heredada previa.
- `.gitignore` en la raíz y también bajo `Nexus_1/`: exclusiones de artefactos de desarrollo ya existen en la rama inspeccionada.

## Clases y responsabilidades observadas en `Program .cs`

Las clases son internas a `Program`, no tipos públicos distribuidos en archivos independientes:

| Tipo | Responsabilidad observable |
|---|---|
| `EstadoJuego` | Reúne nombre/realidad/conexión, `Iris`, clase, mapa GENESIS, exploración, base, lista de enemigos, cadete, lista y misión actual. Mantiene el índice secuencial de misión. |
| `Cadete` | Nombre, energía, estabilidad, vida, poder, realidad, recursos, habilidad especial y XP. La propiedad `Nivel` se calcula como XP / 100 + 1. Incluye métodos para consumir/recuperar recursos, recibir daño, ganar XP y activar capacidad. |
| `Enemigo` | Posición de cuadrícula, tipo, efecto, vida y condición derrotado; limita vida mínima a cero al recibir daño. |
| `Combate` | Reglas para ataque, contraataque, defensa, huida y efectos de enemigos. El daño de ataque del jugador observado está definido por constante. |
| `MapaGenesis` | Cuadrícula de caracteres, zonas exploradas, posición del jugador y revelado local. Metros por celda: 5. |
| `BaseOperativa` | Cuadrícula independiente, posición y validación de movimiento contra paredes. |
| `ExploracionGenesis` | Coordina sesión de exploración; recibe mapa, enemigos y cadete, y lleva distancia/eventos asociados. |
| `Iris` | Bloqueo temporal y ubicación/estado de anomalía localizada. |
| `Botiquin` | Nombre y curación; valida nombre y valor positivo. |
| `Mision` | ID, nombre, descripción/objetivo, enemigo objetivo, contexto y recompensa configurada. `EstadoJuego` inicializa tres objetivos: LIQUIDO, FUEGO, VENENO. |
| `InterfazNexus` | Presenta marcos, estado y mensajes en la consola. El comentario de clase señala que no es responsable de los bucles de movimiento ni de lógica. |
| `AnimacionCombate` | Carga/avanza estados de animación para el combate animado, condicionado a recursos disponibles. |

El resto de las decisiones de flujo se coordinan principalmente desde métodos estáticos de `Program`, lo que hace que `Program` conozca muchos subsistemas. La UI y el dominio tienen una separación parcial, no una arquitectura por capas completa.

## Funcionalidades H3 observables

- Aventura de consola: creación/selección del cadete, base operativa, exploración por cuadrícula en GENESIS y encuentro con enemigos.
- Combate por turnos con ataque, defensa, huida y efectos especiales; ciertas rutas admiten animación en consola apoyada en recursos.
- Botiquines, energía, estabilidad, vida, poder, capacidad especial y recolección de recursos.
- Misiones secuenciales LIQUIDO → FUEGO → VENENO. El comentario del estado documenta que el índice avanza cuando el terminal de base entrega recompensa, no al derrotar al objetivo.
- XP y nivel derivados; estado IRIS para interferencia/anomalía.

El README de H3 describe una ruta de ejecución con `dotnet restore` y `dotnet run --project "Nexus_1/Nexus_1.csproj"`; los requisitos declarados son SDK .NET 10 y Windows recomendado por la dependencia de vídeo.

## Límites técnicos que afectan la evolución

1. **Monolito de fuente:** tipos anidados y métodos coordinadores en un único archivo dificultan encontrar responsabilidades y revisar cambios.
2. **UI acoplada a consola:** mensajes, lectura de teclas, esperas y limpieza de consola no se reutilizan como controles/repintado WinForms.
3. **Modelo de movimiento distinto:** el mapa actual es una cuadrícula de exploración; no implementa salto con gravedad ni colisionadores para plataformas.
4. **Combate por turnos:** las reglas pueden inspirar daño/efectos, pero necesitan una interfaz de acciones en tiempo real y un reloj de actualización.
5. **Persistencia ausente en el proyecto consultado:** no se observó capa SQL, repositorio ni carga/guardado de partida.
6. **Sin cuenta autenticada:** el nombre/perfil de juego no es autenticación contra un servidor. La propuesta local no debe presentarse como inicio de sesión seguro en línea.
7. **Assets condicionados por rutas:** animaciones existentes dependen de recursos externos al código y de rutas válidas; la nueva escena debe tener rutas empaquetadas y fallback explícito.
8. **Dependencias a revisar:** las librerías de vídeo actuales no deben heredarse automáticamente en WinForms si la escena no las necesita.

## Ruta de migración sugerida

Conservar el proyecto H3 como prototipo ejecutable y fuente de reglas. Extraer poco a poco las entidades esenciales (`Cadete`, `Enemigo`, `Mision`, `Combate`) a una biblioteca de dominio sin dependencias de consola. Implementar una nueva app WinForms que consuma esa biblioteca y un repositorio SQL Server. Migrar un caso vertical (perfil → misión → nivel → jefe → recompensa → guardar/cargar) antes de incorporar contenido adicional.

## Fuentes del repositorio

- [Código H3: Program .cs](https://github.com/KristoferLara/Nexus_1/blob/master/Nexus_1/Program%20.cs)
- [Proyecto y dependencias](https://github.com/KristoferLara/Nexus_1/blob/master/Nexus_1/Nexus_1.csproj)
- [README actual](https://github.com/KristoferLara/Nexus_1/blob/master/README.md)
