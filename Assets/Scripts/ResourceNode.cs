using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    public ResourceType type;
    public int amount;
    public GameObject hintMarker;
    private TextMesh textMesh;

    private void OnEnable() {
        if (hintMarker != null) {
            Destroy(hintMarker);
            hintMarker = null;
        }
    }

    private void Awake() {
        amount = Random.Range(1000, 5001);
        
        GameObject textObj = new GameObject("ResourceText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = new Vector3(0, 2, 0);
        
        textMesh = textObj.AddComponent<TextMesh>();
        textMesh.characterSize = 0.5f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        
        UpdateText();
    }

    public void UpdateText() {
        if (textMesh != null) {
            textMesh.text = $"{type}\n{amount}";
        }
    }

    private void Update() {
        if (textMesh != null && Camera.main != null) {
            textMesh.transform.rotation = Camera.main.transform.rotation;
        }
    }

    public int Gather(int requestedAmount) {
        int gathered = Mathf.Min(requestedAmount, amount);
        amount -= gathered;
        if (amount <= 0) {
            Destroy(gameObject);
        } else {
            UpdateText();
        }
        
        return gathered;
    }
}
