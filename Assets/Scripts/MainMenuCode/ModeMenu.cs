using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeMenu : MonoBehaviour
{
    [SerializeField] private List<GameModes> gameModesKeys;
    [SerializeField] private List<GameObject> gameModesValues;
    private Dictionary<GameModes, GameObject> modePanels = new();
    private GameModes currentMode;

    private void Awake()
    {
        modePanels.Clear();
        for (int i = 0; i < gameModesKeys.Count; i++)
        {
            modePanels.Add(gameModesKeys[i], gameModesValues[i]);
        }
    }
    public void NextMode()
    {
        // Знайдемо наступний режим гри
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        currentMode = (GameModes)(((int)currentMode + 1) % modePanels.Count);
        ActivateGameModePanel(currentMode);
    }

    public void PreviousMode()
    {
        // Знайдемо попередній режим гри
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        currentMode = (GameModes)(((int)currentMode - 1 + modePanels.Count) % modePanels.Count);
        ActivateGameModePanel(currentMode);
    }

    private void ActivateGameModePanel(GameModes mode)
    {
        // Вимикаємо всі панелі
        foreach (var panel in modePanels.Values)
        {
            panel.SetActive(false);
        }

        // Активуємо лише обрану панель
        modePanels[mode].SetActive(true);

        // Оновлюємо текст режиму
        
    }

    public void SelectMode()
    {
        // Обираємо режим гри і передаємо його в скрипт з даними
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        IngameData.Instance.SetGameMode(currentMode);
        this.gameObject.SetActive(false);
    }

    public void OpenModeSelection()
    {
        // Відкриваємо меню і встановлюємо панель відповідно до поточного режиму
        currentMode = IngameData.Instance.GetGameMode();
        ActivateGameModePanel(currentMode);
    }

}


