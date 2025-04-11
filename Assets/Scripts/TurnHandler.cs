using System.Collections.Generic;
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

            m_OnlineController.SetTurnID(m_Leader.ID.Value);
        }
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

        m_OnlineController.SetTurnID(nextPlayer.ID.Value);
    }

    private void OnTurnIDChanged(string newID)
    {
        // Get current player from lobby
        m_CurrentPlayer = m_LocalLobby.GetLocalPlayer(newID);

        if (m_CurrentPlayer == null)
        {
            Debug.Log($"Failed to find local player with id {newID}");
            return;
        }

        m_DrawButton.enabled = m_CurrentPlayer.ID.Value == m_OnlineController.LocalPlayer.ID.Value;
    }

    public void EndTurn()
    {
        m_OnlineController.SetLocalPlayerTurn(false);
    }
}

