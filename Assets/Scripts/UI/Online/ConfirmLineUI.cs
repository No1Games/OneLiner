using UnityEngine;
using UnityEngine.UI;

public class ConfirmLineUI : MonoBehaviour
{
    [SerializeField] private NGODrawManager m_DrawingManager;
    [SerializeField] private DrawingUpdate m_DrawingUpdate;

    [SerializeField] private GameObject m_PanelGO;

    [SerializeField] private Button m_ConfirmBtn;
    [SerializeField] private Button m_RemoveBtn;

    private void Awake()
    {
        m_DrawingManager.OnLineDrawn += ConfirmTurnActivate;

        m_RemoveBtn.onClick.AddListener(OnClick_RemoveButton);
        m_ConfirmBtn.onClick.AddListener(OnClick_ConfirmButton);
    }

    private void ConfirmTurnActivate(Line line)
    {
        m_DrawingManager.IsDrawAllowed = false;
        m_PanelGO.SetActive(true);
    }

    private void OnClick_RemoveButton()
    {
        m_PanelGO.SetActive(false);
        m_DrawingManager.RemoveLastLine();
        m_DrawingManager.IsDrawAllowed = true;
    }

    private void OnClick_ConfirmButton()
    {
        Debug.Log("User confirmed his line");

        m_PanelGO.SetActive(false);

        m_DrawingManager.LineConfirmed();
    }
}
