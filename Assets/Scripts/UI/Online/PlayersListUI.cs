using System.Collections.Generic;
using UnityEngine;

public class PlayersListUI : MonoBehaviour
{
    [SerializeField] private PlayerPanelUI _playerPrefab;
    [SerializeField] private Transform _container;

    private ObjectPool<PlayerPanelUI> _itemPool;
    private Dictionary<string, PlayerPanelUI> _itemsById;

    private LocalLobby _localLobby;
    private OnlineController _onlineController;

    private void Awake()
    {
        _itemPool = new ObjectPool<PlayerPanelUI>(_playerPrefab, _container, 6);
        _itemsById = new Dictionary<string, PlayerPanelUI>();

        _onlineController = OnlineController.Instance;
        _localLobby = _onlineController.LocalLobby;
    }

    private void Start()
    {
        UpdateList(_localLobby.LocalPlayers);
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

        panel.gameObject.SetActive(true);

        _itemsById.Add(player.PlayerId.Value, panel);
    }

    public void RemovePlayer(LocalPlayer player)
    {
        if (player == null)
        {
            Debug.LogWarning("Cannot remove player from UI. Player is null");
            return;
        }

        PlayerPanelUI panel;

        if (_itemsById.TryGetValue(player.PlayerId.Value, out panel))
        {
            panel.SetDefault();
            _itemPool.ReturnObject(panel);
            _itemsById.Remove(player.PlayerId.Value);
        }
        else
        {
            Debug.LogWarning($"Cannot find specified player for remove. Player Id: {player.PlayerId.Value}");
        }
    }

    public void UpdateList(List<LocalPlayer> localPlayers)
    {
        var seen = new HashSet<string>();

        foreach (var player in localPlayers)
        {
            PlayerPanelUI item;

            if (_itemsById.TryGetValue(player.PlayerId.Value, out item))
            {
                // Item is already exists - update player
                item.SetLocalPlayer(player);
            }
            else
            {
                item = _itemPool.GetObject();

                item.transform.SetParent(_container, false);
                item.SetLocalPlayer(player);

                if (_localLobby != null)
                {
                    item.SetLocalLobby();
                }

                _itemsById.Add(player.PlayerId.Value, item);
            }

            item.gameObject.SetActive(true);

            seen.Add(player.PlayerId.Value);
        }

        var toRemove = new List<string>();
        foreach (var kvp in _itemsById)
        {
            if (!seen.Contains(kvp.Key))
            {
                kvp.Value.SetDefault();
                _itemPool.ReturnObject(kvp.Value);
            }
        }
    }

    public void ClearList()
    {
        if (_itemsById.Count == 0)
        {
            Debug.LogWarning("Player List is empty.");
            return;
        }

        foreach (var item in _itemsById.Values)
        {
            item.SetDefault();
            _itemPool.ReturnObject(item);
        }

        _itemsById.Clear();
    }
}
