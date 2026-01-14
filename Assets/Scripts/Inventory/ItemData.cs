using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName = "New Item";
    public Sprite icon;          // Картинка для инвентаря
    public GameObject prefab;    // 3D модель (чтобы выкинуть обратно)
    [TextArea] public string description; // Описание
}