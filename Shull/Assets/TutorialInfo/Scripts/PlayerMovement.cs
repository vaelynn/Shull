using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float turnSpeed = 540f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundSnapProbeHeight = 2f;
    [SerializeField] private float groundSnapMaxDistance = 10f;
    [SerializeField] private float groundSnapPadding = 0.02f;
    [SerializeField] private bool autoFitCharacterController = true;
    [SerializeField] private float controllerHeightPadding = 0.05f;

    private CharacterController characterController;
    private Transform cameraTransform;
    private float verticalVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        cameraTransform = Camera.main != null ? Camera.main.transform : null;
    }

    private IEnumerator Start()
    {
        yield return null;
        if (autoFitCharacterController)
        {
            FitCharacterControllerToRenderers();
        }
        SnapToGround();
    }

    private void FitCharacterControllerToRenderers()
    {
        if (characterController == null)
        {
            return;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
            {
                continue;
            }

            bounds.Encapsulate(renderers[i].bounds);
        }

        float scaleY = Mathf.Abs(transform.lossyScale.y);
        float scaleX = Mathf.Abs(transform.lossyScale.x);
        float scaleZ = Mathf.Abs(transform.lossyScale.z);

        if (scaleY <= 0.0001f)
        {
            return;
        }

        float height = Mathf.Max(0.5f, (bounds.size.y / scaleY) + controllerHeightPadding);
        float radiusWorld = Mathf.Max(bounds.extents.x, bounds.extents.z);
        float scaleXZ = Mathf.Max(0.0001f, Mathf.Max(scaleX, scaleZ));
        float radius = Mathf.Max(0.05f, (radiusWorld / scaleXZ) * 0.5f);

        Vector3 localCenter = transform.InverseTransformPoint(bounds.center);

        characterController.enabled = false;
        characterController.height = height;
        characterController.radius = radius;
        characterController.center = localCenter;
        characterController.enabled = true;
    }

    private void Update()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        HandleMovement();
    }

    private void SnapToGround()
    {
        if (characterController == null)
        {
            return;
        }

        Vector3 origin = transform.position + Vector3.up * groundSnapProbeHeight;
        Ray ray = new Ray(origin, Vector3.down);

        if (!TryGetGroundHit(ray, groundSnapProbeHeight + groundSnapMaxDistance, out RaycastHit hit))
        {
            return;
        }

        float currentBottom = characterController.bounds.min.y;
        float deltaY = (hit.point.y - currentBottom) + groundSnapPadding;
        characterController.Move(Vector3.up * deltaY);
        verticalVelocity = 0f;
    }

    private bool TryGetGroundHit(Ray ray, float maxDistance, out RaycastHit hit)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, ~0, QueryTriggerInteraction.Ignore);
        if (hits == null || hits.Length == 0)
        {
            hit = default;
            return false;
        }

        float bestDistance = float.MaxValue;
        int bestIndex = -1;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider c = hits[i].collider;
            if (c == null)
            {
                continue;
            }

            if (c == characterController)
            {
                continue;
            }

            if (c.transform != null && c.transform.IsChildOf(transform))
            {
                continue;
            }

            if (hits[i].distance < bestDistance)
            {
                bestDistance = hits[i].distance;
                bestIndex = i;
            }
        }

        if (bestIndex < 0)
        {
            hit = default;
            return false;
        }

        hit = hits[bestIndex];
        return true;
    }

    private void HandleMovement()
    {
        Vector2 moveInput = GetMoveInput();

        float turnInput = moveInput.x;
        float forwardInput = moveInput.y;

        if (Mathf.Abs(turnInput) > 0.0001f)
        {
            transform.Rotate(0f, turnInput * turnSpeed * Time.deltaTime, 0f, Space.World);
        }

        Vector3 flatForward = transform.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude > 0.0001f)
        {
            flatForward.Normalize();
        }

        Vector3 moveDirection = flatForward * forwardInput;

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 horizontalVelocity = moveDirection * moveSpeed;
        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);
    }

    private Vector2 GetMoveInput()
    {
        Vector2 input = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                input.x -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                input.x += 1f;
            }

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                input.y += 1f;
            }

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                input.y -= 1f;
            }
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (input.sqrMagnitude < 0.0001f)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
        }
#endif

        return Vector2.ClampMagnitude(input, 1f);
    }
}
