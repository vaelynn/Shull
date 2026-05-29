using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ThirdPersonCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float followSmoothTime = 0.1f;
    [SerializeField] private float lookAtHeight = 1.4f;
    [SerializeField] private float orbitRadius = 6f;
    [SerializeField] private float pitch = 20f;
    [SerializeField] private float minPitch = -10f;
    [SerializeField] private float maxPitch = 80f;
    [SerializeField] private float rotateSensitivity = 0.15f;
    [SerializeField] private bool lockCursorWhileRotating = true;

    private Vector3 currentVelocity;
    private float yaw;
    private bool isRotating;
    private Vector3 lastMousePosition;
    private Vector3 followOffsetWorld;

    private void Awake()
    {
        DetachFromParent();
    }

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

        RebuildFollowState();
    }

    private void LateUpdate()
    {
        DetachFromParent();
        if (target == null)
        {
            return;
        }

        UpdateRotation();

        Vector3 desiredPosition = isRotating ? GetDesiredOrbitPosition() : GetDesiredFollowPosition();
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            followSmoothTime
        );

        if (isRotating)
        {
            Vector3 lookPoint = target.position + Vector3.up * lookAtHeight;
            transform.LookAt(lookPoint);
        }
    }

    private void DetachFromParent()
    {
        if (transform.parent == null)
        {
            return;
        }

        transform.SetParent(null, true);
    }

    private void UpdateRotation()
    {
        bool rightDown = false;
        bool rightUp = false;
        bool rightHeld = false;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            rightDown = Mouse.current.rightButton.wasPressedThisFrame;
            rightUp = Mouse.current.rightButton.wasReleasedThisFrame;
            rightHeld = Mouse.current.rightButton.isPressed;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        rightDown |= Input.GetMouseButtonDown(1);
        rightUp |= Input.GetMouseButtonUp(1);
        rightHeld |= Input.GetMouseButton(1);
#endif

        if (rightDown)
        {
            isRotating = true;
            lastMousePosition = GetMousePosition();
            if (lockCursorWhileRotating)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        if (rightUp)
        {
            isRotating = false;
            if (lockCursorWhileRotating)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        if (!isRotating || !rightHeld)
        {
            return;
        }

        Vector2 delta = GetMouseDelta();
        yaw += delta.x * rotateSensitivity;
        pitch -= delta.y * rotateSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        followOffsetWorld = GetOrbitOffset();
    }

    private Vector3 GetMousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            Vector2 pos = Mouse.current.position.ReadValue();
            return new Vector3(pos.x, pos.y, 0f);
        }
#endif

        return Input.mousePosition;
    }

    private Vector2 GetMouseDelta()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            return Mouse.current.delta.ReadValue();
        }
#endif

        Vector3 mousePosition = Input.mousePosition;
        Vector3 delta = mousePosition - lastMousePosition;
        lastMousePosition = mousePosition;
        return new Vector2(delta.x, delta.y);
    }

    private Vector3 GetDesiredOrbitPosition()
    {
        return target.position + GetOrbitOffset();
    }

    private Vector3 GetDesiredFollowPosition()
    {
        return target.position + followOffsetWorld;
    }

    private Vector3 GetOrbitOffset()
    {
        float yawRad = yaw * Mathf.Deg2Rad;
        float pitchRad = pitch * Mathf.Deg2Rad;
        float cosPitch = Mathf.Cos(pitchRad);

        Vector3 direction = new Vector3(
            Mathf.Sin(yawRad) * cosPitch,
            Mathf.Sin(pitchRad),
            Mathf.Cos(yawRad) * cosPitch
        );

        return direction * orbitRadius;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        RebuildFollowState();
    }

    private void RebuildFollowState()
    {
        if (target == null)
        {
            yaw = transform.eulerAngles.y;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            followOffsetWorld = Vector3.zero;
            return;
        }

        followOffsetWorld = transform.position - target.position;
        float planar = new Vector2(followOffsetWorld.x, followOffsetWorld.z).magnitude;
        if (followOffsetWorld.sqrMagnitude > 0.0001f)
        {
            orbitRadius = followOffsetWorld.magnitude;
        }

        if (planar > 0.0001f)
        {
            yaw = Mathf.Atan2(followOffsetWorld.x, followOffsetWorld.z) * Mathf.Rad2Deg;
            pitch = Mathf.Atan2(followOffsetWorld.y, planar) * Mathf.Rad2Deg;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
        else
        {
            yaw = transform.eulerAngles.y;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
    }
}
