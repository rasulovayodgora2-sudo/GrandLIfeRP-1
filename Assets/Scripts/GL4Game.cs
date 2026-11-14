using System;
using System.Collections.Generic;
using UnityEngine;

namespace GrandLife {
    [Serializable] public class GL4Upgrade { public string id; public int level; }
    [Serializable] public class GL4AuctionSave { public string id; public int bid,escrow; public bool closed; public long endTicks; }
    [Serializable] public class GL4SaveData {
        public int cash=500000,bank=250000,level=1,xp=0;
        public bool passport,medical;
        public string faction="Нет",job="Безработный",skin="Стрит";
        public List<string> vehicles=new List<string>{"urban"};
        public List<string> properties=new List<string>();
        public List<string> businesses=new List<string>();
        public List<GL4Upgrade> renovations=new List<GL4Upgrade>();
        public List<GL4AuctionSave> auctionStates=new List<GL4AuctionSave>();
        public string pendingPackage="";
        public long packageReadyTicks;
    }
    public class GL4VehicleInfo {
        public string id,name,kind;public int price;public Color color;
        public GL4VehicleInfo(string i,string n,string k,int p,Color c){id=i;name=n;kind=k;price=p;color=c;}
    }
    public class GL4RealtyInfo {
        public string id,name;public int price;public GL4RealtyInfo(string i,string n,int p){id=i;name=n;price=p;}
    }
    public class GL4BusinessInfo {
        public string id,name;public int price,income;
        public GL4BusinessInfo(string i,string n,int p,int perMinute){id=i;name=n;price=p;income=perMinute;}
    }
    public class GL4AuctionLot {
        public string id,name,kind;public int opening,buyout,bid,escrow;public float endTime;public bool closed;
        public GL4AuctionLot(string i,string n,string k,int start,int buy){id=i;name=n;kind=k;opening=start;buyout=buy;endTime=Time.realtimeSinceStartup+90f;}
    }
    // Stage 4 is deliberately OFFLINE. Auction competitors, account authority and server-side purchases are NOT implemented.
    public class GL4Game : MonoBehaviour {
        public static GL4Game Instance;
        public Vector2 MobileAxis {get;private set;}
        public float MobileVertical {get;private set;}
        public GL4SaveData data;
        public GameObject activeVehicle;
        GameObject avatar;GL4FollowCamera follow;
        bool menu=true,driving,temporaryTransform,adminVisible;
        string notice="Добро пожаловать в Grand Life RP — Stage 4!";
        int tab;Vector2 scroll;float wageTimer,businessTimer;
        readonly string[] tabs={"Транспорт","Гараж","Недвижимость","Аукцион","Бизнес","Постамат","Скины","Работы","Документы","Админ"};
        readonly string[] factions={"Полиция","МЧС","Медики","Правительство","Такси","Автомеханики"};
        readonly string[] jobs={"Курьер","Таксист","Автомеханик","Водитель автобуса","Инкассатор","Крупье","Развозчик посылок"};
        GL4VehicleInfo[] catalog;
        GL4RealtyInfo[] realty;
        GL4BusinessInfo[] businesses;
        GL4AuctionLot[] auctions;
        void Awake(){Instance=this;InitCatalog();Load();}
        void Start(){
            avatar=GameObject.Find("GL4_Player");
            if(Camera.main)follow=Camera.main.GetComponent<GL4FollowCamera>();
            SetSkin(data.skin);
            foreach(GL4RealtyInfo r in realty)ApplyRenovation(r.id);
        }
        void InitCatalog(){
            catalog=new GL4VehicleInfo[]{
                new GL4VehicleInfo("urban","Городской 210","Седан",0,new Color(.19f,.43f,.83f)),
                new GL4VehicleInfo("classic","Волна Classic","Седан",45000,new Color(.8f,.17f,.17f)),
                new GL4VehicleInfo("taxi","Такси Жёлтый","Такси",62000,new Color(.95f,.72f,.09f)),
                new GL4VehicleInfo("ranger","Тайга 4x4","Внедорожник",125000,new Color(.2f,.28f,.19f)),
                new GL4VehicleInfo("van","Форвард Cargo","Фургон",135000,new Color(.86f,.86f,.84f)),
                new GL4VehicleInfo("sport","Фантом GT","Спорткар",280000,new Color(.77f,.09f,.29f)),
                new GL4VehicleInfo("hyper","Молния X","Спорткар",580000,new Color(.1f,.74f,.77f)),
                new GL4VehicleInfo("bike","Вихрь 900","Мотоцикл",98000,new Color(.97f,.4f,.09f)),
                new GL4VehicleInfo("bike2","Ночной Рейдер","Мотоцикл",190000,new Color(.17f,.13f,.2f)),
                new GL4VehicleInfo("heli","Сокол H2","Вертолёт",720000,new Color(.18f,.33f,.4f)),
                new GL4VehicleInfo("heli2","Орион VIP","Вертолёт",1350000,new Color(.79f,.75f,.61f))
            };
            realty=new GL4RealtyInfo[]{
                new GL4RealtyInfo("apt_economy","Квартира Эконом · Нотарка",95000),
                new GL4RealtyInfo("apt_middle","Квартира Комфорт · Нотарка",260000),
                new GL4RealtyInfo("apt_elite","Пентхаус Элит · Батырево",850000),
                new GL4RealtyInfo("house_economy","Дом Эконом · Южный",180000),
                new GL4RealtyInfo("house_middle","Коттедж Комфорт · Южный",560000),
                new GL4RealtyInfo("house_elite","Вилла Элит · Батырево",1750000)
            };
            businesses=new GL4BusinessInfo[]{
                new GL4BusinessInfo("coffee","Кофейня Новоград",180000,350),
                new GL4BusinessInfo("carwash","Автомойка",390000,650),
                new GL4BusinessInfo("market","Мини-маркет",620000,1000),
                new GL4BusinessInfo("service","Автосервис",890000,1400),
                new GL4BusinessInfo("hotel","Гостиница",1650000,2500)
            };
            auctions=new GL4AuctionLot[]{
                new GL4AuctionLot("house_middle","Коттедж Комфорт","Недвижимость",400000,520000),
                new GL4AuctionLot("apt_elite","Пентхаус Элит","Недвижимость",650000,810000),
                new GL4AuctionLot("carwash","Автомойка","Бизнес",290000,375000),
                new GL4AuctionLot("hotel","Гостиница","Бизнес",1300000,1590000)
            };
        }
        void Load(){
            string s=PlayerPrefs.GetString("GL4_SAVE_V1","");
            try{data=string.IsNullOrEmpty(s)?new GL4SaveData():JsonUtility.FromJson<GL4SaveData>(s);}catch{data=new GL4SaveData();}
            if(data==null)data=new GL4SaveData();
            if(data.vehicles==null)data.vehicles=new List<string>();
            if(data.properties==null)data.properties=new List<string>();
            if(data.businesses==null)data.businesses=new List<string>();
            if(data.renovations==null)data.renovations=new List<GL4Upgrade>();
            if(data.auctionStates==null)data.auctionStates=new List<GL4AuctionSave>();
            foreach(GL4AuctionLot lot in auctions){
                GL4AuctionSave a=data.auctionStates.Find(x=>x.id==lot.id);
                if(a==null)continue;
                lot.bid=a.bid;lot.escrow=a.escrow;lot.closed=a.closed;
                lot.endTime=Time.realtimeSinceStartup+(float)Math.Max(0,(a.endTicks-DateTime.UtcNow.Ticks)/(double)TimeSpan.TicksPerSecond);
            }
            if(!data.vehicles.Contains("urban"))data.vehicles.Add("urban");
        }
        void Save(){
            if(data.auctionStates==null)data.auctionStates=new List<GL4AuctionSave>();
            foreach(GL4AuctionLot lot in auctions){
                GL4AuctionSave a=data.auctionStates.Find(x=>x.id==lot.id);
                if(a==null){a=new GL4AuctionSave{id=lot.id};data.auctionStates.Add(a);}
                a.bid=lot.bid;a.escrow=lot.escrow;a.closed=lot.closed;
                a.endTicks=DateTime.UtcNow.AddSeconds(Mathf.Max(0,lot.endTime-Time.realtimeSinceStartup)).Ticks;
            }
            PlayerPrefs.SetString("GL4_SAVE_V1",JsonUtility.ToJson(data));PlayerPrefs.Save();
        }
        void OnApplicationPause(bool paused){if(paused)Save();}
        void OnApplicationQuit(){Save();}
        void Update(){
            if(Input.GetKeyDown(KeyCode.F1))adminVisible=!adminVisible;
            if(Input.GetKeyDown(KeyCode.Tab))menu=!menu;
            if(Input.GetKeyDown(KeyCode.F) && activeVehicle!=null){if(driving)ExitVehicle();else EnterVehicle();}
            if(data.job!="Безработный"){
                wageTimer+=Time.deltaTime;
                if(wageTimer>=60){wageTimer-=60;data.cash+=data.job=="Инкассатор"?900:data.job=="Крупье"?700:500;data.xp+=10;if(data.xp>=data.level*100){data.xp=0;data.level++;}notice="Зарплата за смену начислена";Save();}
            }
            if(data.businesses.Count>0){
                businessTimer+=Time.deltaTime;
                if(businessTimer>=60){businessTimer-=60;int earned=0;foreach(GL4BusinessInfo b in businesses)if(data.businesses.Contains(b.id))earned+=b.income;data.bank+=earned;notice="Доход бизнеса +"+earned.ToString("N0")+" ₽ на банковский счёт";Save();}
            }
            foreach(GL4AuctionLot a in auctions)if(!a.closed && Time.realtimeSinceStartup>=a.endTime){
                a.closed=true;if(a.escrow>0){Award(a);notice="Вы выиграли локальный аукцион: "+a.name;Save();}
            }
        }
        bool Pay(int amount){if(amount<0||data.cash<amount){notice="Недостаточно наличных";return false;}data.cash-=amount;Save();return true;}
        GL4VehicleInfo Vehicle(string id){foreach(GL4VehicleInfo v in catalog)if(v.id==id)return v;return null;}
        void BuyVehicle(GL4VehicleInfo v){if(data.vehicles.Contains(v.id)){notice="Уже в гараже";return;}if(Pay(v.price)){data.vehicles.Add(v.id);notice="Куплено: "+v.name+". Открой гараж для спавна.";Save();}}
        void Spawn(string id,bool home){
            if(!data.vehicles.Contains(id)){notice="Сначала купи транспорт";return;}
            GL4VehicleInfo v=Vehicle(id);if(v==null)return;
            if(driving)ExitVehicle();if(activeVehicle)Destroy(activeVehicle);
            Vector3 p=v.kind=="Вертолёт"?GL4World.Instance.heliSpawn.position:(home?GL4World.Instance.garageSpawn.position:GL4World.Instance.parkingSpawn.position);
            activeVehicle=GL4VisualFactory.Vehicle(v.name,v.kind,v.color,p);
            notice="Транспорт появился: "+v.name+". Нажми «Сесть» или F.";
        }
        void SpawnAtProperty(string vehicleId,string propertyId){
            if(!data.properties.Contains(propertyId)||!data.vehicles.Contains(vehicleId))return;
            GL4PropertyMarker marker;
            if(!GL4World.Instance.properties.TryGetValue(propertyId,out marker))return;
            GL4VehicleInfo v=Vehicle(vehicleId);if(v==null)return;
            if(driving)ExitVehicle();if(activeVehicle)Destroy(activeVehicle);
            Vector3 p=marker.transform.position+new Vector3(7f,0f,-6f);
            if(v.kind=="Вертолёт")p.y=3f;
            activeVehicle=GL4VisualFactory.Vehicle(v.name,v.kind,v.color,p);
            notice="Транспорт появился в гараже вашей недвижимости.";
        }
        void EnterVehicle(){if(!activeVehicle||driving)return;driving=true;activeVehicle.GetComponent<GL4Drive>().controlled=true;avatar.SetActive(false);if(follow)follow.target=activeVehicle.transform;notice="Управление: WASD / стрелки. Вертолёт Q/E — высота.";}
        void ExitVehicle(){if(!driving)return;driving=false;
            Vector3 p=activeVehicle?activeVehicle.transform.position+activeVehicle.transform.right*3:Vector3.zero;
            if(activeVehicle)activeVehicle.GetComponent<GL4Drive>().controlled=false;
            avatar.SetActive(true);CharacterController cc=avatar.GetComponent<CharacterController>();cc.enabled=false;avatar.transform.position=new Vector3(p.x,Mathf.Max(.1f,p.y),p.z);cc.enabled=true;
            if(follow)follow.target=avatar.transform;
            if(temporaryTransform){if(activeVehicle)Destroy(activeVehicle);activeVehicle=null;temporaryTransform=false;}
            notice="Вы вернулись в облик персонажа.";
        }
        void TransformInto(string form,string name,Color color){
            if(driving)ExitVehicle();if(activeVehicle)Destroy(activeVehicle);
            Vector3 p=avatar.transform.position;
            if(form=="Вертолёт")p.y=Mathf.Max(3,p.y);
            activeVehicle=GL4VisualFactory.Vehicle(name,form,color,p);
            temporaryTransform=true;EnterVehicle();notice="ТРАНСФОРМЕР: персонаж превратился в "+name;
        }
        void SetSkin(string skin){
            data.skin=skin;
            if(avatar==null)avatar=GameObject.Find("GL4_Player");
            if(avatar!=null){
                for(int i=avatar.transform.childCount-1;i>=0;i--)Destroy(avatar.transform.GetChild(i).gameObject);
                GameObject model=GL4VisualFactory.Avatar(skin);model.transform.SetParent(avatar.transform,false);
            }
            Save();notice="Скин выбран: "+skin;
        }
        int Level(string id){foreach(GL4Upgrade u in data.renovations)if(u.id==id)return u.level;return 0;}
        void ApplyRenovation(string id){GL4PropertyMarker p;if(GL4World.Instance!=null && GL4World.Instance.properties.TryGetValue(id,out p))p.SetRenovation(Level(id));}
        void BuyProperty(GL4RealtyInfo r){if(data.properties.Contains(r.id)){notice="Объект уже ваш";return;}if(Pay(r.price)){data.properties.Add(r.id);Save();notice="Недвижимость куплена: "+r.name;}}
        void Renovate(string id){if(!data.properties.Contains(id)){notice="Сначала купи недвижимость";return;}int level=Level(id);if(level>=3){notice="Ремонт уже максимальный";return;}
            int cost=(level==0?10000:level==1?30000:80000);if(!Pay(cost))return;
            GL4Upgrade u=data.renovations.Find(x=>x.id==id);if(u==null){u=new GL4Upgrade{id=id,level=0};data.renovations.Add(u);}u.level++;
            ApplyRenovation(id);Save();notice="Ремонт улучшен до уровня "+u.level+". Фасад и декор обновлены.";
        }
        void BuyBusiness(GL4BusinessInfo b){if(data.businesses.Contains(b.id)){notice="Бизнес уже ваш";return;}if(Pay(b.price)){data.businesses.Add(b.id);Save();notice="Куплен бизнес: "+b.name+". Доход поступает в банк раз в минуту.";}}
        bool Owned(GL4AuctionLot a){return a.kind=="Бизнес"?data.businesses.Contains(a.id):data.properties.Contains(a.id);}
        void Award(GL4AuctionLot a){if(a.kind=="Бизнес"){if(!data.businesses.Contains(a.id))data.businesses.Add(a.id);}else if(!data.properties.Contains(a.id))data.properties.Add(a.id);}
        void Bid(GL4AuctionLot a){if(a.closed||Owned(a)){notice="Лот закрыт или уже ваш";return;}
            int next=a.bid==0?a.opening:a.bid+10000;int extra=next-a.escrow;
            if(!Pay(extra))return;a.bid=next;a.escrow=next;notice="Ставка "+next.ToString("N0")+" ₽ принята. Средства удержаны до завершения аукциона.";Save();
        }
        void Buyout(GL4AuctionLot a){if(a.closed||Owned(a)){notice="Лот закрыт или уже ваш";return;}
            int extra=a.buyout-a.escrow;if(!Pay(extra))return;
            a.closed=true;a.escrow=0;Award(a);Save();notice="Лот выкуплен: "+a.name;
        }
        void OrderPackage(){if(!string.IsNullOrEmpty(data.pendingPackage)){notice="Сначала забери предыдущую посылку";return;}if(!Pay(1500))return;
            data.pendingPackage="Коробка с редким стилем";data.packageReadyTicks=DateTime.UtcNow.AddSeconds(20).Ticks;Save();notice="Посылка заказана. Доставка в постамат через 20 секунд.";
        }
        void ClaimPackage(){if(string.IsNullOrEmpty(data.pendingPackage))return;
            if(DateTime.UtcNow.Ticks<data.packageReadyTicks){notice="Посылка ещё в пути";return;}
            data.pendingPackage="";data.packageReadyTicks=0;SetSkin("Кибер");notice="Посылка получена: активирован скин Кибер!";Save();
        }
        void Title(string s){GUILayout.Space(8);GUILayout.Label(s);}
        bool Action(string s){return GUILayout.Button(s,GUILayout.MinHeight(36));}
        void DrawTab(){
            if(tab==0){Title("АВТОСАЛОН — оригинальные модели с кузовом, колёсами, стёклами и фарами");
                foreach(GL4VehicleInfo v in catalog){if(Action(v.name+" · "+v.kind+" · "+(v.price==0?"Стартовый":v.price.ToString("N0")+" ₽")+(data.vehicles.Contains(v.id)?"  ✓ куплено":"")))BuyVehicle(v);}
            }else if(tab==1){Title("ГАРАЖ — спавн только купленного транспорта");
                foreach(string id in data.vehicles){GL4VehicleInfo v=Vehicle(id);if(v==null)continue;
                    if(Action("Вывести на стоянку: "+v.name))Spawn(id,false);
                    if(Action("Спавн у общего гаража: "+v.name))Spawn(id,true);
                    foreach(string propId in data.properties){
                        if(Action("Спавн в домашнем гараже ("+propId+"): "+v.name))SpawnAtProperty(id,propId);
                    }
                }
                if(activeVehicle){if(Action(driving?"Выйти из транспорта":"Сесть в транспорт")){if(driving)ExitVehicle();else EnterVehicle();}
                    if(Action("Убрать транспорт")){if(driving)ExitVehicle();if(activeVehicle)Destroy(activeVehicle);activeVehicle=null;}}
            }else if(tab==2){Title("КВАРТИРЫ И ДОМА · Эконом / Комфорт / Элит");
                foreach(GL4RealtyInfo r in realty){bool own=data.properties.Contains(r.id);
                    if(Action(r.name+" · "+r.price.ToString("N0")+" ₽"+(own?"  ✓ ваша":"")) && !own)BuyProperty(r);
                    if(own){int level=Level(r.id);if(Action("Ремонт "+r.name+" · уровень "+level+"/3"))Renovate(r.id);}
                }
                GUILayout.Label("Ремонт изменяет фасад, мебель и декор на 1–3 уровнях.");
            }else if(tab==3){Title("ЛОКАЛЬНЫЙ АУКЦИОН · без онлайн-соперников");
                foreach(GL4AuctionLot a in auctions){int seconds=Mathf.Max(0,Mathf.CeilToInt(a.endTime-Time.realtimeSinceStartup));
                    GUILayout.Label(a.name+" · "+a.kind+" · "+(a.closed?"ЗАВЕРШЁН":seconds+" сек")+" · ставка "+a.bid.ToString("N0")+" ₽");
                    if(!a.closed){if(Action("Сделать ставку · "+(a.bid==0?a.opening:a.bid+10000).ToString("N0")+" ₽"))Bid(a);
                        if(Action("Выкупить сразу · "+a.buyout.ToString("N0")+" ₽"))Buyout(a);}
                }
            }else if(tab==4){Title("БИЗНЕСЫ · доход на банковский счёт каждую минуту игры");
                foreach(GL4BusinessInfo b in businesses)if(Action(b.name+" · "+b.price.ToString("N0")+" ₽ · +"+b.income+" ₽/мин"+(data.businesses.Contains(b.id)?"  ✓ ваш":"")))BuyBusiness(b);
            }else if(tab==5){Title("ПОСТАМАТ · заказ и получение посылки");
                if(string.IsNullOrEmpty(data.pendingPackage)){if(Action("Заказать коробку с кибер-скином · 1 500 ₽"))OrderPackage();}
                else{long remaining=Math.Max(0,(data.packageReadyTicks-DateTime.UtcNow.Ticks)/TimeSpan.TicksPerSecond);
                    GUILayout.Label("В пути: "+data.pendingPackage+" · ожидание "+remaining+" сек");
                    if(Action("Забрать посылку"))ClaimPackage();}
            }else if(tab==6){Title("СКИНЫ — авторские цветовые стили персонажа");
                foreach(string s in new string[]{"Стрит","Неон","Тактик","Люкс","Кибер"})if(Action("Надеть скин: "+s+(data.skin==s?"  ✓":"")))SetSkin(s);
                Title("ТРАНСФОРМЕР — переключение из скина в транспорт и обратно");
                if(Action("Стать спорткаром Фантом GT"))TransformInto("Спорткар","Трансформер Фантом",new Color(.82f,.08f,.36f));
                if(Action("Стать мотоциклом Вихрь"))TransformInto("Мотоцикл","Трансформер Вихрь",new Color(.12f,.77f,.95f));
                if(Action("Стать вертолётом Сокол"))TransformInto("Вертолёт","Трансформер Сокол",new Color(.72f,.7f,.21f));
                if(driving && Action("Обратно в скин персонажа"))ExitVehicle();
            }else if(tab==7){Title("РАБОТЫ И ФРАКЦИИ · одиночный режим");
                foreach(string j in jobs)if(Action("Начать смену: "+j)){data.job=j;wageTimer=0;Save();notice="Смена начата: "+j;}
                if(Action("Завершить смену")){data.job="Безработный";Save();}
                Title("Казино — только виртуальные деньги прототипа");
                if(Action("Сыграть 500 ₽: орёл или решка")){if(Pay(500)){if(UnityEngine.Random.value>.5f){data.cash+=1000;notice="Выигрыш 1 000 ₽";}else notice="Проигрыш 500 ₽";Save();}}
                Title("Фракции (локальное вступление)");
                foreach(string f in factions)if(Action("Вступить: "+f)){data.faction=f;Save();notice="Вы во фракции: "+f;}
            }else if(tab==8){Title("ДОКУМЕНТЫ / БАНК");
                if(Action(data.passport?"Паспорт ✓":"Оформить паспорт")){data.passport=true;Save();}
                if(Action(data.medical?"Медкарта ✓":"Оформить медкарту")){data.medical=true;Save();}
                if(Action("Положить 5 000 ₽ в банк") && Pay(5000)){data.bank+=5000;Save();}
                if(Action("Снять 5 000 ₽ из банка")){if(data.bank>=5000){data.bank-=5000;data.cash+=5000;Save();}else notice="Недостаточно денег на счёте";}
            }else if(tab==9){Title("АДМИН-ПАНЕЛЬ — ТОЛЬКО ДЛЯ ЛОКАЛЬНОГО ТЕСТИРОВАНИЯ");
                GUILayout.Label("В сетевой игре доступ должен проверять сервер. Здесь это тестовые кнопки.");
                if(Action("Выдать 500 000 ₽ (тест)")){data.cash+=500000;Save();}
                if(Action("Повысить уровень (тест)")){data.level++;Save();}
                if(Action("Очистить фракцию")){data.faction="Нет";Save();}
                if(Action("Показать / скрыть админ-режим (F1)"))adminVisible=!adminVisible;
            }
        }
        void OnGUI(){
            float scale=Mathf.Min(Screen.width/1100f,Screen.height/700f);
            scale=Mathf.Max(.45f,scale);
            GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            float sw=Screen.width/scale,sh=Screen.height/scale;
            GUI.skin.button.fontSize=15;GUI.skin.label.fontSize=16;GUI.skin.box.fontSize=16;
            GUI.Box(new Rect(12,12,Mathf.Min(sw-24,620),98),"GRAND LIFE RP · STAGE 4 — OFFLINE PROTOTYPE\nНаличные "+data.cash.ToString("N0")+" ₽ · Банк "+data.bank.ToString("N0")+" ₽ · Ур. "+data.level+"\nРабота: "+data.job+" · Фракция: "+data.faction);
            GUI.Label(new Rect(16,112,Mathf.Min(sw-24,800),28),notice);
            if(GUI.Button(new Rect(14,145,190,44),menu?"Скрыть меню (Tab)":"Открыть меню"))menu=!menu;
            if(menu){
                float width=Mathf.Min(625,sw-25),height=Mathf.Max(180,sh-210);
                GUI.Box(new Rect(12,194,width,height),"");
                GUILayout.BeginArea(new Rect(24,205,width-24,height-18));
                for(int row=0;row<2;row++){
                    GUILayout.BeginHorizontal();
                    for(int col=0;col<5;col++){int idx=row*5+col;
                        if(GUILayout.Button((tab==idx?"● ":"")+tabs[idx],GUILayout.MinHeight(34))) {tab=idx;scroll=Vector2.zero;}}
                    GUILayout.EndHorizontal();
                }
                scroll=GUILayout.BeginScrollView(scroll);DrawTab();GUILayout.EndScrollView();GUILayout.EndArea();
            }
            if(activeVehicle && GUI.Button(new Rect(sw-210,16,195,43),driving?"ВЫЙТИ (F)":"СЕСТЬ (F)")){if(driving)ExitVehicle();else EnterVehicle();}
            if(driving && temporaryTransform && GUI.Button(new Rect(sw-210,65,195,43),"ОБРАТНО В СКИН"))ExitVehicle();
            if(adminVisible)GUI.Box(new Rect(sw-250,120,235,70),"ТЕСТОВЫЙ АДМИН-РЕЖИМ\nF1 — скрыть");
            // Touch driving and walking controls. RepeatButton is pressed continuously while held.
            float by=sh-142;
            bool left=GUI.RepeatButton(new Rect(sw-280,by,65,58),"◀");
            bool right=GUI.RepeatButton(new Rect(sw-140,by,65,58),"▶");
            bool forward=GUI.RepeatButton(new Rect(sw-210,by-62,65,58),"▲");
            bool backward=GUI.RepeatButton(new Rect(sw-210,by+62,65,58),"▼");
            MobileAxis=new Vector2((right?1:0)-(left?1:0),(forward?1:0)-(backward?1:0));
            if(driving && activeVehicle && activeVehicle.GetComponent<GL4Drive>().kind=="Вертолёт"){
                bool up=GUI.RepeatButton(new Rect(sw-350,by-62,60,58),"ВВЕРХ");
                bool down=GUI.RepeatButton(new Rect(sw-350,by+62,60,58),"ВНИЗ");
                MobileVertical=(up?1:0)-(down?1:0);
            }else MobileVertical=0;
        }
    }
}
