using TMPro;
using UnityEngine;

public class LoadingPanel : MonoBehaviour
{
    private static LoadingPanel _instance;
    public static LoadingPanel Instance => _instance;

    [SerializeField] private TextMeshProUGUI _loadingText;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void Show(string text)
    {
        _loadingText.text = text;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

}