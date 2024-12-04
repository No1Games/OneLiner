using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LocalOnlineMenu : MenuBase
{
    public override MenuName Menu => MenuName.LocalOnline;

    [SerializeField] private Button _localButton;
    [SerializeField] private Button _onlineButton;
    [SerializeField] private Button _backButton;

    private void Start()
    {
        _localButton.onClick.AddListener(OnClick_LocalButton);
        _onlineButton.onClick.AddListener(OnClick_OnlineButton);
        _backButton.onClick.AddListener(OnClick_BackButton);
    }

    private void OnClick_LocalButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        MainMenuManager.Instance.ChangeMenu(MenuName.LocalSetup);

    }

    private void OnClick_OnlineButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);

        Hide();

        SceneManager.LoadScene("OnlineMenu", LoadSceneMode.Additive);

        //SceneManager.SetActiveScene(SceneManager.GetSceneByName("OnlineMenu"));

        // MainMenuManager.Instance.ChangeMenu(MenuName.LobbyList);
    }

    private void OnClick_BackButton()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        MainMenuManager.Instance.ChangeMenu(MenuName.MainScreen);

    }
}
