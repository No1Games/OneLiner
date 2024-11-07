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
    
    private List<PlayerScript> players = new();
    private List<GameObject> playersPlates = new();
    [SerializeField] private AccountManager accountManager;

    private LocalGameSetup_Func lgsFunc;
    

    private void Awake()
    {
        lgsFunc = new(playerPlatePrefab, playerPanel);
        localSetupUI.OnAddPlayerBtnClick += AddPlayer;
        localSetupUI.OnStartGameBtnClick += StartLocalGame;
        localSetupUI.OnRandomLeaderBtnClick += ChoseRandomLeader;
    }

    private void AddPlayer()
    {
        PlayerScript player;
        if (players.Count < maxPlayers)
        {
            if (players.Count == 0)
            {
               player = accountManager.player;
            }
            else
            {
               player = lgsFunc.AddRandomPlayer(players);
            }
           players.Add(player);
            if(players.Count == maxPlayers)
            {
                localSetupUI.AddPlayerBtn_VisibilityChange(false);
            }
           GameObject newPlate = lgsFunc.GeneratePlayerPlate(player);
           
            //if (players.Count == maxPlayers)
            //{
                
            //    newPlate.transform.SetSiblingIndex(playerPanel.transform.childCount - 1);
            //}
            //else
            //{
                newPlate.transform.SetSiblingIndex(playerPanel.transform.childCount - 2);
            //}

            playersPlates.Add(newPlate);
            if(playersPlates.Count == 1)
            {
                lgsFunc.AssignLeader(playersPlates, player);
            }

            newPlate.GetComponent<PlateCustomization>().FirstMiniButton.onClick.AddListener(() => RemovePlayer(player));

            newPlate.GetComponent<PlateCustomization>().SecondMiniButton.onClick.AddListener(EditPlayer);


        }
        else
        {
            Debug.Log("Max Players reached");
        }
    }
    
    private void RemovePlayer(PlayerScript player)
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        if (players.Count > 0)
        {
            if(player.role == PlayerRole.Leader)
            {
                ChoseRandomLeader();
            }
            
            players.Remove(player);

            for (int i = playersPlates.Count - 1; i >= 0; i--)
            {
                var plate = playersPlates[i];
                if (plate.GetComponent<PlateCustomization>().player == player)
                {
                    playersPlates.RemoveAt(i);
                    Destroy(plate);
                }
            }

            if(players.Count == maxPlayers - 1)
            {
                localSetupUI.AddPlayerBtn_VisibilityChange(true);
            }
        }

    }
    private void EditPlayer()
    {

    }

    private void StartLocalGame()
    {
        if (players.Count >= minPlayers)
        {
            AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Play);
            lgsFunc.NextLevel(players);

        }
            else
        {
            AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
            Debug.LogWarning("Перехід на наступну сцену заблоковано.");
        }
    }

    public void ChoseRandomLeader()
    {
        if (players.Count > 1)
        {
            lgsFunc.AssignLeader(playersPlates);
        }

    }
}
