# Modelo de datos propuesto

## Propósito y alcance

SQL Server guardará perfiles locales, una partida por perfil (con posibilidad de varias después), el estado del cadete, misiones entregadas y cantidades de inventario. No se guardan fotogramas, posición a cada tick ni contraseñas de texto plano. La escena guarda en puntos seguros, al regresar a base, al completar misión y al salir.

## Entidades y relaciones

```text
PlayerProfile 1 ─── N SaveGame
SaveGame     1 ─── 1 CadetState
SaveGame     1 ─── N MissionProgress
SaveGame     1 ─── N InventoryItem ─── 1 ItemDefinition
MissionProgress N ─── 1 MissionDefinition
```

### Tablas

| Tabla | Campos esenciales | Notas |
|---|---|---|
| `PlayerProfile` | `PlayerId int PK`, `DisplayName nvarchar(40)`, `CreatedAt datetime2`, `LastPlayedAt datetime2` | Perfil local seleccionable. No representa una cuenta remota. Nombre normalizado/único si se desea evitar duplicados. |
| `SaveGame` | `SaveId int PK`, `PlayerId int FK`, `Slot tinyint`, `CurrentMissionId int NULL FK`, `CurrentLevelId nvarchar(40)`, `CheckpointX/Y float`, `UpdatedAt datetime2`, `Version rowversion` | Restricción única `(PlayerId, Slot)`; el progreso de nivel solo se guarda en punto seguro. |
| `CadetState` | `SaveId int PK/FK`, `CadetName nvarchar(40)`, `ClassCode nvarchar(20)`, `Level int`, `Experience int`, `Health int`, `Energy int`, `Stability int`, `Power int` | `Level` debe coincidir con regla derivada de XP (H3: cada 100 XP); preferible calcularlo al cargar y no duplicar si UI no necesita consulta directa. |
| `MissionDefinition` | `MissionId int PK`, `SequenceNo int`, `Code nvarchar(30)`, `Title nvarchar(100)`, `TargetEnemyCode nvarchar(30)`, `RewardXp int`, `RewardResource int` | Datos del contenido inicial; puede ser semilla en código o tabla. Restricción única en código/orden. |
| `MissionProgress` | `SaveId int FK`, `MissionId int FK`, `Status tinyint`, `TargetDefeated bit`, `RewardClaimed bit`, `CompletedAt datetime2 NULL` | PK compuesta `(SaveId, MissionId)`. Mantiene la regla de H3: vencer objetivo no equivale a cobrar recompensa. |
| `ItemDefinition` | `ItemId int PK`, `Code nvarchar(40) UNIQUE`, `Name nvarchar(80)`, `ItemType tinyint`, `HealAmount int NULL` | Define botiquín y recursos conocidos; reglas de uso en dominio. |
| `InventoryItem` | `SaveId int FK`, `ItemId int FK`, `Quantity int` | PK compuesta `(SaveId, ItemId)`; cantidad no negativa. |

### Cardinalidades y restricciones

- Un perfil puede tener uno o varios espacios; cada slot pertenece a un perfil.
- Una partida tiene exactamente un estado de cadete; contiene cero o más filas de misión e inventario.
- Progreso de misión referencia una definición. Mantener estados como bloqueada, disponible, en curso y completada/recompensa reclamada.
- Los valores de barras deben estar en su rango (0–100 si se conserva el máximo actual), XP y cantidades no negativos.
- Las coordenadas/checkpoint son datos del nivel; no guardar una fila cada cuadro. Usar checkpoint para reanudar de forma entendible y no explotar el tamaño del save.

## Guardado y carga

1. En el arranque, seleccionar/crear perfil; cargar último slot y consultar estado, misiones e inventario con consultas parametrizadas.
2. Mapear resultados a modelos del dominio; validar valores (versión de save, nivel existente y límites) antes de iniciar la escena.
3. Para guardar, abrir conexión, iniciar transacción y actualizar estado de cadete, checkpoint, progreso de misión e inventario como unidad.
4. Confirmar transacción solo si todas las actualizaciones tuvieron éxito; ante error, rollback, conservar el estado en memoria y presentar «No se pudo guardar» con opción de reintentar.
5. Usar `rowversion` si se prevén dos procesos cargando el mismo perfil para detectar sobrescrituras; el primer demo local puede no exponer sincronización.
6. Cambios de esquema con scripts versionados `docs/sql/001-initial-schema.sql` cuando se implemente. En esta fase no se crea ni modifica una base real.

## Decisiones de autenticación y seguridad

El wireframe propone un perfil local. No pedir contraseña en el corte inicial: con un único jugador local la autenticación agrega complejidad sin proteger frente a quien controla el PC y puede leer el cliente/DB. Si el requisito académico obliga a un inicio de sesión, tratarlo como alcance adicional: guardar hash lento y salt mediante biblioteca mantenida, nunca contraseña reversible/texto plano; limitar intentos y validar datos. Para cuentas compartidas entre equipos se necesita un servicio de servidor, fuera del alcance de WinForms directo a SQL Server.

Usar `Microsoft.Data.SqlClient`, parámetros SQL tipados, conexiones cortas/dispose, permisos mínimos y cifrado/certificado apropiado para el destino. No incrustar claves ni habilitar `TrustServerCertificate=true` como solución universal. Para desarrollo, documentar LocalDB/Express y su nombre de instancia.

## Boceto del esquema

```sql
PlayerProfile(PlayerId PK, DisplayName, CreatedAt, LastPlayedAt)
SaveGame(SaveId PK, PlayerId FK, Slot, CurrentMissionId FK, CurrentLevelId,
         CheckpointX, CheckpointY, UpdatedAt, Version)
CadetState(SaveId PK/FK, CadetName, ClassCode, Experience, Health, Energy,
           Stability, Power)
MissionDefinition(MissionId PK, SequenceNo, Code, TargetEnemyCode, RewardXp)
MissionProgress(SaveId FK, MissionId FK, Status, TargetDefeated, RewardClaimed)
ItemDefinition(ItemId PK, Code UNIQUE, Name, ItemType, HealAmount)
InventoryItem(SaveId FK, ItemId FK, Quantity)
```

Se recomienda SQL Server Express LocalDB para desarrollo de escritorio individual; la configuración debe verificarse en la máquina objetivo, ya que el motor se instala por separado. La documentación de LocalDB lo describe como una característica de SQL Server Express dirigida a desarrolladores.
