using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LobbyPlayerItemUI : MonoBehaviour
{
    [Inject(Id = "RuntimeTMP")] private ILogger _logger;

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private Button _kickPlayerButton;

    private Player _player;
    private LocalPlayer _localPlayer;

    private void Awake()
    {
        _kickPlayerButton.onClick.AddListener(KickPlayer);
    }

    public void UpdatePlayer(Player player)
    {
        _player = player;
        _playerNameText.text = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;
    }

    public void SetLocalPlayer(LocalPlayer player)
    {
        Debug.Log($"Setting player item: {player.ID.Value}");

        _localPlayer = player;
        _playerNameText.text = player.DisplayName.Value;
    }

    public void SetKickPlayerButtonVisible(bool visible)
    {
        _kickPlayerButton.gameObject.SetActive(visible);
    }

    private void KickPlayer()
    {
        if (_player != null)
        {
            LobbyManager.Instance.KickPlayer(_player.Id);
        }
    }
}
