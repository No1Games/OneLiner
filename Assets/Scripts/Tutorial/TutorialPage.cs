using UnityEngine;
using UnityEngine.UI;

public class TutorialPage : MonoBehaviour
{
    [SerializeField] private Button tutorialButton;
    public Button TutorialButton => tutorialButton;

    [SerializeField] private int pageCode;
    public int PageCode => pageCode;
}
