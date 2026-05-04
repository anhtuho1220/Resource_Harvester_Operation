using UnityEngine;

public class BaseUnit : MonoBehaviour
{
    public bool isSelected = false;
    private float lastPingTime = -120f;

    private void OnGUI() {
        if (isSelected) {
            GUI.color = Color.white;
            GUILayout.BeginArea(new Rect(Screen.width - 260, 10, 250, 150));
            
            // Background box for visibility
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.normal.background = Texture2D.whiteTexture; 
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUILayout.BeginVertical("box");
            GUI.color = Color.white;

            GUILayout.Label("<b>Base Selected</b>");
            
            bool canDrone = SceneManager.Instance.CanAffordDrone();
            GUI.enabled = canDrone;
            if (GUILayout.Button("Build Drone (500M 200C 1000F) - 30s")) {
                SceneManager.Instance.BuyDrone();
            }
            GUI.enabled = true;

            bool canTruck = SceneManager.Instance.CanAffordTruck();
            GUI.enabled = canTruck;
            if (GUILayout.Button("Build Truck (300M 100C 500F) - 20s")) {
                SceneManager.Instance.BuyTruck();
            }
            GUI.enabled = true;

            float timeSincePing = Time.time - lastPingTime;
            bool canPing = timeSincePing >= 120f;
            GUI.enabled = canPing;
            string pingText = canPing ? "Resource Ping" : $"Resource Ping ({Mathf.CeilToInt(120f - timeSincePing)}s)";
            if (GUILayout.Button(pingText)) {
                lastPingTime = Time.time;
                SceneManager.Instance.RevealNearestResourceHints();
            }
            GUI.enabled = true;

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
