using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayersListUI : MonoBehaviour
{
    [SerializeField] private PlayerPanelUI m_PlayerPrefab;
    [SerializeField] private Transform m_Container;

    private ObjectPool<PlayerPanelUI> m_ItemPool;
    private List<PlayerPanelUI> m_ActiveItems;

    private LocalLobby m_LocalLobby;
    private OnlineController m_OnlineController;

    private void Awake()
    {
        m_ItemPool = new ObjectPool<PlayerPanelUI>(m_PlayerPrefab);
        m_ActiveItems = new List<PlayerPanelUI>();

        m_OnlineController = OnlineController.Instance;
        m_LocalLobby = m_OnlineController.LocalLobby;
    }

    private void Start()
    {
        UpdateList(m_LocalLobby.LocalPlayers);
    }

    public void AddPlayer(LocalPlayer player)
    {
        PlayerPanelUI panel = m_ItemPool.GetObject();

        panel.transform.SetParent(m_Container, false);
        panel.SetLocalPlayer(player);

        if (m_LocalLobby != null)
        {
            panel.SetLocalLobby();
        }

        m_ActiveItems.Add(panel);
    }

    public void RemovePlayer(LocalPlayer player)
    {
        if (player == null)
        {
            Debug.LogWarning("Cannot remove player from UI. Player is null");
            return;
        }

        var item = m_ActiveItems.FirstOrDefault(i => i.LocalPlayer.Equals(player));

        if (item == null)
        {
            Debug.LogWarning($"Cannot find specified player for remove. Player Id: {player.PlayerId.Value}");
            return;
        }

        item.Unsubscribe();

        m_ItemPool.ReturnObject(item);

        m_ActiveItems.Remove(item);
    }

    public void RemovePlayer(int index)
    {
        if (index < 0 || index >= m_ActiveItems.Count)
        {
            Debug.Log("Index out of bounds.");
        }

        m_ActiveItems[index].Unsubscribe();

        m_ItemPool.ReturnObject(m_ActiveItems[index]);

        m_ActiveItems.RemoveAt(index);
    }

    public void UpdateList(List<LocalPlayer> localPlayers)
    {
        if (m_ActiveItems.Count == 0)
        {
            foreach (var player in localPlayers)
            {
                AddPlayer(player);
            }
        }
        else
        {
            var temp = new List<PlayerPanelUI>();
            foreach (var player in localPlayers)
            {
                temp.Add(m_ActiveItems.Find(p => p.PlayerID == player.PlayerId.Value));
            }
            var disable = m_ActiveItems.Where(i => !temp.Contains(i)).ToList();
            foreach (var item in disable)
            {
                item.SetDefault();

                m_ItemPool.ReturnObject(item);

                m_ActiveItems.Remove(item);
            }
        }

        //ClearList();
    }

    public void ClearList()
    {
        if (m_ActiveItems == null || m_ActiveItems.Count == 0)
        {
            Debug.LogWarning("Player List is empty.");
            return;
        }

        foreach (var item in m_ActiveItems)
        {
            m_ItemPool.ReturnObject(item);
        }

        m_ActiveItems.Clear();
    }
}
