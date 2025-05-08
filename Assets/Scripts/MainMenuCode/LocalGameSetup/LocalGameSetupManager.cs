using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalGameSetupManager : MonoBehaviour
{

    private int maxPlayers;
    private int minPlayers;
    [SerializeField] private GameObject playerPlatePrefab;
    [SerializeField] private GameObject playerPanel;
    [SerializeField] LocalSetup localSetupUI;
    private bool leaderSelectInProcess = false;
    private bool playersInitialized = false;
    
    private List<PlayerScript> players = new();
    private List<GameObject> playersPlates = new();
    //[SerializeField] private AccountManager accountManager;
    [SerializeField] private CustomizationDataManager customizationDataManager;
    [SerializeField] private LGS_TimerController timerController;
    [SerializeField] private ModeMenu modeMenu;
    [SerializeField] private PlayerGamesTracker gamesTracker;

    private LocalGameSetup_Func lgsFunc;


    
    private PlayerRole roleKnowsTheWord;

    private event Action OnLevelStarts; 

    

    private void Awake()
    {
        lgsFunc = gameObject.AddComponent<LocalGameSetup_Func>(); 
        lgsFunc.Initialize(playerPlatePrefab, playerPanel);
        localSetupUI.OnAddPlayerBtnClick += (() => AddPlayer());
        localSetupUI.OnStartGameBtnClick += StartLocalGame;
        localSetupUI.OnRandomLeaderBtnClick += ChoseRandomLeader;
        localSetupUI.OnChooseLeaderBtnClick += SelectLeader;
        localSetupUI.OnScreenShow += SetLocalSetup;
        localSetupUI.OnBackBtnClick += ClearLocalSetup;


        customizationDataManager.OnCustomizationEnds += PlateVisualUpdate;
        OnLevelStarts += timerController.UpdateIngameData;

        


    }
    private void Start()
    {
        modeMenu.OnModeSelected += InitiateModeParams;
        AdsManager.Instance.OnInterstitialAddsShown += LoadLocalGame;
        InitiateModeParams();
    }

    private void SetLocalSetup()
    {
        if (!IngameData.Instance.IsTutorialOn)
        {

        
        if (!playersInitialized)
        {
            playersInitialized = true;
            IngameData ingameData = IngameData.Instance;
            InitializeMode();
            timerController.InitializeTimer(ingameData.IsTimerOn, ingameData.TimerDuration);

            if (ingameData.ReturnedFromGame && ingameData.Players != null && ingameData.Players.Count > 0)
            {
                InitializeSavedPlayers();
            }
            else
            {
                InitializeDefaultPlayers();
            }

        }
        }
        else
        {
            
            AddPlayer(lgsFunc.AddCroco());

        }

    }

    public void AddPlayerForTutorial()
    {
        AddPlayer(AccountManager.Instance.player);
    }
    public void FinishTutorial()
    {
        StartLocalGame();
    }

    private void ClearLocalSetup()
    {
        players.Clear();

        foreach (GameObject plate in playersPlates)
        {
            if (plate != null)
            {
                Destroy(plate);
            }
        }
        localSetupUI.AddPlayerBtn_VisibilityChange(true);
        playersInitialized = false;

        
       
        playersPlates.Clear();
        IngameData.Instance.SetReturnedFromGame(false);
    }

    private void InitializeMode()
    {
        if (IngameData.Instance.GameMode != null)
        {
            modeMenu.SetSelectedMode(IngameData.Instance.GameMode);
        }
        else
        {
            Debug.LogWarning("GameMode is null. Using default mode.");
        }
    }

    

    private void InitializeSavedPlayers()
    {
        foreach (PlayerScript p in IngameData.Instance.Players)
        {
            AddPlayer(p);
        }
    }

    private void InitializeDefaultPlayers()
    {
        for (int i = 0; i < minPlayers; i++)
        {
            AddPlayer();
        }
    }


    private void InitiateModeParams()
    {
        ModeInfo currentModeInfo = modeMenu.GetModeInfo();
        if (currentModeInfo != null)
        {
            maxPlayers = currentModeInfo.playersMax;
            minPlayers = currentModeInfo.playersMin;
            roleKnowsTheWord = currentModeInfo.role1;
            Debug.Log($"Mode Params: Max Players = {maxPlayers}, Min Players = {minPlayers}");
        }
        else
        {
            Debug.LogWarning("ModeInfo is null!");
        }
    }


    private void AddPlayer(PlayerScript player = null)
    {
        
        if (players.Count < maxPlayers)
        {
            if (player == null)
            {
                if (players.Count == 0)
                {
                    player = AccountManager.Instance.player;
                }
                else
                {
                    player = lgsFunc.AddRandomPlayer(players);
                }
            }
            
            
           players.Add(player);
            if(players.Count == maxPlayers)
            {
                localSetupUI.AddPlayerBtn_VisibilityChange(false);
            }
           GameObject newPlate = lgsFunc.GeneratePlayerPlate(player);
           
           newPlate.transform.SetSiblingIndex(playerPanel.transform.childCount - 2);
           

            playersPlates.Add(newPlate);
            if(playersPlates.Count == 1)
            {
                lgsFunc.AssignLeader(playersPlates, player);
            }
            newPlate.GetComponent<PlateCustomization>().MainButton.onClick.AddListener(() => PlateBtnController(newPlate));
            newPlate.GetComponent<PlateCustomization>().FirstMiniButton.onClick.AddListener(() => RemovePlayer(player));

            newPlate.GetComponent<PlateCustomization>().SecondMiniButton.onClick.AddListener(() => EditPlayer(player));
            


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
    private void EditPlayer(PlayerScript player)
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_edit);
        customizationDataManager.SetupData(player, MenuName.LocalSetup);
        MainMenuManager.Instance.ChangeMenu(MenuName.CustomizationScreen);
    }

    private void StartLocalGame()
    {
        if (players.Count >= minPlayers)
        {
            AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Play);
            if (gamesTracker.CanRunWithoutAds())
            {
                LoadLocalGame();
            }

            else
            {
                AdsManager.Instance.ShowInterstitialAds();
            }
            

        }
            else
        {
            AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
            Debug.LogWarning("Перехід на наступну сцену заблоковано.");
        }
    }
    private void LoadLocalGame()
    {
        
        OnLevelStarts?.Invoke();
        IngameData.Instance.SetRoleKnowsWord(roleKnowsTheWord);
        IngameData.Instance.SetGameMode(modeMenu.GetModeInfo().modeName);
        lgsFunc.NextLevel(players);
    }

    public void ChoseRandomLeader()
    {
        if (players.Count > 1)
        {
            lgsFunc.AssignLeader(playersPlates);
        }

    }

    private void SelectLeader()
    {
        leaderSelectInProcess = true;
    }
    
    public void PlateBtnController(GameObject plate)
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        PlateCustomization plateInfo = plate.GetComponent<PlateCustomization>();

        if (leaderSelectInProcess)
        {
            lgsFunc.AssignLeader(playersPlates, plateInfo.player);
            leaderSelectInProcess = false;

        }
        else
        {
            
            if (plateInfo.additionalMenu.activeSelf == false)
            {
                plateInfo.additionalMenu.gameObject.SetActive(true);

            }
            else
            {
                plateInfo.additionalMenu.gameObject.SetActive(false);
            }
        }
        

    }

    private void PlateVisualUpdate()
    {
       if(playersPlates.Count  > 0)
        {
            foreach (GameObject plate in playersPlates)
            {

                PlateCustomization plateInfo = plate.GetComponent<PlateCustomization>();
                plateInfo.CustomizePlate(plateInfo.player);
                plateInfo.additionalMenu.gameObject.SetActive(false);

            }
        }
        
    }
}
