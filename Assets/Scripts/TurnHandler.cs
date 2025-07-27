using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TurnHandler : MonoBehaviour
{
    private OnlineController m_OnlineController;

    [SerializeField] private Button m_DrawButton;

    private LocalLobby m_LocalLobby;

    private Queue<LocalPlayer> m_PlayersQueue;
    private LocalPlayer m_Leader;

    private LocalPlayer m_CurrentPlayer;
    public LocalPlayer CurrentPlayer => m_CurrentPlayer;

    private void Awake()
    {
        // Cache controller
        m_OnlineController = OnlineController.Instance;

        // Cache current lobby
        m_LocalLobby = m_OnlineController.LocalLobby;

        // Cache current player
        m_CurrentPlayer = m_OnlineController.LocalPlayer;

    }

    private void Start()
    {
        // Subscribe on turn changing
        m_LocalLobby.CurrentPlayerID.onChanged += OnTurnIDChanged;
        m_OnlineController.PassTurnEvent += PassTurn;


        // Following actions makes only host
        if (m_OnlineController.LocalPlayer.IsHost.Value)
        {
            // Form players queue and find leader
            CreateQueue();

            // Leader first to take turn
            if (m_Leader is null)
            {
                Debug.LogWarning("Can't find leader player!");
                return;
            }

            Task task = m_OnlineController.LobbyManager.LocalLobbyEditor.SetCurrentPlayerId(m_Leader.ID.Value).CommitChangesAsync();
            //m_OnlineController.SetTurnID(m_Leader.ID.Value);
        }
    }

    private void OnDestroy()
    {
        m_LocalLobby.CurrentPlayerID.onChanged -= OnTurnIDChanged;
        m_OnlineController.PassTurnEvent -= PassTurn;
    }

    private void CreateQueue()
    {
        m_PlayersQueue = new Queue<LocalPlayer>();

        foreach (var p in m_LocalLobby.LocalPlayers)
        {
            if (p.Role.Value == PlayerRole.Leader)
            {
                m_Leader = p;
                continue;
            }
            m_PlayersQueue.Enqueue(p);
        }
    }

    private void PassTurn()
    {
        // Only host decides who the next player
        if (!m_OnlineController.LocalPlayer.IsHost.Value) return;

        LocalPlayer nextPlayer = null;

        if (m_CurrentPlayer.Role.Value == PlayerRole.Player)
        {
            m_PlayersQueue.Enqueue(m_CurrentPlayer);
            nextPlayer = m_Leader;
        }
        else if (m_CurrentPlayer.Role.Value == PlayerRole.Leader)
        {
            nextPlayer = m_PlayersQueue.Dequeue();
        }

        if (nextPlayer == null)
        {
            Debug.LogWarning($"Failed to calculate next player");
            return;
        }

        Task task = m_OnlineController.LobbyManager.LocalLobbyEditor
            .SetCurrentPlayerId(nextPlayer.ID.Value)
            .CommitChangesAsync();
        //m_OnlineController.SetTurnID(nextPlayer.ID.Value);
    }

    private void OnTurnIDChanged(string newID)
    {
        // Get current player from lobby
        //m_CurrentPlayer = m_LocalLobby.GetLocalPlayer(newID);

        //if (m_CurrentPlayer == null)
        //{
        //    Debug.Log($"Failed to find local player with id {newID}");
        //    return;
        //}

        m_DrawButton.enabled = newID == m_OnlineController.LocalPlayer.ID.Value;
    }

    public async Task EndTurn()
    {
        //m_OnlineController.SetLocalPlayerTurn(false);
        await m_OnlineController.LobbyManager.LocalPlayerEditor
            .SetIsTurn(false)
            .CommitChangesAsync();
    }
}

