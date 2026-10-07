using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI instance;
    [Header("Health UI")]
    [SerializeField] private Image healthImage;
    [Header("Crosshair")]
    [SerializeField] private RectTransform crossHair;
    [SerializeField] private Animator crossHairAnimator;

    [Header("Hit Text Pool")]
    [SerializeField] private TextMeshProUGUI textNormalHitPrefab;
    [SerializeField] private TextMeshProUGUI textHeadShotHitPrefab;

    [SerializeField] private Transform hitTextParent;

    [SerializeField] private int poolSize = 10;

    [Header("Hit Text Setting")]
    [SerializeField] private int maxActiveHitText = 10;
    [SerializeField] private float textLifeTime = 3f;
    [SerializeField] private float normalTextHeight = 0f;

    [Header("Respawn")]
    [SerializeField] private GameObject imageWattingRespawn;


    // Pool
    private Queue<TextMeshProUGUI> normalTextPool =
        new Queue<TextMeshProUGUI>();

    private Queue<TextMeshProUGUI> headShotTextPool =
        new Queue<TextMeshProUGUI>();


    // Những text đang active
    private Queue<HitTextData> activeHitTexts =
        new Queue<HitTextData>();


    // Khoảng cách Y hiện tại
    private float currentY;


    private void Awake()
    {
        instance = this;

        InitializePool();
    }


    private void OnDisable()
    {
        StopAllCoroutines();
    }


    // =========================================================
    // POOL
    // =========================================================

    private void InitializePool()
    {
        if (hitTextParent == null)
        {
            Debug.LogError("PlayerUI: Chưa gán Hit Text Parent!");
            return;
        }

        // Nếu chưa nhập chiều cao text normal
        // thì lấy trực tiếp từ prefab
        if (normalTextHeight <= 0f)
        {
            RectTransform rect =
                textNormalHitPrefab.GetComponent<RectTransform>();

            normalTextHeight = rect.rect.height;
        }


        // Normal Damage Pool
        for (int i = 0; i < poolSize; i++)
        {
            TextMeshProUGUI text =
                Instantiate(
                    textNormalHitPrefab,
                    hitTextParent
                );

            text.gameObject.SetActive(false);

            normalTextPool.Enqueue(text);
        }


        // Headshot Pool
        for (int i = 0; i < poolSize; i++)
        {
            TextMeshProUGUI text =
                Instantiate(
                    textHeadShotHitPrefab,
                    hitTextParent
                );

            text.gameObject.SetActive(false);

            headShotTextPool.Enqueue(text);
        }
    }
    public void UpdateHealthUI(float fill)
    {
        if (healthImage == null)
            return;

        healthImage.fillAmount = fill;
    }

    // =========================================================
    // NORMAL DAMAGE
    // =========================================================

    public void SetTextHitNormal(int dame)
    {
        ShowHitText(
            dame,
            false
        );
    }


    // =========================================================
    // HEADSHOT
    // =========================================================

    public void SetTextHitHeadShot(int dame)
    {
        ShowHitText(
            dame,
            true
        );
    }


    // =========================================================
    // SHOW HIT TEXT
    // =========================================================

    private void ShowHitText(int dame, bool isHeadShot)
    {
        // Nếu đã full 10 text
        if (activeHitTexts.Count >= maxActiveHitText)
        {
            RemoveOldestText();
        }


        TextMeshProUGUI text = GetTextFromPool(isHeadShot);

        if (text == null)
        {
            Debug.LogWarning(
                "PlayerUI: Pool không còn text!"
            );

            return;
        }


        // Set nội dung
        if (isHeadShot)
        {
            text.text = "HEADSHOT : " + dame + " DMG";
        }
        else
        {
            text.text = "HIT : " + dame + " DMG";
        }


        // Lấy RectTransform
        RectTransform rect =
            text.GetComponent<RectTransform>();


        // =====================================================
        // TÍNH VỊ TRÍ Y
        // =====================================================

        float textHeight = rect.rect.height;

        float spacing =
            textHeight +
            normalTextHeight * 0.25f;


        // Text mới nằm phía trên
        currentY += spacing;


        Vector2 position =
            rect.anchoredPosition;

        position.y = currentY;

        rect.anchoredPosition = position;


        // Active
        text.gameObject.SetActive(true);


        // Tạo data
        HitTextData data =
            new HitTextData();

        data.text = text;
        data.rect = rect;
        data.isHeadShot = isHeadShot;

        // Coroutine tự tắt sau 3 giây
        data.clearCoroutine =
            StartCoroutine(
                HideTextAfterTime(data)
            );


        // Đưa vào queue
        activeHitTexts.Enqueue(data);
    }


    // =========================================================
    // GET TEXT FROM POOL
    // =========================================================

    private TextMeshProUGUI GetTextFromPool(bool isHeadShot)
    {
        if (isHeadShot)
        {
            if (headShotTextPool.Count == 0)
                return null;

            return headShotTextPool.Dequeue();
        }
        else
        {
            if (normalTextPool.Count == 0)
                return null;

            return normalTextPool.Dequeue();
        }
    }


    // =========================================================
    // REMOVE OLDEST
    // =========================================================

    private void RemoveOldestText()
    {
        if (activeHitTexts.Count == 0)
            return;


        // Lấy text cũ nhất
        HitTextData oldest =
            activeHitTexts.Dequeue();


        // Dừng coroutine
        if (oldest.clearCoroutine != null)
        {
            StopCoroutine(
                oldest.clearCoroutine
            );
        }


        // Tắt
        oldest.text.gameObject.SetActive(false);


        // Trả về pool
        ReturnToPool(oldest);


        // Xếp lại vị trí
        RepositionAllText();
    }


    // =========================================================
    // HIDE AFTER 3 SECONDS
    // =========================================================

    private IEnumerator HideTextAfterTime(
        HitTextData data
    )
    {
        yield return new WaitForSeconds(
            textLifeTime
        );


        // Text đã bị tắt trước đó
        if (!data.text.gameObject.activeSelf)
            yield break;


        RemoveTextFromQueue(data);
    }


    // =========================================================
    // REMOVE TEXT KHI HẾT 3 GIÂY
    // =========================================================

    private void RemoveTextFromQueue(
        HitTextData target
    )
    {
        Queue<HitTextData> newQueue =
            new Queue<HitTextData>();


        while (activeHitTexts.Count > 0)
        {
            HitTextData data =
                activeHitTexts.Dequeue();


            if (data == target)
            {
                data.text.gameObject.SetActive(false);

                ReturnToPool(data);
            }
            else
            {
                newQueue.Enqueue(data);
            }
        }


        activeHitTexts = newQueue;


        // Xếp lại vị trí
        RepositionAllText();
    }


    // =========================================================
    // RETURN TO POOL
    // =========================================================

    private void ReturnToPool(
        HitTextData data
    )
    {
        data.text.text = string.Empty;


        if (data.isHeadShot)
        {
            headShotTextPool.Enqueue(
                data.text
            );
        }
        else
        {
            normalTextPool.Enqueue(
                data.text
            );
        }
    }


    // =========================================================
    // REPOSITION
    // =========================================================

    private void RepositionAllText()
    {
        float y = 0f;


        foreach (HitTextData data in activeHitTexts)
        {
            float textHeight =
                data.rect.rect.height;


            float spacing =
                textHeight +
                normalTextHeight * 0.25f;


            y += spacing;


            Vector2 position =
                data.rect.anchoredPosition;

            position.y = y;

            data.rect.anchoredPosition =
                position;
        }


        currentY = y;
    }


    // =========================================================
    // CROSSHAIR
    // =========================================================

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
        crossHairAnimator.Play(
            "HitSignal",
            0,
            0f
        );
    }


    // =========================================================
    // RESPAWN
    // =========================================================

    public void TurnOnWattingImage()
    {
        imageWattingRespawn.SetActive(true);
    }


    public void TurnOffWattingImage()
    {
        imageWattingRespawn.SetActive(false);
    }


    // =========================================================
    // DATA
    // =========================================================

    private class HitTextData
    {
        public TextMeshProUGUI text;
        public RectTransform rect;
        public bool isHeadShot;

        public Coroutine clearCoroutine;
    }
}