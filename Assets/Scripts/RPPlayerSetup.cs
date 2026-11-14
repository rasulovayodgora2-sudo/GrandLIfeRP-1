using UnityEngine;

namespace GrandLife {
    // Автоматически добавляет локальный RP-профиль игроку в сцене.
    public class RPPlayerSetup : MonoBehaviour {
        void Awake() {
            var player = GameObject.Find("Player");
            if (player != null && player.GetComponent<RPPlayerData>() == null)
                player.AddComponent<RPPlayerData>();
        }
    }
}
