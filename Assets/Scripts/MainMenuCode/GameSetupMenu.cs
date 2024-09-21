using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameSetupMenu : MonoBehaviour
{
    private int playerAmount = 0;
    private int maxPlayers = 7;
    private int minPlayers = 0;
    [SerializeField] private TMP_Text playerNumber;

    [SerializeField] private List<GameObject> playersFields;

    public void IncreasePlayers()
    {
        if (playerAmount < maxPlayers && playersFields.Count  > 0)
        {
            playersFields[playerAmount].SetActive(true);
            playerAmount++;
        }
        
    }
    public void DecreasePlayers() 
    { 
        if(playerAmount > minPlayers && playersFields.Count > 0)
        {
            playerAmount--;
            playersFields[playerAmount].SetActive(false);
        }
    
    }

    public void NextLevel()
    {
        bool allFieldsFilled = true; // ���� ��� ��������, �� �� ���� ��������
        int i = 1;

        // �������� ������������ ������ �������
        IngameData.Instance.players.Clear();

        foreach (GameObject playerObject in playersFields)
        {
            if (playerObject.activeSelf)
            {
                TMP_InputField inputField = playerObject.GetComponentInChildren<TMP_InputField>();

                if (inputField != null && !string.IsNullOrEmpty(inputField.text))
                {
                    string playerName = inputField.text;
                    Debug.Log(playerName);
                    IngameData.Instance.players.Add(new PlayerScript(playerName, PlayerRole.NotSetYet, i));
                    i++;
                }
                else
                {
                    // ���� ���� � ���� ���� �� ���������, �������� ������������ � ������������ ����
                    Debug.LogWarning("��'� ������ �� ������� ��� " + playerObject.name);
                    allFieldsFilled = false;
                    break;
                }
            }
        }

        // ���� �� ���� ������, ����������� �������� �����
        if (allFieldsFilled && playerAmount > 1)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            Debug.LogWarning("�� �� ���� ������. ������� �� �������� ����� �����������.");
        }
    }
    public void Update()
    {
        playerNumber.text = playerAmount.ToString();
    }
}
