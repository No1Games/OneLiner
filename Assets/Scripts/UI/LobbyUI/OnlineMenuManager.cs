using UnityEngine;

public class OnlineMenuManager : MonoBehaviour
{
    private Canvas _onlineMenuCanvas;

    [SerializeField] private RoomsListUI _lobbyList;
    [SerializeField] private WaitingRoomUI _roomPanel;

    private static OnlineMenuManager _instance;
    public static OnlineMenuManager Instance => _instance;

    private void Awake()
    {
        _instance = this;

        _onlineMenuCanvas = GetComponent<Canvas>();

        _onlineMenuCanvas.renderMode = RenderMode.ScreenSpaceCamera;

        _onlineMenuCanvas.worldCamera = Camera.main;
    }

    public void OpenRoomPanel(bool isCreate)
    {
        _lobbyList.Hide();
        _roomPanel.Show(isCreate);
    }

    public void OpenLobbyList()
    {
        _lobbyList.Show();
        _roomPanel.gameObject.SetActive(false);
    }
}
