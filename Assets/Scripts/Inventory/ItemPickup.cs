using UnityEngine;

public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;

    public void Interact()
    {
        InventoryManager.Instance.AddItem(itemData);

        Destroy(gameObject);
    }

    public string GetDescription()
    {
        if (itemData != null) return $"Подобрать {itemData.itemName}";
        return "Подобрать предмет";
    }
}