using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostDataUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _displayNameTMP;
    [SerializeField] private Image _nameBackImage;
    [SerializeField] private Image _avatarImage;
    [SerializeField] private Image _avatarBackImage;

    public void SetHostData(LobbyAppearanceData data)
    {
        if (data == null)
        {
            Debug.LogWarning("Host data is null");
            return;
        }

        _displayNameTMP.text = data.Name;
        _nameBackImage.sprite = ItemManager.Instance.GetItemByCode(data.NameBackground).icon;
        _avatarImage.sprite = ItemManager.Instance.GetItemByCode(data.Avatar).icon;
        _avatarBackImage.sprite = ItemManager.Instance.GetItemByCode(data.AvatarBackground).icon;
    }
}
