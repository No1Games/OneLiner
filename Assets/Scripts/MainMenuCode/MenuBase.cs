using UnityEngine;

public class MenuBase : MonoBehaviour
{
    public virtual MenuName Menu { get; }
    public virtual void Init() { }
    public virtual void Show() { gameObject.SetActive(true); }
    public virtual void Hide() { gameObject.SetActive(false); }
}