using System;
using System.Collections.Generic;
using UnityEngine;

namespace GrandLife {
    [Serializable] public class GL5CarTune { public string id; public int engine, turbo, gearbox, suspension, brakes, style; }
    [Serializable] public class GL5Profile { public int grandCoins; public List<string> exclusiveCars=new List<string>(); public List<string> skins=new List<string>{"Street","Neon","Royal","Cyber","Arctic","Inferno"}; public List<GL5CarTune> tunes=new List<GL5CarTune>(); public int casesOpened; }
    public class GL5PremiumSystems : MonoBehaviour {
        public static GL5PremiumSystems Instance;
        public GL5Profile profile; public bool visible=true; int tab; Vector2 scroll; string adminPin=""; string grantAmount="1000"; bool adminUnlocked; string message="Добро пожаловать в премиум-центр Grand Life RP";
        readonly string[] carNames={"Apex V12","Nebula GT","Phantom RS","Vortex X","Titan SUV","Raven S","Aurora E","Drift K","Falcon R","Monarch Limo","Storm Pickup","Nomad Rally","Pulse Roadster","Eclipse Coupe","Nova Hyper","Boreal 4x4","Raptor Track","Sable Classic","Vector Sport","Zenith One","Cyclone RR","Mirage GT","Atlas Truck","Comet S","Spectre EV","Onyx Touring","Tempest","Glacier X","Inferno R","Quantum 9","AeroBike 900","ShadowBike","Volt Scooter","Rally Quad","Desert ATV","Falcon Heli","Orion Heli","Atlas Rescue","Valkyrie Heli","SkyLance"};
        readonly string[] skins={"Street","Neon","Royal","Cyber","Arctic","Inferno","Chrome","Phantom","Galaxy","Toxic","Samurai","Pilot","Racer","Mecha Core","Solar","Lunar"};
        void Awake(){Instance=this;Load();}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Bootstrap(){if(FindObjectOfType<GL5PremiumSystems>()==null){var go=new GameObject("GL5_PremiumSystems");go.AddComponent<GL5PremiumSystems>();}}
        void Load(){string s=PlayerPrefs.GetString("GL5_PREMIUM"," ");try{profile=JsonUtility.FromJson<GL5Profile>(s);}catch{}if(profile==null)profile=new GL5Profile();if(profile.exclusiveCars==null)profile.exclusiveCars=new List<string>();if(profile.skins==null)profile.skins=new List<string>();if(profile.tunes==null)profile.tunes=new List<GL5CarTune>();}
        void Save(){PlayerPrefs.SetString("GL5_PREMIUM",JsonUtility.ToJson(profile));PlayerPrefs.Save();}
        bool Spend(int n){if(profile.grandCoins<n){message="Недостаточно Grand Coins";return false;}profile.grandCoins-=n;Save();return true;}
        GL5CarTune Tune(string id){var t=profile.tunes.Find(x=>x.id==id);if(t==null){t=new GL5CarTune{id=id};profile.tunes.Add(t);}return t;}
        string ActiveId(){var g=GL4Game.Instance;return g!=null&&g.activeVehicle?g.activeVehicle.name:"garage_vehicle";}
        void Upgrade(string part){var g=GL4Game.Instance;if(g==null||g.activeVehicle==null){message="Сначала выведи машину из гаража";return;}var t=Tune(ActiveId());int level=part=="engine"?t.engine:part=="turbo"?t.turbo:part=="gearbox"?t.gearbox:part=="suspension"?t.suspension:part=="brakes"?t.brakes:t.style;int cost=(level+1)*250;
            if(!Spend(cost))return;if(part=="engine")t.engine++;else if(part=="turbo")t.turbo++;else if(part=="gearbox")t.gearbox++;else if(part=="suspension")t.suspension++;else if(part=="brakes")t.brakes++;else t.style++;
            var drive=g.activeVehicle.GetComponent<GL4Drive>();if(drive!=null)drive.speed=Mathf.Max(drive.speed,0)*(1f+0.04f*(t.engine+t.turbo+t.gearbox));Save();message="Установлено: "+part+" · уровень "+(level+1)+". Цена "+cost+" GC";
        }
        void OpenCase(int tier){int cost=tier==0?100:tier==1?350:900;if(!Spend(cost))return;profile.casesOpened++;int roll=UnityEngine.Random.Range(0,100);string reward;
            if(roll<5||tier==2&&roll<28){string car=carNames[UnityEngine.Random.Range(0,carNames.Length)];if(!profile.exclusiveCars.Contains(car))profile.exclusiveCars.Add(car);reward="ЭКСКЛЮЗИВНЫЙ ТРАНСПОРТ: "+car;}
            else if(roll<42){string skin=skins[UnityEngine.Random.Range(0,skins.Length)];if(!profile.skins.Contains(skin))profile.skins.Add(skin);reward="Редкий скин: "+skin;}
            else{int gc=tier==0?20:tier==1?75:220;profile.grandCoins+=gc;reward="Возврат бонусом: "+gc+" GC";}
            Save();message="Кейс открыт! "+reward;
        }
        void SpawnExclusive(string car){var g=GL4Game.Instance;if(g==null){message="Игровой мир ещё загружается";return;}if(!profile.exclusiveCars.Contains(car)){message="Транспорт не получен";return;}// local prototype spawn; server ownership validation is not present yet
            if(g.activeVehicle)Destroy(g.activeVehicle);Vector3 p=GameObject.Find("GL4_Player")?GameObject.Find("GL4_Player").transform.position+Vector3.forward*5:Vector3.zero;
            var obj=GL4VisualFactory.Vehicle(car,"Спорткар",new Color(UnityEngine.Random.value,.35f,UnityEngine.Random.value),p);g.activeVehicle=obj;message="Эксклюзивный транспорт выставлен: "+car;
        }
        void OnGUI(){if(!visible)return;float scale=Mathf.Clamp(Screen.width/1200f,.55f,1f);GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));float w=Screen.width/scale,h=Screen.height/scale;GUI.Box(new Rect(w-355,12,340,70),"GRAND COINS: "+profile.grandCoins+" GC\nКейсов открыто: "+profile.casesOpened);if(GUI.Button(new Rect(w-355,86,340,38),"Премиум-центр GC"))tab=(tab+1)%4;
            if(GUI.Button(new Rect(w-355,128,340,34),"Скрыть / показать премиум-центр"))visible=false;
            GUI.Box(new Rect(12,h-390,Mathf.Min(540,w-24),375),"GRAND LIFE RP · PREMIUM SYSTEMS");GUILayout.BeginArea(new Rect(24,h-360,Mathf.Min(516,w-48),335));GUILayout.BeginHorizontal();string[] names={"Тюнинг","Кейсы","Эксклюзивы","Скины"};for(int i=0;i<4;i++)if(GUILayout.Button(names[i],GUILayout.Height(30)))tab=i;GUILayout.EndHorizontal();scroll=GUILayout.BeginScrollView(scroll);
            if(tab==0){GUILayout.Label("Тюнинг активного транспорта · цены в GC");if(GUILayout.Button("Двигатель +1 · 250 GC"))Upgrade("engine");if(GUILayout.Button("Турбина +1 · 250 GC"))Upgrade("turbo");if(GUILayout.Button("Коробка передач +1 · 250 GC"))Upgrade("gearbox");if(GUILayout.Button("Подвеска +1 · 250 GC"))Upgrade("suspension");if(GUILayout.Button("Тормоза +1 · 250 GC"))Upgrade("brakes");if(GUILayout.Button("Обвес / стиль +1 · 250 GC"))Upgrade("style");}
            else if(tab==1){GUILayout.Label("Кейсы открываются только за игровую валюту GC");if(GUILayout.Button("Street Case · 100 GC"))OpenCase(0);if(GUILayout.Button("Elite Case · 350 GC"))OpenCase(1);if(GUILayout.Button("Mythic Transformer Case · 900 GC"))OpenCase(2);GUILayout.Label("Шанс эксклюзивного транспорта зависит от типа кейса. Никаких реальных платежей в прототипе нет.");}
            else if(tab==2){GUILayout.Label("Коллекция эксклюзивного транспорта");foreach(string c in profile.exclusiveCars)if(GUILayout.Button("Вывести: "+c))SpawnExclusive(c);if(profile.exclusiveCars.Count==0)GUILayout.Label("Пока пусто. Открой кейс.");}
            else if(tab==3){GUILayout.Label("Коллекция скинов");foreach(string s in profile.skins)GUILayout.Label("◆ "+s);}
            if(GUILayout.Button("Админ-панель выдачи GC (локальный тест)")){adminUnlocked=!adminUnlocked;}
            if(adminUnlocked){GUILayout.Label("Локальная тестовая выдача. В онлайн-игре выдачу обязан проверять сервер.");adminPin=GUILayout.TextField(adminPin);grantAmount=GUILayout.TextField(grantAmount);if(GUILayout.Button("Подтвердить тест-выдачу 2468")){int n;if(adminPin=="2468"&&int.TryParse(grantAmount,out n)&&n>0&&n<=1000000){profile.grandCoins+=n;Save();message="Администратор выдал "+n+" GC (локальный тест)";}else message="Неверный тестовый PIN или сумма";}}
            GUILayout.EndScrollView();GUILayout.EndArea();GUI.Label(new Rect(20,h-42,Mathf.Min(850,w-40),32),message);
        }
    }
}
