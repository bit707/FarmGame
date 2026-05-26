using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float distance = 10f;
    [SerializeField] float height = 6f;
    [SerializeField] float smoothSpeed = 5f;
    [SerializeField] float rotationSpeed = 100f;
    [SerializeField] float minDistance = 5f;
    [SerializeField] float maxDistance = 15f;
    [SerializeField] float minVerticalAngle = 20f;
    [SerializeField] float maxVerticalAngle = 60f;

    float currentAngle;
    float currentVerticalAngle = 40f;
    float currentDistance;

    void Start()
    {
        currentDistance = distance;
        currentVerticalAngle = 40f;
        if (target == null)
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null) target = player.transform;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleInput();

        Quaternion rotation = Quaternion.Euler(currentVerticalAngle, currentAngle, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -currentDistance);
        Vector3 desiredPosition = target.position + Vector3.up * 1.5f + offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }

    void HandleInput()
    {
        if (Input.GetMouseButton(1))
        {
            currentAngle += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            currentVerticalAngle -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        currentDistance -= scroll * 3f;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
    }
}
