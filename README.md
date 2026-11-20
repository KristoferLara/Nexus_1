# NEXUS

NEXUS es un prototipo de ciencia ficción, exploración y supervivencia. El jugador controla a un cadete que explora Génesis, administra recursos y enfrenta anomalías y enemigos vinculados con IRIS.

## Versiones disponibles

El repositorio tiene tres ramas con propósitos distintos:

| Rama | Contenido |
|---|---|
| [`main`](https://github.com/KristoferLara/Nexus_1/tree/main) | Documentación, análisis y propuestas técnicas. Es la rama predeterminada. |
| [`nexus_ani`](https://github.com/KristoferLara/Nexus_1/tree/nexus_ani) | Solución C# con los proyectos de consola y Windows Forms. |
| [`feature/mi-codigo-csharp`](https://github.com/KristoferLara/Nexus_1/tree/feature/mi-codigo-csharp) | Versión C# de consola y base de referencia para parte de la documentación técnica. |

Las ramas se desarrollaron por separado. Para trabajar con una versión, cambia explícitamente a su rama; el contenido de `main` no incluye los proyectos C#.

## Requisitos

- Windows para ejecutar Windows Forms.
- SDK de .NET 10.
- Visual Studio compatible con .NET 10 y Windows Forms, o la CLI de .NET.

## Ejecutar Windows Forms

Desde la raíz del repositorio:

```bash
git switch nexus_ani
dotnet run --project Nexus_1_WinForms/Nexus_1_WinForms.csproj
```

También puedes abrir `Nexus_1.slnx` en Visual Studio y seleccionar `Nexus_1_WinForms` como proyecto de inicio. La solución incluye también el proyecto de consola.

## Ejecutar la versión de consola

```bash
git switch feature/mi-codigo-csharp
dotnet run --project "Nexus_1/Nexus_1.csproj"
```

El archivo de entrada de consola se llama literalmente `Program .cs`, con un espacio antes de `.cs`.

## Funcionalidades

La versión Windows Forms incluye selección de cuatro clases (FISICO, ARMAMENTO, MANA y HERRAMIENTAS), exploración de Génesis y la base, combate por turnos, misiones, recompensas, sprites pixel art y temas visuales. La versión de consola tiene su propio flujo de juego; consulta la documentación para saber a qué rama corresponde cada descripción.

## Persistencia: estado actual

El progreso de la partida se mantiene en memoria durante la ejecución. Al iniciar, el prototipo crea una partida nueva. **SQL Server, guardado/carga y persistencia entre ejecuciones no están implementados** en los proyectos inspeccionados; se describen como propuestas técnicas para una evolución posterior.

## Documentación

- [Estructura de Windows Forms](docs/estructura-winforms.md)
- [Diseño del juego](docs/diseno-del-juego.md)
- [Arquitectura actual](docs/arquitectura-actual.md)
- [Arquitectura propuesta](docs/arquitectura-propuesta.md)
- [Modelo de datos propuesto para SQL Server](docs/modelo-de-datos-propuesto.md)
- [Análisis técnico](docs/analisis-tecnico.md)
- [Evidencias, bocetos y diagramas](docs/evidencias/)

Los documentos identifican la rama y separan el comportamiento inspeccionado de las mejoras propuestas.
