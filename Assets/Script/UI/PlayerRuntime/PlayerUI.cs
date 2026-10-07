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
    [SerializeField] private float fadeTime = 0.25f;
    [SerializeField] private float textSpacingPercent = 0.25f;

    [Header("Respawn")]
    [SerializeField] private GameObject imageWattingRespawn;
    [Header("WeaponInfo")]
    [SerializeField] private Image weaponImg;
    [SerializeField] private TextMeshProUGUI textMag;




    // =========================================================
    // POOL
    // =========================================================

    private readonly Queue<TextMeshProUGUI> normalTextPool =
        new Queue<TextMeshProUGUI>();

    private readonly Queue<TextMeshProUGUI> headShotTextPool =
        new Queue<TextMeshProUGUI>();


    // =========================================================
    // ACTIVE TEXT
    // =========================================================

    // Index 0 = text mới nhất
    // Index cuối = text cũ nhất
    private readonly List<TextMeshProUGUI> activeHitTexts =
        new List<TextMeshProUGUI>();


    // Coroutine fade của từng text
    private readonly Dictionary<TextMeshProUGUI, Coroutine> textCoroutines =
        new Dictionary<TextMeshProUGUI, Coroutine>();


    // =========================================================
    // UNITY
    // =========================================================

    private void Awake()
    {
        instance = this;

        InitializePool();
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        textCoroutines.Clear();

        activeHitTexts.Clear();
    }

    public void SetWeapon(Sprite weaponSprite, int currentAmmo, int maxAmmo)
    {
        weaponImg.sprite = weaponSprite;
        if (maxAmmo == 0)
        {
            textMag.text = "";
            return;
        }
        UpdateMag(currentAmmo, maxAmmo);
    }

    public void UpdateMag(int currentAmmo, int maxAmmo)
    {
        textMag.text = $"{currentAmmo} / {maxAmmo}";
    }
    // =========================================================
    // POOL INITIALIZE
    // =========================================================

    private void InitializePool()
    {
        if (hitTextParent == null)
        {
            Debug.LogError(
                "PlayerUI: Chưa gán Hit Text Parent!"
            );

            return;
        }

        if (textNormalHitPrefab == null ||
            textHeadShotHitPrefab == null)
        {
            Debug.LogError(
                "PlayerUI: Chưa gán Hit Text Prefab!"
            );

            return;
        }


        // Normal
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


        // Headshot
        for (int i = 0; i < poolSize; i++)
        {
            TextMeshProUGUI text =
                Instantiate(
                    textHeadShotHitPrefab,
                    hitTextParent
                );

            text.gameObject.SetActive(false);

            headShotTextPool.Enqueue(text);

            // Đánh dấu object này thuộc Headshot Pool
            headShotTexts.Add(text);
        }
    }


    // =========================================================
    // HEALTH
    // =========================================================

    public void UpdateHealthUI(float fill)
    {
        if (healthImage == null)
            return;

        healthImage.fillAmount = fill;
    }


    // =========================================================
    // NORMAL HIT
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

    private void ShowHitText(
        int dame,
        bool isHeadShot
    )
    {
        // Nếu full
        // Xóa text cũ nhất
        if (activeHitTexts.Count >= maxActiveHitText)
        {
            RemoveOldestText();
        }


        // Lấy từ pool
        TextMeshProUGUI text =
            GetTextFromPool(isHeadShot);

        if (text == null)
        {
            Debug.LogWarning(
                "PlayerUI: Pool không còn text!"
            );

            return;
        }


        // =====================================================
        // SET TEXT
        // =====================================================

        text.text = isHeadShot
            ? $"HEADSHOT : {dame} DMG"
            : $"HIT : {dame} DMG";


        // =====================================================
        // RESET ALPHA
        // =====================================================

        Color color = text.color;
        color.a = 1f;
        text.color = color;


        // =====================================================
        // RESET POSITION
        // =====================================================

        text.rectTransform.anchoredPosition =
            Vector2.zero;


        // =====================================================
        // ACTIVE
        // =====================================================

        text.gameObject.SetActive(true);


        // =====================================================
        // INSERT TEXT MỚI VÀO ĐẦU
        // =====================================================

        activeHitTexts.Insert(
            0,
            text
        );


        // =====================================================
        // FADE COROUTINE
        // =====================================================

        Coroutine coroutine =
            StartCoroutine(
                FadeAndRemove(text)
            );

        textCoroutines[text] =
            coroutine;


        // =====================================================
        // REPOSITION
        // =====================================================

        RepositionAllText();
    }


    // =========================================================
    // GET FROM POOL
    // =========================================================

    private TextMeshProUGUI GetTextFromPool(
        bool isHeadShot
    )
    {
        if (isHeadShot)
        {
            if (headShotTextPool.Count == 0)
                return null;

            return headShotTextPool.Dequeue();
        }


        if (normalTextPool.Count == 0)
            return null;

        return normalTextPool.Dequeue();
    }


    // =========================================================
    // REMOVE OLDEST
    // =========================================================

    private void RemoveOldestText()
    {
        if (activeHitTexts.Count == 0)
            return;


        int lastIndex =
            activeHitTexts.Count - 1;


        TextMeshProUGUI text =
            activeHitTexts[lastIndex];


        // Xóa khỏi active list
        activeHitTexts.RemoveAt(
            lastIndex
        );


        // Stop coroutine
        StopTextCoroutine(text);


        // Tắt
        text.gameObject.SetActive(false);


        // Trả pool
        ReturnToPool(text);


        // Xếp lại
        RepositionAllText();
    }


    // =========================================================
    // FADE + REMOVE
    // =========================================================

    private IEnumerator FadeAndRemove(
        TextMeshProUGUI text
    )
    {
        // Giữ nguyên alpha trong thời gian sống
        float waitTime =
            Mathf.Max(
                0f,
                textLifeTime - fadeTime
            );

        yield return new WaitForSeconds(
            waitTime
        );


        // Nếu text đã bị remove
        // bởi maxActiveHitText
        if (!text.gameObject.activeSelf)
            yield break;


        // =====================================================
        // FADE
        // =====================================================

        Color startColor =
            text.color;

        float startAlpha =
            startColor.a;

        float timer = 0f;


        while (timer < fadeTime)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / fadeTime
                );


            Color color =
                text.color;

            color.a =
                Mathf.Lerp(
                    startAlpha,
                    0f,
                    t
                );

            text.color =
                color;


            yield return null;
        }


        // Đảm bảo alpha = 0
        Color finalColor =
            text.color;

        finalColor.a = 0f;

        text.color =
            finalColor;


        // =====================================================
        // REMOVE
        // =====================================================

        RemoveText(text);
    }


    // =========================================================
    // REMOVE TEXT
    // =========================================================

    private void RemoveText(
        TextMeshProUGUI text
    )
    {
        int index =
            activeHitTexts.IndexOf(text);


        // Text đã bị remove trước đó
        if (index < 0)
            return;


        activeHitTexts.RemoveAt(
            index
        );


        textCoroutines.Remove(
            text
        );


        // Tắt
        text.gameObject.SetActive(false);


        // Reset alpha
        Color color =
            text.color;

        color.a = 1f;

        text.color =
            color;


        // Trả pool
        ReturnToPool(text);


        // Kéo text phía trên xuống
        RepositionAllText();
    }


    // =========================================================
    // STOP COROUTINE
    // =========================================================

    private void StopTextCoroutine(
        TextMeshProUGUI text
    )
    {
        if (!textCoroutines.TryGetValue(
            text,
            out Coroutine coroutine))
        {
            return;
        }


        if (coroutine != null)
        {
            StopCoroutine(
                coroutine
            );
        }


        textCoroutines.Remove(
            text
        );
    }


    // =========================================================
    // RETURN TO POOL
    // =========================================================

    private void ReturnToPool(TextMeshProUGUI text)
    {
        text.text = string.Empty;

        if (headShotTexts.Contains(text))
        {
            headShotTextPool.Enqueue(text);
        }
        else
        {
            normalTextPool.Enqueue(text);
        }
    }


    // =========================================================
    // CHECK TYPE
    // =========================================================

    private bool IsHeadShotText(
        TextMeshProUGUI text
    )
    {
        // Các object được Instantiate từ
        // headShotTextPrefab sẽ có cùng
        // prefab source, nhưng cách đơn giản
        // nhất ở đây là kiểm tra parent/name.
        //
        // Tuy nhiên không nên dựa vào name.
        //
        // Vì vậy phần này sẽ được thay bằng
        // HashSet bên dưới.
        return headShotTexts.Contains(text);
    }


    // =========================================================
    // REPOSITION
    // =========================================================

    private void RepositionAllText()
    {
        float y = 0f;


        for (int i = 0;
             i < activeHitTexts.Count;
             i++)
        {
            TextMeshProUGUI text =
                activeHitTexts[i];


            RectTransform rect =
                text.rectTransform;


            // Text mới nhất
            // nằm ngay gốc
            if (i == 0)
            {
                rect.anchoredPosition =
                    Vector2.zero;

                continue;
            }


            TextMeshProUGUI previous =
                activeHitTexts[i - 1];


            float previousHeight =
                previous.rectTransform.rect.height;


            float spacing =
                previousHeight *
                (1f + textSpacingPercent);


            y += spacing;


            Vector2 position =
                rect.anchoredPosition;

            position.y = y;

            rect.anchoredPosition =
                position;
        }
    }


    // =========================================================
    // CROSSHAIR
    // =========================================================

    public void GoToAimMode()
    {
        if (crossHair != null)
            crossHair.gameObject.SetActive(false);
    }


    public void GoToNoneAimMode()
    {
        if (crossHair != null)
            crossHair.gameObject.SetActive(true);
    }


    public void HitSignal()
    {
        if (crossHairAnimator == null)
            return;

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
        if (imageWattingRespawn != null)
            imageWattingRespawn.SetActive(true);
    }


    public void TurnOffWattingImage()
    {
        if (imageWattingRespawn != null)
            imageWattingRespawn.SetActive(false);
    }


    // =========================================================
    // HEADSHOT TRACKING
    // =========================================================

    private readonly HashSet<TextMeshProUGUI> headShotTexts =
        new HashSet<TextMeshProUGUI>();
}