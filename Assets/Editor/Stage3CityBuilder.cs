using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GrandLife {
    public static class Stage3CityBuilder {
        static Material Mat(string name, Color color) {
            if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
            string path="Assets/Materials/"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){var sh=Shader.Find("Universal Render Pipeline/Lit"); if(sh==null)sh=Shader.Find("Standard"); m=new Material(sh); AssetDatabase.CreateAsset(m,path);}
            m.color=color; if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",color); EditorUtility.SetDirty(m); return m;
        }
        static GameObject Cube(string n, Vector3 p, Vector3 s, Material m, Transform root){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(root);g.transform.position=p;g.transform.localScale=s;g.GetComponent<Renderer>().sharedMaterial=m;return g;}
        static void Sign(GameObject building,string text){var go=new GameObject("Вывеска_"+text);go.transform.SetParent(building.transform,false);go.transform.localPosition=new Vector3(0,.1f,-.51f);var t=go.AddComponent<TextMesh>();t.text=text;t.fontSize=48;t.characterSize=.2f;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=Color.white;}
        static void District(Transform root,string name,Vector3 center,Color c,Material road,Material building){
            Cube("Район_"+name+"_площадь",center+Vector3.down*.02f,new Vector3(70,.12f,70),road,root);
            for(int i=0;i<4;i++){float dx=(i%2==0?-19:19),dz=(i<2?-19:19);var b=Cube(name+"_дом_"+(i+1),center+new Vector3(dx,6,dz),new Vector3(15,12,15),building,root);Sign(b,name);}
        }
        static GameObject Landmark(Transform root,string name,Vector3 p,Color color){var b=Cube(name,p+Vector3.up*4,new Vector3(16,8,14),Mat("B_"+name,color),root);Sign(b,name);return b;}
        [MenuItem("GrandLife/Build Stage 3 Complete Prototype City")]
        public static void Build(){
            if(!SceneManager.GetActiveScene().IsValid())EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects,NewSceneMode.Single);
            var old=GameObject.Find("GrandLife_Stage3_World");if(old)Object.DestroyImmediate(old);
            var root=new GameObject("GrandLife_Stage3_World").transform;
            var asphalt=Mat("S3_Asphalt",new Color(.13f,.14f,.16f));var grass=Mat("S3_Grass",new Color(.16f,.34f,.18f));var concrete=Mat("S3_Concrete",new Color(.5f,.5f,.48f));var houses=Mat("S3_Houses",new Color(.55f,.49f,.42f));
            Cube("Главный_проспект",new Vector3(0,0,0),new Vector3(18,.15f,500),asphalt,root);Cube("Поперечная_дорога",new Vector3(0,.02f,0),new Vector3(500,.15f,18),asphalt,root);
            foreach(float x in new float[]{-100,-50,50,100})Cube("Дорога_района",new Vector3(x,.02f,0),new Vector3(10,.15f,500),asphalt,root);
            foreach(float z in new float[]{-100,-50,50,100})Cube("Дорога_района",new Vector3(0,.02f,z),new Vector3(500,.15f,10),asphalt,root);
            District(root,"Нотарка",new Vector3(-150,0,150),Color.gray,concrete,houses);
            District(root,"Батырево",new Vector3(150,0,150),Color.gray,concrete,houses);
            District(root,"Южный",new Vector3(-150,0,-150),Color.gray,concrete,houses);
            District(root,"Эдово",new Vector3(150,0,-150),Color.gray,concrete,houses);
            District(root,"АРЗ",new Vector3(0,0,210),Color.gray,concrete,houses);
            Cube("Центральная_стоянка",new Vector3(0,.08f,-45),new Vector3(65,.2f,48),concrete,root);
            Landmark(root,"АВТОРЫНОК",new Vector3(45,0,-45),new Color(.2f,.35f,.55f));
            Landmark(root,"НОТАРИУС",new Vector3(-150,0,115),new Color(.35f,.3f,.55f));
            Landmark(root,"МЭРИЯ_ПАСПОРТ",new Vector3(0,0,55),new Color(.55f,.4f,.25f));
            Landmark(root,"БОЛЬНИЦА_МЕДКАРТА",new Vector3(-45,0,55),new Color(.75f,.75f,.8f));
            Landmark(root,"БАНК",new Vector3(45,0,55),new Color(.2f,.48f,.3f));
            Landmark(root,"ПОЛИЦИЯ",new Vector3(100,0,55),new Color(.12f,.2f,.45f));
            Landmark(root,"МЧС",new Vector3(100,0,-55),new Color(.7f,.18f,.1f));
            Landmark(root,"КАЗИНО",new Vector3(-45,0,-55),new Color(.55f,.2f,.4f));
            Landmark(root,"АВТОСЕРВИС",new Vector3(45,0,-100),new Color(.7f,.4f,.15f));
            Landmark(root,"ТАКСОПАРК",new Vector3(-45,0,-100),new Color(.7f,.6f,.12f));
            Cube("Вертолетная_площадка",new Vector3(180,.12f,0),new Vector3(42,.25f,42),concrete,root);
            var hmark=Cube("H",new Vector3(180,.3f,0),new Vector3(12,.1f,12),Mat("HelipadMark",Color.yellow),root);
            var spawn=new GameObject("Точка_появления_машин_стоянка");spawn.transform.position=new Vector3(0,1,-45);spawn.transform.SetParent(root);
            var heli=new GameObject("Точка_появления_вертолета");heli.transform.position=new Vector3(180,3,0);heli.transform.SetParent(root);
            var systems=new GameObject("RP_World_Systems").AddComponent<RPWorldSystems>();systems.vehicleSpawn=spawn.transform;systems.helicopterSpawn=heli.transform;
            var player=GameObject.CreatePrimitive(PrimitiveType.Capsule);player.name="Player_Prototype";player.transform.position=new Vector3(0,1,0);player.AddComponent<CharacterController>();player.AddComponent<RPPlayerSetup>();
            if(Camera.main==null){var cam=new GameObject("Main Camera");cam.tag="MainCamera";cam.AddComponent<Camera>();cam.transform.position=new Vector3(0,15,-22);cam.transform.LookAt(player.transform);}
            var light=Object.FindObjectOfType<Light>();if(light==null){var l=new GameObject("Directional Light");light=l.AddComponent<Light>();light.type=LightType.Directional;light.transform.rotation=Quaternion.Euler(50,-30,0);}
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            if(!AssetDatabase.IsValidFolder("Assets/Scenes"))AssetDatabase.CreateFolder("Assets","Scenes");
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/Scenes/GrandLife_Stage3.unity");
            Debug.Log("Stage 3 prototype city generated. Online backend, full vehicle assets and flight physics are not included.");
        }
    }
}
