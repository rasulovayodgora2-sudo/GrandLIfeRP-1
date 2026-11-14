using UnityEngine;

namespace GrandLife {
    [RequireComponent(typeof(Collider))]
    public class JobPoint : MonoBehaviour {
        public string jobTitle = "Работа";
        void Reset() {
            var c = GetComponent<Collider>();
            c.isTrigger = true;
        }
        void OnTriggerEnter(Collider other) {
            var data = other.GetComponent<RPPlayerData>();
            if (data != null) data.SetNearbyJob(this);
        }
        void OnTriggerExit(Collider other) {
            var data = other.GetComponent<RPPlayerData>();
            if (data != null) data.ClearNearbyJob(this);
        }
    }
}
