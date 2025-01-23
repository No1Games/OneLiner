using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanelUI : MonoBehaviour
{
    private LocalPlayer m_LocalPlayer;

    private LocalLobby m_LocalLobby;

    #region Animator Hash

    private int m_InGameHash;
    private int m_IsTurnHash;
    private int m_IsReadyHash;

    #endregion

    [SerializeField] private TextMeshProUGUI _displayNameTMP;
    [SerializeField] private Image _nameBackImage;
    [SerializeField] private Image _avatarImage;
    [SerializeField] private Image _avatarBackImage;
    [SerializeField] private Image _statusImage;
    [SerializeField] private Animator _panelAnimator;
    [SerializeField] private GameObject _crownGO;

    private void Awake()
    {
        m_InGameHash = Animator.StringToHash("InGame");
        m_IsTurnHash = Animator.StringToHash("IsTurn");
        m_IsReadyHash = Animator.StringToHash("IsReady");
    }

    public void SetLocalPlayer(LocalPlayer localPlayer)
    {
        m_LocalPlayer = localPlayer;

        SetDisplayName(localPlayer.DisplayName.Value);
        SetNameBack(localPlayer.NameBackID.Value);
        SetAvatarImage(localPlayer.AvatarID.Value);
        SetAvatarBackImage(localPlayer.AvatarBackID.Value);
        SetStatus(localPlayer.UserStatus.Value);

        SubscribeOnPlayerChanges();
    }

    public void SetLocalLobby()
    {
        m_LocalLobby = OnlineController.Instance.LocalLobby;

        if (m_LocalLobby == null)
        {
            Debug.LogWarning("Local Lobby is NULL. Player is not in the lobby yet");
            return;
        }

        SetIsLeader(m_LocalLobby.LeaderID.Value);

        SubscribeOnLobbyChanges();
    }

    private void SubscribeOnLobbyChanges()
    {
        m_LocalLobby.LeaderID.onChanged += SetIsLeader;
    }

    private void SubscribeOnPlayerChanges()
    {
        m_LocalPlayer.DisplayName.onChanged += SetDisplayName;
        m_LocalPlayer.AvatarID.onChanged += SetAvatarImage;
        m_LocalPlayer.AvatarBackID.onChanged += SetAvatarBackImage;
        m_LocalPlayer.NameBackID.onChanged += SetNameBack;
        m_LocalPlayer.UserStatus.onChanged += SetStatus;
        m_LocalPlayer.IsTurn.onChanged += SetTurn;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Unsubscribe()
    {
        if (m_LocalPlayer != null)
        {
            if (m_LocalPlayer.DisplayName.onChanged != null)
                m_LocalPlayer.DisplayName.onChanged -= SetDisplayName;

            if (m_LocalPlayer.AvatarID.onChanged != null)
                m_LocalPlayer.AvatarID.onChanged -= SetAvatarImage;

            if (m_LocalPlayer.AvatarBackID.onChanged != null)
                m_LocalPlayer.AvatarBackID.onChanged -= SetAvatarBackImage;

            if (m_LocalPlayer.NameBackID.onChanged != null)
                m_LocalPlayer.NameBackID.onChanged -= SetNameBack;

            if (m_LocalPlayer.UserStatus.onChanged != null)
                m_LocalPlayer.UserStatus.onChanged -= SetStatus;

            if (m_LocalPlayer.IsTurn.onChanged != null)
                m_LocalPlayer.IsTurn.onChanged -= SetTurn;
        }

        if (m_LocalLobby != null)
        {
            if (m_LocalLobby.LeaderID.onChanged != null)
                m_LocalLobby.LeaderID.onChanged -= SetIsLeader;
        }
    }

    private void SetDisplayName(string displayName)
    {
        _displayNameTMP.text = displayName;
    }

    private void SetNameBack(int code)
    {
        _nameBackImage.sprite = ItemManager.Instance.GetItemByCode(code).icon;
    }

    private void SetAvatarImage(int code)
    {
        _avatarImage.sprite = ItemManager.Instance.GetItemByCode(code).icon;
    }

    private void SetAvatarBackImage(int code)
    {
        _avatarBackImage.sprite = ItemManager.Instance.GetItemByCode(code).icon;
    }

    private void SetIsLeader(string leaderID)
    {
        _crownGO.SetActive(leaderID == m_LocalPlayer.ID.Value);
    }

    private void SetStatus(PlayerStatus status)
    {
        if (status == PlayerStatus.InGame)
        {
            _panelAnimator.SetBool(m_InGameHash, true);

            _statusImage.gameObject.SetActive(false);
        }
        else
        {
            _panelAnimator.SetBool(m_InGameHash, false);

            _statusImage.gameObject.SetActive(true);

            _panelAnimator.SetBool(m_IsReadyHash, status == PlayerStatus.Ready);
        }
    }

    private void SetTurn(bool isTurn)
    {
        _panelAnimator.SetBool(m_IsTurnHash, isTurn);
    }
}
