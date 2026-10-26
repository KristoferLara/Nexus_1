# Diseño del videojuego NEXUS

> **Estado del documento:** propuesta de evolución del prototipo H3. La versión actual es un juego de consola por turnos; las pantallas gráficas y el movimiento lateral descritos aquí son objetivos, no funciones ya implementadas.

## 1. Visión del juego

NEXUS será una aventura de acción y exploración 2D para Windows, ambientada en la zona GENESIS y conectada con la Base Operativa que ya existen en el proyecto. El jugador dirige a un cadete/explorador, acepta misiones, recorre escenarios de pixel art, esquiva obstáculos, salta, recoge recursos y combate criaturas hasta enfrentar jefes que ponen a prueba lo aprendido.

La fantasía central es ser un cadete que se adentra en una realidad inestable y vuelve a la base con datos, recursos y experiencia. La motivación de largo plazo es desbloquear sectores y misiones, descubrir los efectos de cada enemigo y mejorar el nivel del explorador. Las tres amenazas ya nombradas por H3 —LIQUIDO, FUEGO y VENENO— pueden evolucionar de objetivos de misión a encuentros de jefe por sector. IRIS conserva su papel de sistema de alerta/anomalías.

**Promesa jugable:** en cada incursión el jugador decide si explora para encontrar suministros o avanza hacia el objetivo antes de perder demasiada vida. Los ataques, peligros del terreno y colisiones deben dar una respuesta visual y sonora inmediata.

## 2. Ciclo principal

El ciclo conserva el orden de misiones H3: localizar y derrotar al objetivo, volver a la base y reclamar la recompensa en el terminal. Para que encaje con acción 2D, el combate deja de ser un menú por turnos y pasa a resolverse con movimiento, salto, ataque y colisiones.

Diagrama editable: [ciclo principal](evidencias/ciclo-principal.mmd).

1. El jugador crea o selecciona un perfil local y carga una partida.
2. En la Base Operativa consulta misión, equipo y estado del cadete.
3. Selecciona una misión disponible y entra en el sector GENESIS correspondiente.
4. Explora de izquierda a derecha; puede moverse en profundidad limitada (carriles arriba/abajo), saltar, atacar, interactuar y recoger objetos.
5. Las colisiones activan daño, recolección, puertas, puntos de control o encuentros.
6. Derrota al objetivo de misión/jefe o regresa a la base. La derrota devuelve al último punto seguro con una penalización pequeña, definida en balance.
7. En el terminal entrega la misión y recibe experiencia y recursos; el nivel se calcula a partir de experiencia, como ya hace `Cadete.Nivel`.
8. Se guarda el progreso y se abre la siguiente misión/sector.

## 3. Mecánicas y reglas

### Explorador, movimiento y controles

- El protagonista se basa en `Cadete`: nombre, vida, energía, estabilidad, poder, experiencia, nivel y capacidad especial.
- Controles de teclado configurables: A/D o flechas izquierda/derecha para avanzar, W/S o flechas arriba/abajo para cambiar de carril, Espacio para saltar, J para ataque, E para interactuar, Esc para pausar.
- Movimiento lateral en un plano 2D con dos o tres carriles de profundidad. Arriba/abajo cambia de carril; no se propone un mundo 3D.
- Salto con gravedad sencilla y plataformas acotadas. Los límites, paredes y plataformas usan colisionadores rectangulares.
- La clase elegida existente (`TipoPersonaje`) puede convertirse en cuatro estilos con ataque o habilidad diferenciados. Para el primer corte solo se balancea una clase y se implementa una habilidad.
- La barra de vida del jugador y la del jefe, energía/poder y objetivo actual permanecen legibles durante el juego.

### Mundo, colisiones y objetos

- `MapaGenesis` conserva la idea de cuadrícula, posición y descubrimiento de zonas. La propuesta la presenta como un nivel lateral construido con segmentos y capas visuales; el mapa puede seguir señalando zonas conocidas.
- `BaseOperativa` continúa como zona segura para seleccionar misión, descansar y acceder al equipo.
- `Enemigo` conserva tipo, posición/sector, vida, efecto y estado derrotado. `Combate` aporta reglas de daño; el controlador nuevo consulta estas reglas en vez de duplicarlas.
- Un colisionador se prueba entre jugador-enemigo, jugador-peligro y jugador-objeto. El daño ocurre una vez por contacto o ataque válido y aplica un breve periodo de invulnerabilidad para evitar daño repetido cada fotograma.
- Los botiquines existentes (`Botiquin`) restauran vida hasta su máximo. Recursos y equipo se agregan al inventario; recogerlos muestra nombre, cantidad y confirmación.
- Los peligros ambientales del sector FUEGO/VENENO/LÍQUIDO aplican efectos diferenciados, inspirados en `Enemigo.Efecto`.

### Misiones, jefe y progresión

- Las misiones H3 son secuenciales: LIQUIDO, FUEGO y VENENO; el índice no avanza hasta reclamar recompensa en el terminal.
- Cada misión tiene objetivo, sector, enemigo objetivo, texto, XP/recurso y estado. El progreso se guarda tras eventos importantes.
- El jefe es el enemigo objetivo del sector y tiene una barra de vida propia. La primera versión puede usar una sola fase y ataques telegrafiados (animación breve antes del golpe).
- `Cadete.Nivel` se deriva de XP a razón de 100 XP por nivel en el estado actual. La UI mostrará XP actual y progreso al siguiente nivel. El balance de premios se ajustará para que las tres misiones den progreso visible sin prometer contenido infinito.
- La derrota no debe borrar perfil ni avance de misiones ya reclamadas. Al volver a la base, la vida se restaura parcialmente o se consume un botiquín, decisión que debe fijarse con pruebas de juego.

### Inventario

La versión inicial requiere una lista de recursos y consumibles con cantidad; equipo seleccionable es prioridad secundaria. El inventario permite ver descripción, cantidad y efecto, pero no incluye fabricación, comercio ni objetos con estadísticas complejas en el primer alcance.

## 4. Diseño de pantallas y navegación

Los siguientes wireframes son esquemáticos; ilustran jerarquía y navegación, no un estilo gráfico final. Resolución de referencia: ventana 1280×720, con escalado entero del área pixel art cuando sea posible.

### A. Menú principal

```text
┌──────────────────────────────────────────────────────────────┐
│ N E X U S                         [Perfil: Cadete 01]        │
│                                                              │
│                 [ ilustración pixel art de GENESIS ]         │
│                                                              │
│                    ┌──────────────────────┐                  │
│                    │ CONTINUAR             │                  │
│                    │ NUEVA PARTIDA         │                  │
│                    │ EXPLORADORES          │                  │
│                    │ OPCIONES              │                  │
│                    │ SALIR                 │                  │
│                    └──────────────────────┘                  │
│                                      Build / estado SQL       │
└──────────────────────────────────────────────────────────────┘
```

**Visible:** logo, fondo, perfil activo y estado discreto de guardado. **Navegación:** Continuar abre base/última misión; Nueva Partida crea perfil/partida; Exploradores abre selección; Opciones configura controles/volumen; Salir cierra con confirmación si hay cambios no guardados.

### B. Exploración / mapa

```text
┌──────────────────────────────────────────────────────────────┐
│ GENESIS · SECTOR 01       OBJETIVO: localizar al LIQUIDO     │
│ VIDA ███████░░  ENERGÍA █████░  PODER ███░░  [IRIS: ALERTA] │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│   [botiquín]       (enemigo)             [puerta / objetivo] │
│       +              ▼                         ◇             │
│  ────────────────┐       ┌──────────────────────────────     │
│       cadete  →  │ carril│     plataforma                    │
│  ────────────────┴───────┴──────────────────────────────     │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│ WASD/Flechas mover · Espacio saltar · J atacar · E interactuar│
│ [mini mapa / sector]                             [ESC Pausa] │
└──────────────────────────────────────────────────────────────┘
```

**Visible:** escena pixel art, HUD persistente, objetivo y controles contextuales. El mini mapa se puede ocultar si tapa la acción. La cámara sigue al jugador dentro de límites del nivel.

### C. Misiones

```text
┌──────────────────────────── BASE OPERATIVA ──────────────────┐
│ [Resumen del cadete: Nivel 2 · XP 130/200 · Vida 82/100]    │
├────────────────────────────┬─────────────────────────────────┤
│ DISPONIBLES                 │ DETALLE                         │
│ ● 01 El primer objetivo     │ Sector: GENESIS / Zona húmeda   │
│ 🔒 02 La prueba de fuego    │ Objetivo: derrotar a LIQUIDO    │
│ 🔒 03 El último veneno      │ Recompensa: XP + recursos       │
│                             │ [ACEPTAR] [VOLVER]               │
├────────────────────────────┴─────────────────────────────────┤
│ Terminal de base: entregar misión completada para cobrar XP  │
└──────────────────────────────────────────────────────────────┘
```

**Visible:** estados bloqueada/disponible/en curso/completada y detalle/recompensa. La secuencia impide saltar objetivos. Tras vencer al objetivo el CTA cambia a «Regresar y entregar».

### D. Exploradores / inventario

```text
┌──────────────────────── EXPLORADOR ──────────────────────────┐
│ [sprite cadete]  Nombre: ...  Clase: ...  Nivel: 2           │
│ VIDA ████████░░  ENERGÍA ██████░░  PODER ████░░              │
├──────────────────────────────┬────────────────────────────────┤
│ EQUIPO                       │ INVENTARIO                     │
│ Arma: Katana eléctrica       │ Botiquín x2   Cura 25 vida     │
│ Habilidad: Impulso IRIS      │ Cristal x4    Recurso           │
│ [EQUIPAR]                    │ [USAR] [DETALLE]                │
├──────────────────────────────┴────────────────────────────────┤
│ [VOLVER A BASE]                                  [GUARDAR]     │
└──────────────────────────────────────────────────────────────┘
```

**Visible:** sprite/identidad, barras, equipo e inventario con acción y confirmación. Los objetos no disponibles se muestran con explicación, no como botones silenciosos.

### Navegación global

Menú → Base Operativa → Misiones → Exploración → Resultado → Base. Desde la base se abren Explorador/Inventario y opciones. Pausa durante exploración ofrece Continuar, Reiniciar punto de control, Volver a base y Opciones. Confirmar una acción destructiva o abandonar una misión informa qué progreso se conserva.

## 5. Experiencia de usuario

- **Legibilidad:** texto principal de al menos 16 px a 1280×720; contraste alto; no depender solo del color (usar icono y etiqueta además de color); tipografía simple y pocos renglones por aviso.
- **Jerarquía:** mundo y personaje ocupan el área mayor; objetivo y salud quedan en la franja superior; controles en una franja secundaria; datos no urgentes se consultan en pausa.
- **Consistencia:** mismo icono/color para salud, energía, peligro, misión y selección en todas las pantallas; mismas teclas para navegar y volver.
- **Navegación:** cada pantalla indica dónde está el jugador, ofrece una salida visible y mantiene el foco del teclado. Se puede jugar sin ratón.
- **Retroalimentación:** al moverse, atacar, recibir daño, recoger, fallar una colisión o guardar, mostrar animación/sonido/mensaje breve; barras actualizadas inmediatamente; acción inválida explica por qué.
- **Accesibilidad y comodidad:** pausa en cualquier momento fuera de cinemáticas, volumen separado, reasignación básica de teclas, opción de reducir destellos y animación de daño no basada únicamente en parpadeo.
- **Pixel art:** dibujar a resolución interna fija (p. ej. 320×180) y ampliar con interpolación vecina más cercana, evitando suavizado que difumine píxeles.

## 6. Alcance y restricciones

### Prioridad para una entrega de dos meses y nivel inicial

1. Mantener el código de H3 como referencia y migrar sus entidades/reglas gradualmente.
2. Ventana WinForms, menú básico, selección de perfil local y guardado/carga SQL Server.
3. Un nivel corto de GENESIS, movimiento lateral, salto, colisión con suelo/pared, un enemigo común, un botiquín y un jefe con barra de vida.
4. HUD de salud, energía, objetivo; combate en tiempo real básico; resultado y regreso a base.
5. Una misión completa con XP y guardado; luego adaptar LIQUIDO/FUEGO/VENENO a tres misiones secuenciales si el calendario lo permite.
6. Documentación, pasos de instalación y demostración reproducible.

### Opcional si el núcleo queda estable

- Tres clases jugables balanceadas, varios niveles y jefes con fases.
- Animaciones propias completas, música, efectos y cinemáticas.
- Equipamiento con estadísticas, logros, múltiples espacios de guardado, editor de niveles.
- Inicio de sesión con contraseña y recuperación en línea. Para una entrega local es preferible empezar con selección de perfil sin contraseña; autenticar contra una base local no protege por sí solo contra manipulación del cliente.

### Restricciones y riesgos

- H3 es una aplicación de consola en un único `Program .cs` de aproximadamente 140 KB; no es una base WinForms ya desacoplada. La interfaz, orquestación, estado y dominio se mezclan parcialmente.
- Dos meses y nivel básico no permiten prometer arte extenso ni juego de plataformas completo. Se propone un vertical slice pequeño, no una campaña de muchas horas.
- WinForms es un marco de escritorio Windows, no un motor de videojuegos. Es viable para una escena 2D sencilla dibujada con GDI+ y sprites; requiere controlar el bucle de actualización, repintado y colisiones.
- El proyecto actual apunta a `net10.0` como consola y depende de LibVLCSharp, System.Drawing.Common y VideoLAN.LibVLC.Windows. La app WinForms necesitará un proyecto Windows separado con `net10.0-windows` y `UseWindowsForms`; no se debe cambiar el proyecto existente hasta crear y validar la nueva entrada.
- SQL Server local exige instalar/ejecutar una instancia y configurar conexión. Un perfil local no es un servicio de cuentas remoto ni habilita sincronización entre equipos.
- La carga de sprites debe usar rutas de recursos documentadas y archivos con licencia clara. El arte puede empezar con formas/sprites provisionales.

## 7. Supuestos que deben confirmarse al empezar la implementación

- El juego final será solo para Windows y un jugador.
- El proyecto de dos meses es una demo evaluable; la prioridad es un nivel bien jugable.
- Se usará un perfil local, sin autenticación online.
- El nivel 2D usa carriles para arriba/abajo y salto; no será una vista cenital tradicional.
- Las reglas exactas de penalización al perder y recuperación de vida se definirán tras probar el prototipo.
