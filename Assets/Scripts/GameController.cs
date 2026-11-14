using UnityEngine;
namespace GrandLife {
public class GameController : MonoBehaviour {
    public PlayerController player; public CarController car; public ThirdPersonCamera cam;
    public float enterDistance = 3.5f;
    public bool InCar { get; private set; }
    public float SpeedKmh => InCar ? car.SpeedKmh : 0f;
    void Start() { cam.target = player.transform; car.Driven = false; }
    void Update() { if (Input.GetKeyDown(KeyCode.E)) ToggleVehicle(); }
    public void ToggleVehicle() {
        if (!InCar) {
            if (Vector3.Distance(player.transform.position, car.transform.position) > enterDistance) return;
            InCar = true; player.gameObject.SetActive(false); car.Driven = true; cam.target = car.transform;
        } else {
            InCar = false; car.Driven = false;
            player.Teleport(car.transform.position - car.transform.right * 2.2f + Vector3.up * 0.5f);
            player.gameObject.SetActive(true); cam.target = player.transform;
        }
    }
}}
