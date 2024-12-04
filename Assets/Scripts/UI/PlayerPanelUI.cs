using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanelUI : MonoBehaviour
{
    private LocalPlayer _localPlayer;

    private LocalLobby _localLobby;

    [SerializeField] private TextMeshProUGUI _displayNameTMP;
    [SerializeField] private Image _nameBackImage;
    [SerializeField] private Image _avatarImage;
    [SerializeField] private Image _avatarBackImage;
    [SerializeField] private GameObject _crownGO;

    public void SetLocalPlayer(LocalPlayer localPlayer)
    {
        _localPlayer = localPlayer;

        SetDisplayName(localPlayer.DisplayName.Value);
        //SetNameBack(localPlayer.NameBackID.Value);
        //SetAvatarImage(localPlayer.AvatarID.Value);
        //SetAvatarBackImage(localPlayer.AvatarBackID.Value);

        SubscribeOnPlayerChanges();
    }

    public void SetLocalLobby()
    {
        _localLobby = OnlineGameManager.Instance.LocalLobby;

        if (_localLobby == null)
        {
            Debug.LogWarning("Local Lobby is NULL. Player is not in the lobby yet");
            return;
        }

        Debug.Log($"Player ID: {_localPlayer.ID.Value} --- Leader ID: {_localLobby.LeaderID.Value}");

        SetIsLeader(_localLobby.LeaderID.Value);

        SubscribeOnLobbyChanges();
    }

    private void SubscribeOnLobbyChanges()
    {
        _localLobby.LeaderID.onChanged += SetIsLeader;
    }

    private void SubscribeOnPlayerChanges()
    {
        _localPlayer.DisplayName.onChanged += SetDisplayName;
        //_localPlayer.AvatarID.onChanged += SetAvatarImage;
        //_localPlayer.AvatarBackID.onChanged += SetAvatarBackImage;
        //_localPlayer.NameBackID.onChanged += SetNameBack;
        _localPlayer.Role.onChanged += SetPlayerRole;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Unsubscribe()
    {
        if (_localPlayer != null)
        {
            if (_localPlayer.DisplayName.onChanged != null)
                //_localPlayer.DisplayName.onChanged -= SetDisplayName;

            if (_localPlayer.AvatarID.onChanged != null)
               // _localPlayer.AvatarID.onChanged -= SetAvatarImage;

            if (_localPlayer.AvatarBackID.onChanged != null)
               // _localPlayer.AvatarBackID.onChanged -= SetAvatarBackImage;

            if (_localPlayer.NameBackID.onChanged != null)
                //_localPlayer.NameBackID.onChanged -= SetNameBack;
        }

        if (_localLobby != null)
        {
            if (_localLobby.LeaderID.onChanged != null)
                _localLobby.LeaderID.onChanged -= SetIsLeader;
        }
    }

    private void SetDisplayName(string displayName)
    {
        _displayNameTMP.text = displayName;
    }
    
    //private void SetNameBack(int index)
    //{
    //    _nameBackImage.sprite = GameManager.Instance.AvatarManager.GetNameBackImage(index);
    //}

    //private void SetAvatarImage(int index)
    //{
    //    _avatarImage.sprite = GameManager.Instance.AvatarManager.GetAvatarImage(index);
    //}

    //private void SetAvatarBackImage(int index)
    //{
    //    _avatarBackImage.sprite = GameManager.Instance.AvatarManager.GetAvatarBackImage(index);
    //}

    private void SetIsLeader(string leaderID)
    {
        _crownGO.SetActive(leaderID == _localPlayer.ID.Value);
    }
}
