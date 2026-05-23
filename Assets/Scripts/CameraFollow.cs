using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 offset = Vector2.zero;
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = false;
    [SerializeField] private float minX = float.NegativeInfinity;
    [SerializeField] private float maxX = float.PositiveInfinity;
    [SerializeField] private float minY = float.NegativeInfinity;
    [SerializeField] private float maxY = float.PositiveInfinity;

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = transform.position;
        if (followX) desired.x = target.position.x + offset.x;
        if (followY) desired.y = target.position.y + offset.y;

        desired.x = Mathf.Clamp(desired.x, minX, maxX);
        desired.y = Mathf.Clamp(desired.y, minY, maxY);

        transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
    }
}
