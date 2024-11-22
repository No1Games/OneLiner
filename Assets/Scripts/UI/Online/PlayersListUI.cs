using System.Collections.Generic;
using UnityEngine;

public class PlayersListUI : MonoBehaviour
{
    [SerializeField] private PlayerPanelUI _playerPrefab;
    [SerializeField] private Transform _container;

    private ObjectPool<PlayerPanelUI> _itemPool;
    private List<PlayerPanelUI> _activeItems;

    //private List<PlayerPanelUI> _playersPool = new List<PlayerPanelUI>();

    //private LocalLobby _lobby;

    private void Awake()
    {
        _itemPool = new ObjectPool<PlayerPanelUI>(_playerPrefab);
        _activeItems = new List<PlayerPanelUI>();
    }

    //private void Start()
    //{
    //    _lobby = GameManager.Instance.LocalLobby;

    //    SetPlayersUI();
    //}

    //private void SetPlayersUI()
    //{
    //    //if (_playersPool.Count < _lobby.LocalPlayers.Count)
    //    //{
    //    //    InstantiatePlayers(_lobby.LocalPlayers.Count - _playersPool.Count);
    //    //}

    //    for (int i = 0; i < _lobby.LocalPlayers.Count; i++)
    //    {
    //        //_playersPool[i].gameObject.SetActive(true);
    //        PlayerPanelUI item = _itemPool.GetObject();
    //        _playersPool[i].SetLocalPlayer(_lobby.GetLocalPlayer(i));
    //    }
    //}

    public void AddPlayer(LocalPlayer player)
    {
        PlayerPanelUI panel = _itemPool.GetObject();

        panel.transform.SetParent(_container, false);
        panel.SetLocalPlayer(player);

        _activeItems.Add(panel);
    }

    public void RemovePlayer(int index)
    {
        if (index < 0 || index >= _activeItems.Count)
        {
            Debug.Log("Index out of bounds.");
        }

        _itemPool.ReturnObject(_activeItems[index]);

        _activeItems.RemoveAt(index);
    }

    public void UpdateList(List<LocalPlayer> localPlayers)
    {
        ClearList();

        foreach (var player in localPlayers)
        {
            AddPlayer(player);
        }
    }

    public void ClearList()
    {
        foreach (var item in _activeItems)
        {
            _itemPool.ReturnObject(item);
        }

        _activeItems.Clear();
    }


    //private void InstantiatePlayers(int count)
    //{
    //    for (int i = 0; i < count; i++)
    //    {
    //        InstantiatePlayer();
    //    }
    //}

    //private void InstantiatePlayer()
    //{
    //    PlayerPanelUI playerPanel = Instantiate(_playerPrefab, transform);
    //    playerPanel.gameObject.SetActive(false);
    //    _playersPool.Add(playerPanel);
    //}
}
