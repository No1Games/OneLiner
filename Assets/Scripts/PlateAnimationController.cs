using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateAnimationController : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private Animator animator;

    public void OnTurnStart()
    {
        animator.Play("StartTurnAnimation");
    }
    public void OnTurnEnd()
    {
        animator.Play("EndTurnAnimation");
    }
}

