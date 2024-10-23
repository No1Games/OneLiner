using System.Collections.Generic;
using UnityEngine;

public class IngameData : MonoBehaviour
{

    public int playersAmount;

    public static IngameData Instance;

    public List<PlayerScript> players = new List<PlayerScript>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // «бер≥гаЇмо об'Їкт м≥ж сценами
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
