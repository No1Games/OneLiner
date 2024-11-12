using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarManager : MonoBehaviour
{
    // Статичний екземпляр для реалізації сінглтону
    public static AvatarManager Instance { get; private set; }

    [SerializeField] private ImagesPack avatars;
    [SerializeField] private ImagesPack avatarsBack;
    [SerializeField] private ImagesPack nameBack;

    [SerializeField] private List<Item> avatarItems;
    [SerializeField] private List<Item> avatarBackItems;
    [SerializeField] private List<Item> nameBackItems;


    private void Awake()
    {
        // Перевірка, чи екземпляр уже існує
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Залишати цей об'єкт при переході на інші сцени
        }
        else
        {
            Destroy(gameObject); // Якщо екземпляр уже існує, знищити дублікати
        }
    }

    public Sprite GetAvatarImage(int index)
    {
        
        return avatars.someImage[index];
    }

    public Sprite GetNameBackImage(int index)
    {
        
        return nameBack.someImage[index];
    }

    public Sprite GetAvatarBackImage(int index)
    {
       
        return avatarsBack.someImage[index];
    }

    public void SetRandomPlayerSettings(PlayerScript player)
    {
        player.avatarBackID = Random.Range(0, avatarsBack.someImage.Count);
        player.nameBackID = Random.Range(0, nameBack.someImage.Count);
        player.avatarID = Random.Range(0, avatars.someImage.Count);
    }

    
    #region ItemSystem
    public void SetRandomPlayerItems(PlayerScript player)
    {
        int avatarBackID = Random.Range(0, avatarBackItems.Count);
        int nameBackID = Random.Range(0, nameBackItems.Count);
        int avatarID = Random.Range(0, avatarItems.Count);

        player.avatarBackID = avatarBackItems[avatarBackID].itemCode;
        player.nameBackID = nameBackItems[nameBackID].itemCode;
        player.avatarID = avatarItems[avatarID].itemCode;
    }

    private Sprite SetAvatarImage(int index)
    {

        if (index > 100)
        {
            foreach (var item in nameBackItems)
            {
                if (item.itemCode == index)
                {
                    return item.icon;
                }
            }

        }
        return nameBack.someImage[index];

    }
    public Item GetAvatarById(int itemCode)
    {
        return avatarItems.Find(item => item.itemCode == itemCode);
    }

    public Item GetAvatarBackgroundById(int itemCode)
    {
        return avatarBackItems.Find(item => item.itemCode == itemCode);
    }

    public Item GetNameBackgroundById(int itemCode)
    {
        return nameBackItems.Find(item => item.itemCode == itemCode);
    }
    #endregion
}
