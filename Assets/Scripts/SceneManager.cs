using UnityEngine;
using System.Collections.Generic;


public enum UnitType { Drone, Truck }
public enum ResourceType { Metal, Crystal, Fuel }

[System.Serializable]
public struct ResourceData {
    public ResourceType type;
    public GameObject prefab;
}

public class BuildTask {
    public UnitType type;
    public float startTime;
    public float duration;
}

public class SceneManager : MonoBehaviour
{
    public static SceneManager Instance;

    [Header("Spawn Points")]
    [SerializeField] private Transform baseTransform;
    [SerializeField] private Vector3 droneSpawnOffset = new Vector3(0, 10, 0); // Above base
    [SerializeField] private float truckSpawnRadius = 10f;
    public float BaseRadius { get { return truckSpawnRadius; } }

    [Header("Prefabs")]
    [SerializeField] private GameObject dronePrefab;
    [SerializeField] private GameObject truckPrefab;
    [SerializeField] private List<ResourceData> resourcePrefabs;

    [Header("Map Settings")]
    [SerializeField] private Vector2 mapBounds = new Vector2(500, 500);
    [SerializeField] private int resourcesPerType = 20;

    private Dictionary<ResourceType, int> baseStock = new Dictionary<ResourceType, int>();
    public const int MaxBaseCapacity = 100000;
    
    public List<BuildTask> activeBuilds = new List<BuildTask>();
    public bool isPaused = false;
    public bool isGameActive = false;

    private void Awake() {
        Instance = this;
        if (FindAnyObjectByType<PlayerController>() == null) {
            gameObject.AddComponent<PlayerController>();
        }
        if (baseTransform != null && baseTransform.GetComponent<BaseUnit>() == null) {
            baseTransform.gameObject.AddComponent<BaseUnit>();
        }
        if (gameObject.GetComponent<HarvestPriorityManager>() == null) {
            gameObject.AddComponent<HarvestPriorityManager>();
        }
    }

    public void StartGameEnvironment() {
        isGameActive = true;
        resourcesPerType = GameSettings.resourcesPerType;

        baseStock[ResourceType.Metal] = GameSettings.initialMetal;
        baseStock[ResourceType.Crystal] = GameSettings.initialCrystal;
        baseStock[ResourceType.Fuel] = GameSettings.initialFuel;

        InitializeResources();
        
        for (int i = 0; i < GameSettings.initialDrones; i++) SpawnUnit(UnitType.Drone);
        for (int i = 0; i < GameSettings.initialTrucks; i++) SpawnUnit(UnitType.Truck);
    }

    private void Update() {
        if (UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame) {
            TogglePause();
        }
    }

    public void TogglePause() {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }

    private void InitializeResources() {
        foreach (var resource in resourcePrefabs) {
            baseStock[resource.type] = 0;
            for (int i = 0; i < resourcesPerType; i++) {
                Vector3 randomPos = Vector3.zero;
                bool validPos = false;
                int attempts = 0;

                while (!validPos && attempts < 50) {
                    randomPos = baseTransform.position + new Vector3(
                        Random.Range(-mapBounds.x, mapBounds.x),
                        0,
                        Random.Range(-mapBounds.y, mapBounds.y)
                    );
                    randomPos.y = 0.5f;
                    
                    if (!Physics.CheckSphere(randomPos + Vector3.up * 2f, 1.9f)) {
                        validPos = true;
                    }
                    attempts++;
                }
                
                GameObject res = Instantiate(resource.prefab, randomPos, Quaternion.identity);
                ResourceNode node = res.GetComponent<ResourceNode>();
                if (node == null) {
                    node = res.AddComponent<ResourceNode>();
                }
                node.type = resource.type;
                node.UpdateText();
                res.SetActive(false); 
            }
        }
    }

    public void RevealNearestResourceHints() {
        ResourceNode[] allNodes = FindObjectsByType<ResourceNode>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        List<ResourceNode> hiddenNodes = new List<ResourceNode>();
        foreach (var node in allNodes) {
            if (!node.gameObject.activeSelf && node.hintMarker == null) {
                hiddenNodes.Add(node);
            }
        }

        if (baseTransform != null) {
            hiddenNodes.Sort((a, b) => {
                float distA = Vector3.Distance(a.transform.position, baseTransform.position);
                float distB = Vector3.Distance(b.transform.position, baseTransform.position);
                return distA.CompareTo(distB);
            });
        }

        int hintsToShow = Mathf.Min(3, hiddenNodes.Count);
        for (int i = 0; i < hintsToShow; i++) {
            ResourceNode node = hiddenNodes[i];

            GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Destroy(circle.GetComponent<Collider>());
            circle.name = "ResourceHintCircle_" + i;

            Vector2 randomOffset = Random.insideUnitCircle * 8f;
            circle.transform.position = new Vector3(node.transform.position.x + randomOffset.x, 0.1f, node.transform.position.z + randomOffset.y);
            
            circle.transform.localScale = new Vector3(30f, 0.01f, 30f);

            Renderer rend = circle.GetComponent<Renderer>();
            Material transparentMat = new Material(Shader.Find("Sprites/Default"));
            transparentMat.color = new Color(0.2f, 1f, 0.2f, 0.25f);
            rend.material = transparentMat;

            node.hintMarker = circle;
        }
    }

    public void SpawnUnit(UnitType type) {
        if (type == UnitType.Drone) {
            if (dronePrefab != null) {
                Instantiate(dronePrefab, baseTransform.position + droneSpawnOffset, Quaternion.identity);
                Debug.Log("Spawned Drone");
            } else {
                Debug.LogError("Drone Prefab is not assigned in SceneManager!");
            }
        } else {
            if (truckPrefab != null) {
                Vector2 randCircle = Random.insideUnitCircle.normalized * truckSpawnRadius;
                if (randCircle == Vector2.zero) randCircle = Vector2.right * truckSpawnRadius;
                Vector3 spawnPos = baseTransform.position + new Vector3(randCircle.x, 0, randCircle.y);
                Instantiate(truckPrefab, spawnPos, Quaternion.identity);
                Debug.Log("Spawned Truck");
            } else {
                Debug.LogError("Truck Prefab is not assigned in SceneManager!");
            }
        }
    }

    public void DepositResources(ResourceType type, int amount) {
        if (!baseStock.ContainsKey(type)) {
            baseStock[type] = 0;
        }
        int current = baseStock[type];
        int spaceLeft = MaxBaseCapacity - current;
        int toDeposit = Mathf.Min(amount, spaceLeft);
        baseStock[type] += toDeposit;
    }

    public Vector3 GetBasePosition() {
        return baseTransform.position;
    }

    public bool CanAffordDrone() {
        return GetStock(ResourceType.Metal) >= 500 && GetStock(ResourceType.Crystal) >= 200 && GetStock(ResourceType.Fuel) >= 1000;
    }

    public void BuyDrone() {
        if (!CanAffordDrone()) return;
        baseStock[ResourceType.Metal] -= 500;
        baseStock[ResourceType.Crystal] -= 200;
        baseStock[ResourceType.Fuel] -= 1000;
        StartCoroutine(BuildRoutine(UnitType.Drone, 30f));
    }

    public bool CanAffordTruck() {
        return GetStock(ResourceType.Metal) >= 300 && GetStock(ResourceType.Crystal) >= 100 && GetStock(ResourceType.Fuel) >= 500;
    }

    public void BuyTruck() {
        if (!CanAffordTruck()) return;
        baseStock[ResourceType.Metal] -= 300;
        baseStock[ResourceType.Crystal] -= 100;
        baseStock[ResourceType.Fuel] -= 500;
        StartCoroutine(BuildRoutine(UnitType.Truck, 20f));
    }

    public int GetStock(ResourceType type) {
        return baseStock.ContainsKey(type) ? baseStock[type] : 0;
    }

    private System.Collections.IEnumerator BuildRoutine(UnitType type, float time) {
        BuildTask task = new BuildTask { type = type, startTime = Time.time, duration = time };
        activeBuilds.Add(task);
        yield return new WaitForSeconds(time);
        activeBuilds.Remove(task);
        SpawnUnit(type);
    }
}
