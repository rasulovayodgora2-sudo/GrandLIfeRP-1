using UnityEngine;
using System;

namespace GrandLife {
    [Serializable]
    public class RPVehicleSpec {
        public string modelName;
        public string className;
        public int price;
        public Color color;
        public RPVehicleSpec(string n, string c, int p, Color col) { modelName=n; className=c; price=p; color=col; }
    }

    // Local single-player prototype. All balances and permissions must move to a trusted server for online play.
    public class RPWorldSystems : MonoBehaviour {
        public static RPWorldSystems Instance;
        public string characterName = "Новый игрок";
        public int cash = 5000, bank = 10000, level = 1, xp = 0;
        public bool hasPassport = false, hasMedicalCard = false, adminPanelOpen = false;
        public string faction = "Нет фракции", job = "Безработный", notice = "Добро пожаловать в Новоград!";
        public int casinoChips = 1000;
        public GameObject activeVehicle;
        public Transform vehicleSpawn;
        public Transform helicopterSpawn;
        public RPVehicleSpec[] vehicles;
        Vector2 scroll;
        bool showMenu = true;
        float shiftSeconds;
        string[] factions = {"Полиция", "МЧС", "Медики", "Правительство", "Механики", "Такси"};

        void Awake() {
            Instance = this;
            vehicles = new RPVehicleSpec[] {
                new RPVehicleSpec("Sedan S1", "Седан", 0, new Color(.2f,.35f,.65f)),
                new RPVehicleSpec("Coupe R", "Купе", 45000, new Color(.65f,.12f,.12f)),
                new RPVehicleSpec("SUV X", "Внедорожник", 85000, new Color(.15f,.15f,.17f)),
                new RPVehicleSpec("Sport GT", "Спорткар", 160000, new Color(.75f,.55f,.08f)),
                new RPVehicleSpec("Van Cargo", "Фургон", 60000, new Color(.7f,.7f,.68f)),
                new RPVehicleSpec("Taxi M", "Такси", 55000, new Color(.85f,.72f,.12f))
            };
        }
        void Update() {
            if (Input.GetKeyDown(KeyCode.F1)) adminPanelOpen = !adminPanelOpen;
            if (job != "Безработный") {
                shiftSeconds += Time.deltaTime;
                if (shiftSeconds >= 60f) { shiftSeconds=0; cash += 250; xp += 10; if (xp >= level*100) { xp-=level*100; level++; } notice="Начислена зарплата: 250 ₽"; }
            }
        }
        void OnGUI() {
            GUI.skin.box.fontSize=16; GUI.skin.button.fontSize=15; GUI.skin.label.fontSize=15;
            GUI.Box(new Rect(12,12,310,135), "GRAND LIFE RP — ПРОТОТИП\n"+characterName+"\nУровень "+level+" | Опыт "+xp+"\nНаличные: "+cash.ToString("N0")+" ₽\nБанк: "+bank.ToString("N0")+" ₽\nРабота: "+job+" | Фракция: "+faction);
            GUI.Label(new Rect(16,150,Screen.width-32,28), notice);
            if (GUI.Button(new Rect(12,185,145,42), showMenu ? "Скрыть меню" : "Меню")) showMenu=!showMenu;
            if (!showMenu) return;
            float x=12, y=235, w=Mathf.Min(350,Screen.width-24), h=Mathf.Min(470,Screen.height-250);
            GUI.Box(new Rect(x,y,w,h), "ДЕЙСТВИЯ И ГОРОД");
            GUILayout.BeginArea(new Rect(x+10,y+30,w-20,h-40)); scroll=GUILayout.BeginScrollView(scroll);
            GUILayout.Label("ДОКУМЕНТЫ");
            if (GUILayout.Button(hasPassport?"Паспорт получен":"Получить паспорт — мэрия")) { hasPassport=true; notice="Паспорт оформлен (локально)."; }
            if (GUILayout.Button(hasMedicalCard?"Медкарта получена":"Получить медкарту — больница")) { hasMedicalCard=true; notice="Медицинская карта оформлена (локально)."; }
            GUILayout.Space(5); GUILayout.Label("РАБОТА И ДЕНЬГИ");
            foreach(string j in new string[]{"Курьер","Таксист","Механик","Работник порта","Инкассатор","Крупье"}) if(GUILayout.Button("Начать работу: "+j)) { job=j; shiftSeconds=0; notice="Ты начал смену: "+j+". Зарплата начисляется раз в минуту."; }
            if(GUILayout.Button("Закончить смену")){job="Безработный";notice="Смена завершена.";}
            GUILayout.Label("БАНК");
            if(GUILayout.Button("Положить 500 ₽ в банк") && cash>=500){cash-=500;bank+=500;notice="Внесено 500 ₽.";}
            if(GUILayout.Button("Снять 500 ₽ из банка") && bank>=500){bank-=500;cash+=500;notice="Снято 500 ₽.";}
            GUILayout.Label("ФРАКЦИИ");
            foreach(string f in factions) if(GUILayout.Button("Подать заявку: "+f)){faction=f;notice="Заявка в фракцию "+f+" принята в прототипе.";}
            GUILayout.Label("КАЗИНО / АВТОРЫНОК");
            if(GUILayout.Button("Сыграть в казино (ставка 100)") && cash>=100){cash-=100; if(UnityEngine.Random.value>.48f){cash+=200;notice="Выигрыш 200 ₽!";}else notice="Ставка проиграна.";}
            if(GUILayout.Button("Купить 100 фишек за 100 ₽") && cash>=100){cash-=100;casinoChips+=100;notice="Фишки куплены. Всего: "+casinoChips;}
            GUILayout.Label("СТОЯНКА — ВЫБЕРИ МАШИНУ");
            foreach(var v in vehicles) if(GUILayout.Button(v.modelName+" / "+v.className+" — "+(v.price==0?"бесплатно":v.price.ToString("N0")+" ₽"))) SpawnVehicle(v);
            if(GUILayout.Button("Убрать текущую машину")){if(activeVehicle)Destroy(activeVehicle);notice="Машина убрана.";}
            GUILayout.Label("ВЕРТОЛЁТНАЯ ПЛОЩАДКА");
            if(GUILayout.Button("Вызвать учебный вертолёт")) SpawnHelicopter();
            if(GUILayout.Button("Открыть авторынок — витрина")) notice="Авторынок: каталог машин доступен на стоянке. Онлайн-торги требуют сервер.";
            if(GUILayout.Button("Открыть нотариальную контору")) notice="Нотариус: паспорт и регистрация доступны в прототипе.";
            if(GUILayout.Button("Инкассация: начать маршрут")){job="Инкассатор";notice="Смена инкассатора начата. Маршрутный сценарий — прототип.";}
            if(GUILayout.Button("Крупье: начать смену")){job="Крупье";notice="Смена крупье начата. Казино работает в локальном режиме.";}
            GUILayout.EndScrollView(); GUILayout.EndArea();
            if(adminPanelOpen){GUI.Box(new Rect(Screen.width-330,12,315,250),"АДМИН-ПАНЕЛЬ (F1) — ЛОКАЛЬНО"); if(GUI.Button(new Rect(Screen.width-315,55,285,35),"Выдать 10 000 ₽")){cash+=10000;notice="Админ-команда: выдано 10 000 ₽.";} if(GUI.Button(new Rect(Screen.width-315,98,285,35),"Повысить уровень")){level++;notice="Уровень повышен.";} if(GUI.Button(new Rect(Screen.width-315,141,285,35),"Сбросить фракцию")){faction="Нет фракции";notice="Фракция сброшена.";} if(GUI.Button(new Rect(Screen.width-315,184,285,35),"Закрыть панель"))adminPanelOpen=false;}
        }
        void SpawnVehicle(RPVehicleSpec v){
            if(v.price>cash){notice="Недостаточно денег для покупки.";return;} if(v.price>0)cash-=v.price;
            if(activeVehicle)Destroy(activeVehicle);
            Vector3 p=vehicleSpawn?vehicleSpawn.position:new Vector3(0,1,0);
            activeVehicle=GameObject.CreatePrimitive(PrimitiveType.Cube); activeVehicle.name=v.modelName+"_prototype"; activeVehicle.transform.position=p+Vector3.up*.8f; activeVehicle.transform.localScale=new Vector3(2f,1f,4f);
            var r=activeVehicle.GetComponent<Renderer>(); r.material.color=v.color;
            var rb=activeVehicle.AddComponent<Rigidbody>(); rb.mass=1200;
            notice="На стоянке появилась машина: "+v.modelName+". Это временная геометрическая модель.";
        }
        void SpawnHelicopter(){
            Vector3 p=helicopterSpawn?helicopterSpawn.position:new Vector3(20,3,20);
            var h=GameObject.CreatePrimitive(PrimitiveType.Capsule); h.name="Учебный_вертолет_прототип"; h.transform.position=p; h.transform.localScale=new Vector3(2.5f,1.2f,4f); h.GetComponent<Renderer>().material.color=new Color(.25f,.3f,.28f); notice="Учебный вертолёт создан на площадке. Полётная физика ещё не реализована.";
        }
    }
}
