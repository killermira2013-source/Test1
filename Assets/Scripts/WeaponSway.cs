using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Позиции (Смещение)")]
    [SerializeField] private float moveAmount = 0.02f;
    [SerializeField] private float maxMoveAmount = 0.06f;
    [SerializeField] private float smoothPos = 8f;

    [Header("Sway Вращения (Наклон)")]
    [SerializeField] private float rotationAmount = 4f;
    [SerializeField] private float maxRotation = 10f;
    [SerializeField] private float smoothRot = 10f;

    [Header("Sway Наклона (Z-Tilt / Roll) - ВАЖНО")]
    [SerializeField] private float tiltAmount = 2f;
    [SerializeField] private float maxTilt = 5f;

    [Header("Ссылки")]
    [SerializeField] private Weapon weaponScript;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    private void Update()
    {

        float multiplier = 1f;
        if (weaponScript != null)
        {
            multiplier = 0.1f;
        }

        Vector2 mouseDelta = inputActions.Player.Look.ReadValue<Vector2>();

        float moveX = -mouseDelta.x * moveAmount * multiplier;
        float moveY = -mouseDelta.y * moveAmount * multiplier;

        moveX = Mathf.Clamp(moveX, -maxMoveAmount, maxMoveAmount);
        moveY = Mathf.Clamp(moveY, -maxMoveAmount, maxMoveAmount);

        Vector3 finalPosition = new Vector3(moveX, moveY, 0);

        float rotX = -mouseDelta.y * rotationAmount * multiplier;
        float rotY = mouseDelta.x * rotationAmount * multiplier;
        float rotZ = -mouseDelta.x * tiltAmount * multiplier;

        rotX = Mathf.Clamp(rotX, -maxRotation, maxRotation);
        rotY = Mathf.Clamp(rotY, -maxRotation, maxRotation);
        rotZ = Mathf.Clamp(rotZ, -maxTilt, maxTilt);

        Quaternion swayRotation = Quaternion.Euler(rotX, rotY, rotZ);

        transform.localPosition = Vector3.Lerp(transform.localPosition, initialPosition + finalPosition, Time.deltaTime * smoothPos);
        Quaternion targetAnchor = initialRotation;
        if (weaponScript != null)
        {
            targetAnchor = weaponScript.AnchorRotation;
        }
        transform.localRotation = Quaternion.Slerp(transform.localRotation, swayRotation * targetAnchor, Time.deltaTime * smoothRot);
    }
}