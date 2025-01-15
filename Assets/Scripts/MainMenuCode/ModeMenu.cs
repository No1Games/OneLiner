using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

public class ModeMenu : MonoBehaviour
{
    [SerializeField] private List<GameObject> modePanels;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button confirmButton;

    private Animator modeSelectAnimator;

    private int currentIndex = 0;
    private int toGoIndex = 0;

    private bool modeSelectionIsActive;

    public event Action OnModeSelected;

    private void Awake()
    {
        Init();
    }
   

    private void Init()
    {
        modeSelectionIsActive = false;
        modeSelectAnimator = GetComponent<Animator>();
        leftButton.onClick.AddListener(MoveLeft);
        rightButton.onClick.AddListener(MoveRight);
        confirmButton.onClick.AddListener(ConfirmSelection);

        Debug.Log($"Mode Params: Current Mode = {modePanels[currentIndex].GetComponent<ModeInfo>().modeName}, Min Players = {modePanels[currentIndex].GetComponent<ModeInfo>().playersMin}");

    }
    public ModeInfo GetCurrentModeInfo()
    {
        return modePanels[currentIndex].GetComponent<ModeInfo>();
    }

    

    public void SetSelectedMode(GameModes mode)
    {
        // «найдемо ≥ндекс панел≥ з потр≥бним режимом
        int gameModeIndex = modePanels.FindIndex(mp => mp.GetComponent<ModeInfo>().modeName == mode);

        // якщо ≥ндекс не знайдено, виходимо
        if (gameModeIndex == -1)
        {
            Debug.LogError($"Mode {mode} not found in modePanels!");
            return;
        }

        // якщо панель уже активна, просто викликаЇмо п≥дтвердженн€
        if (gameModeIndex == currentIndex)
        {
            Debug.Log($"Mode {mode} is already selected.");
            OnModeSelected?.Invoke();
            return;
        }

        // ќновлюЇмо поточний ≥ндекс
        currentIndex = gameModeIndex;

        // јктивуЇмо потр≥бну панель ≥ вимикаЇмо ≥нш≥
        for (int i = 0; i < modePanels.Count; i++)
        {
            modePanels[i].SetActive(i == currentIndex);
        }

        // ЌадсилаЇмо под≥ю про обраний режим
        OnModeSelected?.Invoke();
    }


    

    public void MoveRight()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        StartCoroutine(SwitchPanelCoroutine(true));
    }

    public void MoveLeft()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        StartCoroutine(SwitchPanelCoroutine(false));
    }
    private IEnumerator SwitchPanelCoroutine(bool moveRight)
    {
        // Disable button interactivity
        leftButton.interactable = false;
        rightButton.interactable = false;
        confirmButton.interactable = false;

        toGoIndex = currentIndex;

        // Determine the new current index
        if (moveRight)
        {
            currentIndex = (currentIndex + 1) % modePanels.Count;
        }
        else
        {
            currentIndex = (currentIndex - 1 + modePanels.Count) % modePanels.Count;
        }

        // Play exit and enter animations
        Animator currentAnimator = modePanels[currentIndex].GetComponent<Animator>();
        Animator toGoAnimator = modePanels[toGoIndex].GetComponent<Animator>();

        if (toGoAnimator != null)
        {
            toGoAnimator.Play(moveRight ? "ExitRightAnimation" : "ExitLeftAnimation");
        }

        if (currentAnimator != null)
        {
            currentAnimator.Play(moveRight ? "EnterLeftAnimation" : "EnterRightAnimation");
        }

        // Wait for animations to complete // треба зам≥нити на вичисленн€ работи ан≥матор≥в
        yield return new WaitForSeconds(0.5f); // Adjust duration to match animation time

        
        // Re-enable button interactivity
        leftButton.interactable = true;
        rightButton.interactable = true;
        confirmButton.interactable = true;
    }

    public void ConfirmSelection()
    {
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);

        if (!modeSelectionIsActive)
        {
            // якщо меню неактивне, активуЇмо його та запускаЇмо ан≥мац≥ю вниз
            modeSelectionIsActive = true;
            leftButton.interactable = true;
            rightButton.interactable = true;
            modeSelectAnimator.Play("ModeScreenDownAnimation");
        }
        else
        {
            // якщо меню вже активне, закриваЇмо його та запускаЇмо ан≥мац≥ю вгору
            modeSelectionIsActive = false;
            leftButton.interactable = false;
            rightButton.interactable = false;
            modeSelectAnimator.Play("ModeScreenUpAnimation");

            // ЌадсилаЇмо под≥ю про обраний режим
            OnModeSelected?.Invoke();
            



        }
    }

    public ModeInfo GetModeInfo()
    {
         
        return modePanels[currentIndex].GetComponent<ModeInfo>();
    }

}

public enum GameModes
{
    Coop, ReverseCoop
}





