using UnityEngine;

public class OnlineMenuManager : MonoBehaviour
{
    [SerializeField] private LobbyListUI _lobbyList;
    [SerializeField] private WaitingRoomUI _roomPanel;

    private static OnlineMenuManager _instance;
    public static OnlineMenuManager Instance => _instance;

    private void Awake()
    {
        _instance = this;
    }

    public void OpenRoomPanel(bool isCreate)
    {
        _lobbyList.gameObject.SetActive(false);
        _roomPanel.Show(isCreate);
    }

    public void OpenLobbyList()
    {
        _lobbyList.gameObject.SetActive(true);
        _roomPanel.gameObject.SetActive(false);
    }
}
