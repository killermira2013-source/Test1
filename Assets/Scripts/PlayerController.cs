using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Настройки движения")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float gravity = -15f;
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Настройки камеры и обзора")]
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float lookXLimit = 85f;

    [Header("Настройки Приседания")]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    [Header("Настройки Наклонов (Q/E)")]
    [SerializeField] private float leanAngle = 15f;
    [SerializeField] private float leanOffset = 0.5f;
    [SerializeField] private float leanSpeed = 10f;

    [Header("Настройки Высоты Глаз")]
    [SerializeField] private float eyeHeightStanding = 1.6f;
    [SerializeField] private float eyeHeightCrouching = 0.9f;

    private CharacterController characterController;
    private InputSystem_Actions inputActions;

    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;

    private float currentHeight;
    private float targetLean = 0f;
    private float currentLean = 0f;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        characterController = GetComponent<CharacterController>();
        currentHeight = standHeight;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleCrouch();
        HandleLean();
    }

    private void HandleMovement()
    {
        bool isCrouching = inputActions.Player.Crouch.IsPressed();
        float currentSpeed = isCrouching ? crouchSpeed : walkSpeed;

        Vector2 inputVector = inputActions.Player.Move.ReadValue<Vector2>();

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        float curSpeedX = currentSpeed * inputVector.y;
        float curSpeedY = currentSpeed * inputVector.x;

        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        if (inputActions.Player.Jump.WasPressedThisFrame() && characterController.isGrounded && !isCrouching)
        {
            moveDirection.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        if (!characterController.isGrounded)
        {
            moveDirection.y += gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleRotation()
    {
        Vector2 mouseDelta = inputActions.Player.Look.ReadValue<Vector2>();

        rotationX += -mouseDelta.y * mouseSensitivity;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

        playerCamera.localRotation = Quaternion.Euler(rotationX, 0, 0);

        transform.rotation *= Quaternion.Euler(0, mouseDelta.x * mouseSensitivity, 0);
    }

    private void HandleCrouch()
    {
        bool isCrouching = inputActions.Player.Crouch.IsPressed();

        float targetControllerHeight = isCrouching ? crouchHeight : standHeight;
        float targetEyeHeight = isCrouching ? eyeHeightCrouching : eyeHeightStanding;

        characterController.height = Mathf.Lerp(characterController.height, targetControllerHeight, Time.deltaTime * crouchTransitionSpeed);
        characterController.center = Vector3.up * (characterController.height / 2f);

        Vector3 currentCamPos = cameraRoot.localPosition;
        Vector3 targetCamPos = new Vector3(0, targetEyeHeight, 0);

        cameraRoot.localPosition = Vector3.Lerp(currentCamPos, targetCamPos, Time.deltaTime * crouchTransitionSpeed);
    }

    private void HandleLean()
    {
        float inputLean = inputActions.Player.Lean.ReadValue<float>();
        targetLean = inputLean;
        currentLean = Mathf.Lerp(currentLean, targetLean, Time.deltaTime * leanSpeed);
        Quaternion targetRotation = Quaternion.Euler(0, 0, -currentLean * leanAngle);
        float targetX = currentLean * leanOffset;
        cameraRoot.localRotation = targetRotation;
        Vector3 newPos = cameraRoot.localPosition;
        newPos.x = targetX;
        cameraRoot.localPosition = newPos;
    }
    public void SetSensitivity(float newSens)
    {
        mouseSensitivity = newSens;
    }
    public float GetSensitivity()
    {
        return mouseSensitivity;
    }
}