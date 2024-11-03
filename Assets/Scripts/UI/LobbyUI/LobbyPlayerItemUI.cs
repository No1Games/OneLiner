using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LobbyPlayerItemUI : MonoBehaviour
{
    [Inject(Id = "RuntimeTMP")] private ILogger _logger;

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _playerStatusTMP;
    [SerializeField] private Button _kickPlayerButton;

    private LocalPlayer _localPlayer;

    private void Awake()
    {
        _kickPlayerButton.onClick.AddListener(KickPlayer);
    }

    public void SetLocalPlayer(LocalPlayer player)
    {
        _localPlayer = player;

        SetDisplayName(player.DisplayName.Value);
        SetUserStatus(player.UserStatus.Value);

        Subscribe();
    }

    public void SetKickPlayerButtonVisible(bool visible)
    {
        _kickPlayerButton.gameObject.SetActive(visible);
    }

    private void KickPlayer()
    {
        if (_localPlayer != null)
        {
            GameManager.Instance.KickPlayer(_localPlayer.ID.Value);
        }
    }

    private void Subscribe()
    {
        _localPlayer.DisplayName.onChanged += SetDisplayName;
        _localPlayer.UserStatus.onChanged += SetUserStatus;
        // _localPlayer.IsHost.onChanged += SetIsHost;
    }

    private void Unsubscribe()
    {
        if (_localPlayer == null)
            return;

        if (_localPlayer.DisplayName?.onChanged != null)
            _localPlayer.DisplayName.onChanged -= SetDisplayName;

        if (_localPlayer.UserStatus?.onChanged != null)
            _localPlayer.UserStatus.onChanged -= SetUserStatus;

        //if (m_LocalPlayer.IsHost?.onChanged != null)
        //    m_LocalPlayer.IsHost.onChanged -= SetIsHost;

        _localPlayer = null;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void SetDisplayName(string name)
    {
        _playerNameText.text = name;
    }

    public void ResetUI()
    {
        if (_localPlayer == null)
            return;

        SetUserStatus(PlayerStatus.Lobby);

        Unsubscribe();

        gameObject.SetActive(false);
    }

    private void SetUserStatus(PlayerStatus status)
    {
        _playerStatusTMP.text = SetStatusFancy(status);
    }

    private string SetStatusFancy(PlayerStatus status)
    {
        switch (status)
        {
            case PlayerStatus.Lobby:
                return "<color=#56B4E9>In Lobby</color>"; // Light Blue
            case PlayerStatus.Ready:
                return "<color=#009E73>Ready</color>"; // Light Mint
            case PlayerStatus.Connecting:
                return "<color=#F0E442>Connecting...</color>"; // Bright Yellow
            case PlayerStatus.InGame:
                return "<color=#005500>In Game</color>"; // Green
            default:
                return "";
        }
    }
}
