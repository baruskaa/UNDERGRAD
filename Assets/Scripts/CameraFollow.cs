using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;           // Drag your Player GameObject here
    public float smoothSpeed = 5f;      // Higher = snappier, lower = more delayed/floaty
    public Vector3 offset = new Vector3(0f, 0f, -10f); // Keep Z at -10 for 2D so camera stays in front

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
