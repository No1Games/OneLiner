using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarManager : MonoBehaviour
{
    // ��������� ��������� ��� ��������� ��������
    public static AvatarManager Instance { get; private set; }

    [SerializeField] private ImagesPack avatars;
    [SerializeField] private ImagesPack avatarsBack;
    [SerializeField] private ImagesPack nameBack;

    private void Awake()
    {
        // ��������, �� ��������� ��� ����
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �������� ��� ��'��� ��� ������� �� ���� �����
        }
        else
        {
            Destroy(gameObject); // ���� ��������� ��� ����, ������� ��������
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
}
