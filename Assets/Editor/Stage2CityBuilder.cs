using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace GrandLife {
    public static class Stage2CityBuilder {
        static Material Make(string name, Color color) {
            string path = "Assets/Materials/" + name + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                m = new Material(shader);
                AssetDatabase.CreateAsset(m, path);
            }
            m.color = color;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            EditorUtility.SetDirty(m);
            return m;
        }
        static GameObject Cube(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent) {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.name = name; g.transform.SetParent(parent, false);
            g.transform.position = pos; g.transform.localScale = scale;
            g.GetComponent<Renderer>().sharedMaterial = mat;
            return g;
        }
        static void Landmark(Transform root, string title, string job, Vector3 pos, Color color) {
            var mat = Make("Landmark_" + job.Replace(" ", "_"), color);
            var building = Cube(title, pos + Vector3.up * 5f, new Vector3(18, 10, 16), mat, root);
            var sign = new GameObject("Вывеска_" + title);
            sign.transform.SetParent(building.transform, false);
            sign.transform.localPosition = new Vector3(0, .15f, -.51f);
            var tm = sign.AddComponent<TextMesh>();
            tm.text = title; tm.fontSize = 48; tm.characterSize = .18f;
            tm.anchor = TextAnchor.MiddleCenter; tm.alignment = TextAlignment.Center;
            tm.color = Color.white;

            var marker = new GameObject("Работа_" + job);
            marker.transform.position = pos + Vector3.up * 1.1f;
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            sphere.name = "Маркер"; sphere.transform.SetParent(marker.transform, false);
            sphere.transform.localScale = new Vector3(1.2f, .08f, 1.2f);
            sphere.GetComponent<Renderer>().sharedMaterial = mat;
            Object.DestroyImmediate(sphere.GetComponent<Collider>());
            var trigger = marker.AddComponent<SphereCollider>();
            trigger.isTrigger = true; trigger.radius = 4f;
            marker.AddComponent<JobPoint>().jobTitle = job;
        }

        [MenuItem("GrandLife/Build Stage 2 Russian City")]
        public static void Build() {
            if (SceneManager.GetActiveScene().IsValid() == false) {
                EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            }
            var old = GameObject.Find("Novograd_Stage2");
            if (old != null) Object.DestroyImmediate(old);
            var root = new GameObject("Novograd_Stage2").transform;

            var asphalt = Make("City_Asphalt", new Color(.12f,.13f,.15f));
            var sidewalk = Make("City_Sidewalk", new Color(.48f,.48f,.46f));
            var concrete = Make("City_Concrete", new Color(.62f,.62f,.6f));
            var grass = Make("City_Grass", new Color(.19f,.38f,.2f));
            Cube("Проспект_Ленина", new Vector3(0,.04f,0), new Vector3(18,.12f,300), asphalt, root);
            Cube("Улица_Центральная", new Vector3(0,.05f,0), new Vector3(300,.12f,18), asphalt, root);
            Cube("Дорога_Север", new Vector3(0,.04f,70), new Vector3(300,.12f,12), asphalt, root);
            Cube("Дорога_Юг", new Vector3(0,.04f,-70), new Vector3(300,.12f,12), asphalt, root);
            Cube("Дорога_Запад", new Vector3(-75,.04f,0), new Vector3(12,.12f,300), asphalt, root);
            Cube("Дорога_Восток", new Vector3(75,.04f,0), new Vector3(12,.12f,300), asphalt, root);

            // Кварталы и тротуары вокруг центральных улиц.
            foreach (float x in new float[] {-48,-25,25,48}) {
                foreach (float z in new float[] {-105,-45,45,105}) {
                    Cube("Тротуар", new Vector3(x,.13f,z), new Vector3(16,.25f,38), sidewalk, root);
                }
            }
            foreach (float x in new float[] {-120,-95,95,120}) {
                foreach (float z in new float[] {-105,-45,45,105}) {
                    float h = 12 + Mathf.Abs(x+z)%22;
                    var c = new Color(.45f + Mathf.Abs(x)%30/100f, .43f + Mathf.Abs(z)%25/100f, .42f);
                    Cube("Жилой_дом", new Vector3(x,h/2,z), new Vector3(20,h,24), Make("House_"+x+"_"+z,c), root);
                }
            }
            Cube("Площадь_города", new Vector3(0,.02f,32), new Vector3(38,.1f,24), concrete, root);
            Cube("Газон_сквер", new Vector3(0,.08f,-32), new Vector3(34,.12f,22), grass, root);

            Landmark(root, "ПОЛИЦИЯ", "Полицейский", new Vector3(35,0,25), new Color(.12f,.22f,.48f));
            Landmark(root, "БОЛЬНИЦА", "Медик", new Vector3(-35,0,25), new Color(.8f,.8f,.82f));
            Landmark(root, "АВТОСЕРВИС", "Механик", new Vector3(35,0,-35), new Color(.8f,.48f,.12f));
            Landmark(root, "ТАКСОПАРК", "Таксист", new Vector3(-35,0,-35), new Color(.82f,.72f,.12f));
            Landmark(root, "МЭРИЯ", "Государственный служащий", new Vector3(0,0,105), new Color(.55f,.42f,.3f));

            var setup = new GameObject("RP_Player_Setup");
            setup.AddComponent<RPPlayerSetup>();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/Stage1.unity");
            Debug.Log("Novograd Stage 2 city added. This is a local prototype, not multiplayer.");
        }
    }
}
