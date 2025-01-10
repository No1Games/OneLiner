using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WordButtonOnline : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_WordTMP;
    [SerializeField] private Button m_Button;

    [SerializeField] private GameObject m_Leader;
    [SerializeField] private GameObject m_Wrong;

    private int m_Index;

    public event Action<int> WordButtonClick;

    public bool Interactable
    {
        get { return m_Button.interactable; }
        set { m_Button.interactable = value; }
    }

    private void Awake()
    {
        m_Button.onClick.AddListener(OnClick);
    }

    public void SetWord(string word, int index)
    {
        m_WordTMP.text = word;
        m_Index = index;
    }

    public void SetLeaderWord()
    {
        m_Leader.gameObject.SetActive(true);
    }

    private void OnClick()
    {
        if (OnlineController.Instance.LocalPlayer.Role.Value == PlayerRole.Leader)
            return;

        if (!OnlineController.Instance.LocalPlayer.IsTurn.Value)
            return;

        DisableButton();

        WordButtonClick?.Invoke(m_Index);
    }

    public void DisableButton()
    {
        m_Wrong.SetActive(true);

        m_Button.enabled = false;
    }
}