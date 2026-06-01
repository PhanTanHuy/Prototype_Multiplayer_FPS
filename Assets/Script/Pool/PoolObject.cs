using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PoolObject : MonoBehaviour
{
    public static PoolObject Instance;
    [SerializeField] private ParticleSystem muzzleFlashSmoke;
    [SerializeField] private Bullet bullet;
    [SerializeField] private BombProjectile bombProjectile;
    [SerializeField] private ParticleSystem bulletHole;
    [SerializeField] private ParticleSystem blood;
    [SerializeField] private GameObject shell;
    private Queue<ParticleSystem> muzzleFlashSmokePool = new Queue<ParticleSystem>();
    private Queue<ParticleSystem> bulletHolePool = new Queue<ParticleSystem>();
    private Queue<ParticleSystem> bloodPool = new Queue<ParticleSystem>();
    private Queue<Bullet> bulletPool = new Queue<Bullet>();
    private Queue<BombProjectile> bombProjectilePool = new Queue<BombProjectile>();
    private Queue<Shell> shellPool = new Queue<Shell>();

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        InitPool<ParticleSystem>(muzzleFlashSmokePool, muzzleFlashSmoke.gameObject, 10);
        InitPool<ParticleSystem>(bulletHolePool, bulletHole.gameObject, 300);
        InitPool<ParticleSystem>(bloodPool, blood.gameObject, 300);
        InitPool<Bullet>(bulletPool, bullet.gameObject, 300);
        InitPool<Shell>(shellPool, shell.gameObject, 300);
        InitPool<BombProjectile>(bombProjectilePool, bombProjectile.gameObject, 20);

    }
    private void InitPool<T>(Queue<T> q, GameObject obj, int num)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject newObj = Instantiate(obj.gameObject);
            newObj.SetActive(false);
            q.Enqueue(newObj.GetComponent<T>());
        }
    }
    public void CreateBomb(Vector3 position, Vector3 direction, Transform rootPlayer)
    {
        if (bombProjectilePool.Count == 0) return;
        BombProjectile bomb = bombProjectilePool.Dequeue();
        bomb.gameObject.transform.position = position;
        //bomb.gameObject.transform.parent = rootPlayer;
        bomb.gameObject.SetActive(true);
        bomb.SetBombProjectile(direction);
        StartCoroutine(IEReturnToPool<BombProjectile>(bombProjectilePool, bomb.gameObject, 7f));
    }
    public void CreatBullet(Vector3 position, Vector3 direction, float speed, int d, Transform rootPlayer)
    {
        if (bulletPool.Count == 0)
        {
            return;
        }
        Bullet b = bulletPool.Dequeue();
        b.gameObject.transform.position = position;
        //b.gameObject.transform.parent = rootPlayer;
        //Debug.Log(rootPlayer.name);
        b.gameObject.SetActive(true);
        b.SetBullet(direction, speed, d);
        StartCoroutine(IEReturnToPool<Bullet>(bulletPool, b.gameObject, 5f));
    }
    public void CreatShell(Vector3 position, Vector3 direction)
    {
        if (shellPool.Count == 0)
        {
            return;
        }
        Shell b = shellPool.Dequeue();
        b.gameObject.transform.position = position;
        b.gameObject.SetActive(true);
        b.SetShell(direction);
        StartCoroutine(IEReturnToPool<Shell>(shellPool, b.gameObject, 5f));
    }
    public void CreatBulletHole(Vector3 position, Quaternion look)
    {
        if (bulletHolePool.Count == 0)
        {
            return;
        }
        ParticleSystem ps = bulletHolePool.Dequeue();
        ps.gameObject.SetActive(true);
        ps.gameObject.transform.position = position;
        ps.gameObject.transform.rotation = look;
        ps.Play();
        StartCoroutine(IEReturnToPool<ParticleSystem>(bulletHolePool, ps.gameObject, 10f));
    }
    public void CreatBlood(Vector3 position, Quaternion look)
    {
        if (bloodPool.Count == 0)
        {
            return;
        }
        ParticleSystem ps = bloodPool.Dequeue();
        ps.gameObject.SetActive(true);
        ps.gameObject.transform.position = position;
        ps.gameObject.transform.rotation = look;
        ps.Play();
        StartCoroutine(IEReturnToPool<ParticleSystem>(bloodPool, ps.gameObject, 10f));
    }
    public void CreatmuzzleFlashSmoke(Vector3 position, Quaternion rotation)
    {
        if (muzzleFlashSmokePool.Count == 0)
        {
            return;
        }
        ParticleSystem ps = muzzleFlashSmokePool.Dequeue();
        ps.gameObject.SetActive(true);
        ps.gameObject.transform.position = position;
        ps.gameObject.transform.rotation = rotation;
        ps.Play();
        StartCoroutine(IEReturnToPool<ParticleSystem>(muzzleFlashSmokePool, ps.gameObject, 1f));
    }
    private IEnumerator IEReturnToPool<T>(Queue<T> q, GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
        q.Enqueue(obj.GetComponent<T>());
    }
}
