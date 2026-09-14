# AGENTS.md — Surexs Dance Off

## 1. Project identity

This repository contains **Surexs Dance Off**, a short rhythm-based competitive minigame developed in **Unity 6.6** for **PC**.

**Always read `GDD.md` before making gameplay or architectural decisions.**

This file defines how an AI coding agent must work on the project. `GDD.md` defines what the game should be.

---

## 2. Core concept

Surexs Dance Off is a short rhythm minigame inspired by rhythm minigames from Mario Party and LEGO Party.

The player controls a Surexs employee who performs work-related poses according to a musical sequence.

There are only three rhythm actions:

- `LEFT` — press left.
- `CENTER` — press nothing.
- `RIGHT` — press right.

Rhythm tiles move toward a hit zone. The player must perform the correct input at the correct time.

Supported modes:

- `SOLO`
- `1 VS 1 LOCAL`

**There is NO CPU mode.**

Target match length: **30–90 seconds**.

---

## 3. Development rules

### Read before modifying

Before changing code:

1. Read `GDD.md`.
2. Inspect the current project structure.
3. Inspect relevant scripts/assets.
4. Understand the current implementation.
5. Make the smallest change required.

Never assume a system is absent without checking the project.

### Work incrementally

Do not implement the whole game in one operation.

Preferred workflow:

```text
Inspect
  ↓
Plan
  ↓
Implement
  ↓
Compile
  ↓
Test in Unity
  ↓
Report
```

Do not automatically continue into unrelated features.

### Do not over-engineer

Prefer:

- Small, clear classes.
- Single responsibility.
- Unity-native solutions.
- Inspector-configurable values.
- Data-driven charts.
- Minimal dependencies.

Avoid:

- Giant manager classes.
- Excessive abstraction.
- Unnecessary frameworks.
- Dependency injection frameworks unless explicitly requested.
- Unnecessary third-party packages.
- Refactoring unrelated systems.

---

## 4. Unity

Target **Unity 6.6**.

Do not introduce APIs or packages requiring another Unity version without explicitly identifying the compatibility issue.

Do not install external packages without approval.

---

## 5. Input

Use the **Unity Input System**.

Gamepad is the primary input.

Temporary keyboard controls:

### Player 1

```text
A = LEFT
D = RIGHT
No direction input = CENTER
```

### Player 2

```text
Left Arrow  = LEFT
Right Arrow = RIGHT
No direction input = CENTER
```

Do not add UP, DOWN, diagonals, triggers, or extra rhythm buttons unless explicitly requested.

---

## 6. CENTER rule

CENTER is represented by **absence of directional input**.

Do not implement CENTER as a third physical button.

The judge must distinguish:

```text
LEFT
RIGHT
NO INPUT
```

A previously pressed direction that has been released before the CENTER window must not count as CENTER incorrectly.

A LEFT or RIGHT input during the CENTER window is a miss/error.

---

## 7. Rhythm clock

The audio timeline is the source of truth.

Use the playing `AudioSource` / audio playback time as the primary rhythm clock.

**Do not use `Time.time` as the authoritative rhythm clock.**

Conceptually:

```text
Audio playback time
        ↓
Chart event time
        ↓
Tile position
        ↓
Hit window
        ↓
Judge
```

Visual movement can use frame updates, but judgment must use the musical timeline.

---

## 8. Timing

Initial timing windows:

| Result | Window |
|---|---:|
| PERFECT | ±0.050 s |
| GREAT | ±0.100 s |
| GOOD | ±0.175 s |
| MISS | outside GOOD |

These values must be configurable and must not be duplicated across scripts.

---

## 9. Scoring

Initial base scores:

```text
PERFECT = 100
GREAT   = 75
GOOD    = 50
MISS    = 0
```

Initial multipliers:

```text
0–4 combo    = x1
5–9 combo    = x2
10–19 combo  = x3
20–29 combo  = x4
30+ combo    = x5
```

Keep these values configurable.

Do not place scoring logic in UI or animation scripts.

---

## 10. Combo

Every successful hit increases combo.

MISS resets combo.

Track:

- Current combo.
- Maximum combo.

In 1 vs 1, each player has independent combo state.

---

## 11. Chart data

Rhythm charts must be data-driven and separate from gameplay code.

Initial charts should use JSON.

Example:

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

Each event should support at least:

```text
time
direction
pose
```

Keep timing and visual speed separate.

---

## 12. Direction and pose are separate

The rhythm system only cares about the required direction.

The character system cares about the pose.

Examples:

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

Never permanently assume that one direction equals one specific pose.

---

## 13. System responsibilities

Keep systems separated where practical.

### GameManager

Controls overall game flow/state:

```text
MainMenu
ModeSelection
Countdown
Playing
Results
```

It must not become a monolithic gameplay controller.

### AudioManager

Responsible for:

- Music.
- SFX.
- Music time access.
- Volume.
- Start/stop playback.

### InputManager

Converts physical device input into gameplay actions.

Do not put score logic here.

### ChartManager

Responsible for:

- Loading chart data.
- Validating chart data.
- Ordering events.
- Providing events to gameplay systems.

### TileSpawner

Responsible for:

- Creating tiles.
- Positioning tiles.
- Moving tiles visually.
- Cleaning up tiles.

### RhythmJudge

Responsible for:

- Detecting relevant input.
- Matching input against the active event.
- Calculating timing difference.
- Returning PERFECT/GREAT/GOOD/MISS.

It must not own total player score.

### ScoreManager

Responsible for:

- Current score.
- Base score.
- Multiplier.
- Score calculation.

Each player has independent score state.

### ComboManager

Responsible for:

- Current combo.
- Maximum combo.
- Reset.
- Combo milestones.

Each player has independent combo state.

### PlayerController

Responsible for:

- Character presentation.
- Pose animations.
- Error animation.
- Celebration animation.

It must not decide whether input was correct.

### UIManager

Responsible for:

- HUD.
- Score.
- Combo.
- Multiplier.
- Hit feedback.
- Combo messages.
- Countdown.
- Menus.

Do not put rhythm judgment inside UI code.

### ResultsManager

Responsible for:

- Final statistics.
- Final score.
- Comparing players.
- Winner/draw.
- Rematch/menu actions.

### QRCodeManager

Responsible only for the results QR.

It should:

1. Receive a configurable URL.
2. Generate/display a QR for that URL.
3. Allow the URL to be changed without changing gameplay code.

Do not integrate LinkedIn APIs. The QR simply contains a configurable URL leading to the intended LinkedIn experience.

---

## 14. 1 vs 1 rules

There is no CPU.

Modes:

```text
SOLO
1 VS 1 LOCAL
```

For 1 vs 1:

- Both players use the same chart.
- Both players use the same song.
- Each player has independent score.
- Each player has independent combo.
- Each player has independent timing judgments.
- No physical interaction between players in the MVP.
- Highest final score wins.
- Equal scores produce a draw.

Do not implement CPU behavior.

---

## 15. Visual direction

The game is primarily **2D / 2.5D**.

Target direction:

- 2D character animation.
- 3D or pseudo-3D rhythm tiles.
- Surexs office environment.
- Energetic minigame presentation.
- Comedic work-related poses.

Placeholder art is acceptable during development.

Do not block gameplay implementation waiting for final art.

---

## 16. Menu

Main menu should eventually contain:

```text
SUREXS
DANCE OFF

[ JUGAR ]
[ OPCIONES ]
[ SALIR ]
```

The background should support a local gameplay demo video in loop.

Mode selection:

```text
[ SOLO ]
[ 1 VS 1 ]
[ VOLVER ]
```

There is no CPU option.

---

## 17. Results

Solo results should show:

```text
SCORE
PERFECT
GREAT
GOOD
MISS
MAX COMBO
ACCURACY
```

1 vs 1 results should show:

```text
PLAYER 1 SCORE
PLAYER 2 SCORE
WINNER
or
DRAW
```

Actions:

```text
[ REMATCH ]
[ MENU ]
```

---

## 18. QR requirement

The results screen must include a QR code.

Purpose:

> Allow the player to share their experience and reach a LinkedIn destination.

The QR URL must be configurable.

Do not hardcode the final LinkedIn URL inside gameplay code.

Preferred conceptual configuration:

```text
ResultsShareConfig
    shareUrl
    qrSize
```

If a third-party QR library is required:

- Explain why.
- Isolate it behind `QRCodeManager`.
- Do not spread the dependency through the project.
- Do not install it without approval.

---

## 19. Code quality

Prefer:

- Clear names.
- Small methods.
- Single responsibility.
- Inspector configuration where appropriate.
- Enums for fixed states.
- Data classes for chart events.
- Events/callbacks between independent systems where useful.

Avoid:

- Giant `Update()` methods.
- Giant managers.
- Deep inheritance hierarchies.
- Magic numbers.
- Repeated string literals.
- Gameplay logic inside visual-only scripts.
- Excessive scene searches.
- Hidden global state.
- Unnecessary singletons.

Do not refactor unrelated code unless necessary for the requested feature.

---

## 20. Error handling

For chart/configuration loading:

- Validate required fields.
- Handle missing files.
- Handle malformed JSON.
- Handle invalid directions.
- Handle invalid timing.
- Avoid NaN/Infinity.
- Provide clear Unity Console errors/warnings.
- Do not silently fail.

---

## 21. Testing

After implementing a feature:

1. Compile scripts.
2. Check Unity Console for errors.
3. Run the relevant scene.
4. Test manually.
5. Verify existing functionality still works.

For rhythm changes verify:

- Song starts.
- Chart starts synchronized.
- Tiles reach the hit zone at the expected time.
- LEFT works.
- RIGHT works.
- CENTER works through no input.
- Timing windows work.
- PERFECT/GREAT/GOOD/MISS work.
- Combo increases.
- MISS resets combo.
- Score changes correctly.
- Song reaches Results.
- Restart works without duplicated listeners/timers.

---

## 22. Git workflow

Work in small logical changes.

Avoid unrelated changes in one commit.

Suggested commits:

```text
Create initial rhythm prototype
Add chart JSON loading
Add rhythm tile spawning
Add timing judge
Add score and combo system
Add player pose animations
Add solo results screen
Add local versus mode
Add QR results screen
```

Never delete or overwrite unrelated user work.

---

## 23. Communication rules for the AI

Before a substantial change, briefly explain:

1. What will change.
2. Which files will be affected.
3. Why the change is needed.

After the change, report:

1. What was implemented.
2. Files created/modified.
3. How to test it in Unity.
4. Known limitations.
5. Recommended next step.

Keep the explanation concise unless more detail is requested.

---

## 24. Missing assets

If an asset is missing:

- Use a placeholder.
- Clearly identify it.
- Do not invent file paths.
- Do not reference nonexistent sprites, audio clips, prefabs or animations.

Integrate final assets without rewriting the gameplay architecture.

---

## 25. Development phases

### Phase 1 — Rhythm prototype

Implement:

- Game scene.
- Audio.
- Chart data.
- ChartManager.
- TileSpawner.
- Input.
- RhythmJudge.
- Score.
- Combo.
- Basic UI.
- Basic player.
- Basic poses.
- Results.

Target one 30–60 second test song.

### Phase 2 — Local 1 vs 1

Implement:

- Two players.
- Two inputs.
- Independent scores.
- Independent combos.
- Shared chart.
- Versus HUD.
- Winner.
- Draw.
- Rematch.

### Phase 3 — Presentation

Implement:

- Main menu.
- Mode selection.
- Gameplay background/video.
- Better feedback.
- Combo messages.
- Particles/SFX.
- Better animations.
- Office environment.
- QR results screen.

### Phase 4 — Content tools

Implement:

- Chart editor.
- Multiple songs.
- Multiple poses.
- Speed configuration.
- Difficulty configuration.

Do not jump to later phases unless explicitly instructed.

---

## 26. Current priority

Prioritize:

```text
FUNCTIONAL GAMEPLAY
        >
CORRECT RHYTHM
        >
CORRECT INPUT
        >
CORRECT SCORE
        >
STABLE ARCHITECTURE
        >
VISUAL POLISH
```

Do not spend significant development time on visual polish before the core rhythm loop works.

---

## 27. First task in a fresh project

When starting from a fresh project:

1. Read `GDD.md`.
2. Read `AGENTS.md`.
3. Inspect `Packages/manifest.json`.
4. Inspect `ProjectSettings/`.
5. Inspect `Assets/`.
6. Determine whether Unity Input System is configured.
7. Identify existing scenes, scripts and assets.
8. Report the current state.

**Do not immediately create the complete game.**

The first implementation should be proposed before proceeding.

---

## 28. Definition of success

A good implementation is:

- Small.
- Testable.
- Modular.
- Understandable by a beginner Unity developer.
- Compatible with Unity 6.6.
- Easy to modify.
- Data-driven where appropriate.
- Free of unnecessary dependencies.
- Consistent with `GDD.md`.

When choosing between a clever solution and a simple maintainable solution, choose the simple maintainable solution.

---

## 29. Golden rule

> **Build the smallest working version first, verify it in Unity, then add the next feature.**

Never sacrifice a working small prototype for a large unfinished architecture.
