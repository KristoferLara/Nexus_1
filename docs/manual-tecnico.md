# Manual técnico de NEXUS

## Propósito

NEXUS es un juego de exploración, combate y misiones para consola, implementado en C#. Este documento resume la estructura técnica visible en `Nexus_1/Program .cs` y ofrece instrucciones para preparar y ejecutar el proyecto.

## Requisitos

- SDK de .NET 10
- Windows recomendado: el proyecto incluye el paquete `VideoLAN.LibVLC.Windows`
- Terminal capaz de mostrar los caracteres y emojis usados por la interfaz

## Restaurar y ejecutar

Desde la raíz del repositorio:

```sh
dotnet restore
dotnet run --project "Nexus_1/Nexus_1.csproj"
```

También se puede abrir `Nexus_1.slnx` en una versión de Visual Studio compatible con .NET 10 y ejecutar el proyecto `Nexus_1`.

## Estructura del código

La lógica principal está concentrada en `Nexus_1/Program .cs`. Dentro de ese archivo se encuentran el punto de entrada, el estado de la partida, clases de dominio, coordinación de exploración y combate, y presentación por consola.

Entre las clases principales están:

- `EstadoJuego`: mantiene el estado de la sesión y referencias a cadete, mapas, misiones, enemigos y sistemas de exploración.
- `Cadete`: representa al jugador y mantiene energía, estabilidad, vida, poder, experiencia y capacidad especial. Los cambios de varios atributos pasan por métodos de la clase.
- `Enemigo`: agrupa tipo, ubicación, efecto y vida; `RecibirDanio` limita la vida a cero como mínimo.
- `Combate`: coordina las reglas de enfrentamiento entre un cadete y un enemigo.
- `MapaGenesis` y `BaseOperativa`: encapsulan cuadrículas y posiciones del jugador en sus respectivas áreas.
- `ExploracionGenesis`: coordina eventos de exploración de GENESIS.
- `Iris`: conserva el estado de bloqueo y la anomalía localizada.
- `Mision`: representa los objetivos y el estado de avance de las misiones.

## Flujo de juego

1. El programa solicita los datos necesarios y prepara el estado de juego.
2. El jugador elige opciones desde la interfaz de consola.
3. La exploración actualiza la posición y puede activar encuentros o eventos.
4. En combate, las acciones modifican el estado del cadete y del enemigo.
5. Las misiones avanzan según sus objetivos y recompensas.

El orden exacto de pantallas y acciones depende de las opciones que el jugador elija durante la partida. Consulta los mensajes de la consola para los controles disponibles en cada contexto.

## Organización orientada a objetos

El código agrupa datos y comportamientos relacionados en clases. Por ejemplo, `Cadete` administra sus recursos y `Enemigo` su propia vida. Las propiedades con `private set` limitan dónde puede modificarse el estado, mientras que los métodos públicos ofrecen operaciones controladas.

Las instancias se crean con constructores; por ejemplo, los enemigos reciben ubicación, tipo y vida inicial. Las colecciones `List<Enemigo>` y `List<Mision>` representan grupos tipados de elementos del juego.

## Diagrama de arquitectura

El diagrama editable está en [diagrams/architecture.mmd](diagrams/architecture.mmd). Incluye enlaces a las secciones correspondientes del archivo fuente.

## Dependencias

El archivo `Nexus_1/Nexus_1.csproj` declara:

- `LibVLCSharp`
- `System.Drawing.Common`
- `VideoLAN.LibVLC.Windows`

La carga de recursos de animación depende de que los archivos requeridos estén disponibles en las rutas que consulta el programa. Si no se encuentran, algunas animaciones pueden omitirse o el juego puede recurrir a una presentación alternativa.

## Preparación para el Hito 3

Para explicar el diseño del proyecto, practica describiendo con tus propias palabras:

- la diferencia entre clase e instancia;
- cómo se inicializa un objeto mediante un constructor;
- cómo una propiedad con setter privado protege estado;
- cómo se usan listas genéricas para reunir enemigos y misiones;
- cómo se reparten responsabilidades entre cadete, enemigo, combate y mapa.

Este código sirve como material de estudio; para demostrar dominio del tema, intenta modificar o recrear ejemplos sin copiar la implementación.
