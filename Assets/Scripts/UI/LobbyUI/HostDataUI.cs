using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HostDataUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _displayNameTMP;
    [SerializeField] private Image _nameBackImage;
    [SerializeField] private Image _avatarImage;
    [SerializeField] private Image _avatarBackImage;

    public void SetHostData(HostData data)
    {
        if (data == null)
        {
            Debug.LogWarning("Host data is null");
            return;
        }

        _displayNameTMP.text = data.Name;
        _nameBackImage.sprite = AvatarManager.Instance.GetNameBackImage(data.NameBackground);
        _avatarImage.sprite = AvatarManager.Instance.GetAvatarImage(data.Avatar);
        _avatarBackImage.sprite = AvatarManager.Instance.GetAvatarBackImage(data.AvatarBackground);
    }
}
