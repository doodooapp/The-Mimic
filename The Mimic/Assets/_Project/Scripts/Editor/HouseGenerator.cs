using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheMimic
{
    // One-click gray-box house: menu The Mimic > Generate Gray-box House rebuilds the whole
    // prototype layout from primitive cubes under a single "House" root.
    public static class HouseGenerator
    {
        const float WallHeight = 3f;
        const float WallThickness = 0.2f;
        const float DoorWidth = 1.2f;
        const float DoorHeight = 2.2f;
        const float SlabThickness = 0.1f;
        const float EndOverlap = 0.1f; // extend wall runs half a thickness so corners always close

        static Material matHall, matLiving, matKitchen, matBedroom, matBasement;

        [MenuItem("The Mimic/Generate Gray-box House")]
        public static void Generate()
        {
            var old = GameObject.Find("House");
            if (old != null)
                Object.DestroyImmediate(old);

            LoadOrCreateMaterials();

            var root = new GameObject("House") { isStatic = true };
            Undo.RegisterCreatedObjectUndo(root, "Generate Gray-box House");
            Transform t = root.transform;

            // ---- Floors & ceilings (rooms overlap walls by the end-overlap so slabs always meet)
            Slab(t, "Floor_Hallway", -1f, 1f, 0f, 10f, matHall, floor: true);
            Slab(t, "Floor_Living", -7f, -1f, 0f, 5f, matLiving, floor: true);
            Slab(t, "Floor_Kitchen", 1f, 6f, 0f, 4f, matKitchen, floor: true);
            Slab(t, "Floor_Bedroom", -6f, -1f, 5f, 10f, matBedroom, floor: true);
            Slab(t, "Floor_Basement", -2.5f, 2.5f, 10f, 14f, matBasement, floor: true);

            Slab(t, "Ceiling_Hallway", -1f, 1f, 0f, 10f, matHall, floor: false);
            Slab(t, "Ceiling_Living", -7f, -1f, 0f, 5f, matLiving, floor: false);
            Slab(t, "Ceiling_Kitchen", 1f, 6f, 0f, 4f, matKitchen, floor: false);
            Slab(t, "Ceiling_Bedroom", -6f, -1f, 5f, 10f, matBedroom, floor: false);
            Slab(t, "Ceiling_Basement", -2.5f, 2.5f, 10f, 14f, matBasement, floor: false);

            // ---- Walls running north-south (fixed X). Door values are Z positions.
            WallRunZ(t, "Wall_Living_W", -7f, 0f, 5f, matLiving);
            WallRunZ(t, "Wall_Bedroom_W", -6f, 5f, 10f, matBedroom);
            WallRunZ(t, "Wall_Hall_Living", -1f, 0f, 5f, matHall, 2.5f);   // living doorway, mid-hall
            WallRunZ(t, "Wall_Hall_Bedroom", -1f, 5f, 10f, matHall, 9f);   // bedroom doorway, north end
            WallRunZ(t, "Wall_Kitchen_E", 6f, 0f, 4f, matKitchen);
            WallRunZ(t, "Wall_Hall_Kitchen", 1f, 0f, 4f, matHall, 2.5f);   // kitchen doorway, mid-hall
            WallRunZ(t, "Wall_Hall_E_Upper", 1f, 4f, 10f, matHall);
            WallRunZ(t, "Wall_Basement_W", -2.5f, 10f, 14f, matBasement);
            WallRunZ(t, "Wall_Basement_E", 2.5f, 10f, 14f, matBasement);

            // ---- Walls running east-west (fixed Z). Door values are X positions.
            WallRunX(t, "Wall_South", 0f, -7f, 6f, matHall, 0f);           // front door at hallway south
            WallRunX(t, "Wall_Living_Bedroom", 5f, -7f, -1f, matLiving);   // includes the west step
            WallRunX(t, "Wall_Kitchen_N", 4f, 1f, 6f, matKitchen);
            WallRunX(t, "Wall_North_Mid", 10f, -6f, 2.5f, matBedroom, 0f); // basement doorway at hallway north
            WallRunX(t, "Wall_Basement_N", 14f, -2.5f, 2.5f, matBasement);

            // ---- Sightline breakers (1-2 per room, seen from each doorway)
            Box(t, "Break_Living_Stub", new Vector3(-3f, WallHeight / 2f, 2.5f), new Vector3(WallThickness, WallHeight, 1.5f), matLiving);
            Box(t, "Break_Living_Block", new Vector3(-5.5f, 1f, 1f), new Vector3(1f, 2f, 1f), matLiving);
            Box(t, "Break_Kitchen_Block", new Vector3(3.5f, 1f, 2.5f), new Vector3(1f, 2f, 1f), matKitchen);
            Box(t, "Break_Bedroom_Stub", new Vector3(-3.5f, WallHeight / 2f, 8f), new Vector3(1.5f, WallHeight, WallThickness), matBedroom);
            Box(t, "Break_Basement_Block", new Vector3(0.5f, 1f, 12f), new Vector3(1f, 2f, 1f), matBasement);

            // ---- Patrol points: one per room + two hallway
            var patrol = new GameObject("PatrolPoints").transform;
            patrol.SetParent(t, false);
            PatrolPoint(patrol, "PP_Hall_South", 0f, 1.5f);
            PatrolPoint(patrol, "PP_Living", -4f, 3.5f);
            PatrolPoint(patrol, "PP_Kitchen", 4.5f, 1.2f);
            PatrolPoint(patrol, "PP_Hall_North", 0f, 8.5f);
            PatrolPoint(patrol, "PP_Bedroom", -3.5f, 6.5f);
            PatrolPoint(patrol, "PP_Basement", -1f, 12.5f);

            // ---- Hiding spots (bedroom): closet with a 0.8 m slip-behind gap, and under-bed slab
            var hiding = new GameObject("HidingSpots").transform;
            hiding.SetParent(t, false);

            // Closet body 0.8 m off the bedroom west wall (inner face x = -5.9); hide in the gap behind it.
            Box(hiding, "Closet_Body", new Vector3(-4.8f, 1.1f, 7.5f), new Vector3(0.6f, 2.2f, 1.5f), matBedroom);
            HidingVolume(hiding, "Closet_HidingVolume", new Vector3(-5.5f, 1f, 7.5f), new Vector3(0.8f, 2f, 1.5f));

            // Bed slab with 0.5 m clearance underneath, against the bedroom south wall.
            Box(hiding, "Bed_Slab", new Vector3(-2.5f, 0.6f, 6.1f), new Vector3(1.6f, 0.2f, 2f), matBedroom);
            HidingVolume(hiding, "UnderBed_HidingVolume", new Vector3(-2.5f, 0.25f, 6.1f), new Vector3(1.6f, 0.5f, 2f));

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(root.scene);

            Debug.Log(
                "[HouseGenerator] Gray-box house generated. NEXT STEPS:\n" +
                "1. Re-bake the NavMesh: select the object with the NavMeshSurface component and click Bake.\n" +
                "   If you baked on the old ground Plane, you can delete/disable that Plane first and put the\n" +
                "   NavMeshSurface on the House root instead. If the Mimic won't fit through doorways after\n" +
                "   baking, lower the agent radius to 0.4 in Window > AI > Navigation > Agents, then re-bake.\n" +
                "2. Assign patrol points: select the Mimic and drag House/PatrolPoints' children (PP_*) into\n" +
                "   the Patrol Points array, replacing the old ones.\n" +
                "3. Move PlayerCapsule to the front door: position (0, 1, 1). Also move the Mimic, its\n" +
                "   ReDisguisePoint, all items/fakes, and the ExitDoor inside rooms (ExitDoor fits the front\n" +
                "   doorway at (0, 1.2, 0)).\n" +
                "4. Note: the under-bed hiding volume needs a crouch to enter (0.5 m clearance) — crouch is\n" +
                "   out of scope for now, so use the closet gap to test hiding.");
        }

        static void LoadOrCreateMaterials()
        {
            matHall = EnsureMaterial("House_Hallway", 0.50f);
            matLiving = EnsureMaterial("House_Living", 0.58f);
            matKitchen = EnsureMaterial("House_Kitchen", 0.44f);
            matBedroom = EnsureMaterial("House_Bedroom", 0.65f);
            matBasement = EnsureMaterial("House_Basement", 0.36f);
        }

        static Material EnsureMaterial(string name, float shade)
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project"))
                AssetDatabase.CreateFolder("Assets", "_Project");
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Generated"))
                AssetDatabase.CreateFolder("Assets/_Project", "Generated");

            string path = $"Assets/_Project/Generated/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                    shader = Shader.Find("Standard");
                mat = new Material(shader) { color = new Color(shade, shade, shade) };
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        static GameObject Box(Transform parent, string name, Vector3 center, Vector3 size, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.isStatic = true;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        // Wall along X at fixed Z, from x0 to x1, with optional doorway centers (X positions).
        static void WallRunX(Transform parent, string name, float z, float x0, float x1, Material mat, params float[] doors)
        {
            BuildRun(parent, name, x0, x1, doors,
                (from, to, y, h) => Box(parent, name, new Vector3((from + to) / 2f, y, z), new Vector3(to - from, h, WallThickness), mat));
        }

        // Wall along Z at fixed X, from z0 to z1, with optional doorway centers (Z positions).
        static void WallRunZ(Transform parent, string name, float x, float z0, float z1, Material mat, params float[] doors)
        {
            BuildRun(parent, name, z0, z1, doors,
                (from, to, y, h) => Box(parent, name, new Vector3(x, y, (from + to) / 2f), new Vector3(WallThickness, h, to - from), mat));
        }

        // Splits [start, end] into full-height segments around each doorway, plus a lintel above each gap.
        static void BuildRun(Transform parent, string name, float start, float end, float[] doors,
            System.Action<float, float, float, float> makeSegment)
        {
            System.Array.Sort(doors);
            float cursor = start - EndOverlap;
            foreach (float door in doors)
            {
                float gapStart = door - DoorWidth / 2f;
                float gapEnd = door + DoorWidth / 2f;
                if (gapStart - cursor > 0.01f)
                    makeSegment(cursor, gapStart, WallHeight / 2f, WallHeight);
                // lintel above the doorway
                makeSegment(gapStart, gapEnd, (DoorHeight + WallHeight) / 2f, WallHeight - DoorHeight);
                cursor = gapEnd;
            }
            if (end + EndOverlap - cursor > 0.01f)
                makeSegment(cursor, end + EndOverlap, WallHeight / 2f, WallHeight);
        }

        static void Slab(Transform parent, string name, float x0, float x1, float z0, float z1, Material mat, bool floor)
        {
            float y = floor ? -SlabThickness / 2f : WallHeight + SlabThickness / 2f;
            Box(parent, name,
                new Vector3((x0 + x1) / 2f, y, (z0 + z1) / 2f),
                new Vector3(x1 - x0 + 2f * EndOverlap, SlabThickness, z1 - z0 + 2f * EndOverlap), mat);
        }

        static void PatrolPoint(Transform parent, string name, float x, float z)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(x, 0.1f, z);
        }

        static void HidingVolume(Transform parent, string name, Vector3 center, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = center;
            var col = go.AddComponent<BoxCollider>();
            col.isTrigger = true;
            col.size = size;
            go.AddComponent<HidingSpot>();
        }
    }
}
