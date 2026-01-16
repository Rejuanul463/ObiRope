using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public struct RopeSegment
    {
        public Vector3 currentPosition;
        public Vector3 previousPosition;

        public RopeSegment(Vector3 pos)
        {
            currentPosition = pos;
            previousPosition = pos;
        }
    }

    [SerializeField] Transform emptyGameObject;
    [SerializeField] bool isSimulating = true;

    [Header("Endpoints")]
    [SerializeField] Vector3 startPoint;
    [SerializeField] Vector3 endPoint;

    [Header("Rope Settings")]
    [SerializeField] float ropeSegmentLength = 0.25f;
    [SerializeField] int segmentCount = 35;
    [SerializeField] float lineWidth = 0.1f;
    [SerializeField] int constraintIterations = 40;

    [Header("Collision")]
    [SerializeField] float collisionRadius = 0.1f;
    [SerializeField] LayerMask collisionMask;

    LineRenderer lineRenderer;
    List<RopeSegment> ropeSegments = new List<RopeSegment>();

    Collider[] collisionHits = new Collider[16];

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        StartCoroutine(StopSimulation());
    }

    IEnumerator StopSimulation()
    {
        yield return new WaitForSeconds(3f);
        isSimulating = false;
    }
    // This will be called whenever player completes Dragging from one pole to another
    public void Innitialize(float distance, Vector3 stp, Vector3 ep)
    {
        ropeSegments.Clear();

        startPoint = stp;
        endPoint = ep;

        segmentCount = (int)(distance / ropeSegmentLength) + 1;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);
            Vector3 pos = Vector3.Lerp(startPoint, endPoint, t);
            ropeSegments.Add(new RopeSegment(pos));
        }

        if (emptyGameObject != null)
            emptyGameObject.position = startPoint;
    }

    void FixedUpdate()
    {
        if (isSimulating)
            Simulate();
    }

    void LateUpdate()
    {
        if(isSimulating)
            DrawRope();
    }
    private void Simulate()
    {
        Vector3 gravity = Physics.gravity;
        float dt = Time.fixedDeltaTime;

        for (int i = 1; i < segmentCount - 1; i++)
        {
            RopeSegment seg = ropeSegments[i];

            Vector3 velocity = seg.currentPosition - seg.previousPosition;
            seg.previousPosition = seg.currentPosition;

            seg.currentPosition += velocity;
            seg.currentPosition += gravity * dt * dt;

            ropeSegments[i] = seg;
        }

        ResolveCollisions();

        for (int i = 0; i < constraintIterations; i++)
            ApplyConstraints();
    }
    void ApplyConstraints()
    {
        // Move endpoint FIRST
        emptyGameObject.position = Vector3.MoveTowards(
            emptyGameObject.position,
            endPoint,
            1f * Time.fixedDeltaTime
        );

        // Lock start
        RopeSegment startSeg = ropeSegments[0];
        startSeg.currentPosition = startPoint;
        ropeSegments[0] = startSeg;

        // Lock end
        RopeSegment endSeg = ropeSegments[segmentCount - 1];
        endSeg.currentPosition = emptyGameObject.position;
        ropeSegments[segmentCount - 1] = endSeg;

        // Length constraints
        for (int i = 0; i < segmentCount - 1; i++)
        {
            RopeSegment a = ropeSegments[i];
            RopeSegment b = ropeSegments[i + 1];

            float dist = Vector3.Distance(a.currentPosition, b.currentPosition);
            float error = dist - ropeSegmentLength;
            Vector3 dir = (a.currentPosition - b.currentPosition).normalized;
            Vector3 correction = dir * error;

            if (i == 0)
            {
                b.currentPosition += correction;
            }
            else if (i + 1 == segmentCount - 1)
            {
                a.currentPosition -= correction;
            }
            else
            {
                a.currentPosition -= correction * 0.5f;
                b.currentPosition += correction * 0.5f;
            }

            ropeSegments[i] = a;
            ropeSegments[i + 1] = b;
        }
    }
    private void ResolveCollisions()
    {
        for (int i = 1; i < segmentCount - 1; i++)
        {
            RopeSegment seg = ropeSegments[i];

            int hitCount = Physics.OverlapSphereNonAlloc(
                seg.currentPosition,
                collisionRadius,
                collisionHits,
                collisionMask
            );

            for (int h = 0; h < hitCount; h++)
            {
                Collider hit = collisionHits[h];
                Vector3 closest = hit.ClosestPoint(seg.currentPosition);
                Vector3 dir = seg.currentPosition - closest;

                float dist = dir.magnitude;
                if (dist > 0f && dist < collisionRadius)
                {
                    seg.currentPosition = closest + dir.normalized * collisionRadius;
                }
            }

            ropeSegments[i] = seg;
        }
    }
    private void DrawRope()
    {
        if (ropeSegments.Count == 0) return;

        lineRenderer.positionCount = segmentCount;

        for (int i = 0; i < segmentCount; i++)
            lineRenderer.SetPosition(i, ropeSegments[i].currentPosition);
    }
}
