using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    [Header("Player Join Notification")]
    [SerializeField] private TMP_Text playerJoinText;
    [SerializeField] private float messageDuration = 5f;

    [Header("Spawn")]
    [SerializeField] private Transform[] spawnPoint;

    [Header("Attack Direction Minimap")]
    [SerializeField] private RectTransform minimapRawImage;

    [SerializeField] private RectTransform[] attackDots;

    [SerializeField] private float attackDotDuration = 3f;

    [SerializeField]
    [Range(0f, 180f)]
    private float sameDirectionAngle = 15f;

    private bool hasSpawned;
    private Coroutine hideMessageCoroutine;

    private Coroutine[] attackDotCoroutines;

    private void Awake()
    {
        Instance = this;

        if (playerJoinText != null)
            playerJoinText.gameObject.SetActive(false);

        // Tắt toàn bộ attack dot lúc bắt đầu
        if (attackDots != null)
        {
            attackDotCoroutines = new Coroutine[attackDots.Length];

            foreach (RectTransform dot in attackDots)
            {
                if (dot != null)
                    dot.gameObject.SetActive(false);
            }
        }
    }

    private void Start()
    {
        TrySpawnPlayer();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("RoomManager: OnJoinedRoom");

        TrySpawnPlayer();
    }

    // =========================================================
    // PLAYER JOIN
    // =========================================================

    public void ShowPlayerJoinedMessage(string playerName)
    {
        if (playerJoinText == null)
            return;

        playerJoinText.text = $"{playerName} joined the game!";
        playerJoinText.gameObject.SetActive(true);

        if (hideMessageCoroutine != null)
            StopCoroutine(hideMessageCoroutine);

        hideMessageCoroutine = StartCoroutine(HideJoinMessage());
    }

    private IEnumerator HideJoinMessage()
    {
        yield return new WaitForSeconds(messageDuration);

        playerJoinText.gameObject.SetActive(false);
    }

    // =========================================================
    // ATTACK DIRECTION MINIMAP
    // =========================================================
    public bool IsPositionInsideMinimap(Vector3 position, Camera cam)
    {
        if (cam == null)
            return false;

        Vector3 viewportPoint =
            cam.WorldToViewportPoint(position);

        // Không nằm phía sau camera
        if (viewportPoint.z < 0f)
            return false;

        return
            viewportPoint.x >= 0f &&
            viewportPoint.x <= 1f &&
            viewportPoint.y >= 0f &&
            viewportPoint.y <= 1f;
    }
    public void ShowAttackDirection(Vector3 position,
        Vector3 direction,
        Camera cam
    )
    {
        if (IsPositionInsideMinimap(position, cam))
            return;

        if (minimapRawImage == null)
            return;

        if (attackDots == null || attackDots.Length == 0)
            return;

        // Chỉ quan tâm hướng trên mặt phẳng XZ
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        // Kiểm tra xem có dot nào đang hiển thị
        // ở gần hướng này hay không.
        if (IsDirectionAlreadyShowing(direction))
        {
            return;
        }

        // Tìm dot chưa active
        RectTransform availableDot = null;
        int availableIndex = -1;

        for (int i = 0; i < attackDots.Length; i++)
        {
            if (attackDots[i] == null)
                continue;

            if (!attackDots[i].gameObject.activeSelf)
            {
                availableDot = attackDots[i];
                availableIndex = i;
                break;
            }
        }

        // Không còn dot trống
        if (availableDot == null)
            return;

        // Đưa direction World XZ thành tọa độ UI XY
        Vector2 uiDirection = new Vector2(
            direction.x,
            direction.z
        ).normalized;

        // Đưa dot ra rìa RawImage
        Vector2 edgePosition = GetEdgePosition(uiDirection);

        availableDot.anchoredPosition = edgePosition;

        // Hướng từ DOT -> CENTER
        Vector2 directionToCenter = -uiDirection;

        // Mũi tên mặc định đang chĩa xuống
        // nên tính góc từ hướng Down
        float angle = Mathf.Atan2(
            directionToCenter.x,
            -directionToCenter.y
        ) * Mathf.Rad2Deg;

        availableDot.localRotation = Quaternion.Euler(
            0f,
            0f,
            angle
        );

        availableDot.gameObject.SetActive(true);
        // Nếu dot này đang có coroutine thì dừng
        if (attackDotCoroutines[availableIndex] != null)
        {
            StopCoroutine(attackDotCoroutines[availableIndex]);
        }

        attackDotCoroutines[availableIndex] =
            StartCoroutine(
                HideAttackDot(
                    availableDot,
                    availableIndex
                )
            );
    }

    private IEnumerator HideAttackDot(
        RectTransform dot,
        int index
    )
    {
        yield return new WaitForSeconds(attackDotDuration);

        if (dot != null)
            dot.gameObject.SetActive(false);

        if (attackDotCoroutines != null &&
            index >= 0 &&
            index < attackDotCoroutines.Length)
        {
            attackDotCoroutines[index] = null;
        }
    }

    // =========================================================
    // CHECK TRÙNG HƯỚNG
    // =========================================================

    private bool IsDirectionAlreadyShowing(Vector3 newDirection)
    {
        for (int i = 0; i < attackDots.Length; i++)
        {
            RectTransform dot = attackDots[i];

            if (dot == null)
                continue;

            if (!dot.gameObject.activeSelf)
                continue;

            // Lấy hướng của dot hiện tại
            Vector2 dotPosition = dot.anchoredPosition;

            if (dotPosition.sqrMagnitude <= 0.001f)
                continue;

            Vector2 dotDirection =
                dotPosition.normalized;

            Vector2 newDirection2D =
                new Vector2(
                    newDirection.x,
                    newDirection.z
                ).normalized;

            float angle = Vector2.Angle(
                dotDirection,
                newDirection2D
            );

            if (angle <= sameDirectionAngle)
            {
                return true;
            }
        }

        return false;
    }

    // =========================================================
    // TÌM VỊ TRÍ Ở RÌA RAWIMAGE
    // =========================================================

    private Vector2 GetEdgePosition(Vector2 direction)
    {
        Rect rect = minimapRawImage.rect;

        float halfWidth = rect.width * 0.5f;
        float halfHeight = rect.height * 0.5f;

        float x = direction.x;
        float y = direction.y;

        float scaleX = Mathf.Abs(x) > 0.001f
            ? halfWidth / Mathf.Abs(x)
            : float.MaxValue;

        float scaleY = Mathf.Abs(y) > 0.001f
            ? halfHeight / Mathf.Abs(y)
            : float.MaxValue;

        float scale = Mathf.Min(scaleX, scaleY);

        return direction * scale;
    }

    // =========================================================
    // SPAWN
    // =========================================================

    private void TrySpawnPlayer()
    {
        if (hasSpawned)
            return;

        if (!PhotonNetwork.InRoom)
        {
            Debug.Log("Chưa vào Room, chờ OnJoinedRoom...");
            return;
        }

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (hasSpawned)
            return;

        hasSpawned = true;

        Hashtable properties =
            PhotonNetwork.LocalPlayer.CustomProperties;

        string character = "";

        if (properties.TryGetValue(
            "Character",
            out object characterValue))
        {
            character = characterValue.ToString();
        }

        if (string.IsNullOrEmpty(character))
        {
            Debug.LogError("Character không tồn tại!");
            hasSpawned = false;
            return;
        }

        Vector3 spawnPosition = GetSpawnPoint();

        GameObject player = PhotonNetwork.Instantiate(
            character,
            spawnPosition,
            Quaternion.identity
        );

        player
            .GetComponent<PlayerNetworkSetup>()
            .SetLocalPlayer();

        Debug.Log(
            $"Player spawned | Character: {character} | Position: {spawnPosition}"
        );
    }

    public Vector3 GetSpawnPoint()
    {
        return spawnPoint[
            Random.Range(0, spawnPoint.Length)
        ].position;
    }
    
}