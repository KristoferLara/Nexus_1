# NEXUS

NEXUS es un prototipo narrativo de exploración y supervivencia de ciencia ficción. La base funcional para esta documentación está en la rama `feature/mi-codigo-csharp`, donde el jugador registra un cadete, elige Génesis, administra energía y estabilidad, explora anomalías y enfrenta amenazas relacionadas con IRIS desde consola.

## Código de referencia

La rama `feature/mi-codigo-csharp` contiene el proyecto C# principal: `Nexus_1/Nexus_1.csproj` y `Nexus_1/Program .cs`. Es la fuente de verdad para documentar la lógica y el estado actual. El archivo se llama literalmente `Program .cs` (espacio antes de la extensión).

## Funcionalidades implementadas en la rama de referencia

Registro de nombre, contraseña y realidad; selección entre FISICO, ARMAMENTO, MANA y HERRAMIENTAS; estado del cadete con vida, energía, estabilidad y poder; detector, dron y PEM; exploración territorial del mapa Génesis con zonas exploradas, distancia y enemigos; recursos, capacidades y eventos de anomalía/detección de IRIS. Consulta [arquitectura actual](docs/arquitectura-actual.md) para detalles.

## Tecnologías y ejecución

El proyecto apunta a C#/.NET 10 (`net10.0`) y declara LibVLCSharp, System.Drawing.Common y VideoLAN.LibVLC.Windows. Requiere SDK de .NET 10:

```powershell
dotnet run --project ".\Nexus_1\Nexus_1.csproj"
```

La solución de esa rama es `Nexus_1.slnx`.

## Documentación

Consulta `docs/` para el diseño del juego, arquitectura actual y propuesta, modelo SQL Server propuesto y análisis técnico. Wireframes y diagramas editables están en `docs/evidencias/`. La documentación distingue hechos del código de propuestas.

## Persistencia

El estado vive en memoria; SQL Server, guardado/carga y arquitectura por capas son propuestas, no funcionalidades disponibles actualmente.
