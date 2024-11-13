using System.Collections.Generic;
using UnityEngine;

public class HeartsUI : MonoBehaviour
{
    [SerializeField] HeartUI _heartPrefab;

    private int _count;

    private List<HeartUI> _hearts;

    public void Init(int count)
    {
        _count = count;
        _hearts = new List<HeartUI>();

        for (int i = 0; i < _count; i++)
        {
            HeartUI heartUI = Instantiate(_heartPrefab, transform);
            heartUI.Show();
            _hearts.Add(heartUI);
        }
    }

    public void UpdateHearts(int count)
    {
        if (_hearts.Count < count)
        {
            InstantiateHearts(count - _hearts.Count);
        }

        foreach (var heart in _hearts)
        {
            heart.Hide();
        }

        _count = count;

        for (int i = 0; i < _count; i++)
        {
            _hearts[i].Show();
        }
    }

    private void InstantiateHearts(int count)
    {
        for (int i = 0; i < _count; i++)
        {
            HeartUI heartUI = Instantiate(_heartPrefab, transform);
            heartUI.Hide();
            _hearts.Add(heartUI);
        }
    }
}