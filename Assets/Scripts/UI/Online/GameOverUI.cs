using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _resultTMP;
    [SerializeField] private TextMeshProUGUI _scoreTMP;

    [SerializeField] private Button _restartBtn;
    [SerializeField] private Button _exitBtn;

    [SerializeField] private string WIN_TEXT = "Вітаю, ви перемогли!";
    [SerializeField] private string LOOSE_TEXT = "Ви програли :(";

    private void Awake()
    {
        _restartBtn.onClick.AddListener(OnClick_RestartButton);
        _exitBtn.onClick.AddListener(OnClick_ExitButton);
    }

    private void OnClick_RestartButton()
    {
        GameManager.Instance.ExitToLobby();
    }

    private void OnClick_ExitButton()
    {
        GameManager.Instance.ExitToLobbyList();
    }

    public void Show(bool isWin, float score = 0)
    {
        _resultTMP.text = isWin ? WIN_TEXT : LOOSE_TEXT;

        if (isWin)
        {
            _scoreTMP.gameObject.SetActive(true);
            _scoreTMP.text = $"Ваш рахунок: {score}";
        }
        else
        {
            _scoreTMP.gameObject.SetActive(false);
        }

        gameObject.SetActive(true);
    }
}
