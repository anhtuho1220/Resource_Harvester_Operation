using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class DroneUnit : MonoBehaviour
{
    public float speed = 10f;
    public float scanAngle = 60f; 
    public float maxScanDistance = 30f;
    public float hoverHeight = 10f;
    public float scanDuration = 5f;
    public float scanCircleRadius = 5f;
    public float scanCircleSpeed = 3f;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private LineRenderer lineRenderer;
    private Light scanLight;

    void Awake()
    {
        targetPosition = transform.position;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.cyan;
        lineRenderer.endColor = Color.cyan;

        GameObject lightObj = new GameObject("ScanConeLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;
        lightObj.transform.localRotation = Quaternion.Euler(90, 0, 0); 
        
        scanLight = lightObj.AddComponent<Light>();
        scanLight.type = LightType.Spot;
        scanLight.spotAngle = scanAngle;
        scanLight.range = maxScanDistance;
        scanLight.color = Color.cyan;
        scanLight.intensity = 5f;
        scanLight.enabled = false;
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            UpdatePathVisualization();

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
                lineRenderer.positionCount = 0;
                
                bool aboveMarker = false;
                ResourceNode[] allNodes = FindObjectsByType<ResourceNode>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var node in allNodes) {
                    if (node.hintMarker != null) {
                        float dist = Vector3.Distance(
                            new Vector3(transform.position.x, 0, transform.position.z), 
                            new Vector3(node.hintMarker.transform.position.x, 0, node.hintMarker.transform.position.z)
                        );
                        if (dist <= 15f) {
                            aboveMarker = true;
                            break;
                        }
                    }
                }

                if (aboveMarker) {
                    StartCoroutine(ScanRoutine());
                }
            }
        }
    }

    public void MoveTo(Vector3 destination)
    {
        targetPosition = new Vector3(destination.x, destination.y + hoverHeight, destination.z);
        isMoving = true;
        scanLight.enabled = false;
        StopAllCoroutines();
    }

    private void UpdatePathVisualization()
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, targetPosition);
    }

    private IEnumerator ScanRoutine()
    {
        scanLight.enabled = true; 
        
        float timer = 0f;
        float orbitAngle = 0f;
        Vector3 centerPos = targetPosition;

        while (timer < scanDuration) {
            timer += Time.deltaTime;
            orbitAngle += scanCircleSpeed * Time.deltaTime;

            // Smoothly increase radius at the start so it doesn't snap
            float currentRadius = Mathf.Lerp(0, scanCircleRadius, Mathf.Min(1f, timer * 2f));

            float x = centerPos.x + Mathf.Cos(orbitAngle) * currentRadius;
            float z = centerPos.z + Mathf.Sin(orbitAngle) * currentRadius;
            
            transform.position = new Vector3(x, centerPos.y, z);

            yield return null;
        }
        
        // Return to center when done
        transform.position = centerPos;
        scanLight.enabled = false;
        
        bool foundAny = false;
        ResourceNode[] allNodes = FindObjectsByType<ResourceNode>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (ResourceNode node in allNodes)
        {
            if (node.gameObject.activeSelf) continue;

            Vector3 dirToNode = node.transform.position - transform.position;
            float distance = dirToNode.magnitude;

            if (distance <= maxScanDistance)
            {
                float angle = Vector3.Angle(Vector3.down, dirToNode);
                if (angle <= scanAngle / 2f)
                {
                    node.gameObject.SetActive(true);
                    foundAny = true;
                }
            }
        }

        if (foundAny) {
            if (HarvestPriorityManager.Instance != null) {
                HarvestPriorityManager.Instance.OptimizeHarvesting();
            }
            TruckUnit[] trucks = FindObjectsByType<TruckUnit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach(var truck in trucks) {
                truck.CheckForAutoGather();
            }
        }
    }
}
