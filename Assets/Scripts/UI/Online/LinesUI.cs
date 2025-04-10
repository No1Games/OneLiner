using TMPro;
using UnityEngine;

public class LinesUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_LinesText;

    public void SetLines(int count)
    {
        m_LinesText.text = count.ToString();
    }
}
