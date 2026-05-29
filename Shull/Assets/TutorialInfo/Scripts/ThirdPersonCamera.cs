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

    // The camera's self-owned offset from the target, expressed in world space.
    // Only ever written by orbit input — never by the player's rotation.
    private Vector3 offsetWorld;

    // The camera's self-owned rotation — set once after an orbit, then frozen.
    // Never inherited from the player's transform.
    private Quaternion cachedRotation;

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
        if (target == null) return;

        HandleOrbitInput();

        // Desired position is always target + our fixed world-space offset
        Vector3 desiredPosition = target.position + offsetWorld;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            followSmoothTime
        );

        // Apply the camera's own rotation — never the player's
        transform.rotation = cachedRotation;
    }

    private void DetachFromParent()
    {
        if (transform.parent != null)
        {
            transform.SetParent(null, true);
        }
    }

    // Renamed from UpdateRotation — makes it clear this is purely input-driven
    private void HandleOrbitInput()
    {
        bool rightDown = false;
        bool rightUp   = false;
        bool rightHeld = false;

#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            rightDown = Mouse.current.rightButton.wasPressedThisFrame;
            rightUp   = Mouse.current.rightButton.wasReleasedThisFrame;
            rightHeld = Mouse.current.rightButton.isPressed;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        rightDown |= Input.GetMouseButtonDown(1);
        rightUp   |= Input.GetMouseButtonUp(1);
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

        if (!isRotating || !rightHeld) return;

        Vector2 delta = GetMouseDelta();
        yaw   += delta.x * rotateSensitivity;
        pitch -= delta.y * rotateSensitivity;
        pitch  = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Recompute our offset and rotation from the new yaw/pitch
        offsetWorld = GetOrbitOffset();

        // Point the camera at the target's look-at height from the new position
        Vector3 lookPoint = target != null
            ? target.position + Vector3.up * lookAtHeight
            : transform.position + transform.forward;
        cachedRotation = Quaternion.LookRotation(lookPoint - (target.position + offsetWorld));
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
        Vector3 mousePos = Input.mousePosition;
        Vector3 delta    = mousePos - lastMousePosition;
        lastMousePosition = mousePos;
        return new Vector2(delta.x, delta.y);
    }

    private Vector3 GetOrbitOffset()
    {
        float yawRad   = yaw   * Mathf.Deg2Rad;
        float pitchRad = pitch * Mathf.Deg2Rad;
        float cosPitch = Mathf.Cos(pitchRad);

        return new Vector3(
            Mathf.Sin(yawRad) * cosPitch,
            Mathf.Sin(pitchRad),
            Mathf.Cos(yawRad) * cosPitch
        ) * orbitRadius;
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
            yaw            = transform.eulerAngles.y;
            pitch          = Mathf.Clamp(pitch, minPitch, maxPitch);
            offsetWorld    = Vector3.zero;
            cachedRotation = transform.rotation;
            return;
        }

        offsetWorld = transform.position - target.position;
        float planar = new Vector2(offsetWorld.x, offsetWorld.z).magnitude;

        if (offsetWorld.sqrMagnitude > 0.0001f)
        {
            orbitRadius = offsetWorld.magnitude;
        }

        if (planar > 0.0001f)
        {
            yaw   = Mathf.Atan2(offsetWorld.x, offsetWorld.z) * Mathf.Rad2Deg;
            pitch = Mathf.Atan2(offsetWorld.y, planar)        * Mathf.Rad2Deg;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
        else
        {
            yaw   = transform.eulerAngles.y;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // Derive the initial rotation from where the camera actually is,
        // pointed at the target — no dependence on the player's facing
        Vector3 lookPoint = target.position + Vector3.up * lookAtHeight;
        if ((lookPoint - transform.position).sqrMagnitude > 0.0001f)
        {
            cachedRotation = Quaternion.LookRotation(lookPoint - transform.position);
        }
        else
        {
            cachedRotation = transform.rotation;
        }
    }
}