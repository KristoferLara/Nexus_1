# Estructura de la versión Windows Forms

## Alcance

Esta guía describe el proyecto Windows Forms disponible en la rama [`nexus_ani`](https://github.com/KristoferLara/Nexus_1/tree/nexus_ani). La solución de esa rama reúne el proyecto de consola y el proyecto gráfico. Esta guía está en `main`, que conserva la documentación y el análisis del repositorio.

La aplicación Windows Forms es un prototipo jugable con estado mantenido en memoria. En los archivos revisados no se encontró conexión a SQL Server ni guardado y carga de partidas. La persistencia y el modelo de datos SQL Server documentados en este repositorio son propuestas de evolución, no funciones implementadas en esta versión.

## Estructura principal

| Archivo | Responsabilidad observada |
|---|---|
| `Nexus_1.slnx` | Solución que referencia el proyecto de consola y el proyecto Windows Forms. |
| `Nexus_1/Nexus_1.csproj` | Configuración del proyecto de consola. |
| `Nexus_1/Program .cs` | Código de entrada y lógica de la versión de consola. El nombre contiene un espacio antes de `.cs`. |
| `Nexus_1_WinForms/Nexus_1_WinForms.csproj` | Proyecto Windows Forms para `net10.0-windows`. |
| `Nexus_1_WinForms/Program.cs` | Punto de entrada de Windows Forms. |
| `Nexus_1_WinForms/Form1.cs` | Coordina pantallas, entradas de teclado, vistas y ciclo de actualización. |
| `Nexus_1_WinForms/Juego.cs` | Mantiene el estado y las reglas del prototipo: inicio de partida, mapas, misiones, encuentros y turnos de combate. |
| `Nexus_1_WinForms/Modelos.cs` | Modelos de cadete, enemigo, misión y combate. |
| `Nexus_1_WinForms/VistaMenus.cs` | Pantallas de título, registro, selección, menú y manual. |
| `Nexus_1_WinForms/VistaGenesis.cs` | Renderiza el mapa Génesis, sus celdas, sprites y estado de exploración. |
| `Nexus_1_WinForms/VistaBase.cs` | Renderiza la base y sus interacciones. |
| `Nexus_1_WinForms/VistaCombate.cs` | Renderiza el combate, los sprites, ataques y defensas. |
| `Nexus_1_WinForms/VistaJuego.cs` | Funciones compartidas de dibujo y comportamiento de las vistas. |
| `Nexus_1_WinForms/Tema.cs` y `EstiloVisual.cs` | Paletas, temas y utilidades de dibujo pixel art. |

## Funcionalidades visibles en el código

- Selección entre las clases FISICO, ARMAMENTO, MANA y HERRAMIENTAS.
- Exploración del mapa Génesis y de la base.
- Combates por turnos con enemigos LIQUIDO, FUEGO y VENENO.
- Misiones y recompensas de experiencia y poder.
- Vistas pixel art, efectos de combate y cambio de tema.

Esta lista resume la estructura y las funcionalidades inspeccionadas; no sustituye una prueba funcional de cada flujo.

## Abrir y ejecutar en Windows

1. Clona el repositorio y cambia a la rama que contiene Windows Forms:

   ```bash
   git clone https://github.com/KristoferLara/Nexus_1.git
   cd Nexus_1
   git switch nexus_ani
   ```

2. Abre `Nexus_1.slnx` con Visual Studio compatible con .NET 10 y Windows Forms, o usa el SDK de .NET 10 para compilar:

   ```bash
   dotnet build Nexus_1.slnx
   ```

3. Para ejecutar directamente el proyecto gráfico:

   ```bash
   dotnet run --project Nexus_1_WinForms/Nexus_1_WinForms.csproj
   ```

Windows es necesario para ejecutar la interfaz Windows Forms. Si la solución incluye ambos proyectos, Visual Studio puede iniciar el proyecto de consola por defecto; en ese caso selecciona `Nexus_1_WinForms` como proyecto de inicio.

## Estado de persistencia

Los modelos `Cadete` y `Mision` representan datos que podrían guardarse, pero el código inspeccionado crea una nueva partida al iniciar y mantiene su estado en objetos de memoria. Para recordar el progreso entre ejecuciones, una siguiente etapa puede añadir una capa de persistencia y relacionarla con SQL Server. El diseño preliminar está en [Modelo de datos propuesto](modelo-de-datos-propuesto.md) y la separación de responsabilidades en [Arquitectura propuesta](arquitectura-propuesta.md).