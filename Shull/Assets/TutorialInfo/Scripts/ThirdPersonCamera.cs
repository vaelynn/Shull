using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followSmoothTime = 0.1f;
    [SerializeField] private float lookAtHeight = 1.4f;
    [SerializeField] private float distance = 4f;
    [SerializeField] private float height = 2f;
    [SerializeField] private float pitch = 20f;
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float rotateSensitivity = 2.5f;

    private Vector3 currentVelocity;
    private float yaw;
    private bool isRotating;

    private void Start()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }

        yaw = transform.eulerAngles.y;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        UpdateRotation();

        Vector3 desiredPosition = GetDesiredPosition();
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            followSmoothTime
        );

        Vector3 lookPoint = target.position + Vector3.up * lookAtHeight;
        transform.LookAt(lookPoint);
    }

    private void UpdateRotation()
    {
        if (Input.GetMouseButtonDown(1))
        {
            isRotating = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isRotating = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (!isRotating)
        {
            return;
        }

        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        yaw += mouseX * rotateSensitivity;
        pitch -= mouseY * rotateSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private Vector3 GetDesiredPosition()
    {
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * (Vector3.back * distance) + Vector3.up * height;
        return target.position + offset;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
