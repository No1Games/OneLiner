using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;


public class GooglePlayAuthManager : MonoBehaviour
{
    [SerializeField] private AccountManager accountManager;
    void Awake()
    {
        PlayGamesPlatform.Activate();
        SignIn();
    }
    

    public void SignIn()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }
    internal void ProcessAuthentication(SignInStatus status)
    {
        //if (status == SignInStatus.Success)
        //{

        //    accountManager.PrepareAccountData(true);
        //}
        //else
        //{
        //    accountManager.PrepareAccountData(false);
        //}
    }
}

