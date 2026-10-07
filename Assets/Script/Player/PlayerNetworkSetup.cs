using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlayerNetworkSetup : MonoBehaviourPun, IPunObservable
{
    public static PlayerNetworkSetup Instance;
    public bool offlineMode;
    public GameObject playerCamera;
    public PlayerManagerState playerManagerState;
    public CameraHolder cameraHolder;
    public WeaponHolder weaponHolder;
    public WeaponSway weaponSway;
    public TextMeshProUGUI playerNameText;
    public GameObject identityIcon;
    [SerializeField] private PlayerManagerState playerState;

    private int networkAnimState;
    private int receivedAnimState = -1;

    private bool networkAim;
    private bool receivedAim;

    private int networkRigMode;
    private int receivedRigMode = -1;

    private float networkCameraPitch;
    private float receivedCameraPitch;
    private void Awake()
    {
        PhotonNetwork.OfflineMode = offlineMode;
        if (offlineMode) SetLocalPlayer();
    }

    private void Start()
    {
        if (offlineMode) return;
        if (photonView.IsMine)
        {
            playerNameText.transform.parent.gameObject.SetActive(false);
        }
        string na = PhotonNetwork.NickName;
        photonView.RPC(nameof(RPC_ShowPlayerJoined), RpcTarget.Others, na);
    }
    private void Update()
    {
        if (!photonView.IsMine)
            return;

        networkAnimState = playerState.GetNetworkAnimationState();
        networkAim = playerState.IsAiming();
        networkRigMode = playerState.GetNetworkRigMode();
        networkCameraPitch = playerState.GetNetworkCameraPitch();
    }
    public void SetUpRemotePlayer()
    {
        if (offlineMode) return;
        playerNameText.text = photonView.Owner.NickName;
        Transform weaponRoot = playerManagerState.weaponHolder.transform;
        foreach (Transform child in weaponRoot.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = LayerMask.NameToLayer("Default");
        }
}
    public void SetLocalPlayer()
    {
        playerCamera.SetActive(true);
        playerManagerState.enabled = true;
        cameraHolder.enabled = true;
        weaponSway.enabled = true;
        identityIcon.SetActive(true);
        identityIcon.transform.GetChild(0).gameObject.SetActive(true); //local icon
        identityIcon.transform.GetChild(1).gameObject.SetActive(false); //remote icon
        this.enabled = true;
        Instance = this;
        GetComponent<Outline>().enabled = false;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(networkAnimState);
            stream.SendNext(networkAim);
            stream.SendNext(networkRigMode);
            stream.SendNext(networkCameraPitch);
        }
        else
        {
            receivedAnimState = (int)stream.ReceiveNext();
            receivedAim = (bool)stream.ReceiveNext();
            receivedRigMode = (int)stream.ReceiveNext();
            receivedCameraPitch = (float)stream.ReceiveNext();

            playerState.ApplyNetworkCameraPitch(receivedCameraPitch);
            playerState.ApplyNetworkAnimation(receivedAnimState, receivedAim);
            playerState.ApplyNetworkRig(receivedRigMode);
        }
    }

    public void SendReload()
    {
        if (!photonView.IsMine)
            return;

        photonView.RPC(nameof(RPC_Reload), RpcTarget.Others);
    }
    [PunRPC]
    private void RPC_ShowPlayerJoined(string playerName)
    {
        RoomManager.Instance.ShowPlayerJoinedMessage(playerName);
    }

    [PunRPC]
    private void RPC_Reload()
    {
        playerState.PlayNetworkReload();
    }

    public void SendWeaponAttack(Vector3 position)
    {
        photonView.RPC(nameof(RPC_WeaponAttack), RpcTarget.Others, position);
    }

    [PunRPC]
    private void RPC_WeaponAttack(Vector3 positionAttacker)
    {
        RoomManager.Instance.minimapCamera.ShowAttackDirection(positionAttacker, positionAttacker - transform.position);
        weaponHolder.currentWeapon.PlayAttackVisuals(0);
    }

    public void SendDeath()
    {
        if (!photonView.IsMine)
            return;

        photonView.RPC(nameof(RPC_Death), RpcTarget.Others);
    }

    [PunRPC]
    private void RPC_Death()
    {
        //playerState.PlayNetworkDeath();
    }
    
    public void RequestChangeWeapon(int index)
    {
        // Chỉ player sở hữu object này mới được gửi
        if (!photonView.IsMine)
            return;

        photonView.RPC(
            nameof(RPC_SetWeapon),
            RpcTarget.Others,
            index
        );
    }

    [PunRPC]
    private void RPC_SetWeapon(int index)
    {
        playerState.ChangeWeapon(index);
    }
    public void SendActivePlayer()
    {
        photonView.RPC(nameof(RPC_ActivePlayer), RpcTarget.All);
    }
    [PunRPC]
    private void RPC_ActivePlayer()
    {
        playerNameText.enabled = true;
        playerManagerState.ActivePlayer();
    }
    public void SendDisactivePlayer()
    {
        photonView.RPC(nameof(RPC_DisactivePlayer), RpcTarget.All);
    }
    [PunRPC]
    private void RPC_DisactivePlayer()
    {
        playerNameText.enabled = false;
        playerManagerState.DisactivePlayer();
    }
    public void SendPlayWeaponAnimatorClip(int i)
    {
        photonView.RPC(nameof(RPC_PlayWeaponAnimatorClip), RpcTarget.All, i);
    }
    [PunRPC]
    private void RPC_PlayWeaponAnimatorClip(int i)
    {
        AudioSource.PlayClipAtPoint(weaponHolder.currentWeapon.weaponAnimator.reloadAudioClips[i], transform.position);
    }
}
