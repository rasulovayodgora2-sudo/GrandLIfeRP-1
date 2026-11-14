using System.Collections.Generic;
using UnityEngine;

namespace GrandLife {
    // All models are ORIGINAL procedural low-poly models, not imported commercial assets.
    public static class GL4VisualFactory {
        static readonly Dictionary<string, Material> mats = new Dictionary<string, Material>();
        public static Material Mat(Color c) {
            string key = ColorUtility.ToHtmlStringRGBA(c);
            Material m;
            if (mats.TryGetValue(key, out m)) return m;
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Standard");
            if (s == null) s = Shader.Find("Unlit/Color");
            m = new Material(s); m.color = c;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            mats.Add(key, m); return m;
        }
        public static GameObject Part(PrimitiveType type, string name, Transform parent, Vector3 p, Vector3 scale, Color c, bool collider = false) {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = p; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = Mat(c);
            if (!collider) Object.Destroy(go.GetComponent<Collider>());
            return go;
        }
        public static GameObject Box(string name, Transform parent, Vector3 p, Vector3 size, Color c, bool collider = false) {
            return Part(PrimitiveType.Cube, name, parent, p, size, c, collider);
        }
        public static void Label(Transform parent, string text, Vector3 p, float size, Color c) {
            GameObject go = new GameObject("Табличка_" + text);
            go.transform.SetParent(parent, false); go.transform.localPosition = p;
            TextMesh mesh = go.AddComponent<TextMesh>(); mesh.text = text; mesh.fontSize = 64;
            mesh.characterSize = size / 10f; mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center; mesh.color = c;
        }
        static readonly Color tire = new Color(.055f,.06f,.075f);
        static readonly Color glass = new Color(.13f,.33f,.44f);
        static readonly Color steel = new Color(.65f,.69f,.74f);
        static void Wheel(Transform root, float x, float z, float y, float radius, float width) {
            GameObject w = Part(PrimitiveType.Cylinder,"Шина",root,new Vector3(x,y,z),new Vector3(radius*2,width,radius*2),tire);
            w.transform.localRotation = Quaternion.Euler(0,0,90);
            GameObject hub = Part(PrimitiveType.Cylinder,"Литой_диск",root,new Vector3(x+(x>0?.06f:-.06f),y,z),new Vector3(radius*1.14f,width*.65f,radius*1.14f),steel);
            hub.transform.localRotation = Quaternion.Euler(0,0,90);
        }
        public static GameObject Vehicle(string model, string kind, Color paint, Vector3 pos) {
            GameObject root = new GameObject(model + "_3D"); root.transform.position = pos;
            if (kind == "Мотоцикл") { Motorcycle(root.transform,paint); }
            else if (kind == "Вертолёт") { Helicopter(root.transform,paint); root.AddComponent<GL4RotorSpin>(); }
            else { Car(root.transform,kind,paint); }
            GL4Drive drive = root.AddComponent<GL4Drive>(); drive.kind = kind;
            return root;
        }
        static void Car(Transform root, string kind, Color paint) {
            bool suv = kind == "Внедорожник" || kind == "Фургон";
            bool sport = kind == "Спорткар";
            bool van = kind == "Фургон";
            float bodyY = suv ? .9f : .72f, bodyH = suv ? .92f : .64f;
            float length = van ? 5.2f : sport ? 4.7f : 4.35f;
            Box("Кузов_обтекаемый",root,new Vector3(0,bodyY,0),new Vector3(1.95f,bodyH,length),paint);
            Box("Капот",root,new Vector3(0,bodyY+.24f,1.35f),new Vector3(1.78f,.23f,1.42f),paint);
            float roofY = suv ? 1.77f : 1.46f;
            Box("Крыша",root,new Vector3(0,roofY,-.35f),new Vector3(1.72f,van?1.15f:.77f,van?2.9f:2.25f),paint);
            Box("Лобовое_стекло",root,new Vector3(0,roofY+.08f,.78f),new Vector3(1.56f,.59f,.08f),glass);
            Box("Заднее_стекло",root,new Vector3(0,roofY+.08f,-1.37f),new Vector3(1.53f,.57f,.08f),glass);
            for (int side=-1;side<=1;side+=2) {
                Box("Боковые_стекла",root,new Vector3(side*.89f,roofY+.09f,-.32f),new Vector3(.065f,.53f,van?2.5f:1.84f),glass);
                Box("Ручка_двери",root,new Vector3(side*1.005f,bodyY+.2f,-.24f),new Vector3(.045f,.055f,.23f),steel);
                Box("Зеркало",root,new Vector3(side*1.05f,roofY-.27f,.72f),new Vector3(.25f,.16f,.32f),tire);
                Wheel(root,side*1.03f,1.38f,.41f,suv?.48f:.41f,.25f);
                Wheel(root,side*1.03f,-1.35f,.41f,suv?.48f:.41f,.25f);
                Box("Фара_передняя",root,new Vector3(side*.7f,bodyY+.14f,length*.5f+.02f),new Vector3(.43f,.18f,.08f),new Color(.94f,.92f,.72f));
                Box("Фонарь_задний",root,new Vector3(side*.7f,bodyY+.14f,-length*.5f-.02f),new Vector3(.43f,.19f,.08f),new Color(.82f,.08f,.07f));
            }
            Box("Решётка_радиатора",root,new Vector3(0,bodyY-.08f,length*.5f+.05f),new Vector3(.9f,.24f,.06f),tire);
            Box("Бампер_передний",root,new Vector3(0,bodyY-.28f,length*.5f+.04f),new Vector3(1.9f,.14f,.16f),tire);
            Box("Бампер_задний",root,new Vector3(0,bodyY-.28f,-length*.5f-.04f),new Vector3(1.9f,.14f,.16f),tire);
            if (sport) Box("Спойлер",root,new Vector3(0,1.23f,-2.0f),new Vector3(1.8f,.1f,.4f),tire);
            if (kind == "Такси") Box("Шашечки_такси",root,new Vector3(0,roofY+.45f,-.2f),new Vector3(.55f,.28f,.32f),new Color(1f,.85f,.12f));
        }
        static void Motorcycle(Transform root,Color paint) {
            for(int i=-1;i<=1;i+=2){
                GameObject wheel=Part(PrimitiveType.Cylinder,"Мото_колесо",root,new Vector3(0,.57f,i*1.12f),new Vector3(1.06f,.25f,1.06f),tire);
                wheel.transform.localRotation=Quaternion.Euler(0,0,90);
                GameObject disc=Part(PrimitiveType.Cylinder,"Диск",root,new Vector3(.15f,.57f,i*1.12f),new Vector3(.72f,.28f,.72f),steel);
                disc.transform.localRotation=Quaternion.Euler(0,0,90);
            }
            Box("Рама",root,new Vector3(0,.95f,0),new Vector3(.32f,.28f,2.1f),steel);
            Part(PrimitiveType.Sphere,"Бензобак",root,new Vector3(0,1.28f,.25f),new Vector3(.78f,.48f,.88f),paint);
            Box("Сиденье",root,new Vector3(0,1.31f,-.52f),new Vector3(.65f,.16f,1.0f),tire);
            Box("Руль",root,new Vector3(0,1.69f,.92f),new Vector3(1.02f,.11f,.12f),tire);
            Box("Фара",root,new Vector3(0,1.22f,1.3f),new Vector3(.45f,.28f,.12f),new Color(1,.93f,.7f));
            Box("Выхлоп",root,new Vector3(.45f,.72f,-.65f),new Vector3(.14f,.14f,1.2f),steel);
        }
        static void Helicopter(Transform root,Color paint) {
            Part(PrimitiveType.Sphere,"Кабина_вертолета",root,new Vector3(0,2.05f,.45f),new Vector3(2.45f,1.9f,3.45f),paint);
            Part(PrimitiveType.Sphere,"Панорамное_стекло",root,new Vector3(0,2.22f,1.68f),new Vector3(1.86f,1.17f,.48f),glass);
            Box("Хвостовая_балка",root,new Vector3(0,2.18f,-2.6f),new Vector3(.44f,.43f,3.8f),paint);
            Box("Киль",root,new Vector3(0,2.73f,-4.23f),new Vector3(.15f,1.45f,.9f),paint);
            for(int side=-1;side<=1;side+=2){
                Box("Полозья",root,new Vector3(side*1.05f,.67f,0),new Vector3(.15f,.15f,3.9f),steel);
                Box("Стойка",root,new Vector3(side*1.05f,1.12f,.8f),new Vector3(.13f,.88f,.13f),steel);
                Box("Стойка",root,new Vector3(side*1.05f,1.12f,-.8f),new Vector3(.13f,.88f,.13f),steel);
            }
            Box("Мачта_винта",root,new Vector3(0,3.23f,0),new Vector3(.23f,.55f,.23f),steel);
            GameObject rotor=new GameObject("MainRotor");rotor.transform.SetParent(root,false);rotor.transform.localPosition=new Vector3(0,3.53f,0);
            Box("Лопасть_1",rotor.transform,Vector3.zero,new Vector3(8f,.11f,.32f),tire);
            Box("Лопасть_2",rotor.transform,Vector3.zero,new Vector3(.32f,.11f,8f),tire);
            GameObject tail=new GameObject("TailRotor");tail.transform.SetParent(root,false);tail.transform.localPosition=new Vector3(.3f,2.62f,-4.2f);
            Box("Хвостовой_винт",tail.transform,Vector3.zero,new Vector3(.08f,1.55f,.15f),tire);
        }
        public static GameObject Avatar(string skin) {
            GameObject root = new GameObject("Персонаж_"+skin);
            Color jacket = skin == "Кибер" ? new Color(.18f,.72f,.91f) : skin == "Люкс" ? new Color(.85f,.68f,.18f) : skin == "Тактик" ? new Color(.17f,.28f,.21f) : skin == "Неон" ? new Color(.77f,.16f,.7f) : new Color(.17f,.31f,.68f);
            Color pants = skin == "Люкс" ? new Color(.22f,.19f,.2f) : new Color(.1f,.13f,.19f);
            Color face = new Color(.79f,.56f,.4f);
            Box("Куртка",root.transform,new Vector3(0,1.28f,0),new Vector3(.68f,.78f,.35f),jacket);
            Part(PrimitiveType.Sphere,"Голова",root.transform,new Vector3(0,1.96f,0),new Vector3(.43f,.49f,.42f),face);
            Box("Причёска",root.transform,new Vector3(0,2.23f,-.02f),new Vector3(.47f,.16f,.45f),pants);
            for(int side=-1;side<=1;side+=2){
                Box("Рука",root.transform,new Vector3(side*.48f,1.25f,0),new Vector3(.22f,.75f,.24f),jacket);
                Box("Нога",root.transform,new Vector3(side*.2f,.52f,0),new Vector3(.26f,.92f,.29f),pants);
                Box("Кроссовок",root.transform,new Vector3(side*.2f,.11f,.1f),new Vector3(.3f,.2f,.46f),new Color(.9f,.9f,.92f));
            }
            return root;
        }
    }
    public class GL4RotorSpin : MonoBehaviour {
        Transform mainRotor, tailRotor;
        void Start(){ mainRotor=transform.Find("MainRotor");tailRotor=transform.Find("TailRotor"); }
        void Update(){if(mainRotor)mainRotor.Rotate(0,950*Time.deltaTime,0,Space.Self);if(tailRotor)tailRotor.Rotate(600*Time.deltaTime,0,0,Space.Self);}
    }
}
