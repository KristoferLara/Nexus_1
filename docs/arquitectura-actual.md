# Arquitectura actual de NEXUS: rama de referencia C#

## Alcance de esta descripción

Esta documentación se basa en la rama `feature/mi-codigo-csharp` (commit inspeccionado: `8c4965ce75b6a33a3998a2f9df301f7f8c5116d4`). El código central es `Nexus_1/Program .cs`, una aplicación de consola. La carpeta `NEXUS_WF/` en `main` es un prototipo gráfico aproximado de una posible evolución y se mantiene como referencia visual; no es la fuente principal para describir la lógica de la rama de consola.

## Proyecto y tecnologías

La solución de la rama de referencia es `Nexus_1.slnx`, que referencia `Nexus_1/Nexus_1.csproj`. El proyecto es `OutputType=Exe`, `TargetFramework=net10.0`, con implicit usings y nullable habilitados. Declara paquetes LibVLCSharp 3.10.1, System.Drawing.Common 10.0.11 y VideoLAN.LibVLC.Windows 3.0.23.1. El flujo inspeccionado es principalmente consola. No se encontró capa SQL ni persistencia en el árbol descrito.

El archivo fuente se llama literalmente `Program .cs` (con espacio antes de la extensión), contiene la clase estática `Program`, el método `Main` y clases internas/estado auxiliar. La solución organiza el código en un único archivo muy extenso.

## Componentes observados en `Program .cs`

- **`Program.Main`:** configura cursor y codificación UTF-8, dibuja logo ASCII con color/sonidos, solicita nombre, contraseña y realidad, crea `EstadoJuego`, realiza autorización y ejecuta misión.
- **`EstadoJuego`:** estado mutable de la sesión: nombre, realidad, energía, estabilidad, conexión, detector, dron, PEM/pulso, bloqueo de IRIS, anomalía localizada/ubicación, clase, arma, vida, poder, recursos, duración de capacidad, posición/distancia, posición del dron, matriz de Génesis, zonas exploradas y vida/presencia de enemigos.
- **Registro y selección:** `RegistrarNombre`, `RegistrarContraseña`, `RegistrarRealidad`, `LeerEntero`, `SeleccionarPersonaje`. La contraseña literal actualmente aceptada es `k-dete`; es una limitación de prototipo y no autenticación segura.
- **Misión / flujo:** `EjecutarMision` y rutinas de navegación dan secuencia a la operación.
- **Exploración:** movimiento en mapa Génesis, distancia de 10 metros por casilla, zonas reveladas, detector, dron y recolección de recursos.
- **Anomalías:** evento estático `AnomaliaDetectada` y manejador `DispararAlertaIris`, más variables de localización y bloqueo.
- **Combate y clases:** constantes de costes para ataques, habilidades y capacidad; especialidades FISICO, ARMAMENTO, MANA y HERRAMIENTAS.
- **Presentación:** Console, colores y secuencias ASCII/glitch/Beep/Thread.Sleep. La presentación y la lógica comparten el mismo archivo.

## Reglas/valores explícitos

- Umbral bajo: 30.
- Desplegar dron: coste 5 y mínimo 5 de energía; EMP/pulso: coste 15 y mínimo 15, duración de bloqueo IRIS 3.
- Desconexión segura: estabilidad mínima 15.
- Ataque físico: coste 10; ataque fuerte: 25; habilidad Mana: 20.
- Ventaja HERRAMIENTAS: dron coste 3, pulso coste 10.
- Capacidad especial: umbral de poder 50 y duración 3 turnos.
- Cadete comienza con vida 100, poder 0; mapa Génesis es una matriz 8×11; exploración reporta 10 m por casilla.
- El estado de referencia incluye detector inicialmente, dron/PEM inicialmente ausentes y lógica de enemigo representada mediante matrices.

Estos valores se documentan a partir de constantes/campos visibles; no describen una calibración de balance validada.

## Limitaciones reales

1. Una clase y un archivo fuente concentran flujo, reglas, estado y presentación; cambios son difíciles de aislar y probar.
2. Estado de sesión mutable en RAM; al cerrar se pierde.
3. Autenticación por contraseña hardcodeada no es segura ni una identidad persistente.
4. Muchas propiedades son booleanos, strings y matrices; IDs/tipos definidos ayudarían a evitar estados inválidos.
5. El proyecto incluye paquetes gráficos/VLC aunque el flujo inspeccionado es de consola; confirmar su uso y necesidad antes de recortar dependencias.
6. No se encontraron base SQL, repositorio, migraciones ni guardado/carga.
7. No se encontró proyecto de pruebas ni CI en el árbol de esa rama.
8. La reproducción de Beep/Thread.Sleep puede comportarse distinto según terminal/host; el render de consola depende de tamaño y fuente.
9. La rama Windows Forms es una demostración paralela de interfaz; no se debe asumir que sus misiones/combate por turnos sustituyen las reglas de la consola.

## Diagrama actual

```text
Program.Main
 ├─ logo/sonido y terminal
 ├─ registro + autorización + selección
 ├─ EstadoJuego (sesión en memoria)
 ├─ flujo de misión y exploración Génesis
 ├─ equipo/energía/clases/combate
 └─ eventos de anomalía -> alerta IRIS
```

Las propuestas de separación y persistencia se encuentran en los documentos hermanos y no forman parte de esta rama actual.
