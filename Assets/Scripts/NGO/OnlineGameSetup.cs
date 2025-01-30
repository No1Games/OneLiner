using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnlineGameSetup : MonoBehaviour
{
    #region Hearts Fields

    [Header("Hearts Fields")]
    [SerializeField] private HeartsUI m_HeartsUI;
    [SerializeField] private int m_MaxHearts = 2;
    private int m_CurrentHearts;

    #endregion

    #region Turn Handle Fields

    [Header("Turn Handle Fields")]
    [SerializeField] private StartTurnPanelUI m_StartTurnPanel;
    [SerializeField] private TurnHandler m_TurnHandler;

    #endregion

    #region Words Fields

    [Header("Words Fields")]
    [SerializeField] private WordsPanel m_WordsPanel;
    [SerializeField] private WordManager m_WordManager;
    private List<string> m_WordsList = new List<string>();
    private List<int> m_WordsIndexes = new List<int>();
    private string m_LeaderWord;
    private int m_LeaderWordIndex;

    #endregion

    #region Drawing Fields

    [Header("Drawing Fields")]
    [SerializeField] private NGODrawManager m_DrawingManager;
    [SerializeField] private DrawingUpdate m_DrawingUpdate;
    [SerializeField] private Image m_DrawnImage;

    #endregion

    [Space]
    [SerializeField] private Camera m_MainCamera;

    [Space]
    [SerializeField] private GameOverUI m_GameOverUI;

    private RpcHandler m_RpcHandler;
    private OnlineController m_OnlineController;

    private LocalLobby m_LocalLobby;
    private LocalPlayer m_LocalPlayer;

    private void Awake()
    {
        m_OnlineController = OnlineController.Instance;

        m_LocalLobby = m_OnlineController.LocalLobby;
        m_LocalPlayer = m_OnlineController.LocalPlayer;
    }

    private void Start()
    {
        m_CurrentHearts = m_MaxHearts;
        m_HeartsUI.Init(m_CurrentHearts);

        m_RpcHandler = FindAnyObjectByType<RpcHandler>();

        SubscribeOnRpcEvents();

        m_LocalLobby.WordsList.onChanged += OnWordsChanged;
        m_LocalLobby.LeaderWord.onChanged += OnLeaderWordChanged;

        // Host generates words
        if (m_LocalPlayer.IsHost.Value)
        {
            m_WordsList = m_WordManager.FormWordListForRound();
            m_WordsIndexes = m_WordManager.GetWordsIndexes(m_WordsList);
            m_LeaderWordIndex = m_WordManager.GetLeaderWordIndex(m_WordsList);
            m_LeaderWord = m_WordsList[m_LeaderWordIndex];
            m_OnlineController.SetLocalLobbyWords(m_WordsIndexes, m_LeaderWordIndex);
        }

        m_DrawingManager.OnLineConfirmed += OnLineConfirmed;
        m_DrawingManager.OnLineSpawned += OnLineSpawned;
        m_DrawingUpdate.OnScreenshotTaken += OnScreenshotTaken;

        m_WordsPanel.UserClickedWordEvent += OnUserMakeGuess;
    }

    private void SubscribeOnRpcEvents()
    {
        if (m_RpcHandler == null)
        {
            Debug.LogWarning("Can't subscribe on RpcEvents: Rpc Handler is null.");
            return;
        }

        m_RpcHandler.GameOverEvent += ShowGameOverScreen;
        m_RpcHandler.UpdateHeartsEvent += SetHearts;
        m_RpcHandler.DisableWordButtonEvent += DisableButtonByIndex;
    }

    private void UnsubscribeFromRpcEvents()
    {
        if (m_RpcHandler == null)
        {
            Debug.LogWarning("Can't unsubscribe from RpcEvents: Rpc Handler is null.");
            return;
        }

        m_RpcHandler.GameOverEvent -= ShowGameOverScreen;
        m_RpcHandler.UpdateHeartsEvent -= SetHearts;
        m_RpcHandler.DisableWordButtonEvent -= DisableButtonByIndex;
    }

    private void OnDestroy()
    {
        m_LocalLobby.WordsList.onChanged -= OnWordsChanged;
        m_LocalLobby.LeaderWord.onChanged -= OnLeaderWordChanged;

        m_DrawingManager.OnLineConfirmed -= OnLineConfirmed;
        m_DrawingManager.OnLineSpawned -= OnLineSpawned;
        m_DrawingUpdate.OnScreenshotTaken -= OnScreenshotTaken;

        UnsubscribeFromRpcEvents();
    }

    #region Words Methods

    private void OnWordsChanged(List<int> indexes)
    {
        m_WordsList = m_WordManager.GetWordsFromIndexes(indexes);

        m_WordsPanel.SetButtons(m_WordsList);
    }

    private void OnLeaderWordChanged(int index)
    {
        m_LeaderWordIndex = index;

        m_WordsPanel.SetLeaderWord(index);
    }

    public void DisableButtonByIndex(int index)
    {
        m_WordsPanel.DisableButton(index);
    }

    #endregion

    #region Drawing Methods

    public void ToggleScreen(bool isDrawing)
    {
        m_DrawingUpdate.gameObject.SetActive(isDrawing);
        m_MainCamera.gameObject.SetActive(!isDrawing);
        m_DrawingManager.IsDrawAllowed = isDrawing;
    }

    private void UpdatePicture(Texture2D texture)
    {
        Sprite screenshotSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        m_DrawnImage.sprite = screenshotSprite;
    }

    private void OnScreenshotTaken(Texture2D texture)
    {
        UpdatePicture(texture);

        ToggleScreen(false);
    }

    private void OnLineSpawned()
    {
        ToggleScreen(true);
        m_DrawingUpdate.TakeScreenshot();
        ToggleScreen(false);
    }

    private void OnLineConfirmed(Line line)
    {
        m_RpcHandler.SpawnLine(line.Start, line.End);
        m_TurnHandler.EndTurn();
    }

    #endregion

    private void OnUserMakeGuess(int index)
    {
        if (m_LeaderWordIndex != index)
        {
            int newHearts = m_CurrentHearts - 1;

            if (newHearts <= 0)
            {
                m_RpcHandler.OnGameOver(false);
            }
            else
            {
                m_RpcHandler.OnGuessedWrong(index, newHearts);
            }

            m_TurnHandler.EndTurn();
        }
        else
        {
            m_RpcHandler.OnGameOver(true, 100f);
        }
    }

    public void SetHearts(int count)
    {
        m_CurrentHearts = count;

        m_HeartsUI.UpdateHearts(count);
    }

    public void ShowGameOverScreen(bool isWin, float score = 0)
    {
        m_GameOverUI.Show(m_LeaderWord, isWin, score);
    }

}
