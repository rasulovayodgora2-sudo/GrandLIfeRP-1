using UnityEngine;
namespace GrandLife {
[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour {
    public WheelCollider[] wheels;   // 0 FL, 1 FR, 2 RL, 3 RR
    public Transform[] visuals;
    public float motorTorque = 1800f, maxSteer = 30f, brakeTorque = 4000f, maxKmh = 120f;
    public bool Driven;
    Rigidbody rb;
    public float SpeedKmh => rb.velocity.magnitude * 3.6f;
    void Awake() { rb = GetComponent<Rigidbody>(); rb.centerOfMass = new Vector3(0, -0.5f, 0); }
    void FixedUpdate() {
        float thr = 0, st = 0; bool br = true;
        if (Driven) { var m = InputState.CurrentMove; thr = m.y; st = m.x; br = InputState.BrakeNow; }
        float t = SpeedKmh < maxKmh ? thr * motorTorque : 0f;
        wheels[2].motorTorque = t; wheels[3].motorTorque = t;
        foreach (var w in wheels) w.brakeTorque = br ? brakeTorque : 0f;
        wheels[0].steerAngle = wheels[1].steerAngle = st * maxSteer;
    }
    void Update() {
        for (int i = 0; i < wheels.Length; i++) {
            wheels[i].GetWorldPose(out var p, out var q); visuals[i].SetPositionAndRotation(p, q);
        }
    }
}}
