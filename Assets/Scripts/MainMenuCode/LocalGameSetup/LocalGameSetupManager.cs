using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalGameSetupManager : MonoBehaviour
{

    private int maxPlayers = 8;
    private int minPlayers = 2;
    [SerializeField] private GameObject playerPlatePrefab;
    [SerializeField] private GameObject playerPanel;
    [SerializeField] LocalSetup localSetupUI;
    private LocalGameSetup_Func lgsFunc;
    private List<PlayerScript> players = new();
    private List<GameObject> playersPlates = new();
    [SerializeField] private AccountManager accountManager;

    private void Awake()
    {
        lgsFunc = new(playerPlatePrefab, playerPanel);
    }

    private void AddPlayer()
    {
        if (players.Count < maxPlayers)
        {
            if (players.Count == 0)
            {
                players.Add(accountManager.player);
            }
            else
            {
                lgsFunc.AddRandomPlayer(players);
            }

            GameObject newPlate = lgsFunc.GeneratePlayerPlate(players[players.Count - 1]);
            if (players.Count == maxPlayers)
            {
                localSetupUI.AddPlayerBtn_VisibilityChange(false);
                newPlate.transform.SetSiblingIndex(playerPanel.transform.childCount - 1);
            }
            else
            {
                newPlate.transform.SetSiblingIndex(playerPanel.transform.childCount - 2);
            }

            playersPlates.Add(newPlate);



        }
        else
        {
            Debug.Log("Max Players reached");
        }
    }

}
