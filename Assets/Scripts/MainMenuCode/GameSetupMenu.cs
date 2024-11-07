using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class GameSetupMenu : MonoBehaviour
{
    private int playerAmount = 0;
    private int maxPlayers = 8;
    private int minPlayers = 2;
    [SerializeField] private GameObject playerPlatePrefab;
    [SerializeField] private GameObject addPlayerBtn;
    [SerializeField] private GameObject playerPanel;
    [SerializeField] private AvatarManager avatarManager;
    [SerializeField] private AccountManager accountManager;

    private List<PlayerScript> players = new();
    private List<GameObject> playersPlates = new();


    [SerializeField] private GameObject avatarMainMenu;
    [SerializeField] private GameObject avatarBackMainMenu;

    [SerializeField] private GameObject customizationMenu;
    [SerializeField] private CustomizationManager customizationManager;

    private bool leaderChoosingProcess = false;


    private void Awake()
    {
        
        customizationManager.onChangesAccepted += CloseCustomization;
        customizationManager.onChangesAccepted += UpdatePlayerData;
    }
    private void Start()
    {
        SetStartScreen();
    }
    void SetStartScreen()
    {
        if (accountManager.player != null)
        {
            avatarMainMenu.GetComponent<Image>().sprite = avatarManager.GetAvatarImage(accountManager.player.avatarID);
            avatarBackMainMenu.GetComponent<Image>().sprite = avatarManager.GetAvatarBackImage(accountManager.player.avatarBackID);
        }
    }
    public void OpenMainCustomization()
    {
        customizationMenu.SetActive(true);
        customizationManager.SetCustomizationPreview(accountManager.player);

    }
    public void OpenCustomizationFromPlate(PlayerScript player)
    {
        customizationMenu.SetActive(true);
        customizationManager.SetCustomizationPreview(player);
    }
    public void CloseCustomization(PlayerScript updatedPlayer)
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        customizationMenu.SetActive(false);
        if (playersPlates.Count > 0)
        {
            foreach(GameObject p in playersPlates)
            {
                if (p.GetComponent<PlateCustomization>().player == updatedPlayer)
                {
                    p.GetComponent<PlateCustomization>().CustomizePlate(avatarManager, updatedPlayer);
                }
            }
        }
        SetStartScreen();

    }
    private void UpdatePlayerData(PlayerScript playerToSave)
    {
        if (playerToSave == accountManager.player)
        {
            accountManager.SavePlayerData();
        }
    }


    #region Setup
    //public void IncreasePlayers()
    //{
    //    if (playerAmount < maxPlayers)
    //    {
    //        string newPlayerName = "Player " + players.Count;
    //        int newPlayerID = Random.Range(1, 1000);
    //        PlayerScript newPlayer;
    //        if (players.Count > 0)
    //        {
    //            newPlayer = new PlayerScript(newPlayerName, newPlayerID);
    //            avatarManager.SetRandomPlayerSettings(newPlayer);

    //        }
    //        else
    //        {
    //            newPlayer = accountManager.player;
                
    //        }
    //        players.Add(newPlayer);
            

    //        GameObject newPlate = Instantiate(playerPlatePrefab, playerPanel.transform);
    //        if (players.Count == 8)
    //        {
    //            addPlayerBtn.gameObject.SetActive(false);
    //            newPlate.transform.SetSiblingIndex(playerPanel.transform.childCount - 1);
    //        }
    //        else
    //        {
    //            newPlate.transform.SetSiblingIndex(playerPanel.transform.childCount - 2);
    //        }
            
    //        playersPlates.Add(newPlate);
    //        newPlate.GetComponent<PlateCustomization>().SetPlayer(newPlayer);
    //        newPlate.GetComponentInChildren<Button>().onClick.AddListener(() => PlayerPlateClick(newPlayer));
    //        newPlate.GetComponent<PlateCustomization>().CustomizePlate(avatarManager, newPlayer);
    //        AssignLeader(newPlayer);

    //        playerAmount++;
    //    }

    //}
    //public void DecreasePlayers()
    //{
    //    if (playerAmount > 0)
    //    {
    //        playerAmount--;
    //        players.RemoveAt(playerAmount);
    //        Destroy(playersPlates[playerAmount]);
    //        playersPlates.RemoveAt(playerAmount);


    //    }

    //}
    //public void NextLevel()
    //{
    //    // Очищення попереднього списку гравців
    //    IngameData.Instance.players.Clear();

    //    if (playerAmount >= minPlayers)
    //    {
    //        foreach (PlayerScript player in players)
    //        {
    //            IngameData.Instance.players.Add(player);
    //        }
    //        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Play);
    //        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    //    }
    //    else
    //    {
    //        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
    //        Debug.LogWarning("Перехід на наступну сцену заблоковано.");
    //    }
    //}

    // public void AssignLeader(PlayerScript newLeader = null)
    //{
    //    if(newLeader == null)
    //    {
    //        int randomLeaderIndex = UnityEngine.Random.Range(0, players.Count);
    //        newLeader = players[randomLeaderIndex];

    //    } 

    //    newLeader.role = PlayerRole.Leader;
        

    //    foreach (PlayerScript p in players)
    //    {
    //        if (p != newLeader)
    //        {
    //            p.role = PlayerRole.Player;
    //            playersPlates[players.IndexOf(p)].GetComponent<PlateCustomization>().leaderCrown.SetActive(false);
    //        }
    //        else
    //        {
    //            playersPlates[players.IndexOf(p)].GetComponent<PlateCustomization>().leaderCrown.SetActive(true);
    //        }
    //    }
    //}

    //void PlayerPlateClick(PlayerScript player)
    //{
       
    //    if (leaderChoosingProcess)
    //    {
    //        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
    //        AssignLeader(player);
    //        leaderChoosingProcess = false;
            
    //    }
    //    else
    //    {
    //        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_edit);
    //        OpenCustomizationFromPlate(player);
    //    }
    //}

    public void ChoseLeader()
    {
        if(players.Count >  0)
        {
            leaderChoosingProcess = true;
            Debug.Log("Choosing started");
            // change player plate view as active
        }

    }

    //public void ChoseRandomLeader()
    //{
    //    if (players.Count > 0)
    //    {
    //        AssignLeader();
    //    }
        
    //}

    #endregion


}
