using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 20f;
    private Vector2 minBounds = new Vector2(-450f, -450f);
    private Vector2 maxBounds = new Vector2(450f, 450f);

    void Start()
    {
        Terrain terrain = Terrain.activeTerrain;
        if (terrain != null) {
            Vector3 pos = terrain.transform.position;
            Vector3 size = terrain.terrainData.size;
            minBounds = new Vector2(pos.x + 50f, pos.z + 50f);
            maxBounds = new Vector2(pos.x + size.x - 50f, pos.z + size.z - 50f);
        } else {
            GameObject terrainObj = GameObject.Find("Terrain");
            if (terrainObj != null) {
                Collider col = terrainObj.GetComponent<Collider>();
                if (col != null) {
                    minBounds = new Vector2(col.bounds.min.x + 50f, col.bounds.min.z + 50f);
                    maxBounds = new Vector2(col.bounds.max.x - 50f, col.bounds.max.z - 50f);
                } else {
                    Renderer rend = terrainObj.GetComponent<Renderer>();
                    if (rend != null) {
                        minBounds = new Vector2(rend.bounds.min.x + 50f, rend.bounds.min.z + 50f);
                        maxBounds = new Vector2(rend.bounds.max.x - 50f, rend.bounds.max.z - 50f);
                    }
                }
            }
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        if (Time.timeScale == 0f) return;

        Vector3 moveInput = Vector3.zero;

        if (Keyboard.current.wKey.isPressed)
        {
            moveInput.z += 1f;
        }
        if (Keyboard.current.sKey.isPressed)
        {
            moveInput.z -= 1f;
        }
        if (Keyboard.current.aKey.isPressed)
        {
            moveInput.x -= 1f;
        }
        if (Keyboard.current.dKey.isPressed)
        {
            moveInput.x += 1f;
        }

        // Calculate movement direction relative to the camera's current rotation (flattened on Y axis)
        Transform camTransform = Camera.main != null ? Camera.main.transform : transform;
        
        Vector3 forward = camTransform.forward;
        forward.y = 0;
        forward.Normalize();
        if (forward == Vector3.zero) forward = Vector3.forward;

        Vector3 right = camTransform.right;
        right.y = 0;
        right.Normalize();
        if (right == Vector3.zero) right = Vector3.right;

        Vector3 moveDir = (forward * moveInput.z + right * moveInput.x).normalized;
        transform.position += moveDir * moveSpeed * Time.deltaTime;

        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minBounds.x, maxBounds.x);
        clampedPos.z = Mathf.Clamp(clampedPos.z, minBounds.y, maxBounds.y);
        transform.position = clampedPos;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (SceneManager.Instance != null) {
                Vector3 basePos = SceneManager.Instance.GetBasePosition();
                transform.position = new Vector3(basePos.x, transform.position.y, basePos.z);
            }
        }
    }
}
