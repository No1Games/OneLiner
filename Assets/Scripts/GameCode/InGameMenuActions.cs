using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameMenuActions : MonoBehaviour
{
    public void BackToMenu()
    {
        IngameData.Instance.SetReturnedFromGame(true);
        SceneManager.LoadScene(0);
    }
}
