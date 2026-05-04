using UnityEngine;
using System.Collections.Generic;

public class EnvironmentSpawner : MonoBehaviour
{
    [Header("Passthrough Objects (Grass, Small Bushes)")]
    public List<GameObject> passthroughPrefabs;
    public Transform passthroughContainer;
    public int passthroughCount = 200;

    [Header("Obstruction Objects (Rocks, Trees)")]
    public List<GameObject> obstructionPrefabs;
    public Transform obstructionContainer;
    public int obstructionCount = 50;

    [Header("Spawn Settings")]
    public Vector2 spawnAreaBounds = new Vector2(500, 500);

    [ContextMenu("Generate Environment")]
    public void GenerateEnvironment()
    {
        passthroughContainer = ClearOrCreateContainer(passthroughContainer, "PassthroughObjects");
        obstructionContainer = ClearOrCreateContainer(obstructionContainer, "ObstructionObjects");

        SpawnList(passthroughPrefabs, passthroughContainer, passthroughCount);
        SpawnList(obstructionPrefabs, obstructionContainer, obstructionCount);
    }

    private Transform ClearOrCreateContainer(Transform container, string defaultName)
    {
        if (container == null)
        {
            Transform existing = transform.Find(defaultName);
            if (existing != null) {
                container = existing;
            } else {
                container = new GameObject(defaultName).transform;
                container.SetParent(transform);
            }
        }
        
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(container.GetChild(i).gameObject);
        }

        return container;
    }

    private void SpawnList(List<GameObject> prefabs, Transform container, int count)
    {
        if (prefabs == null || prefabs.Count == 0)
        {
            return;
        }



        for (int i = 0; i < count; i++)
        {
            GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
            
            Vector3 randomPos = transform.position + new Vector3(
                Random.Range(-spawnAreaBounds.x, spawnAreaBounds.x),
                0, // Assuming flat ground. Change this if using a terrain with height.
                Random.Range(-spawnAreaBounds.y, spawnAreaBounds.y)
            );

            // Give the object a random Y-axis rotation for natural variety
            Quaternion randomRot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

            GameObject spawnedObj = Instantiate(prefab, randomPos, randomRot, container);
            
            // Slight random scale variation to make them look more organic
            float randomScale = Random.Range(0.8f, 1.25f);
            spawnedObj.transform.localScale = prefab.transform.localScale * randomScale;
        }
    }
}
