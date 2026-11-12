# Evidencias: wireframes y navegación

Bocetos editables de baja fidelidad incluidos en `docs/diseno-del-juego.md`, sección “Pantallas, navegación y wireframes”. Cubren menú principal, exploración/mapa, misiones y cadete/inventario. El último es una propuesta nueva, no una captura funcional.

```mermaid
stateDiagram-v2
 [*] --> Titulo
 Titulo --> Registro: Enter
 Registro --> Seleccion: nombre válido
 Seleccion --> Menu: elegir clase
 Menu --> Base: abrir Nexo
 Base --> Mapa: salida confirmada
 Base --> Misiones: terminal
 Mapa --> Combate: enemigo
 Combate --> Mapa: victoria / huida
 Mapa --> Base: regreso
 Menu --> CadeteInventario: acceso Cadete propuesto
 CadeteInventario --> Menu: Esc
```
