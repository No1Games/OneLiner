
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Customization/Item")]
public class Item : ScriptableObject
{
    public Sprite icon; // ������ ��� ���������� ��������
    public int itemCode; // ��������� ��� ��� ������� ��������
    public int requiredLevel; // ̳�������� ����� ��� ������� �� ��������
    public int cost; // ������� � ������ �����
    public bool isAvailableByDefault; // �� ��������� ������� ������ ��� �������������
}
