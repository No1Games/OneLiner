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
        // �������� ��������� ����� ���
        currentMode = (GameModes)(((int)currentMode + 1) % modePanels.Count);
        ActivateGameModePanel(currentMode);
    }

    public void PreviousMode()
    {
        // �������� ��������� ����� ���
        currentMode = (GameModes)(((int)currentMode - 1 + modePanels.Count) % modePanels.Count);
        ActivateGameModePanel(currentMode);
    }

    private void ActivateGameModePanel(GameModes mode)
    {
        // �������� �� �����
        foreach (var panel in modePanels.Values)
        {
            panel.SetActive(false);
        }

        // �������� ���� ������ ������
        modePanels[mode].SetActive(true);

        // ��������� ����� ������
        
    }

    public void SelectMode()
    {
        // ������� ����� ��� � �������� ���� � ������ � ������
        IngameData.Instance.SetGameMode(currentMode);
        this.gameObject.SetActive(false);
    }

    public void OpenModeSelection()
    {
        // ³�������� ���� � ������������ ������ �������� �� ��������� ������
        currentMode = IngameData.Instance.GetGameMode();
        ActivateGameModePanel(currentMode);
    }

}


