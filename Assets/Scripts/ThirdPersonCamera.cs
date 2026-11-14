using UnityEngine;
using UnityEngine.EventSystems;
namespace GrandLife {
public class ThirdPersonCamera : MonoBehaviour {
    public Transform target; public float distance = 6f, pitch = 15f; float yaw;
    float LookDelta() {
        float d = 0;
        foreach (var t in Input.touches)
            if (t.phase == TouchPhase.Moved && t.position.x > Screen.width * 0.5f &&
                !EventSystem.current.IsPointerOverGameObject(t.fingerId)) d += t.deltaPosition.x * 0.2f;
        if (Input.GetMouseButton(1)) d += Input.GetAxis("Mouse X") * 4f;
        return d;
    }
    void LateUpdate() {
        if (!target) return;
        if (target.GetComponent<Rigidbody>() != null)
            yaw = Mathf.LerpAngle(yaw, target.eulerAngles.y, 3f * Time.deltaTime);
        else yaw += LookDelta();
        var rot = Quaternion.Euler(pitch, yaw, 0);
        var focus = target.position + Vector3.up * 1.5f;
        var want = focus - rot * Vector3.forward * distance;
        transform.position = Vector3.Lerp(transform.position, want, 10f * Time.deltaTime);
        transform.LookAt(focus);
    }
}}
