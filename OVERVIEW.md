# Out of the Way — Project Architecture & Developer Guide

**Out of the Way** is a Unity 6 URP (`6000.6.0f1`) rhythmic driving game built on a call-and-response gameplay loop. The player pilots a city bus along +Z through an infinite urban cityscape. Roadway obstacles block the lane, initiating a rhythmic vocal chant (*"GET • OUT • OF • THE • WAY"*). The player must honk back the exact rhythm in time to clear the obstacle before collision. As the run progresses, tempo upgrades across multiple BPM tiers (130 &rarr; 140 &rarr; 150 BPM) and rhythms progress from easy straight pulses to complex, randomized syncopations.

---

## Table of Contents

1. [Repository & Project Structure](#1-repository--project-structure)
2. [Runtime Architecture & Execution Flow](#2-runtime-architecture--execution-flow)
3. [Core Subsystems (In-Depth Explanation)](#3-core-subsystems-in-depth-explanation)
   - [3.1 Bootstrap & Scene Assembly (`GameBootstrap.cs`)](#31-bootstrap--scene-assembly-gamebootstrapcs)
   - [3.2 State Management & Progression (`GameManager.cs`)](#32-state-management--progression-gamemanagercs)
   - [3.3 Multi-Tempo Audio & Word SFX Engine (`MusicConductor.cs`, `ProceduralAudio.cs`)](#33-multi-tempo-audio--word-sfx-engine-musicconductorcs-proceduralaudiocs)
   - [3.4 Call-and-Response Rhythm Engine (`RhythmDirector.cs`, `PhrasePattern.cs`, `PhraseLibrary.cs`)](#34-call-and-response-rhythm-engine-rhythmdirectorcs-phrasepatterncs-phraselibrarycs)
   - [3.5 Vehicle Physics & Camera (`BusController.cs`, `CameraRig.cs`)](#35-vehicle-physics--camera-buscontrollercs-camerarigcs)
   - [3.6 Spawner & Obstacles (`ObstacleSpawner.cs`, `ObstacleController.cs`)](#36-spawner--obstacles-obstaclespawnercs-obstaclecontrollercs)
   - [3.7 3D World Generation & Normalization (`EndlessCity.cs`, `CityKit.cs`, `Look.cs`, `VehicleFactory.cs`)](#37-3d-world-generation--normalization-endlesscitycs-citykitcs-lookcs-vehiclefactorycs)
   - [3.8 Typography, Hand-Stamped HUD & Accessibility UI (`GameUI.cs`)](#38-typography-hand-stamped-hud--accessibility-ui-gameuics)
4. [Dual-Loop Game Flow (State Diagram)](#4-dual-loop-game-flow-state-diagram)
5. [Configuration & Tuning Guide ("Where to Look")](#5-configuration--tuning-guide-where-to-look)
6. [Asset Pipeline & Team Git Policy](#6-asset-pipeline--team-git-policy)
7. [Common Issues & Troubleshooting](#7-common-issues--troubleshooting)

---

## 1. Repository & Project Structure

```
OutOfWay/
├── Assets/
│   ├── Scenes/
│   │   └── SampleScene.unity             Main gameplay scene with OutOfWay bootstrap object
│   ├── Art/                              Designer 3D models and textures (tracked directly in Git)
│   │   ├── FullScene.fbx                 Master city block (road, dual sidewalk curbs, lamp, buildings)
│   │   ├── StreetAndBuildings.fbx        Combined road + buildings block (used for infinite recycling)
│   │   ├── StreetAndSides.fbx            Asphalt road surface + concrete sidewalks and curbs
│   │   ├── Street.fbx                    Isolated roadway mesh
│   │   ├── Buildings.fbx                 Urban building facades (sides 1, 2, 3)
│   │   ├── StreetLamp.fbx                Metal street lamp with curved neck and lantern
│   │   ├── SM_Bus.fbx                    Modular player bus model (body, front, 4 wheels, wheel wells)
│   │   ├── SM_Tree_01.fbx                Blender street tree (branches + leaves)
│   │   ├── Characters/                   Pedestrian character models and textures
│   │   │   ├── SM_HumanFemale/           Female pedestrian model + Dress/Hair/Skin/Eyes textures
│   │   │   └── SM_HumanMale/             Male pedestrian model + Shirt/Skin/Eyes textures
│   │   └── Textures/                     PBR texture sets (BaseColor, Normal, Roughness, Metalness)
│   │       ├── Street/                   Asphalt road surface textures
│   │       ├── Sides/                    Sidewalk and curb concrete textures
│   │       ├── Buildings_side_2/         Building facade set 2 textures
│   │       ├── Buildings_side_3/         Building facade set 3 textures
│   │       └── Lamp/                     Street lamp metal and light textures
│   ├── Scripts/
│   │   ├── Core/
│   │   │   ├── GameBootstrap.cs          Automated game entry point, asset binder, and scene assembler
│   │   │   ├── GameManager.cs            Session flow, scoring, crash handling, tempo progression
│   │   │   └── MusicConductor.cs         Beat tracking, loop phase synchronization, vocal SFX dispatcher
│   │   ├── Rhythm/
│   │   │   ├── PhrasePattern.cs          Rhythm pattern definition ScriptableObject
│   │   │   └── PhraseLibrary.cs          Rhythm difficulty curves, curated presets, procedural beat generator
│   │   ├── Gameplay/
│   │   │   ├── BusController.cs          Bus acceleration, steering jolt, crash spinout, wheel spin
│   │   │   ├── CameraRig.cs              Smooth tracking camera with honk/crash punch impulses
│   │   │   ├── ObstacleSpawner.cs        Speed-adjusted longitudinal look-ahead spawner
│   │   │   ├── ObstacleController.cs     Obstacle swerve AI and siren animation
│   │   │   └── RhythmDirector.cs         Call-and-response state machine and timing evaluator
│   │   ├── World/
│   │   │   ├── CityKit.cs                Designer FBX inspector, auto-scaling, tree & pedestrian spawner
│   │   │   ├── EndlessCity.cs            Infinite chunk pooling and recycling ahead of the bus
│   │   │   ├── Look.cs                   URP material converter, texture binder, color palette mapper
│   │   │   └── VehicleFactory.cs         Bus and obstacle vehicle constructor
│   │   ├── Audio/
│   │   │   └── ProceduralAudio.cs        Synthesized horns, crash noise, and speed-pitched engine audio
│   │   └── UI/
│   │       └── GameUI.cs                 Typography, tilted HUD stats, word animator, metronome toggle
│   ├── Resources/
│   │   ├── Fonts/                        Custom fonts (BrownieStencil, ArchivoBlack, Anton, Bangers, Bungee)
│   │   ├── Models/                       Runtime-loadable FBX prefabs (SM_Tree_01, SM_HumanFemale, SM_HumanMale)
│   │   ├── Textures/                     Runtime BaseColor textures (Street, Sides, Buildings, Lamp, Characters)
│   │   └── Audio/                        Looping tracks, vocal word cues, and metronome files
│   │       ├── BGM_130.mp3 / Metro_130   130 BPM base background music & metronome (12-beat & 4-beat)
│   │       ├── BGM_140.mp3 / Metro_140   140 BPM Tier 2 background music & metronome
│   │       ├── BGM_150.mp3 / Metro_150   150 BPM Tier 3 background music & metronome
│   │       ├── Word_Get.mp3 (get.mp3)    Vocal sound effect for "GET"
│   │       ├── Word_Out.mp3 (out.mp3)    Vocal sound effect for "OUT"
│   │       ├── Word_Of.mp3 (of.mp3)      Vocal sound effect for "OF"
│   │       ├── Word_The.mp3 (the.mp3)    Vocal sound effect for "THE"
│   │       ├── Word_Way.mp3 (way.mp3)    Vocal sound effect for "WAY"
│   │       ├── honk.mp3                  Crisp bus horn sound effect (Spacebar / HUD button)
│   │       └── BackgroundBed.mp3         Legacy 100 BPM bed fallback
│   ├── Licenses/                         Font attribution and commercial licenses
│   ├── Settings/                         Universal Render Pipeline (URP) assets and volume profiles
│   └── TutorialInfo/                     Unity default template files
├── Packages/                             Package manifest (URP, Input System, Mathematics, etc.)
├── ProjectSettings/                      Project configuration (Unity 6000.6.0f1)
└── OVERVIEW.md                           This architectural guide
```

---

## 2. Runtime Architecture & Execution Flow

When entering Play Mode in Unity (or running a standalone build), the entire runtime environment is configured deterministically without requiring manual hierarchy setup:

```
[GameBootstrap.BootPlay()] (DefaultExecutionOrder: -100)
    │
    ├── 1. Look.Init() ──────────────────────── Configures URP materials, color swatches, shaders
    ├── 2. StyleWorld() ─────────────────────── Configures ambient lighting and warm directional sun
    ├── 3. CityKit.Bind(...) ────────────────── Analyzes FBX meshes, measures road width, sets scale
    ├── 4. VehicleFactory.MakeBus(...) ──────── Instantiates SM_Bus.fbx, scales to 7m, adds colliders
    ├── 5. EndlessCity.Generate(...) ────────── Spawns initial looping block sequence along +Z
    ├── 6. CameraRig.Target = bus ───────────── Binds main camera to chase bus with impulse punch
    ├── 7. ProceduralAudio.Create(...) ──────── Instantiates audio synth for horn, crash, engine
    ├── 8. MusicConductor.SetupBed() ────────── Loads BGM/Metronome tiers & word SFX, begins 130 BPM audio loop
    ├── 9. RhythmDirector.Bind(...) ─────────── Initializes call-and-response state machine
    ├── 10. ObstacleSpawner.Bind(...) ───────── Prepares obstacle spawner with speed-adjusted lookahead
    ├── 11. GameUI.Create(...) ──────────────── Builds canvas, loads BrownieStencil font, constructs tilted HUD
    ├── 12. GameManager.Wire() ─────────────── Connects events between Rhythm, UI, Spawner, and Audio
    └── 13. State = Title ───────────────────── Bus idles; wait for Space / Click / Honk to drive
```

---

## 3. Core Subsystems (In-Depth Explanation)

### 3.1 Bootstrap & Scene Assembly (`GameBootstrap.cs`)
- **Location:** `Assets/Scripts/Core/GameBootstrap.cs`
- **Execution Order:** `[DefaultExecutionOrder(-100)]` ensures all core services exist before any other `MonoBehaviour.Update()` runs.
- **Auto-Boot:** Decorated with `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]`. If the scene does not already have an `OutOfWay` GameObject, it automatically instantiates one.
- **In-Editor Scene Preview:** Uses `[ExecuteAlways]` and `UnityEditor.EditorApplication.delayCall` to spawn a live visual preview (`DesignerStreet`) in the Scene View when working in the editor, automatically cleaned up when entering Play Mode.
- **Inspector Bindings:** Exposes fields for designer FBX prefabs (`StreetAndBuildings`, `Buildings`, `Street`, `Bus`, `StreetLamp`), initial BPM, tolerance scale, restart delays, tempo track overrides, and individual word sound effect overrides (`WordGet`, `WordOut`, `WordOf`, `WordThe`, `WordWay`).

### 3.2 State Management & Progression (`GameManager.cs`)
- **Location:** `Assets/Scripts/Core/GameManager.cs`
- **State Machine (`GameState`):**
  - `Title`: Bus parked, engine rumbling, metronome toggle accessible, prompt: *"HONK TO DRIVE"*.
  - `Playing`: Bus accelerates forward along +Z, obstacles spawn ahead, rhythm director active.
  - `Failed`: Bus spinout, crash SFX, red *"DRIVER'S LICENSE REVOKED"* card with score breakdown, automatic restart after `RestartDelay` seconds.
- **In-Place Restarts:** The game **never** calls `SceneManager.LoadScene()`. Restarting simply calls `Bus.StartDriving()`, clears existing obstacles, resets `RhythmDirector`, and the road continues seamlessly from the bus's current position.
- **Dynamic Tempo Progression:**
  - Evaluated on each obstacle cleared via `CheckTempoUpgrade()`.
  - Configurable milestones: `TempoMilestones = { 4, 8 }`.
  - **Score 0–3:** Tier 0 &rarr; **130 BPM** (`BGM_130.mp3`).
  - **Score 4–7:** Tier 1 &rarr; **140 BPM** (`BGM_140.mp3`), HUD banner displays *"SPEED UP! 140 BPM"*.
  - **Score 8+:** Tier 2 &rarr; **150 BPM** (`BGM_150.mp3`), HUD banner displays *"SPEED UP! 150 BPM"*.
  - On crash/restart, `MusicConductor.ResetToStartingTier()` resets the tempo back to **130 BPM**.

### 3.3 Multi-Tempo Audio & Word SFX Engine (`MusicConductor.cs`, `ProceduralAudio.cs`)
- **Location:** `Assets/Scripts/Core/MusicConductor.cs`, `Assets/Scripts/Audio/ProceduralAudio.cs`
- **Start Beat & Measure Synchronization:**
  - Background music and metronomes are quantized to 4/4 time with downbeats (Start Beats) at regular measure intervals:
    $$\text{BarInterval} = 4 \times \text{BeatInterval} = 4 \times \left(\frac{60}{\text{Bpm}}\right)$$
  - Audio files share an initial downbeat onset offset: `StartBeatOffset = 0.022s` (~22ms).
  - `MusicConductor` maintains a continuous, drift-free `SongTime` that tracks cumulative playback without resetting across audio loops.
  - Detects measure boundaries when `Mathf.FloorToInt((SongTime - StartBeatOffset) / BarInterval)` increments, raising `StartBeatTriggered(bar)` and setting `IsStartBeatThisFrame = true`.
  - Exposes `TimeUntilNextStartBeat` so gameplay systems can schedule events right on the musical grid.
- **Vocal Word Sound Effects:**
  - Auto-loads vocal word clips from `Assets/Resources/Audio/`:
    - `Word_Get.mp3` (`get.mp3`) &rarr; Word index 0 ("GET")
    - `Word_Out.mp3` (`out.mp3`) &rarr; Word index 1 ("OUT")
    - `Word_Of.mp3` (`of.mp3`) &rarr; Word index 2 ("OF")
    - `Word_The.mp3` (`the.mp3`) &rarr; Word index 3 ("THE")
    - `Word_Way.mp3` (`way.mp3`) &rarr; Word index 4 ("WAY")
  - Triggered in real time via `PlayWord(index)` or `PlayWord(name)` on the dedicated `PhraseSource` audio channel.
  - Automatically pitch-scaled by `TempoScale` so chants match the faster 140 and 150 BPM tempos seamlessly.
  - The first word sound ("GET") lands **simultaneously with the Start Beat downbeat** of the background music and the accent click of the metronome!
  - If a crash occurs mid-chant, `StopChant()` cuts vocal playback immediately.
- **BGM & Metronome Loops:**
  - Background music tracks are exactly **3 bars (12 beats)** of seamless looping audio.
  - Metronome tracks are exactly **1 bar (4 beats)** of clicks matching the exact tempo.
  - Duration per tier:
    - **130 BPM:** BGM = 5.538s, Metro = 1.846s ($60 / 130 = 0.4615$s per beat).
    - **140 BPM:** BGM = 5.143s, Metro = 1.714s ($60 / 140 = 0.4286$s per beat).
    - **150 BPM:** BGM = 4.800s, Metro = 1.600s ($60 / 150 = 0.4000$s per beat).
- **Drift-Free Phase Alignment:**
  - `Update()` detects loop seam wrap-around on `Music.time < _lastMusicTime`.
  - Every 250ms, `targetTime = Music.time % metroLen` checks if the metronome has drifted by $> 35$ms and realigns it immediately.
- **Tempo Scaling (`TempoScale`):** Calculated as `Bpm / 130f`. Used by `RhythmDirector` to scale phrase timers and tolerance windows so the rhythm remains in lockstep with the music.
- **Accessibility Metronome Toggle:**
  - Default: **OFF** (`PlayerPrefs.GetInt("OutOfWay.Metronome", 0)`).
  - Toggled via the Title screen button, HUD button, or keyboard shortcut **`M`**.
  - Runs continuously in phase with volume muted, allowing instantaneous, click-free toggling.
- **Procedural SFX:** `ProceduralAudio.cs` generates dynamic horns, crash noise, accents, and pitch-scaled bus engine sound (`SetEngineSpeed(normalized)`).

### 3.4 Call-and-Response Rhythm Engine (`RhythmDirector.cs`, `PhrasePattern.cs`, `PhraseLibrary.cs`)
- **Location:** `Assets/Scripts/Gameplay/RhythmDirector.cs`, `Assets/Scripts/Rhythm/`
- **Rhythm Phases (`RhythmPhase`):**
  1. `Idle`: Waiting for obstacle encounter.
  2. `Call`: Obstacle is approaching. Words illuminate in sequence (*"GET"* &rarr; *"OUT"* &rarr; *"OF"* &rarr; *"THE"* &rarr; *"WAY"*). Each beat triggers its corresponding vocal sound effect clip (`PlayWord(index)`).
  3. `Response`: Prompt switches to *"HONK IT BACK"*. Player input window opens.
  4. `Resolved`: Success (all words matched) or Failed (miss/timeout).
- **Progressive Rhythm Difficulty & Procedural Random Beat Generation (`PhraseLibrary.cs`):**
  - **Tier 1 (Easy / Warmup, Score 0–2):** 100% predictable straight beats (`Even` and `Hold`). Perfect for learning the timing.
  - **Tier 2 (Intermediate, Score 3–5):** Introduces groove variations (`Even`, `Hold`, `Drag`, `Swing`).
  - **Tier 3 (Hard, Score 6–9):** Introduces fast syncopations (`Rush`, `Stutter`, `Syncopate`) with 30% chance of random beats.
  - **Tier 4 (Expert / Random Beats, Score 10+):** 70% to 85% chance of **procedurally generated random beats** (`GenerateRandomPattern`).
    - Randomizes 4 interval gaps from musical multipliers ($0.5b, 0.75b, 1.0b, 1.25b, 1.5b, 1.75b$).
    - Enforces human playability guards (no consecutive rapid sub-$0.6b$ gaps; phrase length bounded between $2.2b$ and $5.2b$).
    - Procedurally labeled (`"Syncopate"`, `"Offbeat"`, `"Freestyle"`, `"Wildcard"`, `"Breakbeat"`, `"Random Beat"`, `"Funky"`, `"Quickstep"`).
- **Relative Honk Timing:**
  - The player's first honk defines `_firstHonkAt = _clock`.
  - Subsequent honks must match the relative offsets: `DueAt(index) = _firstHonkAt + (WordTimes[index] - WordTimes[0])`.
  - This allows the player to start their response naturally without being penalized for small phase shifts, as long as the internal rhythm is maintained!
- **Audio-Clock Synchronization:**
  - Phrase clock `_clock` is computed directly from `(_music.SongTime - _phraseStartSongTime) * _music.TempoScale`.
  - This guarantees that each word ("GET", "OUT", "OF", "THE", "WAY") is sample-accurately pinned to the audio playback clock rather than accumulating frame-time drift.
  - When `BeginPhrase()` is called, `TickCall()` runs immediately on the Start Beat frame, ensuring Word 0 ("GET") triggers on the exact frame the measure begins.
- **Error Evaluation & Tolerances:**
  - `Tolerance` (default ~0.28s scaled by `ToleranceScale` and tightened slightly at high scores): Window around `DueAt(i)`.
  - `PerfectTolerance` (default ~0.13s): Triggers *"PERFECT — THEY MOVED"* bonus.
  - Failures trigger specific causes: `"TOO EARLY"` (honking during Call), `"OFF BEAT"` (timing mismatch), `"TOO SLOW"` (timeout before first honk or between honks).
- **Curated Phrase Presets (`PhraseLibrary.cs`):**
  - Calibrated to base quarter beat $b = 60 / 130 \approx 0.4615$s:
    - **Even:** Straight quarter beats `[0, b, 2b, 3b, 4b]`
    - **Hold:** Dramatic pause on "The" `[0, 0.75b, 1.5b, 2.25b, 3.75b]`
    - **Drag:** Swung delay `[0, b, 2b, 2.75b, 3.75b]`
    - **Swing:** Syncopated groove `[0, 0.833b, 1.417b, 2.25b, 2.833b]`
    - **Rush:** Accelerating tempo `[0, 0.75b, 1.5b, 2.25b, 3b]`
    - **Stutter:** Double hits on words 0 & 1 `[0, 0.5b, b, 2.25b, 3b]`
    - **Syncopate:** Staggered off-beat accents `[0, 0.75b, 1.25b, 2.5b, 3.25b]`

### 3.5 Vehicle Physics & Camera (`BusController.cs`, `CameraRig.cs`)
- **Location:** `Assets/Scripts/Gameplay/BusController.cs`, `Assets/Scripts/Gameplay/CameraRig.cs`
- **Bus Acceleration:**
  - Starts at `StartSpeed = 9f` m/s (~22 MPH).
  - Accelerates at `Acceleration = 0.28f` m/s² up to `MaxSpeed = 28f` m/s (~67 MPH).
  - Rotates wheel transforms found via `SpinWithSpeed` proportional to vehicle velocity.
- **Crash Response:** On crash, speed drops to 0, chassis tilt/shake is applied, and `CameraRig` punches back violently with heavy impulse shake. On retry, rotation resets cleanly to the cached origin pose.
- **Camera Rig:** Positioned $4.4$m high, $10.5$m behind the bus. Smoothly follows on Z with soft damping and impulse trauma decays.

### 3.6 Spawner & Obstacles (`ObstacleSpawner.cs`, `ObstacleController.cs`)
- **Location:** `Assets/Scripts/Gameplay/ObstacleSpawner.cs`, `Assets/Scripts/Gameplay/ObstacleController.cs`
- **Start Beat Quantized Spawning:**
  - When the inter-obstacle cooldown has elapsed (`Time.time >= _readyAt`), the spawner arms itself (`_pendingSpawn = true`).
  - Instead of spawning at an arbitrary millisecond, it subscribes to `MusicConductor.StartBeatTriggered` (and monitors `IsStartBeatThisFrame`).
  - The moment the background music and metronome hit the **downbeat of a new measure (Beat 1)**:
    1. The obstacle is instantiated.
    2. `_rhythm.BeginPhrase()` starts on the exact same frame.
    3. The first vocal sound ("GET") fires synchronously with the BGM kick and metronome accent.
  - A fallback safety timer (2.5s) guarantees obstacles still spawn reliably if audio is ever disabled or unavailable.
- **Dynamic Look-Ahead Distance:**
  - Spawns obstacles ahead based on current vehicle velocity and pattern duration:
    $$\text{distance} = \max\left(38\text{m},\, \text{BusSpeed} \times \frac{\text{PatternDuration}}{\text{TempoScale}} + 10\text{m}\right)$$
  - Guarantees the player always has exact time to hear the chant and honk back before the bus reaches the blocker.
- **Vehicle Types:**
  - **Car:** Standard traffic obstruction.
  - **Bicycle:** Narrower silhouette, faster swerve reaction.
  - **Ambulance:** Features alternating red/blue roof emergency beacon lights.
- **Obstacle Resolution (`Dodge`):** On rhythm success, `ObstacleController.Dodge()` animates a smooth rotation and steering swerve off the roadway onto the sidewalk shoulder.

### 3.7 3D World Generation, Texturing & Environment Props (`EndlessCity.cs`, `CityKit.cs`, `Look.cs`, `VehicleFactory.cs`)
- **Location:** `Assets/Scripts/World/`
- **`CityKit.cs` (FBX Auto-Scaling, Trees & Pedestrians):**
  - Searches imported FBX models for the asphalt road mesh (`Street`).
  - Measures the road's X-axis bounds and computes a uniform scale factor so the asphalt is always exactly **8.2 meters wide**.
  - Aligns the road surface flush with $Y = 0$.
  - Measures the block's Z-length to allow seamless longitudinal tiling.
  - **Street Trees (`PlantTrees`):** Staggers `SM_Tree_01` along the left ($X = -5.4$m) and right ($X = +5.4$m) sidewalks. Strips non-geometry nodes (cameras/lights), normalizes height to ~5.8m, and aligns base flush to sidewalk.
  - **Pedestrians (`PlacePedestrians`):** Places `SM_HumanFemale` and `SM_HumanMale` models along the sidewalks facing the street. Normalizes heights to ~3.4m for prominent arcade-scale readability and grounds flush to sidewalk surface.
- **`EndlessCity.cs` (Infinite Recycling):**
  - Maintains a continuous chain of designer city blocks ahead of the bus along +Z.
  - Recycles blocks that fall $40$m behind the camera to the front of the queue, creating an endless street with zero garbage collection allocations.
- **`Look.cs` (Material Conversion, Texture Pipeline & Palette Mapping):**
  - Replaces default FBX materials with URP Simple Lit or Unlit shaders.
  - **BaseColor Texture Mapping:**
    - `Street` / `lambert5` &rarr; `Street_BaseColor.png` (asphalt road markings and detailed tarmac)
    - `Sides` / `lambert6` &rarr; `Sides_BaseColor.png` (concrete pavement and curb details)
    - `Buildings_side_2` / `lambert3` &rarr; `Buildings_side_2_BaseColor.png` (brickwork, window frames, balconies)
    - `Buildings_side_3` / `lambert4` &rarr; `Buildings_side_3_BaseColor.png` (facade trims, storefronts)
    - `Street_lamp` / `lambert7` &rarr; `Lamp_BaseColor.png` (dark metallic pole and lantern glass)
    - Tree `leaves` &rarr; Organic vibrant green foliage (`#3D9438`)
    - Tree `branches` &rarr; Natural tree bark brown (`#5C3D29`)
    - Character materials &rarr; `Female_Dress.png`, `Female_Skin.png`, `Male_Shirt.png`, `Male_Skin.png`, etc.
  - **Material Caching:** Composite keys (`{matName}___{objName}`) prevent cross-contamination between meshes sharing generic Maya material names.
  - **Gray Mesh Fallback:** If an FBX renderer has no materials or default untextured gray, `PaletteFor` inspects the GameObject hierarchy name (e.g. `street`, `road`, `walk`, `curb`, `building`, `lamp`) to guarantee no untextured gray meshes ever appear in the scene.
- **`VehicleFactory.cs`:**
  - Instantiates `SM_Bus.fbx`, scales it to 7m length, sets up trigger collider bounds, and attaches `SpinWithSpeed` components to wheel nodes.

### 3.8 Typography, Hand-Stamped HUD & Accessibility UI (`GameUI.cs`)
- **Location:** `Assets/Scripts/UI/GameUI.cs`
- **Typography:** Uses custom display font `BrownieStencil` (with fallback chain: `ArchivoBlack` &rarr; `LegacyRuntime` &rarr; OS Sans-Serif).
- **Hand-Stamped Tilted HUD Stats:**
  - Stats use rotated parent holders (`TiltedStat`) so backing chips and text rotate together:
    - **Score Chip:** Rotated `-6.5°` at top-left (`340 × 112`).
    - **Streak Chip:** Rotated `+7.5°` below score (`380 × 92`), punches on increment, turns Red at streaks $\ge 10$.
    - **Speedometer:** Rotated `+5.0°` at top-right (`300 × 104`).
    - **HUD Metronome Toggle:** Rotated `+4.0°` underneath the speedometer.
- **Call-and-Response Typography Animator:**
  - 5 token labels: `GET`, `OUT`, `OF`, `THE`, `WAY`.
  - Call Phase: Token changes from Dim Gray (`#38393F`) to Paper White (`#F5F5F0`).
  - Response Phase: Tokens flip to Mustard Yellow (`#F5C242`), banner displays *"HONK IT BACK"*.
  - Hit Accepted: Words illuminate vibrant Green (`#45D166`).
  - Error: Words stamped with giant Red *"FAIL!"* (`#D94848`).
- **Accessibility Metronome Toggle Buttons:**
  - Available on both the Title screen and In-Game HUD.
  - Reflects active state visually (`METRONOME: OFF` in neutral chip vs `METRONOME: ON` in bright green/mustard).
  - Keyboard hotkey **`M`** triggers toggle instantly.

---

## 4. Dual-Loop Game Flow (State Diagram)

```mermaid
flowchart TD
    subgraph RunLoop [World & Bus Run Loop]
        title[Title Screen: Bus Parked, Engine Idling\nMetronome Toggle Available [M]]
        title -->|Space / Click / Honk| play[Playing: Bus Accelerates +Z]
        play --> spawner[Obstacle Spawner Computes Look-Ahead]
        spawner --> activeBlocker[Obstacle Placed in Bus Lane]
        activeBlocker --> rhythmCheck{Rhythm Outcome}
        rhythmCheck -->|Success: All Honks Hit| swerve[Obstacle Swerves Onto Sidewalk]
        swerve --> scoreUp[Score +1, Streak +1, Check Speed Up]
        scoreUp --> play
        rhythmCheck -->|Miss, Early Honk, or Timeout| crash[Bus Collides with Obstacle]
        crash --> revokeCard[License Revoked Screen Appears]
        revokeCard -->|Restart Delay or Spacebar| restart[Reset Run in Place at 130 BPM]
        restart --> play
    end

    subgraph RhythmLoop [Call-and-Response Rhythm Loop]
        activeBlocker -.-> callPhase[Call Phase: Chant Plays + Vocal Word SFX\nGET • OUT • OF • THE • WAY]
        callPhase --> respPhase[Response Phase: 'HONK IT BACK'\nPlayer Mirrors Rhythm on Space / Button]
        respPhase --> timingEval{Timing Window Check}
        timingEval -->|Within Tolerance| wordGreen[Word Turns Green]
        wordGreen --> checkComplete{All Words Hit?}
        checkComplete -->|Yes| rhythmCheck
        checkComplete -->|No| respPhase
        timingEval -->|Off-Beat, Early, or Timeout| failStamp[Word Row Stamps 'FAIL!']
        failStamp --> rhythmCheck
    end
```

---

## 5. Configuration & Tuning Guide ("Where to Look")

| What You Want to Change | File to Open | Variable / Method to Edit |
|---|---|---|
| **Tempo upgrade scores (when 140 & 150 BPM trigger)** | `GameManager.cs` | `TempoMilestones = { 4, 8 }` |
| **BGM audio tracks & metronome clicks** | `MusicConductor.cs` | `BGM130Resource`, `Metro130Resource`, `SetupBed()` |
| **Downbeat / Start Beat audio alignment offset** | `MusicConductor.cs` | `StartBeatOffset = 0.022f` |
| **Start Beat sync & obstacle spawn timing** | `ObstacleSpawner.cs` | `OnStartBeat()`, `_pendingSpawn`, `FirstDelay` |
| **Metronome click volume or bed volume** | `MusicConductor.cs` | `BedVolume = 0.58f`, `MetronomeVolume = 0.65f` |
| **Bus honk sound effect & volume** | `ProceduralAudio.cs` / `GameBootstrap.cs` | `HonkSound`, `Audio/honk.mp3`, `Honk()` |
| **Vocal word sound effects (`get`, `out`, `of`, `the`, `way`)** | `MusicConductor.cs` / `GameBootstrap.cs` | `WordClips`, `WordGet`, `WordOut`, `WordOf`, `WordThe`, `WordWay` |
| **Rhythm difficulty tiers & score thresholds** | `PhraseLibrary.cs` | `Pick(score)` &rarr; Tier 1 (0-2), Tier 2 (3-5), Tier 3 (6-9), Tier 4 (10+) |
| **Procedural random beat generation & musical gaps** | `PhraseLibrary.cs` | `GenerateRandomPattern()` &rarr; `gapOptions`, tolerance clamps |
| **Hit window generosity / timing tolerance** | `GameBootstrap.cs` / Inspector | `ToleranceScale` (e.g. `1.5f` = 50% more forgiving) |
| **Exact word timings for phrase rhythms** | `PhraseLibrary.cs` | `CreatePreset()` (`Even`, `Rush`, `Drag`, etc.) |
| **Bus driving speed and acceleration** | `BusController.cs` | `StartSpeed = 9f`, `MaxSpeed = 28f`, `Acceleration = 0.28f` |
| **Obstacle spawn distance look-ahead** | `ObstacleSpawner.cs` | `Spawn()` &rarr; `travel` calculation |
| **Obstacle vehicle mix (Car vs Bike vs Ambulance)** | `ObstacleSpawner.cs` | `RollKind()` probability thresholds |
| **Roadway width and tile alignment** | `CityKit.cs` | `StandardRoadWidth = 8.2f`, `FindRoad()` |
| **Street trees and pedestrian spawning** | `CityKit.cs` | `PlantTrees()`, `PlacePedestrians()`, tree height (~5.8m), pedestrian height (~3.4m) |
| **Obstacle vehicle scale (Car, Ambulance, Bike)** | `VehicleFactory.cs` | `MakeCar()` (1.55x), `MakeAmbulance()` (1.5x), `MakeBike()` (1.45x) |
| **Camera distance, height, and punch intensity** | `GameBootstrap.cs` / `CameraRig.cs` | `CameraOffset` (`0, 4.25, -4.8`), `CameraLookAhead` (`0, 1.6, 22`), `Rig.Punch(...)` |

---

## 6. Asset Pipeline & Team Git Policy

### Git LFS Exclusion Policy
All binary assets (`*.fbx`, `*.ttf`, `*.mp3`, `*.png`) are tracked **directly in Git** via `.gitattributes`. 
- **Rule:** Never add `filter=lfs` rules to `.gitattributes`. 
- **Benefit:** Teammates who do not have Git LFS installed will receive real binary files rather than broken 130-byte pointer text files.

### FBX Import Standards
- **Coordinate System:** Maya FBX exports should face along **+Z** (forward) with **+Y** up.
- **Road Width:** Road meshes named `Street` or `road` will be scaled automatically by `CityKit` to match the 8.2m driving lane standard.
- **Material Slot Naming:** To have materials automatically recognized by `Look.cs`, name material slots or parent nodes with standard identifiers:
  - Roadway: `street`, `road`, `lambert5`
  - Sidewalks: `side`, `walk`, `curb`, `lambert6`
  - Buildings: `building`, `lambert3`, `lambert4`
  - Streetlamps: `lamp`, `pole`, `lambert7`
  - Bus parts: `bus`, `wheel`, `wheelclinder`

---

## 7. Common Issues & Troubleshooting

#### 1. Models Rendering in Plain Gray
- **Cause:** FBX was exported without material bindings, or material colors are standard Maya gray `(0.5, 0.5, 0.5)`.
- **Solution:** `Look.UseImported` will automatically fall back to the GameObject's node name (`PaletteFor(r.name)`). Ensure meshes are named descriptively in the FBX (e.g. `Street`, `Buildings_side_1`, `Street_lamp`).

#### 2. Metronome Clicks Drifting from the Music Loop
- **Cause:** Audio sample rate mismatches or system latency over long run sessions.
- **Solution:** `MusicConductor.cs` features automatic loop seam detection and a 250ms periodic drift compensator (`targetTime = Music.time % metroLen`) to snap the metronome back into phase.

#### 3. Unity Throws "Two EventSystems in Scene" Warning
- **Cause:** Loading a scene that already contains an `EventSystem` while `GameUI.Create()` attempts to build one.
- **Solution:** Guarded in `GameUI.Create()` with `if (FindAnyObjectByType<EventSystem>() == null)`.

#### 4. Honk Input Latency
- **Cause:** Input polling delays in `Update()`.
- **Solution:** `GameUI.cs` checks both the new Unity Input System (`Keyboard.current.spaceKey.wasPressedThisFrame`) and mouse/gamepad inputs on the same frame, with an instant `_honkFrame` guard to prevent double-honking.
