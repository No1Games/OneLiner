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
        bool allFieldsFilled = true; // ‘лаг дл€ перев≥рки, чи вс≥ пол€ заповнен≥
        int i = 1;

        // ќчищенн€ попереднього списку гравц≥в
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
                    // якщо хоча б одне поле не заповнене, виводимо попередженн€ ≥ встановлюЇмо флаг
                    Debug.LogWarning("≤м'€ гравц€ не введене дл€ " + playerObject.name);
                    allFieldsFilled = false;
                    break;
                }
            }
        }

        // якщо вс≥ пол€ введен≥, завантажуЇмо наступну сцену
        if (allFieldsFilled && playerAmount > 1)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        else
        {
            Debug.LogWarning("Ќе вс≥ пол€ введен≥. ѕерех≥д на наступну сцену заблоковано.");
        }
    }
    public void Update()
    {
        playerNumber.text = playerAmount.ToString();
    }
}
