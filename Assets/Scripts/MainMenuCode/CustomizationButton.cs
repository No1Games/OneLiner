using UnityEngine.UI;
using UnityEngine;

public class CustomizationButton : MonoBehaviour
{
   public CustomizationPart part;
    public int index;
    [SerializeField] GameObject inFieldImage;

    public void SetInfield(Sprite element)
    {
        inFieldImage.GetComponent<Image>().sprite = element;
    }
}
