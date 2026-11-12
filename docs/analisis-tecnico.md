# Análisis técnico

## Base analizada

El código de referencia es la rama `feature/mi-codigo-csharp`, commit `8c4965ce75b6a33a3998a2f9df301f7f8c5116d4`, con proyecto .NET 10 de consola en `Nexus_1/Program .cs`. La rama contiene registro, selección de clase, estado del cadete, exploración, anomalías, dron/PEM y eventos IRIS. La carpeta `NEXUS_WF/` representa una aproximación gráfica de posible evolución y se conserva como referencia; no se utilizó para sustituir las reglas del programa base.

## Alternativas

| Área | Alternativas | Ventajas | Costes / riesgos | Recomendación |
|---|---|---|---|---|
| Presentación | Consola actual / WinForms de referencia / WPF o motor de juego | Consola ya soporta flujo narrativo; WinForms aporta una aproximación gráfica próxima; un motor resuelve escenas/input avanzados | Consola limita disposición y accesibilidad; UI alternativa puede divergir de reglas; migrar aumenta coste | Mantener consola como base de lógica y usar NEXUS_WF para explorar la experiencia gráfica; extraer dominio antes de unirlas |
| Arquitectura | Mantener Program.cs / separar aplicación, dominio y UI | No cambio rápido / separación facilita pruebas y evolución | Refactor requiere preservar secuencia de juego | Extraer EstadoJuego y reglas a clases testeables, luego crear adaptadores de consola y WinForms |
| Persistencia | RAM / JSON / SQL Server | RAM simple; JSON local; SQL con integridad y consultas | JSON necesita versionado; SQL necesita instancia y configuración | Diseñar guardado SQL Server como fase, no simular disponibilidad actual |
| Datos | Matrices y flags / entidades tipadas | Actual es directo; entidades permiten validación y serialización | Cambio a corto plazo | Migrar gradualmente mapas, casillas, equipo y anomalías a entidades/IDs |
| Dependencias | Mantener paquetes / revisar uso real | Puede habilitar medios futuros | Packages elevan instalación y superficie | Confirmar si LibVLCSharp/System.Drawing se usan en código antes de justificarlos o retirarlos |

## Relación con el código real

- `Program .cs` mezcla Main, terminal, registro, autorización, reglas, exploración y eventos.
- `EstadoJuego` ya modela energía, estabilidad, vida, poder, clase, arma, detector, dron, PEM, recursos, anomalía y posición: el diseño de jugador debe partir de estos conceptos.
- Hay costes diferenciados de dron/PEM y habilidades, además de una ventaja HERRAMIENTAS; diseñar energía como recurso de decisión.
- El mapa Génesis 8×11, zonas exploradas y distancia de 10 m/casilla fundamentan el wireframe de mapa.
- Evento `AnomaliaDetectada` dispara respuesta de IRIS: sirve como retroalimentación narrativa y técnica.
- `NEXUS_WF` sugiere una UI oscura futurista, mapas visibles, HUD y pantallas de misión. Se trata de guía visual aproximada; su estructura o reglas no deben atribuirse a la rama de consola.
- No hay acceso a datos. Toda recomendación SQL es futura.

## Recomendación de arquitectura y viabilidad

Conservar la consola ejecutable como referencia funcional; extraer dominio y casos de uso sin depender de Console. Después, construir UI Windows Forms mediante presentación que invoque los mismos casos de uso. Así, `NEXUS_WF` guía el aspecto sin duplicar reglas. Diseñar persistencia SQL Server sobre el estado ya definido pero versionando, evitando guardar estructuras de matrices/strings sin validar.

## Fuentes oficiales

- [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [Windows Forms](https://learn.microsoft.com/dotnet/desktop/winforms/)
- [Microsoft.Data.SqlClient](https://learn.microsoft.com/sql/connect/ado-net/introduction-microsoft-data-sqlclient-namespace)
- [EF Core SQL Server provider](https://learn.microsoft.com/ef/core/providers/sql-server/)
- [EF Core migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)
- [ADO.NET local transactions](https://learn.microsoft.com/dotnet/framework/data/adonet/local-transactions)
- [GitHub: ignoring files](https://docs.github.com/get-started/git-basics/ignoring-files)

Las fuentes respaldan las capacidades y alcance de plataforma, proveedor, migraciones y transacciones, no la decisión de producto. Revisar versiones exactas durante implementación.

## Verificación de información asistida por IA

Los hechos de implementación se contrastaron con el árbol y `Program .cs` de `feature/mi-codigo-csharp`, y con los archivos .csproj/slnx. Se comprobaron los nombres de estado, mapa 8×11, costes/umbrales y existencia de eventos de anomalía. La afirmación de que no hay persistencia se limita al árbol inspeccionado. Arquitectura por capas y SQL Server son inferencias propuestas, verificadas contra documentación técnica oficial pero no implementadas ni probadas en esta rama.
