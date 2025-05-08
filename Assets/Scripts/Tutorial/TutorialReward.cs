using System;
using UnityEngine;
using UnityEngine.UI;

public class TutorialReward : MonoBehaviour
{
    [SerializeField] private Button acceptBtn;
    public event Action OnRewardEarned; 

    private void Awake()
    {
        acceptBtn.onClick.AddListener(GiveReward);

        
    }
    public void GiveReward()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        GemManager.Instance.AddGems(90);
        AccountData data = AccountManager.Instance.CurrentAccountData;
        data.tutorialStatus = TutorialStatus.Completed;
        IngameData.Instance.IsTutorialOn = false;
        AccountManager.Instance.SaveAccountData();
        OnRewardEarned?.Invoke();
        this.gameObject.SetActive(false);

    }
}
