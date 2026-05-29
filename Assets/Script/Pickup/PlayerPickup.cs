using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPickup : MonoBehaviour
{
    // iemInfoUI
    [Header("Item Info UI")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private WeaponHolder weaponHolder;
    public void ShowPickupItemInfo(PickupAbleItem item)
    {
        // can anh, ten
        //itemIcon.enabled = true;
        //itemName.enabled = true;
        //itemIcon.sprite = item.Icon;
        //itemName.text = item.ItemName;
        weaponHolder.AddWeapon(item as Weapon);

    }
    public void ClosePickupItemInfo()
    {
        //itemIcon.enabled = false;
        //itemName.enabled = false;
    }
    

}
