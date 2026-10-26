# NEXUS

NEXUS es un proyecto de aventura de exploración y combate cuyo prototipo H3 está desarrollado en C# para consola. La evolución propuesta lo convierte en un videojuego 2D de acción con pixel art, interfaz Windows Forms y guardado de perfiles/progreso en SQL Server.

## Estado del proyecto

**Implementado en H3 (consola):** cadete con atributos y experiencia/nivel, base operativa y exploración por cuadrícula GENESIS, combate por turnos, enemigos, botiquín, IRIS y tres misiones secuenciales (LIQUIDO, FUEGO, VENENO). El nivel se deriva de XP; la misión se cobra en el terminal de la base.

**Propuesta futura, todavía no implementada:** movimiento y salto 2D con colisiones, HUD gráfico con barras, jefes en pixel art, pantallas WinForms y persistencia SQL Server. Consulta el diseño antes de interpretar la propuesta como funcionalidades existentes.

## Tecnologías actuales

- C# y SDK .NET 10
- Aplicación de consola definida en `Nexus_1/Nexus_1.csproj` (`net10.0`)
- Paquetes declarados: LibVLCSharp, System.Drawing.Common y VideoLAN.LibVLC.Windows
- .NET 10 SDK; Windows recomendado por dependencias de vídeo

## Ejecutar el prototipo H3

Desde la raíz:

```bash
dotnet restore
dotnet run --project "Nexus_1/Nexus_1.csproj"
```

También se puede abrir `Nexus_1.slnx` en Visual Studio compatible con .NET 10. Algunos recursos de animación dependen de archivos locales y pueden no mostrarse si faltan.

## Estructura

- `Nexus_1/Program .cs`: punto de entrada, clases y lógica principal del prototipo de consola.
- `Nexus_1/Nexus_1.csproj`: configuración y dependencias actuales.
- `docs/`: diseño de juego, arquitectura actual/propuesta, modelo de datos e investigación técnica.
- `.gitignore`: exclusión de archivos temporales y artefactos de herramientas.

## Documentación

- [Diseño del videojuego](docs/diseno-del-juego.md): visión, ciclo, reglas, wireframes, experiencia y alcance de dos meses.
- [Arquitectura actual H3](docs/arquitectura-actual.md): clases, funciones y límites observados en el código.
- [Arquitectura propuesta](docs/arquitectura-propuesta.md): capas WinForms, dominio y acceso SQL.
- [Modelo de datos propuesto](docs/modelo-de-datos-propuesto.md): entidades y estrategia de guardado/carga.
- [Análisis técnico](docs/analisis-tecnico.md): alternativas, decisiones, plan de ocho semanas, fuentes y verificación de información asistida por IA.
- [Diagramas y evidencias](docs/evidencias/): diagramas Mermaid editables.
- [Índice de documentación](docs/README.md): índice completo y documentación técnica anterior.
