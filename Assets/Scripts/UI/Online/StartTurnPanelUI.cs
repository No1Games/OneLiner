using UnityEngine;

public class StartTurnPanelUI : MonoBehaviour
{
    //[SerializeField] private TextMeshProUGUI _turnText;
    //[SerializeField] private Button _okBtn;

    //private readonly string _yourTurnStr = "��� ���!";
    //private readonly string _otherTurnStr = "����� ������ ";

    public void OnTurnChanged(LocalPlayer currentPlayer)
    {
        if (currentPlayer.ID.Value == GameManager.Instance.LocalUser.ID.Value)
        {
            gameObject.SetActive(true);
            // _turnText.text = _yourTurnStr;
        }
        else
        {
            gameObject.SetActive(false);
            //_turnText.text = $"{_otherTurnStr} {currentPlayer.DisplayName.Value}";
        }

        //gameObject.SetActive(true);
    }
}
