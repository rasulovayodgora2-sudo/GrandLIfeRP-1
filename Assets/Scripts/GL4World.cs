using System.Collections.Generic;
using UnityEngine;

namespace GrandLife {
    public class GL4PropertyMarker : MonoBehaviour {
        public string propertyId;
        public Renderer facade;
        public GameObject furniture;
        public GameObject premiumDetails;
        public void SetRenovation(int level){
            if(facade!=null)facade.sharedMaterial=GL4VisualFactory.Mat(level==0?new Color(.54f,.53f,.5f):level==1?new Color(.82f,.75f,.59f):level==2?new Color(.68f,.82f,.86f):new Color(.94f,.85f,.55f));
            if(furniture)furniture.SetActive(level>=1);
            if(premiumDetails)premiumDetails.SetActive(level>=3);
        }
    }
    public class GL4World : MonoBehaviour {
        public static GL4World Instance;
        public Transform garageSpawn, parkingSpawn, heliSpawn;
        public readonly Dictionary<string,GL4PropertyMarker> properties=new Dictionary<string,GL4PropertyMarker>();
        Transform root;
        static Color asphalt=new Color(.17f,.18f,.21f), concrete=new Color(.54f,.56f,.57f), green=new Color(.23f,.45f,.29f);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart(){if(FindObjectOfType<GL4World>()==null)new GameObject("GrandLife_Stage4_Bootstrap").AddComponent<GL4World>();}
        void Awake(){Instance=this;Build();}
        void Start(){if(GL4Game.Instance==null)new GameObject("GrandLife_Stage4_Gameplay").AddComponent<GL4Game>();}
        GameObject Box(string n,Vector3 pos,Vector3 size,Color c,bool collision=true){return GL4VisualFactory.Box(n,root,pos,size,c,collision);}
        void Sign(Transform t,string name,float y){GL4VisualFactory.Label(t,name,new Vector3(0,y,-.62f),1.6f,Color.white);}
        void District(string name,Vector3 center,Color color){
            Box("Площадь_"+name,center+Vector3.down*.13f,new Vector3(85,.2f,85),concrete);
            for(int i=0;i<6;i++){
                Vector3 p=center+new Vector3((i%3-1)*24,0,(i/3==0?-24:24));
                float h=8+i%3*3;
                GameObject b=Box(name+"_дом_"+(i+1),p+Vector3.up*h*.5f,new Vector3(14,h,14),color);
                GL4VisualFactory.Label(b.transform,name,new Vector3(0,.5f,-.55f),1.2f,Color.white);
                for(int f=0;f<3;f++)for(int w=-1;w<=1;w++)
                    GL4VisualFactory.Box("Окно",b.transform,new Vector3(w*.28f,-.35f+f*.33f,-.505f),new Vector3(.15f,.16f,.02f),new Color(.38f,.65f,.78f));
            }
        }
        void Landmark(string name,Vector3 pos,Color c){
            GameObject b=Box(name,pos+Vector3.up*3.6f,new Vector3(16,7.2f,13),c);
            GL4VisualFactory.Label(b.transform,name,new Vector3(0,.36f,-.55f),1.4f,Color.white);
            for(int i=-2;i<=2;i++)GL4VisualFactory.Box("Витрина",b.transform,new Vector3(i*.17f,-.15f,-.51f),new Vector3(.13f,.3f,.025f),new Color(.35f,.63f,.8f));
        }
        void SpawnMarker(string n,Vector3 p,out Transform tr){GameObject g=new GameObject(n);g.transform.SetParent(root,false);g.transform.position=p;tr=g.transform;}
        void Property(string id,string label,Vector3 pos,bool house,bool elite){
            GameObject holder=new GameObject("Недвижимость_"+id);holder.transform.SetParent(root,false);holder.transform.position=pos;
            GL4PropertyMarker marker=holder.AddComponent<GL4PropertyMarker>();marker.propertyId=id;
            GL4VisualFactory.Box("Участок",holder.transform,new Vector3(0,-.1f,0),new Vector3(21,.2f,20),new Color(.4f,.5f,.39f),true);
            GameObject body=GL4VisualFactory.Box("Фасад",holder.transform,new Vector3(0,house?3.5f:6f,0),new Vector3(10,house?7f:12f,8),concrete,true);
            marker.facade=body.GetComponent<Renderer>();
            GL4VisualFactory.Box("Крыша",holder.transform,new Vector3(0,house?7.2f:12.3f,0),new Vector3(11,.55f,9),elite?new Color(.22f,.24f,.27f):new Color(.43f,.23f,.2f));
            for(int i=-1;i<=1;i++)for(int f=0;f<(house?2:4);f++){
                GL4VisualFactory.Box("Окно",holder.transform,new Vector3(i*2.5f,2.4f+f*2.6f,-4.04f),new Vector3(1.35f,1.45f,.08f),new Color(.35f,.65f,.8f));
            }
            GL4VisualFactory.Label(holder.transform,label,new Vector3(0,house?7.9f:13.1f,-4.4f),1.2f,Color.white);
            GL4VisualFactory.Box("Гараж_ворота",holder.transform,new Vector3(7.1f,1.4f,0),new Vector3(4.6f,2.8f,4),new Color(.35f,.36f,.39f));
            GameObject furn=new GameObject("Ремонт_мебель");furn.transform.SetParent(holder.transform,false);
            GL4VisualFactory.Box("Диван",furn.transform,new Vector3(-2,1.1f,-4.5f),new Vector3(2.6f,.9f,1),new Color(.23f,.43f,.65f));
            GL4VisualFactory.Box("Стол",furn.transform,new Vector3(2,.65f,-4.6f),new Vector3(2,.25f,1.2f),new Color(.47f,.3f,.17f));
            marker.furniture=furn;
            GameObject premium=new GameObject("Элитный_ремонт");premium.transform.SetParent(holder.transform,false);
            GL4VisualFactory.Box("Бассейн",premium.transform,new Vector3(-5.8f,.12f,5.8f),new Vector3(5,.2f,4),new Color(.2f,.72f,.86f));
            GL4VisualFactory.Box("Золотой_декор",premium.transform,new Vector3(0,house?6.9f:12.1f,-4.1f),new Vector3(8,.15f,.2f),new Color(.94f,.75f,.22f));
            marker.premiumDetails=premium;marker.SetRenovation(0);properties.Add(id,marker);
        }
        void Build(){
            root=new GameObject("Новоград_Stage4_Мир").transform;root.SetParent(transform,false);
            Box("Земля",new Vector3(0,-.8f,0),new Vector3(650,1.5f,650),green);
            for(int i=-3;i<=3;i++){
                Box("Улица_Север_Юг",new Vector3(i*50,.015f,0),new Vector3(13,.13f,610),asphalt);
                Box("Улица_Запад_Восток",new Vector3(0,.02f,i*50),new Vector3(610,.13f,13),asphalt);
            }
            District("Нотарка",new Vector3(-180,0,165),new Color(.59f,.52f,.47f));
            District("Батырево",new Vector3(175,0,165),new Color(.52f,.55f,.62f));
            District("Южный",new Vector3(-180,0,-165),new Color(.63f,.57f,.47f));
            District("Эдово",new Vector3(175,0,-165),new Color(.5f,.58f,.52f));
            District("АРЗ",new Vector3(0,0,220),new Color(.52f,.48f,.6f));
            District("Стоянка",new Vector3(0,0,-180),new Color(.58f,.57f,.54f));
            Landmark("БАНК",new Vector3(45,0,65),new Color(.22f,.5f,.35f));
            Landmark("МЭРИЯ_ПАСПОРТ",new Vector3(0,0,65),new Color(.6f,.46f,.28f));
            Landmark("БОЛЬНИЦА_МЕДКАРТА",new Vector3(-45,0,65),new Color(.68f,.7f,.75f));
            Landmark("АВТОРЫНОК",new Vector3(60,0,-60),new Color(.23f,.36f,.56f));
            Landmark("АУКЦИОН",new Vector3(-60,0,-60),new Color(.56f,.37f,.26f));
            Landmark("ПОСТАМАТ",new Vector3(30,0,25),new Color(.9f,.57f,.15f));
            Landmark("ПОЛИЦИЯ",new Vector3(120,0,65),new Color(.18f,.25f,.55f));
            Landmark("МЧС",new Vector3(120,0,-65),new Color(.72f,.2f,.12f));
            Landmark("КАЗИНО",new Vector3(-120,0,-65),new Color(.48f,.22f,.5f));
            Landmark("ТАКСОПАРК",new Vector3(-60,0,-110),new Color(.8f,.65f,.18f));
            Landmark("АВТОСЕРВИС",new Vector3(60,0,-110),new Color(.63f,.39f,.18f));
            Landmark("НОТАРИУС",new Vector3(-120,0,115),new Color(.35f,.35f,.58f));
            Box("Стоянка_асфальт",new Vector3(0,.15f,-40),new Vector3(50,.2f,38),asphalt);
            Box("Гараж_общий",new Vector3(20,2,-40),new Vector3(13,4,9),new Color(.38f,.39f,.41f));
            GL4VisualFactory.Label(root,"ГАРАЖ / SPAWN",new Vector3(20,5,-45),2,Color.white);
            SpawnMarker("Гараж_спавн",new Vector3(20,0,-28),out garageSpawn);
            SpawnMarker("Стоянка_спавн",new Vector3(0,0,-40),out parkingSpawn);
            Box("Вертолётная_площадка",new Vector3(205,.13f,0),new Vector3(46,.2f,46),concrete);
            GL4VisualFactory.Label(root,"H / HELIPAD",new Vector3(205,1,0),2,Color.yellow);
            SpawnMarker("Вертолёт_спавн",new Vector3(205,0,0),out heliSpawn);
            Property("apt_economy","Квартира Эконом",new Vector3(-92,0,145),false,false);
            Property("apt_middle","Квартира Комфорт",new Vector3(-92,0,195),false,false);
            Property("apt_elite","Пентхаус Элит",new Vector3(88,0,145),false,true);
            Property("house_economy","Дом Эконом",new Vector3(-90,0,-145),true,false);
            Property("house_middle","Коттедж Комфорт",new Vector3(88,0,-145),true,false);
            Property("house_elite","Вилла Элит",new Vector3(88,0,195),true,true);
            GameObject avatar=new GameObject("GL4_Player");avatar.transform.position=new Vector3(0,.12f,0);
            CharacterController cc=avatar.AddComponent<CharacterController>();cc.height=2.4f;cc.center=new Vector3(0,1.2f,0);cc.radius=.35f;
            avatar.AddComponent<GL4AvatarMove>();
            GameObject skin=GL4VisualFactory.Avatar("Стрит");skin.transform.SetParent(avatar.transform,false);
            Camera cam=Camera.main;
            if(cam==null){GameObject go=new GameObject("Main Camera");go.tag="MainCamera";cam=go.AddComponent<Camera>();}
            cam.transform.position=new Vector3(0,7,-10);cam.gameObject.AddComponent<GL4FollowCamera>().target=avatar.transform;
            if(FindObjectOfType<Light>()==null){GameObject l=new GameObject("Солнце");Light light=l.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.1f;l.transform.rotation=Quaternion.Euler(48,-30,0);}
            RenderSettings.ambientLight=new Color(.7f,.73f,.77f);
        }
    }
}
