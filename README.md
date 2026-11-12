# NEXUS

NEXUS es un prototipo narrativo de ciencia ficción, exploración y supervivencia. El jugador interpreta a un cadete que se conecta con la realidad Génesis, administra sus recursos y responde a anomalías vinculadas con IRIS.

## Código fuente y estado del repositorio

La solución funcional de consola está actualmente en la rama **`feature/mi-codigo-csharp`**. La rama predeterminada `main` contiene esta documentación y las evidencias, pero todavía no incluye la solución C#; por eso, el comando de ejecución requiere cambiar a la rama de código:

```bash
git switch feature/mi-codigo-csharp
```

La solución es `Nexus_1.slnx`; el proyecto se encuentra en `Nexus_1/Nexus_1.csproj` y su archivo de entrada se llama literalmente `Nexus_1/Program .cs` (con un espacio antes de `.cs`).

## Funcionalidades implementadas

En la rama de código, el prototipo incluye registro de cadete, selección de una de cuatro clases (FISICO, ARMAMENTO, MANA y HERRAMIENTAS), consulta de estado, exploración de Génesis, movimiento y detección de anomalías, recursos, dron y pulso PEM, y eventos de riesgo asociados a IRIS. La experiencia implementada es de consola.

El progreso se mantiene en memoria durante la sesión. Guardado/carga, SQL Server y la arquitectura por capas aparecen en la documentación como propuestas y aún no están implementados.

## Tecnologías

- C# y .NET 10 (`net10.0`)
- Aplicación de consola
- Paquetes declarados en el proyecto: LibVLCSharp, System.Drawing.Common y VideoLAN.LibVLC.Windows

Se requiere el SDK de .NET 10 instalado.

## Ejecución

Desde la raíz del repositorio, después de cambiar a la rama funcional:

```bash
dotnet run --project "./Nexus_1/Nexus_1.csproj"
```

En PowerShell también puedes ejecutar:

```powershell
dotnet run --project ".\Nexus_1\Nexus_1.csproj"
```

## Documentación

La carpeta [`docs/`](docs/) contiene:

- [Diseño del juego](docs/diseno-del-juego.md)
- [Arquitectura actual](docs/arquitectura-actual.md)
- [Arquitectura propuesta](docs/arquitectura-propuesta.md)
- [Modelo de datos propuesto](docs/modelo-de-datos-propuesto.md)
- [Análisis técnico](docs/analisis-tecnico.md)
- [Evidencias, bocetos y diagramas](docs/evidencias/)

Los documentos separan el comportamiento observado en el código de las propuestas futuras.
