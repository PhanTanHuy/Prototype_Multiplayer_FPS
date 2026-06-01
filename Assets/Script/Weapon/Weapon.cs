using System;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class Weapon : PickupAbleItem
{
    [SerializeField] protected int damage;
    [SerializeField] protected int cost;
    public Transform leftHandTarget, rightHandTarget;
    [HideInInspector] public PlayerManagerState playerHolder;
    [SerializeField] protected float aimSentivity = 2f;
    public float AimSentivity { get { return aimSentivity; } }
    [HideInInspector] public Transform rootParent;


    public int Damage { get { return damage; } }
    public int Cost { get { return cost; } }

    public abstract void WeaponAttack(float aimValue);
    public abstract void WeaponReload();
    public abstract void WeaponReloadDone();
    public virtual void PlayReloadVisuals() { }
    public virtual void PlayAttackVisuals(float aimValue) { }


}
