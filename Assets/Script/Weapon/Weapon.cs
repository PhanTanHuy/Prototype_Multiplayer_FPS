using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected int damage;
    [SerializeField] protected int cost;
    public Transform leftHandTarget, rightHandTarget;
    [HideInInspector] public PlayerManagerState playerHolder;
    [SerializeField] protected float aimSentivity = 2f;
    public float AimSentivity { get { return aimSentivity; } }
    [HideInInspector] public Transform rootParent;
    [HideInInspector] public WeaponAnimator weaponAnimator;
    public Sprite imageWeapon;

    private void Awake()
    {
        weaponAnimator = transform.parent.GetComponent<WeaponAnimator>();
    }
    public virtual int CurrentAmmo { get; }
    public virtual int Mag { get; }

    public abstract void WeaponAttack(float aimValue);
    public abstract void WeaponReload();
    public abstract void WeaponReloadDone();
    public virtual void PlayReloadVisuals() { }
    public virtual void PlayAttackVisuals(float aimValue) { }
    public virtual void SetLocalWeapon(bool isMine) { }
    public virtual void CreateObjectOnAttack() { }
    public virtual void PlayAttackSFX() { } 


}
