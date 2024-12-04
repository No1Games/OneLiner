using TMPro;
using UnityEngine;

public class PlayerItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _displayName_TMP;

    private LocalPlayer _localPlayer;

    public void SetPlayer(LocalPlayer player)
    {
        Unsubscribe();

        _localPlayer = player;

        SetDisplayName(player.DisplayName.Value);

        // TO DO: EXPAND FOR CUSTOMIZATION

        Subscribe();
    }

    private void Subscribe()
    {
        _localPlayer.DisplayName.onChanged += SetDisplayName;
    }

    private void Unsubscribe()
    {
        if (_localPlayer == null)
            return;

        if (_localPlayer.DisplayName?.onChanged != null)
            _localPlayer.DisplayName.onChanged -= SetDisplayName;

        _localPlayer = null;
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void SetDisplayName(string name)
    {
        _displayName_TMP.text = name;
    }

    public void ResetUI()
    {
        if (_localPlayer == null)
            return;

        Unsubscribe();

        gameObject.SetActive(false);
    }
}
