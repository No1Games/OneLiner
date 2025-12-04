using UnityEngine;

public class HeartUI : MonoBehaviour
{
    [SerializeField] private Animator m_Animator;
    private int m_IsFullHash;

    public bool IsFull
    {
        get
        {
            return m_Animator.GetBool(m_IsFullHash);
        }
    }

    private void Awake()
    {
        m_IsFullHash = Animator.StringToHash("IsFull");
    }

    public void SetFull(bool isFull)
    {
        m_Animator.SetBool(m_IsFullHash, isFull);
    }
}
