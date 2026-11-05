using UnityEngine;

/// <summary>
/// Optional simple camera follow. Put on Main Camera only if not using Cinemachine
/// or another camera-follow implementation.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float smoothTime = 0.12f;

    private Transform target;
    private Vector3 velocity;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(
            transform.position, desiredPosition, ref velocity, smoothTime);
    }
}
