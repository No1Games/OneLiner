using System.Collections.Generic;
using UnityEngine;

public class PlayersListUI : MonoBehaviour
{
    [SerializeField] private PlayerPanelUI _playerPrefab;
    [SerializeField] private Transform _container;

    private ObjectPool<PlayerPanelUI> _itemPool;
    private List<PlayerPanelUI> _activeItems;

    private LocalLobby _localLobby;

    private void Awake()
    {
        _itemPool = new ObjectPool<PlayerPanelUI>(_playerPrefab);
        _activeItems = new List<PlayerPanelUI>();
    }

    public void AddPlayer(LocalPlayer player)
    {
        PlayerPanelUI panel = _itemPool.GetObject();

        panel.transform.SetParent(_container, false);
        panel.SetLocalPlayer(player);

        if (_localLobby != null)
        {
            panel.SetLocalLobby();
        }

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
        _localLobby = OnlineController.Instance.LocalLobby;

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
}
