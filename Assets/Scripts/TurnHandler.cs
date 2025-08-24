using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TurnHandler : NetworkBehaviour
{
    public NetworkVariable<FixedString64Bytes> CurrentPlayerId = new NetworkVariable<FixedString64Bytes>();
    private FixedString64Bytes _leaderId;
    private Queue<FixedString64Bytes> _idQueue;

    private OnlineController _onlineController;

    [SerializeField]
    private Button _drawButton;

    private void Awake()
    {
        // Cache controller
        _onlineController = OnlineController.Instance;
        _drawButton.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        CurrentPlayerId.OnValueChanged += UpdateUI;

        // Following actions makes only host
        if (IsHost)
        {
            _leaderId = (FixedString64Bytes)_onlineController.LocalLobby.LeaderID.Value;

            // Leader first to take turn
            if (_leaderId.IsEmpty)
            {
                Debug.LogWarning("Can't find leader player!");
                return;
            }

            // Form players queue
            CreateQueue();

            SetTurnTo(_leaderId);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        CurrentPlayerId.OnValueChanged -= UpdateUI;
    }

    private void UpdateUI(FixedString64Bytes prevId, FixedString64Bytes newId)
    {
        _drawButton.enabled = _onlineController.LocalPlayer.PlayerId.Value.Equals(newId.ToString());
    }

    private void CreateQueue()
    {
        _idQueue = new Queue<FixedString64Bytes>();

        foreach (var p in _onlineController.LocalLobby.LocalPlayers)
        {
            if (p.PlayerId.Value == _leaderId.ToString()) continue;

            _idQueue.Enqueue((FixedString64Bytes)p.PlayerId.Value);
        }
    }

    public void SetTurnTo(FixedString64Bytes value)
    {
        CurrentPlayerId.Value = value;
    }

    [Rpc(SendTo.Server)]
    public void EndTurnRpc()
    {
        FixedString64Bytes nextId;

        if (_leaderId == CurrentPlayerId.Value)
        {
            nextId = _idQueue.Dequeue();
        }
        else
        {
            _idQueue?.Enqueue(CurrentPlayerId.Value);
            nextId = _leaderId;
        }

        SetTurnTo(nextId);
    }
}

