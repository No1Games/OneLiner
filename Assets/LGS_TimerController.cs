using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class LGS_TimerController : MonoBehaviour
{
    [SerializeField] private Slider timerSlider;
    [SerializeField] private Button timerSwitcher;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject isOnImage;

    private bool isTimerOn;
    private int timerValue;

    private void Awake()
    {
        
        timerSlider.onValueChanged.AddListener(OnSliderValueChange);
        timerSwitcher.onClick.AddListener(TimerSwitcher);

        isTimerOn = true;
        timerValue = (int)timerSlider.value;
        UpdateTimerText();
    }

    private void OnSliderValueChange(float value)
    {
       
        timerValue = (int)value;
        UpdateTimerText();
    }

    private void TimerSwitcher()
    {
        // Перемикання стану таймера
        AudioManager.Instance.PlaySoundInMain(GameSounds.Menu_Click);
        isTimerOn = !isTimerOn;
        timerSlider.interactable = isTimerOn;
        isOnImage.SetActive(isTimerOn);
    }

    private void UpdateTimerText()
    {
        
        timerText.text = timerValue.ToString();
    }

    public void UpdateIngameData()
    {
        IngameData.Instance.InitializeTimer(isTimerOn, timerValue);
    }
}
