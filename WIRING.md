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
