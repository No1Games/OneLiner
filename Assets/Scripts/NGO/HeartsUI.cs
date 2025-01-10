using System.Collections.Generic;
using UnityEngine;

public class HeartsUI : MonoBehaviour
{
    [SerializeField] private HeartUI m_HeartPrefab;

    private int m_Count = 2;

    private List<HeartUI> m_Hearts;

    public void Init(int count)
    {
        m_Count = count;
        m_Hearts = new List<HeartUI>();

        for (int i = 0; i < m_Count; i++)
        {
            var heart = Instantiate(m_HeartPrefab, transform);
            heart.SetFull(true);
        }
    }

    public void UpdateHearts(int fullCount)
    {
        for (int i = 0; i < m_Hearts.Count; i++)
        {
            m_Hearts[i].SetFull(i <= fullCount);
        }
    }
}