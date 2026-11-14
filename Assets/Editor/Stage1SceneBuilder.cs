using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
namespace GrandLife {
public static class Stage1SceneBuilder {
    static Material Mat(string n, Color c) {
        var sh = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var m = new Material(sh) { color = c };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        var path = "Assets/Materials/" + n + ".mat"; AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(m, path); return m;
    }
    static GameObject Prim(PrimitiveType t, string n, Transform par, Vector3 pos, Vector3 scale, Material m, bool collider = true) {
        var g = GameObject.CreatePrimitive(t); g.name = n; g.transform.SetParent(par, false);
        g.transform.localPosition = pos; g.transform.localScale = scale; g.GetComponent<Renderer>().sharedMaterial = m;
        if (!collider) Object.DestroyImmediate(g.GetComponent<Collider>()); return g;
    }
    [MenuItem("GrandLife/Build Stage 1 Scene")]
    public static void Build() {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        System.IO.Directory.CreateDirectory("Assets/Materials"); System.IO.Directory.CreateDirectory("Assets/Scenes");
        var grass = Mat("Grass", new Color(.25f, .5f, .25f)); var asph = Mat("Asphalt", new Color(.15f, .15f, .17f));
        var white = Mat("White", Color.white); var red = Mat("CarRed", new Color(.8f, .1f, .1f));
        var dark = Mat("Dark", new Color(.05f, .05f, .05f)); var glass = Mat("Glass", new Color(.4f, .6f, .8f));
        var env = new GameObject("City").transform;
        Prim(PrimitiveType.Cube, "Ground", env, new Vector3(0, -.5f, 0), new Vector3(300, 1, 300), grass);
        Prim(PrimitiveType.Cube, "RoadZ", env, new Vector3(0, .05f, 0), new Vector3(14, .1f, 300), asph);
        Prim(PrimitiveType.Cube, "RoadX", env, new Vector3(0, .06f, 0), new Vector3(300, .1f, 14), asph);
        for (int i = -14; i <= 14; i++) if (i != 0) Prim(PrimitiveType.Cube, "Line", env, new Vector3(0, .12f, i * 10), new Vector3(.3f, .02f, 4), white, false);
        var rnd = new System.Random(7);
        for (int i = 0; i < 12; i++) foreach (int side in new[] { -1, 1 }) {
            float z = -110 + i * 20 + 10, h = 8 + rnd.Next(0, 30);
            if (Mathf.Abs(z) < 12) continue;
            var c = new Color(.5f + (float)rnd.NextDouble() * .3f, .5f + (float)rnd.NextDouble() * .2f, .55f);
            Prim(PrimitiveType.Cube, "Building", env, new Vector3(side * 22, h / 2, z), new Vector3(14, h, 14), Mat("B" + i + (side > 0 ? "R" : "L"), c));
        }
        // player
        var pl = GameObject.CreatePrimitive(PrimitiveType.Capsule); pl.name = "Player";
        Object.DestroyImmediate(pl.GetComponent<Collider>()); pl.transform.position = new Vector3(-1.5f, 1.1f, 0);
        pl.GetComponent<Renderer>().sharedMaterial = Mat("Player", new Color(.2f, .4f, .9f));
        pl.AddComponent<CharacterController>(); var pc = pl.AddComponent<PlayerController>();
        // car
        var car = new GameObject("Car"); car.transform.position = new Vector3(-3.5f, 1f, 5f);
        var rb = car.AddComponent<Rigidbody>(); rb.mass = 1200f;
        Prim(PrimitiveType.Cube, "Body", car.transform, new Vector3(0, .1f, 0), new Vector3(1.9f, .6f, 4.2f), red);
        Prim(PrimitiveType.Cube, "Cabin", car.transform, new Vector3(0, .65f, -.2f), new Vector3(1.6f, .5f, 2.2f), glass);
        var cc = car.AddComponent<CarController>(); cc.wheels = new WheelCollider[4]; cc.visuals = new Transform[4];
        Vector3[] wp = { new Vector3(-.9f, -.3f, 1.3f), new Vector3(.9f, -.3f, 1.3f), new Vector3(-.9f, -.3f, -1.3f), new Vector3(.9f, -.3f, -1.3f) };
        for (int i = 0; i < 4; i++) {
            var w = new GameObject("WheelCol" + i); w.transform.SetParent(car.transform, false); w.transform.localPosition = wp[i];
            var wc = w.AddComponent<WheelCollider>(); wc.radius = .35f; wc.suspensionDistance = .25f;
            wc.suspensionSpring = new JointSpring { spring = 35000f, damper = 4500f, targetPosition = .5f };
            var v = new GameObject("WheelVis" + i); v.transform.SetParent(car.transform, false);
            var cyl = Prim(PrimitiveType.Cylinder, "Tire", v.transform, Vector3.zero, new Vector3(.7f, .12f, .7f), dark, false);
            cyl.transform.localRotation = Quaternion.Euler(0, 0, 90);
            cc.wheels[i] = wc; cc.visuals[i] = v.transform;
        }
        // camera, game logic, HUD, event system
        var cam = Camera.main; cam.transform.position = new Vector3(-1.5f, 3.5f, -6f);
        var tpc = cam.gameObject.AddComponent<ThirdPersonCamera>();
        var gc = new GameObject("GameController").AddComponent<GameController>(); gc.player = pc; gc.car = cc; gc.cam = tpc;
        new GameObject("HUD").AddComponent<MobileHUD>().game = gc;
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Stage1.unity");
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Stage1.unity", true) };
        Debug.Log("Stage 1 scene built: Assets/Scenes/Stage1.unity");
    }
}}
