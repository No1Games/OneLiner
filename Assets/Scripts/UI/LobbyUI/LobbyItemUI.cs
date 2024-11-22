using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyItemUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private HostDataUI _hostData_Panel;
    [SerializeField] private TextMeshProUGUI _modeName_TMP;
    [SerializeField] private TextMeshProUGUI _playersCount_TMP;
    [SerializeField] private Button _buttonComponent;

    public event Action<LocalLobby> LobbySelectedEvent;

    private LocalLobby _localLobby;

    private void Awake()
    {
        _buttonComponent.onClick.AddListener(() =>
        {
            if (_localLobby != null)
                LobbySelectedEvent?.Invoke(_localLobby);
        });
    }

    public void SetLocalLobby(LocalLobby lobby)
    {
        Unsubscribe();

        _localLobby = lobby;

        Subscribe();

        UpdateUI();
    }

    private void UpdateUI()
    {
        SetHostPanel(_localLobby.HostData.Value);
        SetModeTMP(_localLobby.Mode.Value);
        SetPlayersCountTMP(_localLobby.PlayerCount);
    }

    private void Subscribe()
    {
        _localLobby.HostData.onChanged += SetHostPanel;
        _localLobby.Mode.onChanged += SetModeTMP;
        _localLobby.PlayersCountChangedEvent += SetPlayersCountTMP;
    }

    private void Unsubscribe()
    {
        if (_localLobby == null)
            return;

        if (_localLobby.HostData.onChanged != null)
            _localLobby.HostData.onChanged -= SetHostPanel;

        if (_localLobby.Mode.onChanged != null)
            _localLobby.Mode.onChanged -= SetModeTMP;

        if (_localLobby.PlayersCountChangedEvent != null)
            _localLobby.PlayersCountChangedEvent -= SetPlayersCountTMP;

        _localLobby = null;
    }

    private void SetHostPanel(HostData newValue)
    {
        _hostData_Panel.SetHostData(newValue);
    }

    private void SetModeTMP(string newValue)
    {
        _modeName_TMP.text = newValue;
    }

    private void SetPlayersCountTMP(int newValue)
    {
        _playersCount_TMP.text = $"{newValue}/{_localLobby.MaxPlayerCount.Value}";
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    //private void OnClick_JoinButton()
    //{
    //    try
    //    {
    //        GameManager.Instance.JoinLobby(_localLobby.LobbyID.Value, _localLobby.LobbyCode.Value);
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogException(e);
    //    }
    //}
}
