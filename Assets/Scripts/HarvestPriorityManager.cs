using UnityEngine;
using System.Collections.Generic;

public class HarvestPriorityManager : MonoBehaviour
{
    public static HarvestPriorityManager Instance;

    private void Awake() {
        Instance = this;
    }

    public void OptimizeHarvesting()
    {
        if (SceneManager.Instance == null) return;

        // 1. Find the resource type with the lowest stockpile
        ResourceType lowestType = ResourceType.Metal;
        int lowestStock = int.MaxValue;
        
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            int stock = SceneManager.Instance.GetStock(type);
            if (stock < lowestStock)
            {
                lowestStock = stock;
                lowestType = type;
            }
        }

        // Find all active, unexhausted deposits of the lowest type
        ResourceNode[] allNodes = FindObjectsByType<ResourceNode>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        List<ResourceNode> priorityNodes = new List<ResourceNode>();
        foreach (var node in allNodes)
        {
            if (node.gameObject.activeSelf && node.amount > 0 && node.type == lowestType)
            {
                priorityNodes.Add(node);
            }
        }

        if (priorityNodes.Count == 0) return;

        Vector3 basePos = SceneManager.Instance.GetBasePosition();
        priorityNodes.Sort((a, b) => {
            float distA = Vector3.Distance(a.transform.position, basePos);
            float distB = Vector3.Distance(b.transform.position, basePos);
            return distA.CompareTo(distB);
        });

        TruckUnit[] allTrucks = FindObjectsByType<TruckUnit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var pNode in priorityNodes)
        {
            // Determine if a truck is already targeting this node
            bool hasTruck = false;
            foreach (var t in allTrucks) {
                if (t.TargetResource == pNode) {
                    hasTruck = true;
                    break;
                }
            }

            if (!hasTruck)
            {
                AssignTruckToNode(pNode, allTrucks, lowestType);
            }
        }
    }

    private void AssignTruckToNode(ResourceNode node, TruckUnit[] allTrucks, ResourceType priorityType)
    {
        TruckUnit bestTruck = null;
        float bestDist = float.MaxValue;

        // Priority 1: An idle truck
        foreach (var t in allTrucks) {
            if (t.IsIdle) {
                float dist = Vector3.Distance(t.transform.position, node.transform.position);
                if (dist < bestDist) {
                    bestDist = dist;
                    bestTruck = t;
                }
            }
        }

        if (bestTruck != null) {
            bestTruck.AssignResource(node);
            return;
        }

        // Priority 2 & 3: A nearest truck carrying nothing and assigned on an already abundant resource deposit
        bestDist = float.MaxValue;
        foreach (var t in allTrucks) {
            if (t.IsCarryingNothing && t.TargetResource != null && t.TargetResource.type != priorityType) {
                float dist = Vector3.Distance(t.transform.position, node.transform.position);
                if (dist < bestDist) {
                    bestDist = dist;
                    bestTruck = t;
                }
            }
        }

        if (bestTruck != null) {
            bestTruck.AssignResource(node);
        }
    }
}
