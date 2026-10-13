# NEXUS

NEXUS es una aventura de exploración y combate por consola, desarrollada en C#. El jugador controla a un cadete que explora la zona GENESIS, se enfrenta a enemigos, reúne recursos y completa misiones desde la Base Operativa.

El proyecto organiza partes del juego en clases, como `Cadete`, `Enemigo`, `Combate`, `MapaGenesis`, `BaseOperativa` e `Iris`.

## Requisitos

- .NET 10 SDK
- Windows para las dependencias de vídeo incluidas en el proyecto

## Ejecutar el proyecto

Desde la carpeta que contiene `Nexus_1.slnx`:

```bash
dotnet restore
dotnet run --project "Nexus_1/Nexus_1.csproj"
```

También puedes abrir `Nexus_1.slnx` en Visual Studio compatible con .NET 10 y ejecutar el proyecto `Nexus_1`.

## Controles

Los controles se muestran en pantalla durante la partida. En los combates clásicos, las opciones son:

- `1`: atacar
- `2`: defender
- `3`: huir

En los combates animados:

- `A`: atacar durante el turno del cadete
- `D`: defender durante el turno del enemigo
- `H`: huir durante el turno del cadete

## Recursos gráficos y animaciones

El programa incluye rutas relativas para cargar animaciones y recursos visuales. Si esos archivos no están presentes, algunas escenas animadas pueden omitirse y el juego puede continuar con la presentación alternativa disponible.

## Estructura

- `Nexus_1/Program .cs`: lógica del juego, clases y punto de entrada.
- `Nexus_1/Nexus_1.csproj`: configuración y dependencias del proyecto.
- `docs/`: manual técnico y diagramas del sistema.

Consulta el [índice de documentación](docs/README.md) para acceder al [manual técnico](docs/manual-tecnico.md) y al [diagrama de arquitectura](docs/diagrams/architecture.mmd).
