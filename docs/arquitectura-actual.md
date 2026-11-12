# Arquitectura actual de NEXUS (H3)

## Evidencia

Revisión del código del commit original `6543e3a`, recuperado del historial. Se describe la implementación versionada; no se afirma que se haya compilado en esta tarea.

## Plataforma y archivos

`NEXUS_WF.slnx` referencia un solo proyecto. `NEXUS_WF.csproj` es una aplicación `WinExe` con `net10.0-windows`, `UseWindowsForms=true`, nullable e implicit usings. `Program.cs` inicia Windows Forms y abre `Form1`. No aparecen paquetes externos, pruebas o proyectos adicionales en el árbol original.

## Clases

| Clase / archivo | Hechos observados | Limitaciones |
|---|---|---|
| `Form1` en `Form1.cs` | Controla pantalla, entrada, timer, mapas, combate, misiones, mensajes, animación y dibujo. 1,192 líneas. | Acopla UI, reglas y datos; estado no persistente. |
| `Form1` parcial en `Form1.Designer.cs` | Formulario 960×640, timer 16 ms, eventos Paint/KeyDown/KeyPress. | Layout fijo y manejo manual. |
| `Cadete` en `Modelos.cs` | Nombre, energía, estabilidad, vida, poder, XP, recursos, nivel derivado y duración de capacidad. | Solo memoria; energía tiene uso limitado. |
| `Enemigo` | Posición, efecto, vida máxima 25, daño y derrota. | Instancias hardcodeadas. |
| `Mision` | Texto, tipo objetivo, consejo, recompensas y banderas de objetivo/completada. | Lista y secuencia gestionadas por el formulario. |
| `Combate` | Ataque, contraataque, efectos de líquido/fuego/veneno, poder por enemigo y daño al huir. | Reglas fijas; efectos identificados por cadenas. |
| Enums `Pantalla`, `FaseCombate` | Estados de UI y combate. | Transiciones manuales en Form1. |

También hay matrices `FilasBase` y `FilasGenesis`, lista de misiones y datos narrativos en el formulario. No existe entidad de inventario; `Recursos` es un contador y `tieneArma` un booleano.

## Funcionalidad real

Título, registro de nombre, cuatro clases, menú, manual y archivos IRIS, Nexo con Génesis disponible, base con cama/terminal/equipamiento/salida/puerta, exploración con casillas reveladas, botiquines y enemigos, combate por turnos y tres misiones secuenciales. La pantalla dibuja HUD y escenas mediante System.Drawing.

## Limitaciones

1. No se encontró DbContext, SQL Client, conexión, esquema/migración ni guardado/carga.
2. Toda la sesión se pierde al cerrar.
3. UI, reglas y catálogo están mezclados en Form1.
4. No existe pantalla ni uso funcional de inventario.
5. Mapas y misiones se alteran editando código.
6. Ventana fija Windows Forms; escala y tipografía limitan adaptación.
7. No aparecen pruebas automatizadas ni CI en el commit original.

```text
Program.Main -> Form1 -> timer / Paint / KeyDown
                         ├─ estado y navegación
                         ├─ mapas, cadete, misiones
                         ├─ combate
                         └─ dibujo System.Drawing
```
