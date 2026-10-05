using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text roomNameText;
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private TMP_Text modeText;
    [SerializeField] private TMP_Text mapText;

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

        // ----------------------------
        // MAP
        // ----------------------------

        if (room.CustomProperties.TryGetValue(
                "Map",
                out object map))
        {
            mapText.text =
                map.ToString();
        }
        else
        {
            mapText.text = "-";
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