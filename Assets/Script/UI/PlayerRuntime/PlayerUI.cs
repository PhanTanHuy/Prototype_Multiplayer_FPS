using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI instance;
    
    [SerializeField] private RectTransform crossHair;
    [SerializeField] private Animator crossHairAnimator;
    [SerializeField] private TextMeshProUGUI textNormalHit, textHeadShotHit;
    private void Awake()
    {
        instance = this;
    }
    public void GoToAimMode()
    {
        //crossHair.gameObject.SetActive(false);
    }
    public void GoToNoneAimMode()
    {
        crossHair.gameObject.SetActive(true);
    }
    public void HitSignal()
    {
        crossHairAnimator.Play("HitSignal", 0, 0f);
    }
    public void SetTextHitNormal(int dame)
    {
        textNormalHit.text = "HIT : " + dame.ToString() + " DMG";
        if (textHeadShotHit.text != string.Empty) textHeadShotHit.text = string.Empty;
    }
    public void SetTextHitHeadShot(int dame)
    {
        textHeadShotHit.text = "HEADSHOT : " + dame.ToString() + " DMG";
        if (textNormalHit.text != string.Empty) textNormalHit.text = string.Empty;
    }
}
