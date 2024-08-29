using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DrawManagerScript : MonoBehaviour
{
    private TouchControls touchControl;
    [SerializeField] private GameObject linePrefab;
    private bool lineStarted;
    GameObject line;

    private int lineAmount;

    private void Awake()
    {
        touchControl = new TouchControls();
    }

    private void OnEnable()
    {
        touchControl.Enable();
    }

    private void OnDisable()
    {
        touchControl.Disable();
    }

    private void Start()
    {


        touchControl.Touch.TouchPress.started += ctx => OnTouchStart();        
        touchControl.Touch.TouchPress.canceled += ctx => OnTouchEnd(ctx);
    }

    void Update()
    {
        
    }

    void OnTouchStart()
    {
        Vector2 touchPosition = touchControl.Touch.TuchPosition.ReadValue<Vector2>();
        Vector3 spawnPosition = Camera.main.ScreenToWorldPoint(new Vector3(touchPosition.x, touchPosition.y, Camera.main.nearClipPlane + 1));

        //Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        //RaycastHit hit;

        //if (Physics.Raycast(ray, out hit, Mathf.Infinity, 6))
        //{
        //    Debug.Log("Ray hits");
        //    if (hit.collider.CompareTag("DrawArea"))
        //    {
                if (!lineStarted)
                {
                    line = Instantiate(linePrefab, spawnPosition, Quaternion.identity);
                    LineRenderer lr = line.GetComponent<LineRenderer>();
                    lr.SetPosition(0, spawnPosition);
                    lr.SetPosition(1, spawnPosition);

                    lineStarted = true;

                }

                else
                {
                    LineRenderer lr = line.GetComponent<LineRenderer>();
                    lr.SetPosition(1, spawnPosition);
                    lineStarted = false;

                }

        //    }
        //}           
        
    }
    
    void OnTouchEnd (InputAction.CallbackContext ctx)
    {
        if (!lineStarted) return;

            Debug.Log("touch ended " + touchControl.Touch.TuchPosition.ReadValue<Vector2>());
    }
}
