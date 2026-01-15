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

    [SerializeField] private bool isSimulating = true;

    [Header("Endpoints")]
    [SerializeField] private Vector3 startPoint;
    [SerializeField] private Vector3 endPoint;

    [Header("Rope Settings")]
    [SerializeField] private float ropeSegmentLength = 0.25f;
    [SerializeField] private int segmentCount = 35;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private int constraintIterations = 40;

    [Header("Collision")]
    [SerializeField] private float collisionRadius = 0.1f;
    [SerializeField] private LayerMask collisionMask;
    [SerializeField] private int collisionIterations = 2;


    private LineRenderer lineRenderer;
    private List<RopeSegment> ropeSegments = new List<RopeSegment>();

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        StartCoroutine(StopSimulation());
    }

    IEnumerator StopSimulation()
    {
        yield return new WaitForSeconds(2f);
        isSimulating = false;
    }

    public void Innitialize(float distance, Vector3 stp, Vector3 ep)
    {
        segmentCount = (int)(distance / ropeSegmentLength) + 1;

        startPoint = stp;
        endPoint = ep;


        Vector3 startPos = startPoint;
        Vector3 endPos = endPoint;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);
            ropeSegments.Add(new RopeSegment(pos));
        }
    }

    private void FixedUpdate()
    {
        if(isSimulating)
            Simulate();
    }

    private void LateUpdate()
    {
        if(isSimulating)
            DrawRope();
    }


    private void ResolveCollisions()
    {
        for (int i = 1; i < segmentCount - 1; i++)
        {
            RopeSegment seg = ropeSegments[i];

            Collider[] hits = Physics.OverlapSphere(
                seg.currentPosition,
                collisionRadius,
                collisionMask
            );

            foreach (Collider hit in hits)
            {
                if (hit is SphereCollider sphere)
                {
                    Vector3 center = sphere.transform.TransformPoint(sphere.center);
                    float radius = sphere.radius * sphere.transform.lossyScale.x;

                    Vector3 dir = seg.currentPosition - center;
                    float dist = dir.magnitude;

                    if (dist < radius + collisionRadius)
                    {
                        seg.currentPosition = center + dir.normalized * (radius + collisionRadius);
                    }
                }
                else
                {
                    Vector3 closest = hit.ClosestPoint(seg.currentPosition);
                    Vector3 dir = seg.currentPosition - closest;

                    float dist = dir.magnitude;
                    if (dist < collisionRadius)
                    {
                        seg.currentPosition = closest + dir.normalized * collisionRadius;
                    }
                }
            }

            ropeSegments[i] = seg;
        }
    }



    private void Simulate()
    {
        Vector3 gravity = Physics.gravity;
        float dt = Time.fixedDeltaTime;

        float velocityScale = 0.85f;
        float maxDisplacement = ropeSegmentLength * 0.5f;

        for (int i = 1; i < segmentCount - 1; i++)
        {
            RopeSegment seg = ropeSegments[i];

            Vector3 velocity = seg.currentPosition - seg.previousPosition;

            velocity = Vector3.ClampMagnitude(velocity * velocityScale, maxDisplacement);

            seg.previousPosition = seg.currentPosition;
            seg.currentPosition += velocity;
            seg.currentPosition += gravity * dt * dt;

            ropeSegments[i] = seg;
        }
        ResolveCollisions();
        for (int i = 0; i < constraintIterations; i++)
            ApplyConstraints();
    }

    private void ApplyConstraints()
    {
        // start
        RopeSegment startSeg = ropeSegments[0];
        startSeg.currentPosition = startPoint;
        ropeSegments[0] = startSeg;

        // end
        RopeSegment endSeg = ropeSegments[segmentCount - 1];
        endSeg.currentPosition = endPoint;
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

    private void DrawRope()
    {
        lineRenderer.positionCount = segmentCount;

        for (int i = 0; i < segmentCount; i++)
            lineRenderer.SetPosition(i, ropeSegments[i].currentPosition);
    }
}
