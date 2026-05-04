using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(LineRenderer))]
public class TruckUnit : MonoBehaviour
{
    public int capacity = 100;
    private int currentCargo = 0;
    private ResourceType currentCargoType;

    private NavMeshAgent agent;
    private LineRenderer lineRenderer;
    private ResourceNode targetResource;

    private enum State { Idle, MovingToResource, Gathering, MovingToBase, Unloading }
    private State currentState = State.Idle;

    public bool IsIdle { get { return currentState == State.Idle; } }
    public bool IsCarryingNothing { get { return currentCargo == 0; } }
    public ResourceNode TargetResource { get { return targetResource; } }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.radius = 1f;
        agent.height = 2f;
        
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.yellow;
        lineRenderer.endColor = Color.yellow;
    }

    void Update()
    {
        UpdatePathVisualization();

        switch (currentState)
        {
            case State.MovingToResource:
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (targetResource != null && targetResource.gameObject.activeSelf)
                    {
                        StartCoroutine(GatherRoutine());
                    }
                    else
                    {
                        currentState = State.Idle;
                        if (HarvestPriorityManager.Instance != null) HarvestPriorityManager.Instance.OptimizeHarvesting();
                    }
                }
                break;
            case State.MovingToBase:
                float distToBase = Vector3.Distance(transform.position, SceneManager.Instance.GetBasePosition());
                if (!agent.pathPending && distToBase <= SceneManager.Instance.BaseRadius + 1f)
                {
                    agent.ResetPath();
                    StartCoroutine(UnloadRoutine());
                }
                break;
        }
    }

    public void MoveTo(Vector3 destination)
    {
        targetResource = null;
        agent.SetDestination(destination);
        currentState = State.Idle;
    }

    public void CheckForAutoGather() {
        if (currentState == State.Idle) {
            ResourceNode closestNode = null;
            float closestDist = float.MaxValue;

            ResourceNode[] allNodes = FindObjectsByType<ResourceNode>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach(var node in allNodes) {
                if (node.gameObject.activeSelf && node.amount > 0) {
                    float dist = Vector3.Distance(transform.position, node.transform.position);
                    if (dist < closestDist) {
                        closestDist = dist;
                        closestNode = node;
                    }
                }
            }

            if (closestNode != null) {
                AssignResource(closestNode);
            }
        }
    }

    public void AssignResource(ResourceNode node)
    {
        targetResource = node;
        agent.SetDestination(node.transform.position);
        currentState = State.MovingToResource;
    }

    private IEnumerator GatherRoutine()
    {
        currentState = State.Gathering;
        yield return new WaitForSeconds(1f);

        if (targetResource != null && targetResource.gameObject.activeSelf)
        {
            currentCargoType = targetResource.type;
            int amountToGather = capacity - currentCargo;
            int gathered = targetResource.Gather(amountToGather);
            currentCargo += gathered;

            if (currentCargo >= capacity || targetResource.amount <= 0)
            {
                agent.SetDestination(SceneManager.Instance.GetBasePosition());
                currentState = State.MovingToBase;
            }
            else
            {
                StartCoroutine(GatherRoutine());
            }
        }
        else
        {
            if (currentCargo > 0)
            {
                agent.SetDestination(SceneManager.Instance.GetBasePosition());
                currentState = State.MovingToBase;
            }
            else
            {
                currentState = State.Idle;
                if (HarvestPriorityManager.Instance != null) HarvestPriorityManager.Instance.OptimizeHarvesting();
            }
        }
    }

    private IEnumerator UnloadRoutine()
    {
        currentState = State.Unloading;
        yield return new WaitForSeconds(1f);

        SceneManager.Instance.DepositResources(currentCargoType, currentCargo);
        currentCargo = 0;

        if (targetResource != null && targetResource.amount > 0 && targetResource.gameObject.activeSelf)
        {
            agent.SetDestination(targetResource.transform.position);
            currentState = State.MovingToResource;
        }
        else
        {
            currentState = State.Idle;
            if (HarvestPriorityManager.Instance != null) HarvestPriorityManager.Instance.OptimizeHarvesting();
        }
    }

    private void UpdatePathVisualization()
    {
        if (agent.hasPath)
        {
            lineRenderer.positionCount = agent.path.corners.Length;
            lineRenderer.SetPositions(agent.path.corners);
        }
        else
        {
            lineRenderer.positionCount = 0;
        }
    }
}
