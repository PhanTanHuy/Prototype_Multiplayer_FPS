using UnityEngine;

public class Bomb : Weapon
{
    [SerializeField] private Transform firePos;
    private Transform cameraTransform;
    [SerializeField] private float timePerShoot;
    private float lastShootTime;
    private Transform rootPlayer;
    private void Start()
    {
        cameraTransform = Camera.main.transform;
        lastShootTime = Time.time - timePerShoot * 2f;
        rootPlayer = transform.root;
    }
    public override void WeaponAttack(float aimValue)
    {
        if (Time.time >= lastShootTime + timePerShoot)
        {
            PoolObject.Instance.CreateBomb(firePos.position, cameraTransform.forward, rootPlayer);
            Debug.Log("Create Bomb");
            lastShootTime = Time.time;
        }
    }

    public override void WeaponReload()
    {
    }
    public override void WeaponReloadDone()
    {

    }
}
