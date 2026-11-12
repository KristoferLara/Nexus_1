# NEXUS

Aventura narrativa de ciencia ficción, exploración táctica y combate por turnos. El jugador crea un cadete, explora Génesis, enfrenta anomalías y reconstruye la historia de IRIS.

## Implementado

- Registro de cadete y selección entre cuatro clases.
- Menú, manual, archivos narrativos y Nexo multiversal (solo Génesis disponible).
- Exploración de base/mapa, niebla de guerra, recolección de recursos y enemigos.
- Combate por turnos con efectos de enemigo, victoria, derrota y huida.
- Tres misiones secuenciales con objetivos, consejos y recompensas.
- HUD, escenas y animaciones dibujadas en Windows Forms.

## Tecnologías y ejecución

C#, .NET 10 para Windows, Windows Forms y `System.Drawing`. El estado de partida solo vive en memoria; no existe guardado ni SQL Server en el prototipo.

Requiere Windows y .NET 10 SDK:

```powershell
dotnet run --project .\NEXUS_WF\NEXUS_WF.csproj
```

También se puede abrir `NEXUS_WF.slnx` en Visual Studio compatible. Controles: Enter confirma, flechas navegan, WASD/flechas mueven, A ataca, D defiende, H huye, Escape regresa.

## Documentación

Consulta `docs/` para diseño del juego, arquitectura real y propuesta, modelo de datos SQL Server y análisis técnico. Los wireframes y diagramas están en `docs/evidencias/`. Las propuestas de arquitectura, inventario y persistencia aún no están implementadas.
