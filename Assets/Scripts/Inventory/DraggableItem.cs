using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public ItemData itemData;

    private Image image;
    private CanvasGroup canvasGroup;
    private Transform canvasTransform;

    private void Awake()
    {
        image = GetComponent<Image>();

        canvasGroup = GetComponent<CanvasGroup>();

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvasTransform = canvas.transform;
        }
    }

    public void Initialize(ItemData data)
    {
        if (data == null)
        {
            Debug.LogError("ОШИБКА: В DraggableItem пришли пустые данные (null)! Проверь ItemPickup на сцене.");
            return;
        }

        itemData = data;

        if (image == null)
        {
            image = GetComponent<Image>();
        }

        if (image != null)
        {
            image.sprite = data.icon;
        }
        else
        {
            Debug.LogError("ОШИБКА: На префабе иконки нет компонента Image!");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentAfterDrag = transform.parent;

        if (canvasTransform != null)
        {
            transform.SetParent(canvasTransform);
            transform.SetAsLastSibling();
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.6f;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Mouse.current != null)
        {
            transform.position = Mouse.current.position.ReadValue();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            DropItemToWorld();
            Destroy(gameObject);
        }
        else
        {
            transform.SetParent(parentAfterDrag);
            transform.localPosition = Vector3.zero;
        }
    }

    private void DropItemToWorld()
    {
        if (Camera.main == null) return;

        Transform playerTransform = Camera.main.transform;
        Vector3 dropPosition = playerTransform.position + playerTransform.forward * 1.5f;

        if (itemData != null && itemData.prefab != null)
        {
            Instantiate(itemData.prefab, dropPosition, Quaternion.identity);
        }
    }
}