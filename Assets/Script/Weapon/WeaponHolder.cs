using UnityEngine;
using System.Collections.Generic;

public class WeaponHolder : MonoBehaviour
{
    public Transform targetLeft, targetRight;
    public Weapon currentWeapon;
    [SerializeField] private PlayerManagerState playerHolder;
    [SerializeField] private List<Weapon> weapons = new List<Weapon>();
    public WeaponSway weaponSway;
    private int currentWeaponIndex = 0;
    private void Start()
    {
        foreach (Weapon weapon in weapons)
        {
            weapon.playerHolder = playerHolder;
            weapon.rootParent = transform.root;
            weapon.transform.parent.gameObject.SetActive(false);
        }
        if (weapons.Count != 0) SetCurrentWeapon(weapons[0]);
    }
    // Update is called once per frame
    void Update()
    {
        if (currentWeapon == null) return;

        targetLeft.position = currentWeapon.leftHandTarget.position;
        //targetLeft.rotation = currentWeapon.leftHandTarget.rotation;
        targetRight.position = currentWeapon.rightHandTarget.position;
        //targetLeft.rotation = currentWeapon.rightHandTarget.rotation;
    }
    public void AddWeapon(Weapon wp)
    {
        if (wp == null)
        {
            Debug.Log("ko phai weapon");
            return;
        }
        if (currentWeapon != null) currentWeapon.transform.parent.gameObject.SetActive(false);
        weapons.Add(wp);
        wp.playerHolder = playerHolder;
        wp.transform.parent.transform.SetParent(this.transform);
        wp.rootParent = transform.root;
        wp.transform.parent.localRotation = Quaternion.identity;
        wp.transform.parent.localPosition = Vector3.zero;
        wp.transform.parent.localScale = Vector3.one;
        currentWeaponIndex = weapons.Count - 1;
        SetCurrentWeapon(wp);
    }
    public void ChangeWeapon(int i)
    {
        if (weapons.Count <= 1) return;
        currentWeapon.transform.parent.gameObject.SetActive(false);
        currentWeaponIndex += i;
        currentWeaponIndex = Mathf.Abs(currentWeaponIndex % weapons.Count);
        SetCurrentWeapon(weapons[currentWeaponIndex]);
    }
    private void SetCurrentWeapon(Weapon wp)
    {
        currentWeapon = wp;
        weaponSway.targetSway = wp.transform;  
        currentWeapon.transform.parent.gameObject.SetActive(true);
        currentWeapon.transform.parent.localPosition = Vector3.zero;
        currentWeapon.transform.parent.localRotation = Quaternion.identity;
    }
    public void ReleaseAllWeapon()
    {

    }
}
