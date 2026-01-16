using UnityEngine;

public class TouchObjectTracker : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject ropePrefabe;

    [Header("Camera Movement")]
    [SerializeField] float cameraMoveSpeed = 20f;

    Camera cam;
    private GameObject lastHitObject;

    Vector3 startPos;
    Vector3 endPos;
    GameObject startPole;

    private bool isDraggingCamera;
    Vector2 lastTouchPos;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount == 0)
            return;

        Touch touch = Input.GetTouch(0);
        Ray ray = cam.ScreenPointToRay(touch.position);

        bool hitPole = Physics.Raycast(ray, out RaycastHit hit, 100f, targetLayer);

        if (touch.phase == TouchPhase.Began)
        {
            lastTouchPos = touch.position;

            if (hitPole)
            {
                lastHitObject = hit.collider.gameObject;
                isDraggingCamera = false;
                OnTouchStart(lastHitObject);
            }
            else
            {
                isDraggingCamera = true;
                startPole = null;
                lastHitObject = null;
            }
        }
        else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
        {
            if (isDraggingCamera)
            {
                MoveCamera(touch);
            }
            else if (hitPole && hit.collider.gameObject != lastHitObject)
            {
                lastHitObject = hit.collider.gameObject;
                OnTouchEnter(lastHitObject);
            }
        }
        else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
        {
            lastHitObject = null;
            startPole = null;
            isDraggingCamera = false;
        }
    }
    void OnTouchStart(GameObject stp)
    {
        startPole = stp;
        startPos = startPole.transform.position;
        // Debug.Log("Touch started on: " + startPole.name);
    }

    void OnTouchEnter(GameObject endPole)
    {
        endPos = endPole.transform.position;
        float dist = Vector3.Distance(startPos, endPos);

        GameObject rope = Instantiate(ropePrefabe, startPos, Quaternion.identity);
        rope.GetComponent<Rope>().Innitialize(dist, startPos, endPos);
        ConnectionManager.Instance.Connect(startPole.GetComponent<Pole>(), endPole.GetComponent<Pole>());
        startPos = endPos;
    }
    private void MoveCamera(Touch touch)
    {
        Vector2 delta = touch.position - lastTouchPos;
        lastTouchPos = touch.position;
        Vector3 move = new Vector3(
            -delta.x * cameraMoveSpeed * Time.deltaTime,
            0f,
            -delta.y * cameraMoveSpeed * Time.deltaTime
        );

        transform.Translate(move, Space.World);

        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, -10f, 3.5f);
        clampedPos.z = Mathf.Clamp(clampedPos.z, -8f, 8f);
        transform.position = clampedPos;
    }

}
