using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class BaseUnit : MonoBehaviour
{
    public bool isSelected = false;
    public float lastPingTime = -120f;
    private NavMeshAgent agent;

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = GameSettings.baseSpeed;
    }

    public void MoveTo(Vector3 destination) {
        agent.SetDestination(destination);
    }

    public void Ping() {
        lastPingTime = Time.time;
        SceneManager.Instance.RevealNearestResourceHints();
    }
}
