using UnityEngine;
using UnityEngine.EventSystems;
namespace GrandLife {
public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler {
    public RectTransform knob; public float radius = 100f;
    RectTransform rt;
    void Awake() { rt = (RectTransform)transform; }
    public void OnPointerDown(PointerEventData e) { OnDrag(e); }
    public void OnDrag(PointerEventData e) {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, e.position, e.pressEventCamera, out var p);
        var v = Vector2.ClampMagnitude(p, radius);
        knob.anchoredPosition = v; InputState.Move = v / radius;
    }
    public void OnPointerUp(PointerEventData e) { knob.anchoredPosition = Vector2.zero; InputState.Move = Vector2.zero; }
}
public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    public void OnPointerDown(PointerEventData e) { InputState.Brake = true; }
    public void OnPointerUp(PointerEventData e) { InputState.Brake = false; }
}}
