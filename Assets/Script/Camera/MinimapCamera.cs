using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    [Header("Minimap")]
    [Tooltip("RawImage của Minimap")]
    [SerializeField] private RectTransform minimapRawImage;
    [SerializeField] private Camera minimapCameraRender;

    [Header("Attack Dot")]
    [Tooltip("Chỉ cần kéo 1 Image mũi tên vào đây")]
    [SerializeField] private RectTransform AttackSignalPrefab;

    [SerializeField] private int AttackSignalPoolSize = 10;

    [SerializeField] private float AttackSignalDuration = 3f;

    [SerializeField]
    [Range(0f, 180f)]
    private float sameDirectionAngle = 15f;

    [Header("Test")]
    [SerializeField] private float testMinDistance = 20f;
    [SerializeField] private float testMaxDistance = 50f;
    [HideInInspector] public Transform follow;
    private float yPos;
    // Pool các dot đang rảnh
    private Queue<RectTransform> availableAttackSignals =
        new Queue<RectTransform>();

    // Các dot đang active
    private List<AttackSignalData> activeAttackSignals =
        new List<AttackSignalData>();

    private class AttackSignalData
    {
        public RectTransform dot;

        // Hướng từ Player -> Enemy
        public Vector2 direction;

        public Coroutine coroutine;
    }
    private void Start()
    {
        yPos = minimapCameraRender.transform.position.y;
        CreateAttackSignalPool();
    }
    private void LateUpdate()
    {
        Vector3 positionFollow = follow.position;
        positionFollow.y = yPos;
        minimapCameraRender.transform.position = positionFollow;
    }
    // =========================================================
    // CREATE POOL
    // =========================================================

    private void CreateAttackSignalPool()
    {
        if (minimapRawImage == null)
        {
            Debug.LogError(
                "MinimapCamera: Chưa gán Minimap RawImage!"
            );

            return;
        }

        if (AttackSignalPrefab == null)
        {
            Debug.LogError(
                "MinimapCamera: Chưa gán Attack Dot Prefab!"
            );

            return;
        }

        for (int i = 0; i < AttackSignalPoolSize; i++)
        {
            RectTransform dot =
                Instantiate(
                    AttackSignalPrefab,
                    minimapRawImage
                );
            dot.gameObject.SetActive(false);

            availableAttackSignals.Enqueue(dot);
        }

        Debug.Log(
            $"Attack Dot Pool Created: {AttackSignalPoolSize}"
        );
    }

    // =========================================================
    // TEST
    // =========================================================

    public void TestAttackSignal(Transform localPlayer)
    {
        if (localPlayer == null)
        {
            Debug.LogWarning(
                "MinimapCamera: Không tìm thấy Player để test!"
            );

            return;
        }

        Vector3 playerPosition =
            localPlayer.position;

        // Random hướng trên mặt phẳng XZ
        Vector2 randomDirection2D =
            Random.insideUnitCircle.normalized;

        Vector3 direction =
            new Vector3(
                randomDirection2D.x,
                0f,
                randomDirection2D.y
            );

        // Random khoảng cách
        float distance =
            Random.Range(
                testMinDistance,
                testMaxDistance
            );

        // Vị trí giả lập enemy
        Vector3 fakeEnemyPosition =
            playerPosition + direction * distance;

        Debug.Log(
            $"[TEST] Fake Enemy Position: {fakeEnemyPosition} | " +
            $"Distance: {distance}"
        );

        ShowAttackDirection(
            fakeEnemyPosition,
            direction
        );
    }

    // =========================================================
    // SHOW ATTACK DIRECTION
    // =========================================================

    public void ShowAttackDirection(
        Vector3 position,
        Vector3 direction
    )
    {
        // Chỉ lấy hướng XZ
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        // World Direction -> UI Direction
        Vector2 uiDirection =
            new Vector2(
                direction.x,
                direction.z
            ).normalized;

        // Check trùng hướng
        if (IsDirectionAlreadyShowing(uiDirection))
            return;

        // Pool hết
        if (availableAttackSignals.Count == 0)
            return;

        RectTransform dot =
            availableAttackSignals.Dequeue();

        // Đặt dot ở rìa minimap
        Vector2 edgePosition =
            GetEdgePosition(uiDirection);

        dot.anchoredPosition =
            edgePosition;

        // =====================================================
        // XOAY MŨI TÊN VỀ CENTER
        // =====================================================

        Vector2 directionToCenter =
            -uiDirection;

        float angle =
            Mathf.Atan2(
                directionToCenter.x,
                -directionToCenter.y
            ) * Mathf.Rad2Deg;

        dot.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );

        dot.gameObject.SetActive(true);

        // =====================================================
        // SAVE DATA
        // =====================================================

        AttackSignalData data =
            new AttackSignalData();

        data.dot = dot;
        data.direction = uiDirection;

        activeAttackSignals.Add(data);

        // =====================================================
        // AUTO RETURN
        // =====================================================

        data.coroutine =
            StartCoroutine(
                ReturnAttackSignalToPool(data)
            );
    }

    // =========================================================
    // RETURN DOT
    // =========================================================

    private IEnumerator ReturnAttackSignalToPool(
        AttackSignalData data
    )
    {
        yield return new WaitForSeconds(
            AttackSignalDuration
        );

        if (data.dot != null)
        {
            data.dot.gameObject.SetActive(false);

            data.dot.localRotation =
                Quaternion.identity;

            data.dot.anchoredPosition =
                Vector2.zero;

            availableAttackSignals.Enqueue(
                data.dot
            );
        }

        activeAttackSignals.Remove(data);
    }

    // =========================================================
    // CHECK SAME DIRECTION
    // =========================================================

    private bool IsDirectionAlreadyShowing(
        Vector2 newDirection
    )
    {
        for (int i = 0;
             i < activeAttackSignals.Count;
             i++)
        {
            AttackSignalData data =
                activeAttackSignals[i];

            float angle =
                Vector2.Angle(
                    data.direction,
                    newDirection
                );

            if (angle <= sameDirectionAngle)
                return true;
        }

        return false;
    }

    // =========================================================
    // GET EDGE POSITION
    // =========================================================

    private Vector2 GetEdgePosition(
        Vector2 direction
    )
    {
        Rect rect =
            minimapRawImage.rect;

        float halfWidth =
            rect.width * 0.5f;

        float halfHeight =
            rect.height * 0.5f;

        float x = direction.x;
        float y = direction.y;

        float scaleX =
            Mathf.Abs(x) > 0.001f
                ? halfWidth / Mathf.Abs(x)
                : float.MaxValue;

        float scaleY =
            Mathf.Abs(y) > 0.001f
                ? halfHeight / Mathf.Abs(y)
                : float.MaxValue;

        float scale =
            Mathf.Min(
                scaleX,
                scaleY
            );

        return direction * scale;
    }
}