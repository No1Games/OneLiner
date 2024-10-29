using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPanelUI : MonoBehaviour
{
    private LocalPlayer _localPlayer;

    [SerializeField] private TextMeshProUGUI _displayNameTMP;
    [SerializeField] private Image _nameBackImage;
    [SerializeField] private Image _avatarImage;
    [SerializeField] private Image _avatarBackImage;
    [SerializeField] private GameObject _crownGO;

    public void SetLocalPlayer(LocalPlayer localPlayer)
    {
        _localPlayer = localPlayer;

        SetDisplayName(localPlayer.DisplayName.Value);
        SetNameBack(localPlayer.NameBackID.Value);
        SetAvatarImage(localPlayer.AvatarID.Value);
        SetAvatarBackImage(localPlayer.AvatarBackID.Value);

        Subscribe();
    }

    private void Subscribe()
    {
        _localPlayer.DisplayName.onChanged += SetDisplayName;
        _localPlayer.AvatarID.onChanged += SetAvatarImage;
        _localPlayer.AvatarBackID.onChanged += SetAvatarBackImage;
        _localPlayer.NameBackID.onChanged += SetNameBack;
        _localPlayer.Role.onChanged += SetPlayerRole;
    }

    private void OnDestroy()
    {
        UnSubscribe();
    }

    private void UnSubscribe()
    {
        if (_localPlayer == null) return;

        _localPlayer.DisplayName.onChanged -= SetDisplayName;
        _localPlayer.AvatarID.onChanged -= SetAvatarImage;
        _localPlayer.AvatarBackID.onChanged -= SetAvatarBackImage;
        _localPlayer.NameBackID.onChanged -= SetNameBack;
        _localPlayer.Role.onChanged -= SetPlayerRole;
    }

    private void SetDisplayName(string displayName)
    {
        _displayNameTMP.text = displayName;
    }

    private void SetNameBack(int index)
    {
        _nameBackImage.sprite = GameManager.Instance.AvatarManager.GetNameBackImage(index);
    }

    private void SetAvatarImage(int index)
    {
        _avatarImage.sprite = GameManager.Instance.AvatarManager.GetAvatarImage(index);
    }

    private void SetAvatarBackImage(int index)
    {
        _avatarBackImage.sprite = GameManager.Instance.AvatarManager.GetAvatarBackImage(index);
    }

    private void SetPlayerRole(PlayerRole role)
    {
        _crownGO.SetActive(role == PlayerRole.Leader);
    }
}
