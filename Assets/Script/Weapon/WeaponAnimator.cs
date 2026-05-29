using UnityEngine;
[RequireComponent(typeof(Animator))]
public class WeaponAnimator : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private Weapon weapon;
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
}
