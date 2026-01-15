using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchObjectTracker : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private bool requireTrigger = true;
    [SerializeField] private GameObject ropePrefabe;

    private Camera cam;
    private GameObject lastHitObject;

    private Vector3 startPos;
    private Vector3 endPos;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);
        Ray ray = cam.ScreenPointToRay(touch.position);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, targetLayer))
        {
            GameObject hitObject = hit.collider.gameObject;

            //if (requireTrigger && !hit.collider.isTrigger)
            //    return;

            if (touch.phase == TouchPhase.Began)
            {
                lastHitObject = hitObject;
                OnTouchStart(hitObject);
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                if (hitObject != lastHitObject)
                {
                    lastHitObject = hitObject;
                    OnTouchEnter(hitObject);
                }
            }
        }

        if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            lastHitObject = null;
        }
    }

    private void OnTouchStart(GameObject obj)
    {
        Debug.Log("Touch started on: " + obj.name);
        startPos = obj.transform.position;
    }

    private void OnTouchEnter(GameObject obj)
    {
        Debug.Log("Touch moved over: " + obj.name);
        endPos = obj.transform.position;
        float dist = Vector3.Distance(startPos, endPos);
        GameObject rope = Instantiate(ropePrefabe, startPos, Quaternion.identity);

        rope.GetComponent<Rope>().Innitialize(dist, startPos, endPos);

        startPos = endPos;
    }

    
}

