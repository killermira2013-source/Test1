using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    [Header("UI —сылки")]
    [SerializeField] private GameObject inventoryPanel;

    private bool isOpen = false;

    [SerializeField] private MonoBehaviour playerControllerScript;
    [SerializeField] private MonoBehaviour weaponScript;

    private void Start()
    {
        inventoryPanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryPanel.SetActive(isOpen);

        if (isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerControllerScript) playerControllerScript.enabled = false;
            if (weaponScript) weaponScript.enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerControllerScript) playerControllerScript.enabled = true;
            if (weaponScript) weaponScript.enabled = true;
        }
    }
}