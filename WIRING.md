# The Mimic — Editor Wiring Instructions

Follow each task's steps literally, in order. Done when the test at the end of the section passes.

## Task 1 — PlayerInteraction + Prop/PropRegistry

### 0. Open the project
1. Pull this branch and open the `The Mimic` folder in Unity **6000.3.19f1**.
2. Unity will generate `.meta` files for the new scripts under `Assets/_Project/` — commit those together with your scene changes when you're done.

### 1. Import Starter Assets (the player character)
1. In a browser, open the Unity Asset Store and add **"Starter Assets: Character Controllers | URP"** (free, by Unity Technologies) to your assets.
2. In Unity: **Window > Package Manager**. In the **left column** of the window, click **My Assets** (sign in to the same Unity account you used in the browser if prompted). Find **Starter Assets: Character Controllers**, click **Download**, then **Import**. Import everything. Accept any prompt to install dependencies (Cinemachine).
3. If Unity warns the asset was made for an older Unity version, import anyway. If Cinemachine components later show as **Deprecated** or Unity offers a Cinemachine upgrade, decline/ignore it for now — the deprecated components still work.
4. If a "New Input System" restart prompt appears, accept it (the project already uses the Input System).

### 2. Put the player in the scene
1. Open your working scene (e.g. `Assets/Scenes/SampleScene.unity`).
2. If the scene has no floor: **GameObject > 3D Object > Plane**, set its position to `(0, 0, 0)`.
3. Delete the scene's existing **Main Camera** GameObject if there is one.
4. In the Project window go to `Assets/StarterAssets/FirstPersonController/Prefabs/` and drag these three prefabs into the Hierarchy:
   - **PlayerCapsule**
   - **MainCamera**
   - **PlayerFollowCamera**
5. Move **PlayerCapsule** so it stands on the floor (e.g. position `(0, 1, 0)`).
6. Wire the camera to the player (the prefabs can't ship pre-wired to each other): expand **PlayerCapsule** in the Hierarchy and find its child **PlayerCameraRoot**. Select **PlayerFollowCamera**; in its Cinemachine camera component, drag **PlayerCameraRoot** into the **Follow** field. (If the component is `CinemachineCamera` with a **Tracking Target** field instead — that's Cinemachine 3 — drag it there.)
7. Press Play briefly to check: the view should be first-person from the capsule and turn with the mouse. Stop Play mode. If the camera doesn't move, re-check step 6.

### 3. Create the interaction config asset
1. In the Project window, create folder `Assets/_Project/Configs`.
2. Right-click inside it: **Create > The Mimic > Interaction Config**. Keep the name `InteractionConfig`.
3. Leave the defaults: **Interact Range = 3**, **Interact Layers = Everything**.

### 4. Add and wire PlayerInteraction
1. Select **PlayerCapsule** in the Hierarchy.
2. **Add Component > Player Interaction** (script `PlayerInteraction`).
3. Set its Inspector fields:
   - **Config** → the `InteractionConfig` asset from step 3.
   - **Player Camera** → drag the **MainCamera** GameObject from the Hierarchy. (Leaving it empty also works — it falls back to the camera tagged MainCamera.)
   - **Interact Action** → click the circle picker, type `Interact` in the search box, and double-click the entry named **Player/Interact** (it comes from the `InputSystem_Actions` asset — the only Interact action in the project).

### 5. Make a test prop
1. **GameObject > 3D Object > Cube**, name it `TestProp`, place it ~2 m in front of the player at eye height (e.g. `(0, 1, 2)`).
2. **Add Component > Prop** (script `Prop`). The Cube's built-in Box Collider is all it needs.

### 6. Test
1. Press **Play**. Move with WASD, look with the mouse.
2. Aim the **center of the screen** at the cube (there's no crosshair UI yet — DebugHUD comes in a later task) and press **E** within ~3 m.
3. ✅ Pass: the Console prints `[Prop] Interacted with 'TestProp'.`
4. Also check: pressing E while aiming at the floor or from >3 m away prints nothing, and there are no **red errors** on entering Play mode (yellow warnings — e.g. about deprecated Cinemachine components — are fine).

### Tags / Layers
None needed for Task 1. The interaction raycast uses **Everything** and ignores trigger colliders.

## Task 2 — PhoneController + DebugHUD

New scripts: `PhoneController`, `PhoneConfig`, `PhoneUI`, `DebugHUD`. `PlayerInteraction` now also tracks what you're aiming at for the HUD prompt. A **Phone** action (hold **Tab**) was added to `InputSystem_Actions` — Unity picks it up automatically on reimport.

### 1. Phone config asset
1. In `Assets/_Project/Configs`: **Create > The Mimic > Phone Config**. Keep the name `PhoneConfig`.
2. Defaults: **Start Percent = 100**, **Drain Per Second = 4** (25 s of total phone time — tune later).

### 2. Phone UI canvas
1. **GameObject > UI > Canvas**, name it `PhoneCanvas` (Render Mode: Screen Space - Overlay is the default — keep it). This also creates an EventSystem — keep it.
2. Right-click `PhoneCanvas` → **Create Empty**, name it `PhonePanel`. In its Rect Transform, click the anchor square and pick **stretch/stretch** (bottom-right icon while holding Alt) so it fills the screen.
3. Right-click `PhonePanel` → **UI > Image**, name it `PhoneBackground`. Set its anchors to center, **Width 400, Height 600**, and its Image color to dark gray, alpha ~230.
4. Inside `PhonePanel`, create three **UI > Image** objects: `Photo1`, `Photo2`, `Photo3`. Size each **Width 300, Height 140**, positioned vertically inside the background (e.g. Pos Y = 150, 0, -150). Give them three different obvious colors (red / green / blue).
5. Inside each photo Image, right-click → **UI > Legacy > Text**, name them `Label1/2/3`. Type a placeholder belonging name in each (e.g. `Portrait`, `Clock`, `Teddy`). Color: white, Font Size 24.
6. Inside `PhonePanel`, add one more **UI > Legacy > Text** named `BatteryText`, anchored to the top of the background (Pos Y ≈ 260), Font Size 28, white. Text can say `Battery 100%`.
7. Select `PhoneCanvas` (the root, NOT the panel) → **Add Component > Phone UI** and assign:
   - **Panel Root** → `PhonePanel`
   - **Battery Text** → `BatteryText`
   - **Photo Slots** (size 3) → `Photo1`, `Photo2`, `Photo3`
   - **Photo Labels** (size 3) → `Label1`, `Label2`, `Label3`

### 3. PhoneController on the player
1. Select **PlayerCapsule** → **Add Component > Phone Controller**, assign:
   - **Config** → the `PhoneConfig` asset
   - **Phone Action** → circle picker, search `Phone`, pick **Player/Phone**
   - **Phone UI** → the `PhoneCanvas` object (it has the PhoneUI component)

### 4. DebugHUD
1. Create an empty GameObject named `HUD` → **Add Component > Debug HUD**, assign:
   - **Phone** → `PlayerCapsule` (PhoneController)
   - **Interaction** → `PlayerCapsule` (PlayerInteraction)

### 5. Pass test
1. Press Play. You see a **white crosshair dot** at screen center and `Battery: 100%` top-left.
2. Hold **Tab**: the phone panel appears with 3 colored photos + labels; the battery % visibly counts down both on the panel and the HUD. Release Tab: panel hides and the **battery stops draining**.
3. Aim at `TestProp`: HUD shows `[E] TestProp` only while aiming within range.
4. Hold Tab until 0%: the panel closes itself, HUD shows `Battery: 0% (DEAD)`, and Tab does nothing for the rest of the run. Console logs the battery-dead message once.

## Task 3 — Objectives + GameManager

New scripts: `TargetItem`, `ObjectiveManager`, `ExitDoor`, `GameManager`. DebugHUD now shows the collected count.

### 1. Scene must be in the build scene list (needed for restart)
1. **File > Build Profiles** (or **Build Settings** on older layouts) → **Scene List** → with your working scene open, click **Add Open Scenes**. Without this, pressing R to restart throws a scene-load error.

### 2. GameManager
1. Create an empty GameObject named `GameManager` → **Add Component > Game Manager**. No fields to set. Keep exactly one in the scene.

### 3. Three target items
1. Make three small cubes (or reuse `TestProp` as the first): name them `Item_Portrait`, `Item_Clock`, `Item_Teddy`, scatter them around the scene at reachable height.
2. On each: **Add Component > Prop**, then **Add Component > Target Item**. Set **Prop Id** to `Portrait`, `Clock`, `Teddy` respectively (must match exactly below).

### 4. ObjectiveManager
1. Create an empty GameObject named `Objectives` → **Add Component > Objective Manager**.
2. Set **Target Prop Ids** size 3, entries exactly: `Portrait`, `Clock`, `Teddy`.

### 5. ExitDoor
1. Create a cube named `ExitDoor` (e.g. scale `(1.2, 2.4, 0.15)`), place it against a wall or at the scene edge.
2. **Add Component > Exit Door**, assign **Objectives** → the `Objectives` GameObject.
   (Note: ExitDoor is interactable but deliberately NOT a Prop.)

### 6. DebugHUD
1. On `HUD`, assign the new **Objectives** field → the `Objectives` GameObject.

### 7. Pass test
1. Press Play. HUD shows `Items: 0/3`.
2. Press E on the exit door first: Console logs `Locked — 0/3 items collected.`
3. Collect each item with E: it disappears, HUD counts up, Console logs progress. Collecting the last logs the unlock message.
4. Press E on the exit door again: **"YOU ESCAPED — Press R to restart"** overlay appears and the game freezes.
5. Press **R**: the scene reloads, everything is back (items restored, battery full, `Items: 0/3`).

## Task 4 — MimicController

> ### ⚠ MANUAL SCENE SETUP REQUIRED — READ FIRST
> This task does not work until YOU bake a NavMesh and place patrol/re-disguise points in the scene (steps 1, 4, 5). Unity 6 uses the **AI Navigation** package's `NavMeshSurface` component — do NOT look for the legacy Window > AI > Navigation bake window.

New scripts: `MimicController`, `MimicConfig`.

### 1. Bake the NavMesh
1. The **AI Navigation** package is already in the project (`com.unity.ai.navigation`). If walls/furniture exist, make sure they and the floor are **not** marked as anything weird — default static geometry is fine.
2. Select your floor Plane → **Add Component > Nav Mesh Surface** → click **Bake**. A blue overlay should cover the walkable floor (toggle visibility with the scene-view AI Navigation overlay if you don't see it).
3. Re-bake whenever you move/add large geometry.

### 2. Mimic config asset
1. In `Assets/_Project/Configs`: **Create > The Mimic > Mimic Config**, keep name `MimicConfig`. Defaults are sensible; tune later.

### 3. Build the Mimic
1. Create an empty GameObject named `Mimic`, place it in a corner of the map, on the floor.
2. **Add Component > Mimic Controller** (this auto-adds a **NavMeshAgent** — keep its defaults).
3. Right-click `Mimic` → **3D Object > Capsule**, name it `Body`, set local position `(0, 1, 0)`. Give it an obvious material color later if you want.
4. On the `Mimic` root, assign:
   - **Config** → the `MimicConfig` asset
   - **Disguised Prop** → the fake prop from step 6
   - **Player** → the `PlayerCapsule` object
   - **Body** → the `Body` child
   - **Patrol Points** → the empties from step 4
   - **Re Disguise Point** → the empty from step 5

### 4. Patrol points
1. Create 3–4 empty GameObjects named `PatrolPoint1..4`, spread across the walkable floor (on the blue NavMesh). Assign them to the Mimic's **Patrol Points** array.

### 5. Re-disguise point
1. Create one empty GameObject named `ReDisguisePoint` somewhere on the NavMesh (this is where the fake prop reappears after a hunt). Assign it.

### 6. The fake prop
1. Create a cube named `Fake_Portrait` (visually it's a "flawed copy" — for gray-box, just another cube). Place it somewhere plausible.
2. **Add Component > Prop**. Do **NOT** add a TargetItem to it — the fake is never collectible.
3. Assign it as the Mimic's **Disguised Prop**.

### 7. Pass test
1. Press Play. The Mimic body is invisible; the fake cube sits in the scene. HUD prompt shows `[E] Fake_Portrait` when aimed at.
2. Press E on the fake: the cube vanishes, Console logs `REVEALED`, and after ~1.5 s a capsule appears there and starts patrolling between your points.
3. Walk into its view cone: it speeds up and chases you. Touch = **"YOU DIED"** overlay.
4. Press R, reveal it again, then break line of sight (hide behind geometry) and keep away for the hunt duration (20 s default): Console logs `retreating`, the capsule walks to `ReDisguisePoint`, disappears, and the fake cube reappears **at that point**. Interacting with it starts the cycle again.
5. Known Task-4 limitation (fixed in Task 5): there is no hiding system yet — only distance/geometry breaks line of sight.

## Task 5 — Hiding spots

> ### ⚠ MANUAL SCENE SETUP REQUIRED
> You must place at least one trigger volume (step 2) — nothing hides you until you do.

New scripts: `PlayerHideState`, `HidingSpot`. Mimic line-of-sight now respects hiding, with one documented exception: **if the Mimic is pursuing you and can see you at the moment you enter a spot, the spot does NOT save you** until it loses sight of you (simplest rule that prevents "dive into the closet mid-chase" exploits).

### 1. Player
1. Select **PlayerCapsule** → **Add Component > Player Hide State**. No fields.
2. On `HUD`, assign the new **Hide State** field → `PlayerCapsule`.

### 2. Hiding volumes
1. Create an empty GameObject named `Closet1` where a closet/bed would be.
2. **Add Component > Box Collider**, check **Is Trigger**, set Size to roughly `(1.2, 2, 1.2)` — big enough to fully stand inside.
3. **Add Component > Hiding Spot**.
4. (Optional) Add a cube child WITHOUT a collider as a visual marker, or leave it invisible for now. If you give the visual its own solid collider, the Mimic's sight ray will treat it as a wall — that's desirable for a closet with walls, but then leave an opening to walk in through.

### 3. Pass test
1. Press Play, walk into the volume: HUD shows `HIDDEN`, Console logs the hiding message. Walk out: `HIDDEN` disappears.
2. Reveal the Mimic, break its line of sight FIRST, then enter the spot: it patrols past without pursuing even at close range (as long as its sight ray to you is clear of the volume — remember the spot itself is a trigger and doesn't block sight; solid geometry does).
3. Reveal it, let it chase you with clear sight, and dive into the spot while it's looking: it keeps coming — the exception working as documented. Escape its sight completely and re-enter: you're safe again.

## Task 6 — Reveal atmosphere: light flicker

New scripts: `LightsController`, `FlickerableLight`, `LightsConfig`. On reveal, every registered light flickers violently until the Mimic retreats. No audio (out of scope).

### 1. Config asset
1. In `Assets/_Project/Configs`: **Create > The Mimic > Lights Config**, keep name `LightsConfig`. Defaults: min 0, max 1.6, interval 0.05.

### 2. Controller
1. Create an empty GameObject named `Lights` → **Add Component > Lights Controller**, assign **Config** → `LightsConfig`.

### 3. Register lights
1. On every scene light that should react (e.g. add a few **GameObject > Light > Point Light** around the map at Y ≈ 2.5, Range ~10): **Add Component > Flickerable Light**.
2. For the effect to be visible, consider making the scene darker: Window > Rendering > Lighting > Environment, drop the ambient/skybox intensity — optional, gray-box.

### 4. Pass test
1. Press Play, press E on the fake prop: all registered lights strobe/flicker immediately and keep flickering the whole hunt.
2. When the Console logs `retreating`, every light returns to its exact original intensity.
3. Lights without a FlickerableLight component are unaffected.

## Task 7 — Death/Win screen

New script: `DeathScreen`. It replaces GameManager's bare overlay with one showing which prop was the Mimic (id + position) and the run stats — on both death AND win, for playtest discussion.

### 1. Wiring
1. On the `HUD` GameObject: **Add Component > Death Screen**, assign:
   - **Phone** → `PlayerCapsule`
   - **Objectives** → the `Objectives` GameObject
   - **Mimic** → the `Mimic` GameObject

### 2. Pass test
1. Get caught by the Mimic: a centered panel shows **YOU DIED**, `The Mimic was disguised as 'Fake_Portrait' at (x, y, z)`, items collected, battery remaining, time survived, and `Press R to restart`. The old bare overlay does NOT also appear.
2. Win a run (collect 3, exit door): same panel with **YOU ESCAPED** and the same stats.
3. R restarts cleanly; stats are re-captured fresh on the next run's end.

## Task 8 — Run randomization (RunDirector)

> ### ⚠ MANUAL SCENE SETUP REQUIRED
> You need 5+ TargetItems and 2+ fake-candidate Props placed in the scene (steps 2–3) before this does anything interesting.

New scripts: `RunDirector`, `RunConfig`. `TargetItem` gains a **Photo Color** field; the phone photos now update per run. DebugHUD gains a seed readout + input field (bottom-left).

### 1. Config asset
1. In `Assets/_Project/Configs`: **Create > The Mimic > Run Config**, keep name `RunConfig` (**Targets Per Run = 3**).

### 2. Grow the candidate pools
1. Add 2+ more target items the same way as Task 3 (Prop + TargetItem, unique Prop Ids, e.g. `Radio`, `Vase`) so you have **5+ TargetItems** total.
2. On every TargetItem, set a distinct **Photo Color** — this is what shows on the phone.
3. Add 1+ more fake props like Task 4's (Prop only, NO TargetItem, e.g. `Fake_Clock`), so you have **2+ fake candidates**.

### 3. RunDirector
1. Create an empty GameObject named `RunDirector` → **Add Component > Run Director**, assign:
   - **Config** → `RunConfig`
   - **Randomize Seed Each Run** → leave checked (uncheck + set **Inspector Seed** for a fixed run)
   - **Target Candidates** → ALL TargetItems in the scene (5+)
   - **Fake Candidates** → ALL fake Props (2+)
   - **Objectives** → the `Objectives` GameObject
   - **Mimic** → the `Mimic` GameObject
   - **Phone UI** → the `PhoneCanvas` object
2. The Inspector target list on `Objectives` and **Disguised Prop** on the `Mimic` are now just fallbacks — RunDirector overrides both at scene start.
3. On `HUD`, assign the new **Run Director** field → the `RunDirector` GameObject.

### 4. Pass test
1. Press Play. Console logs `Seed N: targets [...], fake '...'`. HUD bottom-left shows the seed.
2. Hold Tab: the 3 photos show the chosen targets' colors + ids (not what you typed in Task 2).
3. Collecting a NON-chosen target item does nothing (it stays put, count unchanged); the 3 chosen ones count up and unlock the door.
4. Type a seed (e.g. `42`) into the bottom-left field → **Set seed + restart**: the same 3 targets and same fake are picked every time for that seed. Press R after dying/winning: the seed stays 42 (sticky) — type a new one to change it.
5. The fake prop per run is whichever candidate the seed picked — check the Console line.

## Task 9 — Gray-box house generator

New script: `Editor/HouseGenerator.cs` (editor-only; adds a menu item, ships nothing at runtime).

### 1. Generate
1. Menu bar: **The Mimic > Generate Gray-box House**. One click builds everything under a `House` root: 5 spaces (hallway, living, kitchen, bedroom, basement stand-in), floors + ceilings, doorways with lintels, per-room gray shades (material assets in `Assets/_Project/Generated/`), sightline breakers, `PatrolPoints` (6 children), and 2 bedroom hiding spots (closet gap + under-bed).
2. Re-running the menu item deletes the old `House` and rebuilds — don't hand-edit inside `House`, it's disposable.

### 2. Re-wire the scene to the house (the Console prints these too)
1. Delete/disable the old ground **Plane**. Put the **NavMeshSurface** on the `House` root (Add Component if needed) and click **Bake**. Blue overlay should cover every room and flow through all 5 doorways. If a doorway pinches shut, lower the agent radius to **0.4** (Window > AI > Navigation > Agents) and re-bake.
2. Select the `Mimic` → replace the **Patrol Points** array with the 6 `PP_*` children of `House/PatrolPoints`.
3. Move **PlayerCapsule** to the front door: `(0, 1, 1)`. Move the `Mimic` + `ReDisguisePoint` into rooms (on the NavMesh), scatter the target items/fakes through different rooms, and put the `ExitDoor` in the front doorway at `(0, 1.2, 0)`.
4. Spread your Task 6 point lights so each room has one.

### 3. Pass test
1. One menu click produces the house; walk every room — no wall gaps, all 5 doorways passable.
2. NavMesh bake covers all floors; reveal the Mimic and watch it patrol through the rooms without snagging on doorways.
3. The closet gap in the bedroom (behind the box on the west wall) flags `HIDDEN`.
4. Known limitation: the under-bed volume needs a crouch (0.5 m clearance) — untestable until crouch exists; the closet is the testable spot.

## Task 10 — Player movement tuning (horror pacing)

New scripts: `PlayerConfig`, `PlayerMovementTuner`. The Starter Assets scripts are untouched — the tuner drives the FirstPersonController's public fields from the config every frame. The **Crouch** action's keyboard binding moved from **C** to **Left Ctrl**, and the bed in the house generator was raised to 1.2 m clearance (a crouched CharacterController is ~1 m tall — 0.5 m was physically impossible to enter).

### 1. Config asset
1. In `Assets/_Project/Configs`: **Create > The Mimic > Player Config**, keep name `PlayerConfig`.
2. Defaults: **Walk Speed 2.2**, **Sprint Enabled OFF** (tick it for A/B playtests), **Sprint Speed 4.5**, crouch multipliers 0.5, **Look Sensitivity 1**, **Look Smoothing 0** (try 0.05–0.1 if raw feels twitchy).

### 2. Tuner on the player
1. Select **PlayerCapsule** → **Add Component > Player Movement Tuner**, assign:
   - **Config** → the `PlayerConfig` asset
   - **Crouch Action** → circle picker, search `Crouch`, pick **Player/Crouch**
   - **Camera Root** → the **PlayerCameraRoot** child of PlayerCapsule (drag from the Hierarchy)

### 3. Regenerate the house (for the raised bed)
1. **The Mimic > Generate Gray-box House** again, then re-**Bake** the NavMeshSurface. (Remember: regenerating replaces the whole `House` root.)

### 4. Pass test
1. **Space does nothing** — no jump, ever.
2. **Shift does nothing** — same speed with or without it. Tick **Sprint Enabled** on the config asset during Play mode: Shift now sprints at 4.5. Untick — gone again.
3. Walking feels deliberate (~2.2 m/s — roughly half the old default).
4. **Hold Ctrl**: camera lowers smoothly, movement slows to ~1.1; walk under the bed slab → HUD shows `HIDDEN`. Release Ctrl while under the bed: you STAY crouched until you walk out (no standing up through the slab), then stand automatically... release Ctrl again if you kept holding — standing happens on release with headroom.
5. All of it (speeds, sprint toggle, crouch multipliers, sensitivity, smoothing) tunable live from the one `PlayerConfig` asset in Play mode.

## Task 11 — HDRP → URP material converter (for the Vintage Haunted House pack)

New script: `Editor/HdrpToUrpMaterialConverter.cs` (editor-only). Converts HDRP materials — including ones showing `Hidden/InternalErrorShader` — to **URP/Lit** in place: base color + texture + tiling, normal maps, emission, transparency/alpha-clip, and it unpacks HDRP mask maps into generated `_URP_MetallicSmoothness` / `_URP_Occlusion` PNGs.

### 1. Run it
1. Re-import the pack if you deleted it (double-click `VintageInteriorHDRP.unitypackage`). Expect pink — that's what we're fixing.
2. In the Project window, **select the `GhostbuGaming` folder** (single click).
3. Menu: **The Mimic > Convert HDRP Materials To URP** → confirm the dialog. A progress bar runs (the pack is ~1 GB, mask-map unpacking can take a few minutes).
4. Read the Console report: how many converted, which materials had no base map (stay gray), which base maps were guessed.

### 2. Rules of use
1. **Do NOT open `DemoSceneHDRP`** — its lighting, volume profiles, and PostFX are HDRP-only and unconvertible. Drag the pack's **prefabs/models** into your own scene instead.
2. Re-running the converter is safe — already-converted materials are skipped, generated PNGs are reused.
3. To restore the originals, just re-import the `.unitypackage`.
4. **Don't commit the pack to git** without telling Claude first (it needs LFS/ignore setup — ~1 GB).

### 3. Pass test
1. After conversion, the pack's material previews are no longer pink.
2. Drag a few furniture prefabs into your working scene: textured surfaces, normal detail visible under your point lights.
3. Console report has no red errors (a "NO BASE MAP" list is possible for custom-shader materials — fix those by assigning **Base Map** manually on each listed material).
