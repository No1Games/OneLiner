using System.Collections.Generic;
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

    public void RemovePlayer(int index)
    {
        if (index < 0 || index >= m_ActiveItems.Count)
        {
            Debug.Log("Index out of bounds.");
        }

        m_ItemPool.ReturnObject(m_ActiveItems[index]);

        m_ActiveItems.RemoveAt(index);
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
        if (m_ActiveItems == null || m_ActiveItems.Count == 0)
        {
            Debug.Log("Player List is empty.");
            return;
        }

        foreach (var item in m_ActiveItems)
        {
            m_ItemPool.ReturnObject(item);
        }

        m_ActiveItems.Clear();
    }
}
