using UnityEngine;

public abstract class PickupAbleItem : MonoBehaviour
{
    [Header("Pickup Able Item Settings")]
    [SerializeField] private Sprite icon;
    [SerializeField] private string itemName;
    public Sprite Icon { get { return icon; } }
    public string ItemName { get { return itemName; } }
}
