using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 20f;

    void Update()
    {
        if (Keyboard.current == null) return;

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
    }
}
