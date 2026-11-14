using UnityEngine;

namespace GrandLife {
    // Локальная основа RP-профиля для одиночного прототипа.
    // Для настоящего онлайна деньги и опыт должны проверяться сервером.
    public class RPPlayerData : MonoBehaviour {
        public string characterName = "Новый игрок";
        public int level = 1;
        public int money = 5000;
        public int experience = 0;
        public string currentJob = "Безработный";
        public bool onShift = false;

        JobPoint nearbyJob;
        float shiftTimer;

        void Update() {
            if (onShift) {
                shiftTimer += Time.deltaTime;
                if (shiftTimer >= 60f) {
                    shiftTimer = 0f;
                    money += 250;
                    experience += 10;
                    if (experience >= level * 100) {
                        experience -= level * 100;
                        level++;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.F) && nearbyJob != null) {
                if (onShift) EndShift();
                else StartShift(nearbyJob);
            }
        }

        public void SetNearbyJob(JobPoint point) { nearbyJob = point; }
        public void ClearNearbyJob(JobPoint point) {
            if (nearbyJob == point) nearbyJob = null;
        }
        public void StartShift(JobPoint point) {
            if (point == null || onShift) return;
            currentJob = point.jobTitle;
            onShift = true;
            shiftTimer = 0f;
        }
        public void EndShift() {
            onShift = false;
            currentJob = "Безработный";
            shiftTimer = 0f;
        }

        void OnGUI() {
            GUI.color = Color.white;
            GUI.Box(new Rect(16, 16, 280, 112),
                characterName + "\nУровень: " + level + "   Опыт: " + experience +
                "\nНаличные: " + money.ToString("N0") + " ₽" +
                "\nРабота: " + currentJob);
            if (nearbyJob != null) {
                string label = onShift ? "Завершить смену" : "Устроиться: " + nearbyJob.jobTitle;
                if (GUI.Button(new Rect(Screen.width - 300, Screen.height - 150, 280, 90), label)) {
                    if (onShift) EndShift();
                    else StartShift(nearbyJob);
                }
                GUI.Label(new Rect(Screen.width - 300, Screen.height - 185, 280, 30),
                    "Нажми F или кнопку на экране");
            }
        }
    }
}
