using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float runSpeed = 7f;
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] float gravity = -20f;

    [Header("Animation")]
    [SerializeField] Animator animator;

    CharacterController controller;
    Transform cameraTransform;
    Vector3 velocity;
    Vector2 moveInput;
    bool isRunning;

    static readonly int AnimSpeed = Animator.StringToHash("Speed");
    static readonly int AnimIsGrounded = Animator.StringToHash("IsGrounded");

    public Vector3 Position => transform.position;
    public bool IsMoving => moveInput.sqrMagnitude > 0.01f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        ApplyGravity();
        Move();
        UpdateAnimator();
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;
        velocity.y += gravity * Time.deltaTime;
    }

    void Move()
    {
        if (moveInput.sqrMagnitude < 0.01f)
        {
            controller.Move(velocity * Time.deltaTime);
            return;
        }

        float speed = isRunning ? runSpeed : walkSpeed;
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 direction = forward * moveInput.y + right * moveInput.x;
        direction.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        controller.Move((direction * speed + velocity) * Time.deltaTime);
    }

    void UpdateAnimator()
    {
        if (animator == null) return;
        float speed = IsMoving ? (isRunning ? 1f : 0.5f) : 0f;
        animator.SetFloat(AnimSpeed, speed, 0.1f, Time.deltaTime);
        animator.SetBool(AnimIsGrounded, controller.isGrounded);
    }

    public void OnMove(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    public void OnRun(InputAction.CallbackContext ctx)
    {
        if (ctx.started) isRunning = true;
        else if (ctx.canceled) isRunning = false;
    }
}
