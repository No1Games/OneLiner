using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using UnityEngine;

public class OnlineGameManager : MonoBehaviour
{
    static OnlineGameManager _instance;
    public static OnlineGameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<OnlineGameManager>();

                if (_instance == null)
                {
                    GameObject singletonObj = new GameObject("OnlineGameManager");
                    _instance = singletonObj.AddComponent<OnlineGameManager>();
                }

                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }

    #region Lobby Fields and Props

    public LobbyManager LobbyManager { get; private set; }

    private LocalLobby _localLobby;
    public LocalLobby LocalLobby => _localLobby;

    private LocalPlayer _localUser;
    private bool _doesNeedCleanup;

    public LocalPlayer LocalUser => _localUser;
    public LocalLobbyList LobbyList { get; private set; } = new LocalLobbyList();

    #endregion

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        _localUser = new LocalPlayer("", 0, false, "LocalPlayer");
        _localLobby = new LocalLobby { LocalLobbyState = { Value = LobbyState.Lobby } };
        LobbyManager = new LobbyManager();

        AuthenticatePlayer();

        QueryLobbies();
    }

    void AuthenticatePlayer()
    {
        var localId = AuthenticationService.Instance.PlayerId;

        // TODO: GET NAME FROM PLAYER PREFS OR ACCOUNT

        var randomName = RandomNameGenerator.GetName();

        _localUser.ID.Value = localId;

        SetLocalUserName(randomName);
    }

    #region Local Player Setters

    public void SetLocalUserName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Debug.Log("Empty Name not allowed.");
            return;
        }
        _localUser.DisplayName.Value = name;
        SendLocalUserData();
    }

    public void SetLocalUserStatus(PlayerStatus status)
    {
        _localUser.UserStatus.Value = status;

        SendLocalUserData();
    }

    private async void SendLocalUserData(LocalPlayer localPlayer = null, string id = null)
    {
        await LobbyManager.UpdatePlayerDataAsync(
            LobbyConverters.LocalToRemoteUserData(localPlayer == null ? _localUser : localPlayer), id);
    }

    #endregion

    #region Local Lobby Setters



    #endregion

    #region Lobby Commands

    public async void CreateLobby(string name, bool isPrivate, int maxPlayers = 4)
    {
        try
        {
            var lobby = await LobbyManager.CreateLobbyAsync(
                name,
                maxPlayers,
                isPrivate,
                _localUser);

            LobbyConverters.RemoteToLocal(lobby, _localLobby);

            await CreateLobby();

            // MainMenuManager.Instance.ChangeMenu(MenuName.Lobby);
        }
        catch (LobbyServiceException exception)
        {
            Debug.Log($"Error creating lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    public async void JoinLobby(string lobbyID, string lobbyCode)
    {
        try
        {
            var lobby = await LobbyManager.JoinLobbyAsync(lobbyID, lobbyCode, _localUser);

            LobbyConverters.RemoteToLocal(lobby, _localLobby);
            await JoinLobby();

            // SetLocalUserRole(PlayerRole.Player);
        }
        catch (LobbyServiceException exception)
        {
            MainMenuManager.Instance.ChangeMenu(MenuName.LobbyList);
            Debug.Log($"Error joining lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    private async Task CreateLobby()
    {
        _localUser.IsHost.Value = true;
        // _localLobby.onUserReadyChange += OnPlayersReady;
        try
        {
            await BindLobby();

            SetLocalUserStatus(PlayerStatus.Lobby);

            // SetLocalUserRole(PlayerRole.Leader);
        }
        catch (LobbyServiceException exception)
        {
            Debug.Log($"Couldn't join Lobby : ({exception.ErrorCode}) {exception.Message}");
        }
    }

    private async Task JoinLobby()
    {
        _localUser.IsHost.ForceSet(false);
        await BindLobby();

        // SetLocalUserRole(PlayerRole.Player);
    }

    async Task BindLobby()
    {
        await LobbyManager.BindLocalLobbyToRemote(_localLobby.LobbyID.Value, _localLobby);

        //_localLobby.LocalLobbyState.onChanged += OnLobbyStateChanged;

        //_localLobby.CurrentPlayerID.onChanged += OnCurrentTurnIDChanged;

        // _localLobby.onUserTurnChanged += OnAnyUserTurnChanged;

        //SetLobbyView();
        //MainMenuManager.Instance.ChangeMenu(MenuName.Lobby);
    }

    public async void LeaveLobby()
    {
        if (_localLobby == null) return;

        _localUser.ResetState();
        await LobbyManager.LeaveLobbyAsync();
        _localLobby.ResetLobby();
        LobbyList.Clear();
    }

    #endregion

    #region Lobby List Commands

    public async void QueryLobbies()
    {
        LobbyList.QueryState.Value = LobbyQueryState.Fetching;
        var qr = await LobbyManager.GetLobbyListAsync();
        if (qr == null)
        {
            return;
        }

        SetCurrentLobbies(LobbyConverters.QueryToLocalList(qr));
    }

    private void SetCurrentLobbies(IEnumerable<LocalLobby> lobbies)
    {
        var newLobbyDict = new Dictionary<string, LocalLobby>();
        foreach (var lobby in lobbies)
            newLobbyDict.Add(lobby.LobbyID.Value, lobby);

        LobbyList.CurrentLobbies = newLobbyDict;
        LobbyList.QueryState.Value = LobbyQueryState.Fetched;
    }

    #endregion
}
