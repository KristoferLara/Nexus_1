# NEXUS

NEXUS es un prototipo narrativo de exploración y supervivencia de ciencia ficción. En la rama de referencia `feature/mi-codigo-csharp`, el jugador registra un cadete, se autentica, elige Génesis y opera desde una interfaz de consola mientras explora anomalías, administra energía/estabilidad, usa equipo y enfrenta amenazas relacionadas con IRIS.

## Código de referencia y dirección visual

- **Base de lógica:** rama `feature/mi-codigo-csharp`, proyecto `Nexus_1/Nexus_1.csproj` y archivo `Nexus_1/Program .cs`. Esa rama contiene la implementación C# de consola que fundamenta la documentación.
- **Referencia de evolución visual:** `NEXUS_WF.slnx` y carpeta `NEXUS_WF/` contienen una aproximación Windows Forms de cómo podría quedar el juego. Se conservan como referencia visual y prototipo paralelo; no reemplazan la base de lógica de consola.
- `main` mantiene documentación y el prototipo NEXUS_WF reconstruido desde el historial. Consulta la rama de referencia para el flujo C# base.

## Funcionalidades del código de referencia

El programa de consola contiene registro de nombre/contraseña/realidad, selección entre FISICO, ARMAMENTO, MANA y HERRAMIENTAS, estado del cadete con vida/energía/estabilidad/poder, recursos y equipo (detector, dron y PEM), exploración territorial con mapa Génesis, zonas exploradas, ubicación del dron y enemigos, anomalías y evento de detección de IRIS. La secuencia y las funciones se detallan en [arquitectura actual](docs/arquitectura-actual.md).

El prototipo Windows Forms añade una aproximación gráfica con mapas, combate por turnos, misiones secuenciales y escenas dibujadas. Sus comportamientos no deben confundirse con lo que actualmente implementa la rama de consola.

## Tecnologías

El proyecto de consola de la rama de referencia apunta a C#/.NET 10 (`net10.0`) e incluye paquetes LibVLCSharp, System.Drawing.Common y VideoLAN.LibVLC.Windows. El proyecto separado `NEXUS_WF` apunta a .NET 10 Windows y Windows Forms. No hay persistencia SQL Server implementada.

## Ejecutar el código de referencia

Requisitos: SDK de .NET 10. Desde la raíz del repositorio:

```powershell
dotnet run --project ".\Nexus_1\Nexus_1.csproj"
```

Para consultar/ejecutar la aproximación gráfica Windows Forms, se requiere Windows y .NET 10 SDK; abrir `NEXUS_WF.slnx` en Visual Studio compatible o ejecutar su proyecto.

## Documentación

La visión de jugador, wireframes y mecánicas están en `docs/diseno-del-juego.md`. `docs/arquitectura-actual.md` describe específicamente la rama de consola `feature/mi-codigo-csharp`; el resto separa propuesta de arquitectura, SQL Server e investigación técnica. Diagramas y bocetos editables están en `docs/evidencias/`.

## Persistencia

La partida actualmente vive en memoria; SQL Server, guardado/carga y la arquitectura por capas son objetivos propuestos, no funciones disponibles.
