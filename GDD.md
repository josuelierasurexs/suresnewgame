# GDD — Surexs Dance Off

**Documento de diseño de juego / especificación para desarrollo con IA**

- **Proyecto:** Surexs Dance Off
- **Engine:** Unity 6.6
- **Plataforma:** PC
- **Input principal:** Gamepad / control
- **Input secundario de desarrollo:** Teclado
- **Jugadores:** Solo / 1 vs 1 local
- **Duración objetivo:** 30–90 segundos por partida
- **Estilo:** 2D / 2.5D, con elementos 3D opcionales
- **Referencia de diseño:** minijuegos de ritmo de Mario Party y LEGO Party, especialmente la idea de dos jugadores siguiendo indicaciones de movimiento.

---

# 1. Resumen

**Surexs Dance Off** es un minijuego de ritmo competitivo en el que uno o dos jugadores controlan a un empleado de Surexs mientras realizan diferentes poses relacionadas con situaciones de trabajo.

Durante una canción aparecen tiles que se desplazan hacia una zona de ejecución.

Cada tile representa una de tres acciones:

- `LEFT` — presionar izquierda.
- `CENTER` — no presionar ningún botón.
- `RIGHT` — presionar derecha.

Cuando el tile llega a la zona de ejecución, el jugador debe realizar la acción correcta en el momento adecuado.

Cada acierto proporciona puntos. Los aciertos consecutivos generan un combo y un multiplicador de puntuación.

En el modo 1 vs 1, ambos jugadores reciben exactamente la misma secuencia musical y compiten por obtener la puntuación más alta.

El juego debe ser sencillo de entender, rápido de jugar y fácil de repetir.

---

# 2. Objetivos del proyecto

## Objetivos principales

1. Crear un minijuego de ritmo funcional para PC.
2. Tener partidas de menos de 90 segundos.
3. Utilizar controles extremadamente sencillos.
4. Hacer que el juego sea divertido incluso con una sola mecánica principal.
5. Crear un sistema de charts editable sin necesidad de modificar código.
6. Separar completamente:
   - ritmo,
   - input,
   - puntuación,
   - animaciones,
   - UI,
   - datos del chart.
7. Permitir agregar nuevas canciones, poses y personajes posteriormente.
8. Permitir jugar:
   - Solo.
   - 1 vs 1 local.

## No objetivos del MVP

No implementar inicialmente:

- Online multiplayer.
- CPU / inteligencia artificial.
- Tienda.
- Inventario.
- Selección compleja de personajes.
- Progresión RPG.
- Sistema de monedas.
- Editor visual completo de charts.
- Múltiples escenarios complejos.
- Cinemáticas.
- Sistema de matchmaking.
- Backend.
- Cuenta de usuario.

---

# 3. Fantasía del jugador

La fantasía es:

> "Soy un empleado de Surexs y tengo que demostrar que soy el empleado más chingón de la oficina."

El humor debe venir de las poses y situaciones de trabajo.

Ejemplos:

- Contestando el teléfono.
- Escribiendo en la computadora.
- Presentando una gráfica.
- Tomando café.
- Tomando notas.
- Levantando la mano en una junta.
- Revisando documentos.
- Haciendo una videollamada.
- Celebrando una venta.
- Entrando en pánico por un entregable.
- "Reunión que pudo ser un correo".
- "Entregable para ayer".
- Baile corporativo.

La mecánica es sencilla; la personalidad debe venir principalmente del arte, las animaciones, los textos y el feedback.

---

# 4. Modos de juego

## 4.1 Solo

Un jugador juega una canción y busca conseguir la mayor puntuación posible.

Al terminar se muestran:

- Score.
- Perfect.
- Great.
- Good.
- Miss.
- Accuracy.
- Combo máximo.
- Multiplicador máximo.

El modo Solo NO necesita un oponente.

---

## 4.2 1 vs 1 Local

Dos jugadores juegan simultáneamente.

Cada jugador tiene:

- Su propio personaje.
- Su propia puntuación.
- Su propio combo.
- Su propio multiplicador.
- Su propio feedback.

Ambos reciben exactamente los mismos eventos del chart.

No debe existir interacción física entre personajes en el MVP.

La competencia se basa exclusivamente en quién ejecuta mejor la secuencia musical.

Al terminar la canción se comparan los scores y se muestra el ganador.

Debe existir la posibilidad de:

- Rematch.
- Regresar al menú principal.

---

# 5. Controles

El juego está diseñado principalmente para controles.

## Jugador 1

- D-Pad izquierda = `LEFT`
- D-Pad derecha = `RIGHT`
- Ningún botón = `CENTER`

## Jugador 2

- D-Pad izquierda = `LEFT`
- D-Pad derecha = `RIGHT`
- Ningún botón = `CENTER`

Para desarrollo y pruebas también debe existir soporte temporal de teclado.

### Teclado P1

- `A` = LEFT
- `D` = RIGHT
- Ninguna tecla = CENTER

### Teclado P2

- `Left Arrow` = LEFT
- `Right Arrow` = RIGHT
- Ninguna tecla = CENTER

---

# 6. Regla especial de CENTER

`CENTER` representa una ausencia de input.

Esto significa que el jugador NO debe presionar izquierda ni derecha durante la ventana de ejecución.

El sistema debe detectar correctamente:

- Que no exista input durante la ventana.
- Que una entrada izquierda/derecha durante CENTER sea un error.
- Que el jugador pueda haber presionado una dirección previamente pero la haya soltado antes de la ventana.

La lógica de CENTER debe estar claramente separada de la lógica de LEFT/RIGHT.

---

# 7. Gameplay

La pantalla representa una pista de ritmo.

Los tiles aparecen a distancia y avanzan hacia una línea de ejecución.

Ejemplo conceptual:

```text
             FUTURO

        ┌─────────────┐
        │      ←      │
        └─────────────┘

             ┌─────┐
             │  ○  │
             └─────┘

                  ┌─────┐
                  │  →  │
                  └─────┘


=================================
          HIT ZONE
=================================

             PERSONAJE
```

El jugador debe ejecutar la acción correspondiente cuando el tile alcanza la zona de ejecución.

---

# 8. Direcciones

Solo existen tres direcciones en el MVP:

```text
LEFT
CENTER
RIGHT
```

No implementar inicialmente:

- UP.
- DOWN.
- Diagonales.
- Botones adicionales.
- Joysticks.
- Gatillos.
- Combinaciones.

La simplicidad es parte fundamental del diseño.

---

# 9. Poses

Las direcciones y las poses deben ser sistemas independientes.

Una dirección puede tener múltiples poses.

Ejemplos:

```text
LEFT + PHONE
LEFT + COFFEE
LEFT + DOCUMENT

CENTER + TYPING
CENTER + MOUSE
CENTER + THINKING

RIGHT + PRESENTATION
RIGHT + MEETING
RIGHT + CELEBRATION
```

El sistema de ritmo solamente debe evaluar la dirección.

El sistema del personaje decide qué animación/pose reproducir.

Esto permite agregar nuevas poses sin modificar el sistema de ritmo.

---

# 10. Diseño visual

## Estilo

El proyecto debe ser principalmente 2D / 2.5D.

Referencia visual conceptual:

- Paper Mario.
- Juegos de minijuegos de Mario Party.
- LEGO Party.

## Personajes

Los personajes principales deben ser 2D.

Preferentemente utilizar:

- Sprite sheets.
- Animator.
- Animaciones independientes para las diferentes poses.

## Tiles

Los tiles pueden utilizar elementos 3D.

Esto permite:

- Profundidad.
- Perspectiva.
- Iluminación.
- Rotación.
- Animaciones.
- Sensación de objeto físico.

No es obligatorio que todos los elementos sean 3D.

---

# 11. Escenario inicial

El primer escenario será una oficina de Surexs.

Debe funcionar como fondo visual y no debe requerir sistemas complejos.

Elementos posibles:

- Escritorios.
- Computadoras.
- Plantas.
- Ventanas.
- Monitores.
- Sillas.
- Pizarrones.
- Elementos corporativos de Surexs.

La prioridad visual debe estar en:

1. Personaje.
2. Tiles.
3. Zona de ejecución.
4. Score.
5. Combo.

---

# 12. Sistema de ritmo

El tiempo de la canción debe ser la referencia principal del gameplay.

El sistema debe utilizar el tiempo de reproducción del `AudioSource` como reloj principal.

NO utilizar `Time.time` como reloj principal del ritmo.

Debe existir un sistema central que permita convertir:

```text
song time
    ↓
chart event
    ↓
tile position
    ↓
hit window
```

---

# 13. Timing / Judge

Cada evento debe evaluarse según la diferencia entre:

```text
tiempo esperado
-
tiempo real del input
```

Ventanas iniciales:

| Resultado | Ventana |
|---|---:|
| PERFECT | ±0.050 s |
| GREAT | ±0.100 s |
| GOOD | ±0.175 s |
| MISS | > ±0.175 s |

Estos valores deben ser configurables desde Inspector.

No deben estar hardcodeados en múltiples scripts.

---

# 14. Score

Puntuación base:

| Resultado | Puntos |
|---|---:|
| PERFECT | 100 |
| GREAT | 75 |
| GOOD | 50 |
| MISS | 0 |

La puntuación final debe considerar el multiplicador.

Ejemplo:

```text
PERFECT = 100
COMBO = 25
MULTIPLIER = x4

100 × 4 = 400 puntos
```

---

# 15. Combo

El combo aumenta por cada acierto consecutivo.

Cualquier MISS rompe el combo.

Multiplicadores iniciales:

```text
0–4 combo   = x1
5–9 combo   = x2
10–19 combo = x3
20–29 combo = x4
30+ combo   = x5
```

Estos valores deben ser configurables.

El sistema de combo debe ser independiente del sistema de puntuación.

---

# 16. Feedback de gameplay

Cada evaluación debe generar feedback visual y sonoro.

## PERFECT

Mostrar:

```text
PERFECT
+500
COMBO x12
```

## GREAT

```text
GREAT
+300
```

## GOOD

```text
GOOD
+200
```

## MISS

```text
MISS
COMBO BREAK
```

El personaje debe reproducir la pose asociada cuando el evento es ejecutado correctamente.

---

# 17. Mensajes de racha

Cuando el jugador alcanza determinados combos, mostrar mensajes especiales.

Valores iniciales:

```text
5  = GOOD START!
10 = COMBO!
20 = AMAZING!
30 = FANTASTIC!
50 = UNSTOPPABLE!
75 = SUREXS LEGEND!
```

Estos textos deben poder editarse desde una configuración central.

No deben estar distribuidos como strings hardcodeados en diferentes scripts.

---

# 18. Animaciones de feedback

Cuando el jugador acierta:

- Ejecutar pose.
- Mostrar resultado.
- Mostrar puntos.
- Actualizar combo.
- Actualizar score.
- Aplicar pequeño feedback visual.
- Reproducir SFX.

Opcional:

- Partículas.
- Screen shake leve.
- Escala temporal de UI.
- Flash.
- Animación del tile.

Cuando falla:

- Mostrar MISS.
- Romper combo.
- Reproducir animación de error.
- Reproducir SFX de error.

---

# 19. Chart

El chart debe estar separado del código.

Inicialmente utilizar un archivo JSON.

Ejemplo:

```json
{
  "song": "surexs_demo",
  "visualSpeed": 1.0,
  "tiles": [
    {
      "time": 1.20,
      "direction": "left",
      "pose": "phone"
    },
    {
      "time": 1.80,
      "direction": "center",
      "pose": "typing"
    },
    {
      "time": 2.40,
      "direction": "right",
      "pose": "presentation"
    }
  ]
}
```

---

# 20. Datos del tile

Cada tile debe poder contener como mínimo:

```text
time
direction
pose
```

Opcionalmente:

```text
speed
duration
variant
```

Pero no agregar campos innecesarios en el MVP.

---

# 21. Timing vs velocidad visual

Separar:

## Timing

Indica cuándo debe ejecutarse el evento.

```text
time = 5.25
```

## Visual Speed

Indica qué tan rápido se desplaza visualmente el tile.

```text
visualSpeed = 1.0
```

Esto permite modificar la presentación visual sin cambiar el ritmo real de la canción.

---

# 22. Editor de charts

## Primera versión

NO construir inmediatamente un editor visual complejo.

El chart debe poder editarse directamente en JSON.

El sistema debe permitir cambiar fácilmente:

- Tiempo.
- Dirección.
- Pose.
- Velocidad visual.

## Segunda versión

Crear posteriormente un editor dentro de Unity que permita:

- Cargar canción.
- Reproducir / pausar.
- Ver tiempo actual.
- Agregar tile.
- Seleccionar dirección.
- Seleccionar pose.
- Modificar tiempo.
- Modificar velocidad.
- Eliminar tile.
- Guardar chart.

Concepto:

```text
---------------------------------------
CHART EDITOR
---------------------------------------

Song: [ surexs_demo ]

Time: 05.250

[ PLAY ] [ PAUSE ]

---------------------------------------

TIME     DIRECTION      POSE

1.20     LEFT           PHONE
1.80     CENTER         TYPING
2.40     RIGHT          PRESENTATION
3.10     LEFT           COFFEE

---------------------------------------

[ ADD TILE ]
[ SAVE CHART ]
```

---

# 23. Duración de canciones

El objetivo inicial es que una partida dure:

```text
30–90 segundos
```

La primera canción de prueba puede durar aproximadamente:

```text
45–60 segundos
```

No diseñar inicialmente canciones de varios minutos.

---

# 24. Menú principal

El menú inicial debe ser sencillo.

Debe utilizar un video de fondo de gameplay en loop.

Concepto:

```text
SUREXS
DANCE OFF

[ JUGAR ]

[ OPCIONES ]

[ SALIR ]
```

El video de fondo será un archivo local dentro del proyecto.

Debe poder configurarse desde Inspector.

No implementar streaming ni servicios externos.

---

# 25. Selección de modo

Al seleccionar JUGAR:

```text
¿CÓMO QUIERES JUGAR?

[ SOLO ]

[ 1 VS 1 ]

[ VOLVER ]
```

No debe existir modo CPU.

---

# 26. Countdown

Antes de iniciar la canción:

```text
3

2

1

GO!
```

El audio y el chart deben comenzar sincronizados.

La implementación debe evitar que el countdown provoque un desfase entre:

- música,
- tiles,
- judge.

---

# 27. HUD — Solo

Ejemplo conceptual:

```text
-----------------------------------------

SCORE                 COMBO
8,450                 x12

              PROGRESS
██████████████████░░░

                 TILE

              PERSONAJE

-----------------------------------------
```

Mostrar como mínimo:

- Score.
- Combo.
- Multiplicador.
- Resultado del último input.
- Progreso de canción.

---

# 28. HUD — 1 vs 1

La pantalla debe mostrar ambos jugadores.

Ejemplo:

```text
PLAYER 1                         PLAYER 2

8,450                            7,950
x12                              x8

███████████████                  █████████████

              GAMEPLAY

          PLAYER 1     PLAYER 2
```

Cada jugador debe tener su propio:

- Score.
- Combo.
- Multiplicador.
- Feedback.

---

# 29. Resultado final

Cuando termina la canción:

1. Detener / finalizar el gameplay.
2. Reproducir animación final.
3. Mostrar resultados.
4. Comparar scores en modo 1 vs 1.
5. Determinar ganador.
6. Mostrar opciones de rematch y menú.

## Solo

```text
RESULTADOS

SCORE
8,450

PERFECT     58
GREAT       12
GOOD         4
MISS         3

MAX COMBO   27

ACCURACY    91%

[ JUGAR DE NUEVO ]
[ MENÚ ]
```

## 1 vs 1

```text
RESULTADOS

PLAYER 1
8,450

PLAYER 2
7,950


🏆 GANADOR

PLAYER 1


[ REMATCH ]
[ MENÚ ]
```

En caso de empate:

```text
¡EMPATE!
```

---

# 30. QR de resultados

Al final de TODA partida debe existir un área destinada a mostrar un código QR.

El objetivo del QR es permitir al usuario compartir su experiencia y dirigirlo a una página de LinkedIn.

Texto sugerido:

```text
¿TE DIVERTISTE?

COMPARTE TU EXPERIENCIA

[ QR ]

Escanea para compartir tu experiencia en LinkedIn.
```

## Requisito técnico

El QR debe generarse a partir de una URL configurable.

NO hardcodear la URL dentro del código de gameplay.

Crear una configuración similar a:

```text
ResultsShareConfig

shareUrl
qrSize
```

La URL inicial será proporcionada posteriormente por el equipo del proyecto.

Ejemplo conceptual:

```text
https://www.linkedin.com/...
```

La URL debe poder cambiarse desde Inspector sin modificar código.

## Importante

El MVP no necesita integrar APIs de LinkedIn.

El QR únicamente debe contener una URL.

El teléfono del usuario será el encargado de abrir LinkedIn.

---

# 31. Arquitectura

Mantener una arquitectura modular.

Sistemas principales:

```text
GameManager
GameStateManager
AudioManager
InputManager
ChartManager
TileSpawner
RhythmJudge
ScoreManager
ComboManager
PlayerController
UIManager
ResultsManager
QRCodeManager
```

No crear un GameManager monolítico que controle todo.

---

# 32. Responsabilidad de cada sistema

## GameManager

Control general de la partida.

Estados posibles:

```text
MainMenu
ModeSelection
Countdown
Playing
Results
```

---

## AudioManager

Responsable de:

- Reproducir canción.
- Reproducir SFX.
- Exponer song time.
- Controlar volumen.
- Iniciar / detener música.

---

## InputManager

Responsable de recibir:

- LEFT.
- RIGHT.
- CENTER / ausencia de input.

Debe soportar múltiples dispositivos.

Utilizar Unity Input System.

---

## ChartManager

Responsable de:

- Cargar chart.
- Validar datos.
- Exponer eventos.
- Mantener orden temporal.

No debe controlar directamente la UI.

---

## TileSpawner

Responsable de:

- Crear tiles.
- Colocarlos.
- Moverlos.
- Eliminar tiles.
- Sincronizar posición con el tiempo de canción.

---

## RhythmJudge

Responsable de:

- Detectar inputs.
- Encontrar el evento correspondiente.
- Comparar timing.
- Determinar PERFECT/GREAT/GOOD/MISS.

No debe manejar la puntuación final.

---

## ScoreManager

Responsable de:

- Score.
- Puntos base.
- Multiplicador.
- Score final.

Debe existir una instancia por jugador.

---

## ComboManager

Responsable de:

- Combo actual.
- Combo máximo.
- Incremento.
- Reset.
- Eventos de milestones.

Debe existir una instancia por jugador.

---

## PlayerController

Responsable de:

- Animaciones.
- Poses.
- Feedback del personaje.

Debe recibir la pose desde los datos del evento.

No debe decidir por sí mismo si un input fue correcto.

---

## UIManager

Responsable de:

- HUD.
- Score.
- Combo.
- Multiplicador.
- Feedback.
- Mensajes de racha.
- Countdown.
- Menús.

---

## ResultsManager

Responsable de:

- Resultados finales.
- Comparación de scores.
- Ganador.
- Empate.
- Estadísticas.

---

## QRCodeManager

Responsable exclusivamente de:

- Recibir una URL.
- Generar / mostrar el QR.
- Actualizar el QR si cambia la URL.

No debe conocer información de gameplay.

---

# 33. Arquitectura de datos

Separar datos del código.

Estructura sugerida:

```text
Assets/
│
├── Art/
│   ├── Characters/
│   ├── Poses/
│   ├── Tiles/
│   ├── UI/
│   └── Backgrounds/
│
├── Audio/
│   ├── Music/
│   └── SFX/
│
├── Data/
│   ├── Charts/
│   ├── Songs/
│   └── Poses/
│
├── Prefabs/
│   ├── Player/
│   ├── RhythmTile/
│   └── UI/
│
├── Scenes/
│   ├── MainMenu
│   └── Game
│
└── Scripts/
    ├── Core/
    ├── Rhythm/
    ├── Input/
    ├── Player/
    ├── UI/
    ├── Results/
    └── Data/
```

---

# 34. Escalabilidad

El diseño debe permitir posteriormente:

- Más canciones.
- Más personajes.
- Más poses.
- Más escenarios.
- Diferentes dificultades.
- Diferentes velocidades.
- Diferentes charts.
- Nuevos tipos de eventos.

No implementar estas características todavía.

La arquitectura solamente debe evitar bloquearlas.

---

# 35. MVP — Primera versión jugable

La primera versión funcional debe incluir ÚNICAMENTE:

## Escena

- Oficina sencilla.
- Personaje 2D.

## Gameplay

- Una canción.
- 30–60 segundos.
- 3 direcciones.
- Tiles.
- Línea de ejecución.
- Input.
- Timing.

## Puntuación

- PERFECT.
- GREAT.
- GOOD.
- MISS.
- Combo.
- Multiplicador.

## Animaciones

- 3 poses básicas.
- Una animación de error.
- Una animación de celebración.

## UI

- Score.
- Combo.
- Multiplicador.
- Feedback.
- Countdown.
- Final.

## Modos

- Solo.

Aunque el menú debe estar preparado para:

- Solo.
- 1 vs 1.

El modo 1 vs 1 puede implementarse inmediatamente después de validar el loop Solo.

---

# 36. Segunda fase

Agregar:

- 1 vs 1 local.
- Dos jugadores.
- Dos personajes.
- Score independiente.
- Combo independiente.
- Resultados versus.
- Ganador.
- Empate.
- Rematch.

---

# 37. Tercera fase

Agregar:

- Mensajes de racha.
- Mejores efectos visuales.
- Más poses.
- Más animaciones.
- Mejor escenario.
- SFX.
- Feedback de tiles.
- QR en resultados.

---

# 38. Cuarta fase

Agregar:

- Editor de charts.
- Más canciones.
- Configuración de velocidad.
- Configuración de timing.
- Diferentes dificultades.

---

# 39. Reglas importantes para la IA de desarrollo

La IA que trabaje en este proyecto debe seguir estas reglas:

1. **No implementar características no solicitadas.**
2. Mantener el código modular.
3. Evitar scripts gigantes.
4. Evitar valores mágicos.
5. Exponer configuraciones importantes mediante Inspector o datos externos.
6. No hardcodear posiciones de gameplay cuando puedan configurarse.
7. No hardcodear puntuaciones en múltiples lugares.
8. No mezclar input, scoring y animaciones.
9. No mezclar lógica de UI con lógica de gameplay.
10. Utilizar Unity Input System.
11. Utilizar el tiempo del AudioSource como referencia del ritmo.
12. Mantener el chart independiente del código.
13. Priorizar una implementación funcional antes que efectos visuales.
14. Crear sistemas simples antes de abstraerlos excesivamente.
15. Cada nueva característica debe evitar romper las anteriores.
16. Antes de modificar una arquitectura existente, explicar brevemente qué se modificará y por qué.
17. No instalar paquetes externos sin necesidad.
18. No agregar dependencias innecesarias.
19. Si se necesita una librería externa para generar QR, documentar claramente la dependencia y mantener `QRCodeManager` aislado para poder sustituirla posteriormente.

---

# 40. Orden recomendado de implementación

La IA debe implementar el proyecto en este orden:

```text
FASE 1
Proyecto Unity 6.6
    ↓
Escena Game
    ↓
Audio
    ↓
Chart JSON
    ↓
ChartManager
    ↓
TileSpawner
    ↓
Input
    ↓
RhythmJudge
    ↓
Score
    ↓
Combo
    ↓
UI
    ↓
Animaciones
    ↓
Fin de partida
```

Después:

```text
FASE 2
1 vs 1
    ↓
Player 1
Player 2
    ↓
Scores independientes
    ↓
Combos independientes
    ↓
Resultados
    ↓
Ganador
```

Después:

```text
FASE 3
Menú completo
    ↓
Solo
1 vs 1
    ↓
QR
    ↓
Feedback
    ↓
Polish
```

Después:

```text
FASE 4
Chart Editor
    ↓
Más canciones
    ↓
Más poses
    ↓
Dificultades
```

---

# 41. Criterio de éxito del MVP

El MVP se considera exitoso cuando:

1. El juego puede iniciar una canción.
2. Los tiles aparecen sincronizados con la música.
3. Los tiles llegan correctamente a la zona de ejecución.
4. El jugador puede utilizar LEFT y RIGHT.
5. CENTER funciona mediante ausencia de input.
6. El sistema detecta timing.
7. Se muestran PERFECT/GREAT/GOOD/MISS.
8. El score aumenta correctamente.
9. El combo funciona.
10. El multiplicador funciona.
11. El personaje reproduce diferentes poses.
12. La canción termina correctamente.
13. Se muestran resultados.
14. El juego puede reiniciarse sin errores.
15. El sistema no depende de valores hardcodeados difíciles de modificar.

---

# 42. Principio de diseño principal

El juego debe poder explicarse en una sola frase:

> **"Sigue las poses al ritmo de la música y consigue más puntos que tu rival."**

El jugador debe poder entender cómo jugar en menos de 10 segundos.

La profundidad debe venir de:

- Timing.
- Combo.
- Multiplicador.
- Memoria muscular.
- Velocidad.
- Precisión.
- Competencia.

No de controles complejos.

---

# 43. Estado final esperado

La experiencia ideal es:

```text
MENÚ
  ↓
SOLO / 1 VS 1
  ↓
3
2
1
GO!
  ↓
MÚSICA
  ↓
← ○ → ← → ○ → ...
  ↓
PERFECT!
  ↓
COMBO x10
  ↓
AMAZING!
  ↓
COMBO x20
  ↓
FANTASTIC!
  ↓
FIN
  ↓
RESULTADOS
  ↓
🏆 GANADOR
  ↓
QR — COMPARTE TU EXPERIENCIA
  ↓
REMATCH / MENÚ
```

---

# 44. Nota para futuras iteraciones

La canción inicial será definida por el equipo del proyecto.

La secuencia de tiles será inicialmente creada manualmente en JSON.

La frecuencia, velocidad y timing deben ser editables mediante datos.

El objetivo es que posteriormente una persona pueda crear un nuevo chart para una canción sin modificar los sistemas principales del juego.

El juego debe sentirse como un minijuego corto, pulido y extremadamente fácil de volver a jugar.
