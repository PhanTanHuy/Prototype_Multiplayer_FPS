using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class Gun : Weapon
{
    public AudioClip shootSound;
    [Header("Gun Power")]
    [SerializeField] protected float distanceRaycast = 800;
    [SerializeField] protected float rpm = 600;
    [SerializeField] protected int mag;
    [SerializeField] protected float recoilAmount = 2f;
    [Header("Gun Position Setting")]
    [SerializeField] protected Transform firePosition;
    [SerializeField] protected Transform bolt;
    [SerializeField] protected float offsetBoltZ;
    [SerializeField] protected Transform shellEjectPosition;
    [Header("Gun References")]
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
    protected LayerMask hitBoxLayer;
    public override int CurrentAmmo => currentAmmo;
    public override int Mag => mag;
    public virtual void Start()
    {
        originLocalPosition = transform.localPosition;
        currentAmmo = mag;
        isReload = false;
        timePerRecoil = 60f / rpm;
        localOriginBoltPosition = bolt.localPosition;
        localEndBoltPosition = bolt.localPosition + new Vector3(0f, 0f, offsetBoltZ);
        camTransform = CameraHolder.instance.cameramain.transform;
        hitBoxLayer = LayerMask.GetMask("HitBox", "StaticMap");
    }
    public override void WeaponAttack(float aimValue)
    {
        if (Time.time >= lastShootTime + (timePerRecoil))
        {
            Shoot(aimValue);
            lastShootTime = Time.time;
        }
    }
    // Cập nhật lại các hàm này trong script Gun.cs

    // Hàm phụ để máy Client gọi khi nhận lệnh từ mạng
    public override void PlayReloadVisuals()
    {
        if (weaponAnimator != null) weaponAnimator.AnimatedReload();
    }

    public override void WeaponReload()
    {
        isReload = true;

        // 1. Máy Owner tự chạy hiệu ứng của mình
        playerHolder.CameraRotReload(); // Thay đổi IK Weight
        PlayReloadVisuals();            // Chạy Animation súng

    }

    public override void WeaponReloadDone()
    {
        currentAmmo = mag;
        PlayerUI.instance.UpdateMag(mag, mag);
        isReload = false;
    }
    // Tách riêng các hiệu ứng hình ảnh/hạt (Visuals) ra một hàm
    public override void PlayAttackVisuals(float aimValue)
    {
        // Chạy Coroutine giật súng
        if (gunShakeCoroutine != null) StopCoroutine(gunShakeCoroutine);
        gunShakeCoroutine = StartCoroutine(GunShake(aimValue));

        // Chạy các hiệu ứng tia lửa đạn và khói
        muzzleFlash.Play();
        muzzleFlash.gameObject.transform.localRotation = Quaternion.Euler(new Vector3(Random.Range(0f, 360f), -90, 0));
        PoolObject.Instance.CreatmuzzleFlashSmoke(firePosition.position, firePosition.rotation);
        PlayAttackSFX();
    }
    public override void PlayAttackSFX()
    {
        AudioSource.PlayClipAtPoint(shootSound, transform.position);
    }
    public virtual bool Shoot(float aimValue)
    {
        if (isReload) return false;
        if (currentAmmo <= 0)
        {
            WeaponReload();
            return false;
        }

        // 1. LOGIC NÒNG CỐT (Chỉ chạy trên máy bạn)
        currentAmmo--;
        PlayerUI.instance.UpdateMag(currentAmmo, mag);
        // Camera giật chỉ cần máy bạn thấy, không cần đồng bộ sang màn hình người khác
        playerHolder.cameraHolder.RecoilCamera(recoilAmount);

        PlayAttackVisuals(aimValue);
        playerHolder.SendWeaponAttack(); // Gửi lệnh cho máy khác chạy hiệu ứng bắn

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
    public void OnHit(RaycastHit hit, Vector3 moveDir)
    {
        Vector3 pos = hit.point + hit.normal * 0.01f;
        Quaternion rot = Quaternion.LookRotation(hit.normal);
        if (hit.collider.TryGetComponent<HitBox>(out HitBox target))
        {
            target.TakeDame(damage, this.transform, pos, rot);
            PlayerUI.instance.HitSignal();
        }
        else
        {
            PoolObject.Instance.CreatBulletHole(pos, rot);
        }
    }
}
