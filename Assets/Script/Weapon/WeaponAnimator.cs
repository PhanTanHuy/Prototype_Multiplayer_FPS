using UnityEngine;
[RequireComponent(typeof(Animator))]
public class WeaponAnimator : MonoBehaviour
{
    [HideInInspector] public Animator animator;
    [SerializeField] private Weapon weapon;
    public AudioClip[] reloadAudioClips;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public void DisableAnimator()
    {
        animator.enabled = false;
        weapon.WeaponReloadDone();
    }
    public void WeaponReload()
    {
        weapon.WeaponReload();
    }
    public void AnimatedReload()
    {
        animator.enabled = true;
    }
    public void CreateObject()
    {
        weapon.CreateObjectOnAttack();
    }
    public void PlaySFX(int i)
    {
        if (i >= 0 && i < reloadAudioClips.Length)
        {
            weapon.playerHolder.playerNetworkSetup.SendPlayWeaponAnimatorClip(i);
        }
    }
    
}
