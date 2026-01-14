using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Weapon : MonoBehaviour
{
    [Header("=== Характеристики ===")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float fireRate = 0.1f;
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private float bulletSpeed = 200f;

    [Header("=== Прицеливание (ADS) ===")]
    [SerializeField] private Vector3 aimPosition;
    [SerializeField] private Vector3 aimRotation;
    [SerializeField] private float aimSpeed = 10f;
    [SerializeField] private bool aimIsToggle = false;
    [Tooltip("Насколько сильно гасить анимацию в прицеле. 0 = трясет как обычно, 1 = руки замерли намертво.")]
    [Range(0f, 1f)]
    [SerializeField] private float aimStability = 0.8f;

    [Header("=== Тайминги Анимаций ===")]
    [SerializeField] private float reloadTime = 2.0f;
    [SerializeField] private float drawTime = 1.0f;

    [Header("=== Ссылки ===")]
    [SerializeField] private Camera fpsCamera;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("=== Аниматоры ===")]
    [SerializeField] private Animator handsAnimator;
    [SerializeField] private Animator gunAnimator;

    [Header("=== Отдача (Recoil) ===")]
    [SerializeField] private WeaponRecoil weaponRecoil;
    [SerializeField] private CameraRecoil cameraRecoil;

    [Header("=== Старт ===")]
    [SerializeField] private bool startHolstered = false;

    public bool IsAiming { get; private set; }
    public Quaternion AnchorRotation { get; private set; }

    private int currentAmmo;
    private float nextFireTime = 0f;
    private bool isHolstered = false;
    private bool isBusy = false;

    private Vector3 hipPosition;
    private Quaternion hipRotation; 
    private bool isAimingToggleState = false;

    private bool shouldShootThisFrame = false;

    private Coroutine currentRoutine = null;
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void Start()
    {
        currentAmmo = maxAmmo;
        GameUI.Instance.UpdateAmmo(currentAmmo, maxAmmo);
        if (playerController == null) playerController = GetComponentInParent<CharacterController>();

        hipPosition = transform.localPosition;
        hipRotation = transform.localRotation;
        AnchorRotation = hipRotation;

        if (startHolstered) { isHolstered = true; }
        else { isHolstered = true; StartAction(DrawSequence()); }
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    private void Update()
    {
        HandleMovementAnimation();
        bool isTriggerHeld = inputActions.Player.Attack.IsPressed();
        if (handsAnimator != null)
        {
            handsAnimator.SetBool("TriggerHold", isTriggerHeld);
        }
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            if (isHolstered) StartAction(DrawSequence());
            else StartAction(HolsterSequence());
        }

        HandleAiming();

        if (isHolstered || isBusy) return;

        if ((Keyboard.current.rKey.wasPressedThisFrame && currentAmmo < maxAmmo) ||
            (currentAmmo <= 0 && inputActions.Player.Attack.IsPressed()))
        {
            StartAction(ReloadSequence());
            return;
        }

        if (inputActions.Player.Attack.IsPressed() && Time.time >= nextFireTime && currentAmmo > 0)
        {
            nextFireTime = Time.time + fireRate;
            shouldShootThisFrame = true;
        }
    }

    private void HandleAiming()
    {
        if (isHolstered)
        {
            IsAiming = false;
            isAimingToggleState = false;
            transform.localPosition = Vector3.Lerp(transform.localPosition, hipPosition, Time.deltaTime * aimSpeed);
            return;
        }

        if (aimIsToggle)
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                isAimingToggleState = !isAimingToggleState;
            }
            IsAiming = isAimingToggleState;
        }
        else
        {
            IsAiming = Mouse.current.rightButton.isPressed;
        }

        Vector3 targetPos = IsAiming ? aimPosition : hipPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * aimSpeed);

        Quaternion targetRot = IsAiming ? Quaternion.Euler(aimRotation) : hipRotation;
        AnchorRotation = Quaternion.Slerp(AnchorRotation, targetRot, Time.deltaTime * aimSpeed);

        float targetValue = IsAiming ? aimStability : 0f;

        if (handsAnimator != null)
        {
            float current = handsAnimator.GetFloat("AimBlend");
            float next = Mathf.Lerp(current, targetValue, Time.deltaTime * 10f);
            handsAnimator.SetFloat("AimBlend", next);
        }

        if (gunAnimator != null)
        {
            float current = gunAnimator.GetFloat("AimBlend");
            float next = Mathf.Lerp(current, targetValue, Time.deltaTime * 10f);
            gunAnimator.SetFloat("AimBlend", next);
        }
    }

    private void LateUpdate()
    {
        if (shouldShootThisFrame)
        {
            PerformShoot();
            shouldShootThisFrame = false;
        }
    }

    private void PerformShoot()
    {
        currentAmmo--;
        GameUI.Instance.UpdateAmmo(currentAmmo, maxAmmo);

        if (handsAnimator) handsAnimator.SetTrigger("Fire");
        if (gunAnimator) gunAnimator.SetTrigger("Fire");
        if (muzzleFlash) muzzleFlash.Play();
        if (gunAnimator) gunAnimator.Play("Fire", 1, 0f);

        if (weaponRecoil) weaponRecoil.RecoilFire(IsAiming);
        if (cameraRecoil) cameraRecoil.RecoilFire(IsAiming);

        Vector3 shootDirection = firePoint.forward;

        Bullet newBullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        newBullet.Setup(damage, shootDirection, bulletSpeed, playerController.velocity);
    }

    private void StartAction(IEnumerator newRoutine)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(newRoutine);
    }

    private void HandleMovementAnimation()
    {
        Vector2 inputVector = inputActions.Player.Move.ReadValue<Vector2>();
        bool isMoving = inputVector.magnitude > 0.1f;
        if (handsAnimator) handsAnimator.SetBool("IsMoving", isMoving);
        if (gunAnimator) gunAnimator.SetBool("IsMoving", isMoving);
        if (handsAnimator)
        {
            handsAnimator.SetFloat("InputX", inputVector.x, 0.1f, Time.deltaTime);
            handsAnimator.SetFloat("InputY", inputVector.y, 0.1f, Time.deltaTime);
        }
        if (gunAnimator)
        {
            gunAnimator.SetFloat("InputX", inputVector.x, 0.1f, Time.deltaTime);
            gunAnimator.SetFloat("InputY", inputVector.y, 0.1f, Time.deltaTime);
        }
    }

    private IEnumerator DrawSequence()
    {
        isBusy = true; isHolstered = false;
        if (handsAnimator) handsAnimator.ResetTrigger("Holster");
        if (gunAnimator) gunAnimator.ResetTrigger("Holster");
        if (handsAnimator) handsAnimator.SetTrigger("Draw");
        if (gunAnimator) gunAnimator.SetTrigger("Draw");

        yield return new WaitForSeconds(drawTime);

        isBusy = false; currentRoutine = null;
    }

    private IEnumerator HolsterSequence()
    {
        isBusy = true; isHolstered = true;
        if (handsAnimator) { handsAnimator.ResetTrigger("Draw"); handsAnimator.ResetTrigger("Reload"); }
        if (gunAnimator) { gunAnimator.ResetTrigger("Draw"); gunAnimator.ResetTrigger("Reload"); }
        if (handsAnimator) handsAnimator.SetTrigger("Holster");
        if (gunAnimator) gunAnimator.SetTrigger("Holster");

        yield return new WaitForSeconds(drawTime);

        isBusy = false; currentRoutine = null;
    }

    private IEnumerator ReloadSequence()
    {
        isBusy = true;
        if (handsAnimator) handsAnimator.SetTrigger("Reload");
        if (gunAnimator) gunAnimator.SetTrigger("Reload");

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        GameUI.Instance.UpdateAmmo(currentAmmo, maxAmmo);
        isBusy = false; currentRoutine = null;
    }
}