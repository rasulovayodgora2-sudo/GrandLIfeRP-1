using UnityEngine;
using UnityEngine.UI;
namespace GrandLife {
public class MobileHUD : MonoBehaviour {
    public GameController game; Text speedTxt, useTxt; Font font;
    RectTransform Box(string n, Transform par, Vector2 anc, Vector2 pos, Vector2 size, Color c) {
        var go = new GameObject(n, typeof(RectTransform), typeof(Image)); go.transform.SetParent(par, false);
        var r = (RectTransform)go.transform; r.anchorMin = r.anchorMax = anc; r.pivot = new Vector2(.5f, .5f);
        r.anchoredPosition = pos; r.sizeDelta = size; go.GetComponent<Image>().color = c; return r;
    }
    Text Label(Transform par, string s, int size, TextAnchor a) {
        var go = new GameObject("Text", typeof(RectTransform), typeof(Text)); go.transform.SetParent(par, false);
        var r = (RectTransform)go.transform; r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero;
        var t = go.GetComponent<Text>(); t.font = font; t.text = s; t.fontSize = size; t.alignment = a; t.color = Color.white; return t;
    }
    void Start() {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var cgo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        cgo.transform.SetParent(transform, false);
        cgo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var sc = cgo.GetComponent<CanvasScaler>(); sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1920, 1080); sc.matchWidthOrHeight = .5f;
        var cr = cgo.transform;
        var bg = Box("Joystick", cr, new Vector2(0, 0), new Vector2(260, 260), new Vector2(260, 260), new Color(1, 1, 1, .2f));
        var knob = Box("Knob", bg, new Vector2(.5f, .5f), Vector2.zero, new Vector2(110, 110), new Color(1, 1, 1, .6f));
        bg.gameObject.AddComponent<VirtualJoystick>().knob = knob;
        var use = Box("Use", cr, new Vector2(1, 0), new Vector2(-220, 380), new Vector2(280, 110), new Color(0, 0, 0, .55f));
        var btn = use.gameObject.AddComponent<Button>(); btn.targetGraphic = use.GetComponent<Image>();
        btn.onClick.AddListener(game.ToggleVehicle); useTxt = Label(use, "Сесть", 40, TextAnchor.MiddleCenter);
        var br = Box("Brake", cr, new Vector2(1, 0), new Vector2(-220, 220), new Vector2(280, 110), new Color(.7f, .1f, .1f, .6f));
        br.gameObject.AddComponent<HoldButton>(); Label(br, "Тормоз", 40, TextAnchor.MiddleCenter);
        var sp = Box("Speed", cr, new Vector2(1, 1), new Vector2(-200, -70), new Vector2(360, 80), new Color(0, 0, 0, .45f));
        speedTxt = Label(sp, "0 км/ч", 44, TextAnchor.MiddleCenter);
        var ti = Box("Title", cr, new Vector2(0, 1), new Vector2(260, -50), new Vector2(480, 70), new Color(0, 0, 0, .45f));
        Label(ti, "GRAND LIFE RP · этап 1", 30, TextAnchor.MiddleCenter);
    }
    void Update() {
        if (!speedTxt) return;
        speedTxt.text = Mathf.RoundToInt(game.SpeedKmh) + " км/ч";
        useTxt.text = game.InCar ? "Выйти" : "Сесть";
    }
}}
