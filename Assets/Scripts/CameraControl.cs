using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
       animator = gameObject.GetComponent<Animator>(); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShackCamera()
    {
        animator.SetTrigger("missClick");
    }
}
