using UnityEngine;

public class StartTurnPanelUI : MonoBehaviour
{
    //[SerializeField] private TextMeshProUGUI _turnText;
    //[SerializeField] private Button _okBtn;

    //private readonly string _yourTurnStr = "Ваш хід!";
    //private readonly string _otherTurnStr = "Зараз ходить ";

    public void OnTurnChanged(string currentPlayerID)
    {
        if (currentPlayerID == GameManager.Instance.LocalUser.ID.Value)
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
