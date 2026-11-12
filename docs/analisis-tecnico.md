# Análisis técnico

## Hechos y alternativas

Evidencia: proyecto C# `net10.0-windows`, WinForms, formulario fijo, reglas en `Modelos.cs` y gran parte de flujo/renderizado en `Form1.cs`. No se encontró persistencia, pruebas o SQL. Recomendaciones abajo son propuestas y no implementación.

| Área | Alternativas | Ventajas | Costes | Recomendación |
|---|---|---|---|---|
| UI | WinForms / WPF / motor de juego | WinForms reutiliza prototipo; WPF facilita binding; Unity/Godot ofrecen escenas y animación | WinForms escala y se adapta menos; migrar UI cambia herramientas y consume tiempo | Mantener WinForms en el MVP; revaluar motor si se requiere multiplataforma/escenas complejas |
| Lógica | Dejar en Form1 / extraer dominio | Extracción permite pruebas y reduce acoplamiento | Refactor debe preservar turnos y transiciones | Extraer primero combate y misión de las 1,192 líneas de Form1 |
| Guardado | RAM / JSON / SQL Server | RAM simple; JSON local; SQL ofrece integridad y consultas | JSON requiere versionado; SQL requiere instancia/configuración | Por requisito, SQL Server tras estabilizar modelo |
| Acceso SQL | ADO.NET / EF Core | ADO.NET explícito; EF Core ofrece mapping y migraciones | EF añade paquete/abstracción; SQL requiere operación | EF Core si modelo evoluciona; ADO.NET válido para primera vertical slice |
| Contenido | Hardcode / JSON / tablas | Hardcode simple; JSON separa contenido; tablas editables | Desincronización o tooling adicional | Extraer catálogos versionados primero |

## Vinculación con archivos reales

- `NEXUS_WF.slnx` contiene un proyecto.
- `NEXUS_WF.csproj` fija .NET 10 Windows y Windows Forms.
- `Program.cs` arranca directamente Form1.
- `Form1.Designer.cs` fija 960×640, timer de 16ms y eventos de dibujo/teclado.
- `Form1.cs` mezcla pantallas, mapas, lista de misiones, reglas de interacción y render.
- `Modelos.cs` contiene Cadete, Enemigo, Mision y Combate; strings de efectos sugieren IDs/enums.
- Cadete tiene energía, pero el loop usa especialmente vida/poder/estabilidad.
- Recursos no es inventario funcional; `tieneArma` no implementa equipamiento.
- No hay proveedor SQL ni guardado/carga en árbol original.

## Plan viable

1. Recuperar código y estandarizar README/.gitignore/docs.
2. Proteger reglas con pruebas unitarias al extraer dominio.
3. Separar casos de uso de UI sin cambiar controles.
4. Incorporar inventario mínimo.
5. Añadir esquema/migraciones y guardar/cargar SQL Server, con secretos fuera del repositorio.
6. Probar transacciones, recuperación y doble reclamación en SQL real.

## Fuentes técnicas oficiales

- [.NET support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core)
- [Windows Forms documentation](https://learn.microsoft.com/dotnet/desktop/winforms/)
- [Microsoft.Data.SqlClient](https://learn.microsoft.com/sql/connect/ado-net/introduction-microsoft-data-sqlclient-namespace)
- [EF Core SQL Server provider](https://learn.microsoft.com/ef/core/providers/sql-server/)
- [EF Core migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)
- [ADO.NET local transactions](https://learn.microsoft.com/dotnet/framework/data/adonet/local-transactions)
- [GitHub: ignoring files](https://docs.github.com/get-started/git-basics/ignoring-files)

Las páginas oficiales respaldan el alcance de plataforma, proveedor y capacidades de migración/transacción. Revisar versiones exactas al implementar.

## Verificación de afirmaciones asistidas por IA

Se contrastaron las afirmaciones sobre implementación con los archivos originales del commit `6543e3a` y el árbol completo del repositorio. Se verificaron target framework, WinForms, clases, valores de combate, mapas, misiones y ausencia de proveedor SQL/guardar en el código inspeccionado. Las decisiones sobre separar capas, inventario y EF Core son inferencias recomendadas, no hechos. Las capacidades tecnológicas se contrastaron con las fuentes oficiales anteriores. No se declara verificada compilación ni conexión a SQL Server.
