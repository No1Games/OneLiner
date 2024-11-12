
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Customization/Item")]
public class Item : ScriptableObject
{
    public Sprite icon; // Іконка або зображення елемента
    public int itemCode; // Унікальний код для кожного елемента
    public int requiredLevel; // Мінімальний рівень для доступу до елемента
    public int cost; // Вартість в ігровій валюті
    public bool isAvailableByDefault; // Чи доступний елемент відразу без розблокування
}
