using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerItemUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private Button _kickPlayerButton;

    private Player _player;

    private void Awake()
    {
        _kickPlayerButton.onClick.AddListener(KickPlayer);
    }

    public void UpdatePlayer(Player player)
    {
        _player = player;
        _playerNameText.text = player.Data[LobbyManager.KEY_PLAYER_NAME].Value;
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
