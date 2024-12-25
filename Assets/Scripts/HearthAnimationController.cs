using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HearthAnimationController : MonoBehaviour
{

    [SerializeField] private Button btn;
    [SerializeField] private GameObject hearth;

    private void Start()
    {
        btn.onClick.AddListener(OnBtnClick_hearth);
        
    }

    private void OnBtnClick_hearth()
    {
        hearth.GetComponent<Animator>().Play("EmptyHeartAnimation");
    }
}
