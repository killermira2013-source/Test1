using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI Настройки")]
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private GameObject itemIconPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(ItemData newItem)
    {
        foreach (Transform slot in itemsContainer)
        {
            if (slot.childCount == 0)
            {
                GameObject iconObj = Instantiate(itemIconPrefab, slot);

                DraggableItem draggable = iconObj.GetComponent<DraggableItem>();
                if (draggable != null)
                {
                    draggable.Initialize(newItem);
                }
                return;
            }
        }

        Debug.Log("Инвентарь полон! Некуда положить предмет.");
    }
}