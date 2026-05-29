using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class Gun : Weapon
{
    [Header("Gun Power")]
    [SerializeField] protected float force;
    [SerializeField] protected float rpm = 600;
    [SerializeField] protected int mag;
    [SerializeField] protected float recoilAmount = 2f;
    [Header("Gun Position Setting")]
    [SerializeField] protected Transform firePosition;
    [SerializeField] protected Transform bolt;
    [SerializeField] protected float offsetBoltZ;
    [SerializeField] protected Transform shellEjectPosition;
    [Header("Gun References")]
    [SerializeField] protected WeaponAnimator weaponAnimator;
    [Header("Gun VFX")]
    [SerializeField] protected ParticleSystem muzzleFlash;
    private Vector3 originLocalPosition;
    protected Coroutine gunShakeCoroutine, boltActionCoroutine;
    private float lastShootTime = 0f;
    protected int currentAmmo;
    private bool isReload;
    protected float timePerRecoil;
    protected Vector3 localOriginBoltPosition, localEndBoltPosition;
    protected Transform camTransform;
    public virtual void Start()
    {
        originLocalPosition = transform.localPosition;
        currentAmmo = mag;
        isReload = false;
        timePerRecoil = 60f / rpm;
        localOriginBoltPosition = bolt.localPosition;
        localEndBoltPosition = bolt.localPosition + new Vector3(0f, 0f, offsetBoltZ);
        camTransform = Camera.main.transform;
    }
    public override void WeaponAttack(float aimValue)
    {
        if (Time.time >= lastShootTime + (timePerRecoil))
        {
            Shoot(aimValue);
            lastShootTime = Time.time;
        }
    }
    public override void WeaponReload()
    {
        playerHolder.CameraRotReload();
        isReload = true;
        weaponAnimator.AnimatedReload();
    }
    public override void WeaponReloadDone()
    {
        currentAmmo = mag;
        isReload = false;
    }
    public virtual bool Shoot(float aimValue)
    {
        if (isReload) return false;
        if (currentAmmo <= 0)
        {
            WeaponReload();
            return false;
        }
        if (gunShakeCoroutine != null) StopCoroutine(gunShakeCoroutine);
        gunShakeCoroutine = StartCoroutine(GunShake(aimValue));
        currentAmmo--;
        muzzleFlash.Play();
        muzzleFlash.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(Random.Range(0f, 360f), -90, 0));
        PoolObject.Instance.CreatmuzzleFlashSmoke(firePosition.position, firePosition.rotation);
        playerHolder.cameraHolder.RecoilCamera(recoilAmount); 
        return true;
    }
    public virtual void BoltAction()
    {
        
    }
    IEnumerator GunShake(float aimValue)
    {
        float a = Mathf.Abs(aimValue - 1f);
        Vector3 ShakePosition = originLocalPosition + new Vector3(0f, 0f, Random.Range(-0.02f, -0.04f) * (a + 0.25f));
        Quaternion ShakeRotation = Quaternion.Euler(new Vector3(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, Random.Range(-3f, 3f) * a));
        float distanceRemaining = Vector3.Distance(transform.localPosition, ShakePosition);
        while (distanceRemaining > 0.0001f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, ShakePosition, Time.deltaTime * 50f);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, ShakeRotation, Time.deltaTime * 20f);
            distanceRemaining = Vector3.Distance(transform.localPosition, ShakePosition);
            yield return null;
        }
        while (Vector3.Distance(transform.localPosition, originLocalPosition) > 0.0001f)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, originLocalPosition, Time.deltaTime * 50f);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, ShakeRotation, Time.deltaTime * 20f);
            yield return null;
        }
        transform.localPosition = originLocalPosition;
    }
}
