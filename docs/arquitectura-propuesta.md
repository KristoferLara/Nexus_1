# Arquitectura propuesta

## Objetivo

Transformar la experiencia de consola en una app gráfica de acción 2D sin desechar las reglas y el mundo H3. La arquitectura separa presentación, reglas de dominio y persistencia; la UI no emite SQL ni decide daño, y el dominio no dibuja controles ni lee teclas.

## Diagrama

![Diagrama de arquitectura propuesta](evidencias/arquitectura-propuesta.mmd)

## Capas y responsabilidades

### 1. Presentación — `Nexus.WinForms`

- `MainForm`: navegación entre menú, base, misiones, explorador/inventario y escena.
- `GameView` (control personalizado): dibuja fondo, sprites, colisionadores visibles en modo depuración y HUD desde el estado recibido.
- `InputController`: mantiene teclas pulsadas; no usa bloqueos `Console.ReadKey`.
- `GameLoop`: temporizador WinForms actualiza estado a intervalo estable y solicita repintado; separar actualización de dibujo para no avanzar la partida al repintar.
- Vistas ligeras (`MenuView`, `MissionView`, `InventoryView`) presentan comandos. La escena se dibuja con `Graphics`/sprites y escalado vecino más cercano; doble búfer reduce parpadeo.

### 2. Aplicación / dominio — `Nexus.GameCore`

- `Cadete`: evolución de los atributos y métodos del H3.
- `Enemy`, `Mission`, `Item`, `PlayerState`: evolución de `Enemigo`, `Mision` y `EstadoJuego`.
- `World`, `Level`, `Collider`, `Physics2D`: nuevos tipos para carriles, gravedad simple, suelo, paredes y colisiones.
- `CombatService`: transforma reglas de `Combate` a ataque/daño en tiempo real, manteniendo efectos y límites.
- `MissionService`, `ProgressionService`: verifican objetivo, secuencia de entrega, recompensa y XP; derivan nivel como actualmente.
- `GameSession`: coordina acciones de jugador y eventos, sin dependencias de WinForms ni SQL.
- Las reglas se expresan mediante métodos deterministas para que se puedan verificar independientemente de la interfaz.

### 3. Acceso a datos — `Nexus.Data.SqlServer`

- `SqlConnectionFactory` centraliza la conexión; credenciales y cadena no se incrustan en el código.
- `PlayerRepository`, `SaveRepository`, `MissionRepository` cargan perfiles y progreso con `Microsoft.Data.SqlClient`.
- Consultas parametrizadas; comandos con transacción cuando se guarda un conjunto de filas relacionadas.
- El guardado se inicia desde casos de uso/eventos importantes y un botón de guardar; no desde cada fotograma.

### 4. SQL Server

SQL Server LocalDB/Express sirve para desarrollo individual, con instancia y esquema documentados. Un despliegue compartido requiere servidor, permisos, política de copias de seguridad y autenticación configurada; no se asume para el demo.

## Límites entre capas

```text
WinForms -> GameCore <- Data.SqlServer
                 ^
                 | modelos/puertos de persistencia (interfaces)
                 +----------------- Data implementa repositorios
```

El dominio depende de interfaces como `ISaveRepository`; la implementación SQL depende del dominio para mapear modelos. La presentación llama a casos de uso, actualiza vista con resultados y expone errores legibles. El temporizador de UI no debe realizar consultas SQL sincrónicas que congelen la ventana.

## Relación entre H3 y propuesta

| H3 | Destino sugerido |
|---|---|
| `Program.EstadoJuego` | `GameSession` y modelos de estado; mantener referencias explícitas, no un global estático. |
| `Program.Cadete` | `GameCore.Cadete`, conservar XP, nivel calculado, vida/energía/poder y métodos de cambio controlado. |
| `Program.Enemigo` + `Combate` | `Enemy` + `CombatService`; adaptar acciones turnadas a ataques por tiempo/contacto. |
| `MapaGenesis` + `BaseOperativa` | `Level`/`World` y escena de base; datos de geometría fuera del formulario. |
| `ExploracionGenesis` | `ExplorationService` y eventos de encuentros/colección. |
| `Mision` + índice del estado | `MissionService` y estado persistido; mantener regla de reclamar recompensa en la base. |
| `Iris`, `Botiquin` | sistemas de anomalía y `Item` consumible. |
| `InterfazNexus`, menús y lectura de consola | reemplazar por vistas WinForms; no portar texto de consola a botones uno por uno. |
| `AnimacionCombate` | animador de sprites/UI; reutilizar reglas de combate, no el bucle/entrada de consola. |

## Plan de implementación incremental

1. Crear `Nexus.GameCore` y mover un modelo a la vez, preservando el proyecto consola para comparar.
2. Construir formulario con navegación y HUD; escena temporal con formas y un nivel fijo.
3. Añadir movimiento, salto, colisión de plataformas y un enemigo; una sola clase y un jefe.
4. Añadir SQL: esquema, perfil local, guardar/cargar partida y recompensa de una misión.
5. Sustituir formas por arte pixel art, ajustar controles y accesibilidad; importar otras dos misiones solo tras estabilizar el corte vertical.
