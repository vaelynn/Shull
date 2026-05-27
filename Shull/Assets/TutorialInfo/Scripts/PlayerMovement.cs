using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float crouchSpeed = 1.75f;

    [Header("Animation Parameters")]
    [SerializeField] private string isMovingParam = "IsMoving";
    [SerializeField] private string isCrouchingParam = "IsCrouching";
    [SerializeField] private string attackTriggerParam = "Attack";

    [Header("Physics")]
    [SerializeField] private float gravity = -20f;

    private Animator animator;
    private CharacterController characterController;
    private float verticalVelocity;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
        HandleAttack();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(horizontal, 0f, vertical);
        Vector3 moveDirection = Vector3.ClampMagnitude(input, 1f);

        bool isCrouching = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isCrouching ? crouchSpeed : walkSpeed;
        Vector3 worldMove = transform.TransformDirection(moveDirection) * currentSpeed;

        if (characterController != null)
        {
            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * Time.deltaTime;
            Vector3 velocity = worldMove + Vector3.up * verticalVelocity;
            characterController.Move(velocity * Time.deltaTime);
        }
        else
        {
            transform.position += worldMove * Time.deltaTime;
        }

        bool isMoving = moveDirection.sqrMagnitude > 0.01f;
        SetAnimatorBool(isMovingParam, isMoving);
        SetAnimatorBool(isCrouchingParam, isCrouching);
    }

    private void HandleAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SetAnimatorTrigger(attackTriggerParam);
        }
    }

    private void SetAnimatorBool(string parameterName, bool value)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        animator.SetBool(parameterName, value);
    }

    private void SetAnimatorTrigger(string parameterName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            return;
        }

        animator.SetTrigger(parameterName);
    }
}
