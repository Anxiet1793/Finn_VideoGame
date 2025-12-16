using UnityEngine;

public class CameraFollow2DClamp : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 8f;
    public Vector3 offset = new Vector3(0, 0, -10);

    public float minX, maxX, minY, maxY;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (!target) return;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        Vector3 desired = new Vector3(target.position.x, target.position.y, 0f) + offset;

        desired.x = Mathf.Clamp(desired.x, minX + halfW, maxX - halfW);
        desired.y = Mathf.Clamp(desired.y, minY + halfH, maxY - halfH);

        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    }
}
