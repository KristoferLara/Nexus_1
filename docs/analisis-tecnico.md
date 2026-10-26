# Análisis técnico y decisiones

## Resumen

La ruta más viable para dos meses y nivel inicial es un vertical slice Windows, un solo nivel lateral, un cadete, un enemigo común y un jefe; conservar misión, recompensa, XP, botiquín y efectos conocidos del H3. WinForms es adecuado para formularios y una escena 2D modesta; no reemplaza un motor con editor de niveles/animación. SQL Server se usa para perfiles y guardados, no para actualizar estado por fotograma.

## Alternativas evaluadas

| Decisión | Alternativas | Ventajas | Desventajas | Recomendación |
|---|---|---|---|---|
| Interfaz 2D | WinForms; WPF; motor 2D (p. ej. MonoGame) | WinForms: ya está en el objetivo, controles/eventos, fácil de aprender si el proyecto es C#. | No es motor de juegos; animación, cámara y colisión se implementan a mano; está limitado a Windows. | WinForms para la demo exigida. Si el objetivo central pasa a ser juego de plataformas de alcance mayor, evaluar MonoGame antes de invertir en más niveles. |
| Dibujo | controles/imágenes por objeto; control de dibujo personalizado `Paint` con `Graphics` | Paint centraliza escena/HUD y permite escalar sprites; doble búfer ayuda contra parpadeo. | GDI+ no aporta editor, físicas ni atlas; se debe medir rendimiento. | Un `GameView` dibujado en Paint, timer de actualización fijo y doble búfer; mantener lógica fuera del método Paint. |
| Persistencia | archivos JSON; SQL Server con ADO.NET; EF Core | JSON es simple y útil para prototipos sin servidor. SqlClient permite SQL directo y control explícito. EF Core reduce mapeo manual y facilita migraciones si el modelo crece. | JSON no satisface persistencia relacional SQL requerida; ADO.NET requiere consultas/mapas explícitos; EF Core suma conceptos y configuración inicial. | SqlClient/ADO.NET parametrizado para pocas tablas y aprendizaje; valorar EF Core solo si el esquema crece. No persistir por fotograma. |
| SQL de desarrollo | SQL Server LocalDB; Express como instancia; servidor remoto | LocalDB minimiza administración local; Express permite instancia más completa y acceso configurado; servidor permite compartir perfiles. | Hay que instalar motor; entornos de máquinas escolares varían; SQL remoto agrega despliegue y seguridad. | LocalDB/Express local para demo. No conectar aplicación cliente directamente a DB pública. |
| Inicio de sesión | sin contraseña/perfil local; usuario+contraseña local; servicio de autenticación en red | Perfil local facilita elegir guardado y es suficiente para un usuario. Servicio habilita cuentas reales. | Login local no ofrece recuperación ni identidad segura online; backend supera calendario. | Perfil local sin contraseña en el alcance base. Si evaluación lo exige, añadir autenticación local como opción documentada y hashing de contraseña. |
| Reutilización de H3 | reescritura total; copiar código a la UI; extraer dominio progresivamente | Extracción preserva reglas de clase/efectos/XP y reduce riesgo. | Requiere refactor gradual; partes mezcladas necesitan adaptación. | Mantener consola como referencia y crear biblioteca `GameCore`; migrar primero `Cadete`, `Enemigo`, `Mision` y reglas, sin copiar la interfaz de consola. |

## Decisiones recomendadas

1. **Separar solución en proyectos:** `Nexus.GameCore` (modelos/reglas), `Nexus.Data.SqlServer` (repositorios) y `Nexus.WinForms` (presentación). El proyecto H3 queda hasta que el corte nuevo demuestre paridad básica.
2. **Dibujar con escala fija:** área lógica 320×180, sprites pixel art y vecino más cercano; los menús pueden usar controles WinForms. Actualizar el estado con temporizador, dibujar desde estado inmutable y habilitar doble búfer.
3. **Colisiones simples:** rectángulos de jugador, plataformas, enemigos y objetos. No simular física compleja; probar esquinas, paredes, salto y daño repetido.
4. **Preservar decisiones de diseño:** tres misiones secuenciales, entrega de recompensa en base, efectos LIQUIDO/FUEGO/VENENO, curación de botiquín, XP/nivel de `Cadete`, datos IRIS.
5. **Guardar en límites de misión:** al regresar a base y completar objetivo/checkpoint. Guardar un conjunto relacionado dentro de transacción. La capa UI no espera a SQL dentro del bucle de escena.
6. **Mantener prototipo jugable:** una migración incremental permite comparar controles/reglas y volver atrás si el refactor supera el calendario.

## Plan sugerido de ocho semanas

| Semana | Resultado comprobable |
|---|---|
| 1 | Solución con proyectos y GameCore mínimo; ejecutar prototipo H3 y registrar reglas que se conservan. |
| 2 | Ventana WinForms, menú/base y navegación por teclado; escena con formas provisionales. |
| 3 | Movimiento, salto, suelo/plataformas y colisiones depurables. |
| 4 | Enemigo común, ataque, daño, botiquín, HUD de vida/energía. |
| 5 | Jefe de un patrón, barra, victoria/derrota y regreso a base. |
| 6 | Perfil/partida SQL, esquema y guardar/cargar en puntos seguros. |
| 7 | Misión completa con XP, entrega en terminal y continuidad secuencial; arte pixel de alcance pequeño. |
| 8 | Corrección guiada, README, instrucciones de instalación y demo; funciones opcionales solo si todo lo anterior funciona. |

## Riesgos y mitigaciones

- **Alcance artístico:** crear un sprite y un tileset pequeño; evitar prometer múltiples zonas/animaciones.
- **Complejidad gráfica:** un control de dibujo para escena; formularios WinForms ordinarios para pantallas de menú/inventario.
- **Bloqueos de UI por SQL:** persistir solo al guardar/eventos y usar operaciones asíncronas o fuera del timer cuando sea necesario.
- **Diferencia entre turnos y acción:** adaptar reglas, no reusar bucles de consola. Daño, defensa e invulnerabilidad deben balancearse jugando.
- **Instalación SQL en equipo de evaluación:** entregar guía para LocalDB/Express y una comprobación de conexión visible. Una opción JSON para modo sin DB puede ayudar a desarrollo, pero no reemplaza la entrega SQL.
- **IA generativa:** puede inventar APIs, compatibilidad o comportamiento de paquetes; verificar cada decisión técnica contra documentación oficial y el archivo de proyecto real. Mantener decisiones y límites visibles en estos documentos.

## Fuentes técnicas consultadas

Fuentes oficiales consultadas el 26-10-2026; las páginas vivas pueden cambiar. Para propiedades de paquetes, confirmar la versión usada al momento de implementarla.

- Microsoft Learn, [Windows Forms overview](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/overview/): WinForms es una plataforma de escritorio Windows con controles, gráficos, enlace de datos, eventos y diseñador.
- Microsoft Learn, [Target frameworks in SDK-style projects](https://learn.microsoft.com/en-us/dotnet/standard/frameworks): indica `net10.0-windows` para WinForms/WPF; el H3 actual declara `net10.0`.
- Microsoft Learn, [Windows Forms graphics and drawing](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/advanced/graphics-and-drawing-in-windows-forms): dibujo personalizado se apoya en objetos gráficos y el ciclo de pintura.
- Microsoft Learn, [Double buffered graphics](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/advanced/double-buffered-graphics): el doble búfer compone primero fuera de pantalla y reduce parpadeo.
- Microsoft Learn, [SQL Server Express LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb): LocalDB es una característica de Express dirigida a desarrolladores y se instala/configura aparte.
- Microsoft Learn, [Microsoft.Data.SqlClient for SQL Server](https://learn.microsoft.com/en-us/sql/connect/ado-net): proveedor ADO.NET para conectar, ejecutar consultas parametrizadas y comandos.
- Microsoft Learn, [Configure parameters](https://learn.microsoft.com/en-us/sql/connect/ado-net/configure-parameters): usar parámetros para enviar comandos SQL y valores separados.
- Microsoft Learn, [SqlClient security best practices](https://learn.microsoft.com/en-us/sql/connect/ado-net/sql/application-security-scenarios-sql-server): tratar cifrado, identidad, consultas y privilegios como partes distintas de la seguridad.
- Repositorio fuente: [Nexus_1/Program .cs](https://github.com/KristoferLara/Nexus_1/blob/master/Nexus_1/Program%20.cs), [Nexus_1.csproj](https://github.com/KristoferLara/Nexus_1/blob/master/Nexus_1/Nexus_1.csproj), [README](https://github.com/KristoferLara/Nexus_1/blob/master/README.md).

## Verificación de información obtenida con IA

Se contrastaron las afirmaciones principales con el repositorio y documentación oficial:

| Afirmación usada | Verificación |
|---|---|
| El H3 ya tiene niveles, XP, 3 misiones y clases | Revisado en `Program .cs`: `Cadete.Nivel` se deriva de XP/100; `EstadoJuego` crea LIQUIDO/FUEGO/VENENO y conserva tipos de personaje. |
| El H3 ya guarda en SQL | **Falso** para el commit inspeccionado: árbol y `.csproj` no muestran módulo SQL/paquete SqlClient; se documenta como trabajo nuevo. |
| El H3 usa WinForms y movimiento de plataformas | **Falso**: `.csproj` declara `Exe` y `net10.0`, no `UseWindowsForms`; el mapa es cuadrícula y el combate es por turnos/terminal. |
| WinForms funciona al declarar solo `net10.0` | Verificado como incorrecto: la guía TFM recomienda `net10.0-windows` para WinForms y `UseWindowsForms=true`. |
| WinForms incluye un motor de plataformas con física | No sustentado por documentación WinForms; colisiones, actualización y reglas deben implementarse o usarse otro framework. La propuesta limita la física a rectángulos. |
| Los parámetros SQL sirven para separar datos de la consulta | Contrastado en guía oficial SqlClient; se recomienda parametrizar, transaccionar guardados relacionados y evitar SQL concatenado. |
| Contraseña local equivale a cuenta online protegida | No. La propuesta lo limita a perfil local, recomienda no introducir contraseña inicialmente y exige hashing/servidor separado si se amplía a login real. |

Esta verificación confirma afirmaciones documentales, no que el juego propuesto ya haya sido implementado. El código y las decisiones deben volver a revisarse si cambia la rama o versión.
