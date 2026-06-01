using UnityEngine;
using Coherence.Toolkit;

public class PlayerNetworkSetting : MonoBehaviour
{
    private CoherenceSync coherenceSync;

    [Header("Kéo Object 'swat' từ Hierarchy vào đây")]
    public GameObject selfCamera;
    // --- CÁC BIẾN ĐỒNG BỘ MẠNG (Cần tick chọn trong Coherence Configuration) ---
    [Header("Coherence Sync Fields of PlayerManagerState")]
    public float netRigMoveWeight;
    public float netRigRunWeight;
    public float netRigAimWeight;
    public float netRigReloadWeight;
    public float netRigChangeWpWeight;
    public int netAnimState;     // 0: Idle, 1: Walk, 2: Run, 3: Jump, 4: Death
    public int netWeaponIndex;   // Lưu vị trí súng hiện tại để đồng bộ súng của bạn trên máy khác

    [Header("Coherence Sync Fields of CameraHolder")]
    public float netPitch; // Góc xoay X (lên/xuống) đồng bộ qua mạng

    void Start()
    {
        coherenceSync = GetComponent<CoherenceSync>();

        if (coherenceSync.HasStateAuthority)
        {
            SetupLocalPlayer();
        }
        else
        {
            SetupRemotePlayer();
        }
    }

    void SetupLocalPlayer()
    {
        // Chính bạn: Giữ nguyên mọi thứ để điều khiển bình thường
        Debug.Log("Đang thiết lập Local Player (Chính mình)");
    }

    void SetupRemotePlayer()
    {
        Debug.Log("Đang thiết lập Remote Player (Người chơi khác)");

        // 1. TẮT CAMERA ĐỂ KHÔNG BỊ PHÂN TÂM / ĐÈ MÀN HÌNH
        selfCamera.SetActive(false);
       
    }
}