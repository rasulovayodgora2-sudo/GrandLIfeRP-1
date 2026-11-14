using UnityEngine;
namespace GrandLife {
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
    public float speed = 4.5f;
    CharacterController cc; float vy;
    void Awake() { cc = GetComponent<CharacterController>(); }
    void Update() {
        var m = InputState.CurrentMove; var cam = Camera.main.transform;
        Vector3 f = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        Vector3 r = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        Vector3 d = f * m.y + r * m.x; if (d.sqrMagnitude > 1) d.Normalize();
        if (d.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(d), 12f * Time.deltaTime);
        vy = cc.isGrounded ? -1f : vy + Physics.gravity.y * Time.deltaTime;
        cc.Move((d * speed + Vector3.up * vy) * Time.deltaTime);
    }
    public void Teleport(Vector3 p) { cc.enabled = false; transform.position = p; cc.enabled = true; }
}}
