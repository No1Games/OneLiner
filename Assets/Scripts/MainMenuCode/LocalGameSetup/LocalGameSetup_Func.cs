using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LocalGameSetup_Func : MonoBehaviour
{
    private GameObject platePrefab;
    private GameObject playerPanel;
    private AccountManager accountManager;


    public void Initialize(GameObject platePrefab, GameObject playerPanel, AccountManager accountManager)
    {
        this.platePrefab = platePrefab;
        this.playerPanel = playerPanel;
        this.accountManager = accountManager;

    }

    public PlayerScript AddRandomPlayer(List<PlayerScript> players)
    {

        string newPlayerName = "Player " + players.Count;
        int newPlayerID = Random.Range(1, 1000);
        PlayerScript newPlayer = new PlayerScript(newPlayerName, newPlayerID);
        SetRandomPlayerCustomization(newPlayer);
        return newPlayer;
    }

    private void SetRandomPlayerCustomization(PlayerScript player)
    {
        // Отримання доступних кодів айтемів для кожної категорії
        List<int> availableAvatars = ItemManager.Instance.GetItemCodesByCategory(ItemCategory.Avatars);
        List<int> availableAvatarBackgrounds = ItemManager.Instance.GetItemCodesByCategory(ItemCategory.AvatarBackgrounds);
        List<int> availableNameBackgrounds = ItemManager.Instance.GetItemCodesByCategory(ItemCategory.NameBackgrounds);

        // Отримання косметичних кодів акаунта
        List<int> accountCosmeticCodes = accountManager.GetAccountData().cosmeticCodes;

        // Використання LINQ для фільтрації тільки тих айтемів, які є в списку акаунта
        List<int> filteredAvatars = availableAvatars.Where(code => accountCosmeticCodes.Contains(code)).ToList();
        List<int> filteredAvatarBackgrounds = availableAvatarBackgrounds.Where(code => accountCosmeticCodes.Contains(code)).ToList();
        List<int> filteredNameBackgrounds = availableNameBackgrounds.Where(code => accountCosmeticCodes.Contains(code)).ToList();

        // Перевірка, чи є доступні айтеми для вибору
        if (filteredAvatars.Count == 0 || filteredAvatarBackgrounds.Count == 0 || filteredNameBackgrounds.Count == 0)
        {
            Debug.LogError("Not enough items in account's cosmetic codes for random customization.");
            return;
        }

        // Вибір випадкових айтемів
        System.Random random = new System.Random();
        int randomAvatar = filteredAvatars[random.Next(filteredAvatars.Count)];
        int randomAvatarBackground = filteredAvatarBackgrounds[random.Next(filteredAvatarBackgrounds.Count)];
        int randomNameBackground = filteredNameBackgrounds[random.Next(filteredNameBackgrounds.Count)];

        // Присвоєння вибраних кодів гравцю
        player.avatarID = randomAvatar;
        player.avatarBackID = randomAvatarBackground;
        player.nameBackID = randomNameBackground;
    }


    public GameObject GeneratePlayerPlate(PlayerScript player)
    {
        GameObject newPlate = Instantiate(platePrefab, playerPanel.transform);
        newPlate.GetComponent<PlateCustomization>().CustomizePlate(player);
        newPlate.GetComponent<PlateCustomization>().SetPlayer(player);
        return newPlate;
    }

    public void NextLevel(List<PlayerScript> players)
    {
        // Очищення попереднього списку гравців
        IngameData.Instance.players.Clear();
        foreach (PlayerScript player in players)
        {
            if(player.role == PlayerRole.NotSetYet)
            {
                player.role = PlayerRole.Player;
            }
            IngameData.Instance.players.Add(player);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void AssignLeader(List<GameObject> playersPlates,  PlayerScript newLeader = null)
    {

        if (newLeader == null)
        {
            do
            {
                int randomLeaderIndex = UnityEngine.Random.Range(0, playersPlates.Count);
                newLeader = playersPlates[randomLeaderIndex].GetComponent<PlateCustomization>().player;

            }
            while (newLeader.role == PlayerRole.Leader); 
        }

        newLeader.role = PlayerRole.Leader;

        foreach(GameObject plate in playersPlates)
        {
            PlateCustomization plateInfo = plate.GetComponent<PlateCustomization>();
            if (plateInfo.player == newLeader)
            {
                plateInfo.leaderCrown.SetActive(true);
            }
            else
            {
                plateInfo.player.role = PlayerRole.Player;
                plateInfo.leaderCrown.SetActive(false);

            }
        }

    }


}




