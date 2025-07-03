using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomItemUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private HostDataUI _hostData_Panel;
    [SerializeField] private TextMeshProUGUI _modeName_TMP;
    [SerializeField] private TextMeshProUGUI _playersCount_TMP;
    [SerializeField] private Button _buttonComponent;
    [SerializeField] private GameObject _joinBtnParent;
    [SerializeField] private Button _join_Btn;

    public event Action<LocalLobby> LobbySelectedEvent;
    public event Action<LocalLobby> JoinLobbyEvent;

    private LocalLobby _localLobby;
    public LocalLobby LocalLobby => _localLobby;

    private void Awake()
    {
        _buttonComponent.onClick.AddListener(OnClick_SelfButton);
        _join_Btn.onClick.AddListener(OnClick_JoinButton);
    }

    private void UpdateUI()
    {
        SetHostPanel(_localLobby.ApperanceData.Value);
        SetModeTMP(_localLobby.Mode.Value);
        SetPlayersCountTMP(_localLobby.PlayerCount);
    }

    private void Subscribe()
    {
        _localLobby.ApperanceData.onChanged += SetHostPanel;
        _localLobby.Mode.onChanged += SetModeTMP;
        _localLobby.PlayersCountChangedEvent += SetPlayersCountTMP;
    }

    private void Unsubscribe()
    {
        if (_localLobby == null)
            return;

        if (_localLobby.ApperanceData.onChanged != null)
            _localLobby.ApperanceData.onChanged -= SetHostPanel;

        if (_localLobby.Mode.onChanged != null)
            _localLobby.Mode.onChanged -= SetModeTMP;

        if (_localLobby.PlayersCountChangedEvent != null)
            _localLobby.PlayersCountChangedEvent -= SetPlayersCountTMP;

        _localLobby = null;
    }

    #region Setters

    public void SetLocalLobby(LocalLobby lobby)
    {
        Unsubscribe();

        _localLobby = lobby;

        Subscribe();

        UpdateUI();
    }

    private void SetHostPanel(LobbyAppearanceData newValue)
    {
        if (newValue == null)
        {
            Debug.LogWarning("Host data is null");
            return;
        }

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

    #endregion

    public void Deselect()
    {
        _joinBtnParent.SetActive(false);
    }

    private void Select()
    {
        LobbySelectedEvent?.Invoke(_localLobby);
        _joinBtnParent.SetActive(true);
    }

    #region Click Handlers

    private void OnClick_SelfButton()
    {
        Select();
    }

    private void OnClick_JoinButton()
    {
        JoinLobbyEvent?.Invoke(_localLobby);
    }

    #endregion

    private void OnDestroy()
    {
        Unsubscribe();
    }
}
