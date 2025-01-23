using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button m_RestartBtn;
    [SerializeField] private Button m_ExitBtn;

    [Header("Contents")]
    [SerializeField] private GameObject m_OverallContent;
    [SerializeField] private GameObject m_LoseContent;
    [SerializeField] private GameObject m_WinContent;
    [SerializeField] private List<GameObject> m_Stars;

    [SerializeField] private LocalizeStringEvent m_WordLocalStrEvent;

    private void Awake()
    {
        m_RestartBtn.onClick.AddListener(OnClick_RestartButton);
        m_ExitBtn.onClick.AddListener(OnClick_ExitButton);
    }

    private void OnClick_RestartButton()
    {
        Debug.LogWarning("NOT IMPLEMENTED");
    }

    private void OnClick_ExitButton()
    {
        Debug.LogWarning("NOT IMPLEMENTED");
    }

    public void Show(string word, bool isWin, float score = 0)
    {
        AudioManager.Instance.PauseSoundInBack();

        m_OverallContent.SetActive(true);

        m_WordLocalStrEvent.StringReference.TableEntryReference = word;

        if (!isWin)
        {
            AudioManager.Instance.PlaySoundInAdditional(GameSounds.Game_Lose);
            m_LoseContent.SetActive(true);
            m_WinContent.SetActive(false);
        }
        else
        {
            AudioManager.Instance.PlaySoundInAdditional(GameSounds.Game_Victory);
            m_WinContent.SetActive(true);
            m_LoseContent.SetActive(false);

            if (score <= 600)
            {
                m_Stars[0].SetActive(true);
            }
            else if (score > 600 && score <= 800)
            {
                m_Stars[1].SetActive(true);
            }
            else if (score > 800 && score < 1000)
            {
                m_Stars[2].SetActive(true);
            }
            else if (score >= 1000)
            {
                m_Stars[3].SetActive(true);
            }
        }
    }
}
