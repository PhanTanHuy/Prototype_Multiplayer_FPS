using UnityEngine;
using Coherence.Toolkit;

public class PlayerSetup : MonoBehaviour
{
    private CoherenceSync coherenceSync;

    [Header("Kéo Object 'swat' từ Hierarchy vào đây")]
    public GameObject swatObject;

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
        Transform cameraHolder = swatObject.transform.Find("CameraHolder");
        if (cameraHolder != null)
        {
            cameraHolder.gameObject.SetActive(false);
        }

        // 2. TẮT VẬT LÝ VÀ ĐIỀU KHIỂN (Bắt buộc phải tắt trên máy người khác)
        // Tắt Character Controller để nhân vật của họ không tự tính toán va chạm trên máy bạn
        if (swatObject.TryGetComponent<CharacterController>(out var cc))
        {
            cc.enabled = false;
        }

        // Tắt script quản lý trạng thái di chuyển/nhận phím bấm từ bàn phím/chuột
        if (swatObject.TryGetComponent<PlayerManagerState>(out var managerState))
        {
            managerState.enabled = false;
        }

        // Tắt script nhặt đồ tự động (Tránh việc họ đứng gần vũ khí trên máy bạn rồi tự nhặt hộ bạn)
        if (swatObject.TryGetComponent<PlayerPickup>(out var pickup))
        {
            pickup.enabled = false;
        }


        // 3. GIỮ LẠI CÁC THÀNH PHẦN SAU ĐỂ ĐỒNG BỘ HIỂN THỊ:
        /*
         * - Animator & Rig Builder: GIỮ NGUYÊN (Để Coherence đồng bộ các biến float/bool, 
         * giúp máy bạn vẫn thấy chân tay họ chạy, nhảy, ngắm bắn qua hệ thống Rigging).
         * * - Health Manager & Player Life Manager: GIỮ NGUYÊN (Để khi bạn bắn trúng các HitBox, 
         * lượng máu của họ vẫn được trừ và xử lý logic mất máu chuẩn xác).
         * * - HitBox (Leg, Head, Spine): GIỮ NGUYÊN (Để bạn có thể bắn trúng họ).
         */
    }
}