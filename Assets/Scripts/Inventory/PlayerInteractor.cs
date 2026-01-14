using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactLayer;
    [SerializeField] private TextMeshProUGUI promptText;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        CheckInteraction();
    }

    private void CheckInteraction()
    {
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, interactLayer))
        {
            if (hit.collider.TryGetComponent(out IInteractable interactable))
            {
                if (promptText)
                {
                    promptText.text = $"[F] {interactable.GetDescription()}";
                    promptText.gameObject.SetActive(true);
                }

                if (Keyboard.current.fKey.wasPressedThisFrame)
                {
                    interactable.Interact();
                }
                return;
            }
        }

        if (promptText) promptText.gameObject.SetActive(false);
    }
}