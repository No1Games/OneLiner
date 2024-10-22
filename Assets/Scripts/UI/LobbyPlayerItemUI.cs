using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class LobbyPlayerItemUI : MonoBehaviour
{
    [Inject(Id = "RuntimeTMP")] private ILogger _logger;

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private Button _kickPlayerButton;

    private LocalPlayer _localPlayer;

    private void Awake()
    {
        _kickPlayerButton.onClick.AddListener(KickPlayer);
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
        if (_localPlayer != null)
        {
            GameManager.Instance.KickPlayer(_localPlayer.ID.Value);

            // LobbyManager.Instance.KickPlayer(_localPlayer.ID.Value);
        }
    }
}
