using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private TMP_Text modeText;

    private string roomName;

    public void Setup(RoomInfo room)
    {
        roomName = room.Name;

        roomNameText.text =
            room.Name;

        playersText.text =
            $"{room.PlayerCount}/{room.MaxPlayers}";

        // ----------------------------
        // MODE
        // ----------------------------

        if (room.CustomProperties.TryGetValue(
                "Mode",
                out object mode))
        {
            modeText.text =
                mode.ToString();
        }
        else
        {
            modeText.text = "-";
        }
    }

    public void JoinRoom()
    {
        MenuManager.Instance.JoinUI(roomName);
    }
    public string GetRoomName()
    {
        return roomName;
    }
}