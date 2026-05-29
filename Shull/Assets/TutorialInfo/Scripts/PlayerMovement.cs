using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float turnSpeed = 540f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundSnapProbeHeight = 2f;
    [SerializeField] private float groundSnapMaxDistance = 10f;
    [SerializeField] private float groundSnapPadding = 0.02f;

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
        SnapToGround();
    }

    private void Update()
    {
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

        float halfHeight = characterController.bounds.extents.y;
        Vector3 pos = transform.position;
        pos.y = hit.point.y + halfHeight + groundSnapPadding;

        characterController.enabled = false;
        transform.position = pos;
        characterController.enabled = true;
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
            if (c == null || c == characterController)
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
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(inputX, 0f, inputZ);
        Vector3 inputNormalized = Vector3.ClampMagnitude(input, 1f);

        Vector3 moveDirection = inputNormalized;
        if (cameraTransform != null)
        {
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0f;
            camForward.Normalize();

            Vector3 camRight = cameraTransform.right;
            camRight.y = 0f;
            camRight.Normalize();

            moveDirection = (camForward * inputNormalized.z + camRight * inputNormalized.x).normalized;
        }

        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 horizontalVelocity = moveDirection * moveSpeed;
        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
        characterController.Move(velocity * Time.deltaTime);

        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }
    }
}
