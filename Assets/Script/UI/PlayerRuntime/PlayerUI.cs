using TMPro;
using UnityEngine;
using System.Collections;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI instance;

    [SerializeField] private RectTransform crossHair;
    [SerializeField] private Animator crossHairAnimator;
    [SerializeField] private TextMeshProUGUI textNormalHit;
    [SerializeField] private TextMeshProUGUI textHeadShotHit;
    [SerializeField] private GameObject imageWattingRespawn;

    private Coroutine normalHitCoroutine;
    private Coroutine headShotCoroutine;

    private void Awake()
    {
        instance = this;
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    public void TurnOnWattingImage()
    {
        imageWattingRespawn.SetActive(true);
    }
    public void TurnOffWattingImage()
    {
        imageWattingRespawn.SetActive(false);
    }
    public void GoToAimMode()
    {
        crossHair.gameObject.SetActive(false);
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
        textNormalHit.text = "HIT : " + dame + " DMG";

        if (normalHitCoroutine != null)
            StopCoroutine(normalHitCoroutine);

        normalHitCoroutine = StartCoroutine(ClearNormalHitText());
    }

    public void SetTextHitHeadShot(int dame)
    {
        textHeadShotHit.text = "HEADSHOT : " + dame + " DMG";

        if (headShotCoroutine != null)
            StopCoroutine(headShotCoroutine);

        headShotCoroutine = StartCoroutine(ClearHeadShotText());
    }

    private IEnumerator ClearNormalHitText()
    {
        yield return new WaitForSeconds(3f);

        textNormalHit.text = string.Empty;
        normalHitCoroutine = null;
    }

    private IEnumerator ClearHeadShotText()
    {
        yield return new WaitForSeconds(3f);

        textHeadShotHit.text = string.Empty;
        headShotCoroutine = null;
    }
}