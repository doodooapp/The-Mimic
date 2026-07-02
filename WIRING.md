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
