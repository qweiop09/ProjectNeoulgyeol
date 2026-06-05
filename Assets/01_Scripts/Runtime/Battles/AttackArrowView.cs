using UnityEngine;

namespace _01_Scripts.Runtime.Battles
{
public class AttackArrowView : MonoBehaviour
{
    [SerializeField] private LineRenderer outlineLineRenderer;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Transform outlineArrowHead;
    [SerializeField] private Transform arrowHead;
    [SerializeField] private float verticalOffset = 0.8f;
    [SerializeField] private float startPadding = 0.35f;
    [SerializeField] private float endPadding = 0.45f;
    [SerializeField] private float arcHeight = 1.2f;
    [SerializeField] private float distanceArcHeightRatio = 0.15f;
    [SerializeField] private int segmentCount = 24;

    private Transform from;
    private Transform fixedTarget;
    private Vector3 trackingTargetPosition;
    private bool useTrackingTarget;

    public void Initialize(
        Material material,
        Color arrowColor,
        Color outlineColor,
        float lineWidth,
        float outlineWidth,
        float arrowHeadSize)
    {
        EnsureLineRenderer(ref outlineLineRenderer, "OutlineLine", material, outlineColor, outlineWidth, 49);
        EnsureLineRenderer(ref lineRenderer, "Line", material, arrowColor, lineWidth, 50);
        EnsureArrowHead(ref outlineArrowHead, "OutlineArrowHead", material, outlineColor, arrowHeadSize * 1.35f, 49);
        EnsureArrowHead(ref arrowHead, "ArrowHead", material, arrowColor, arrowHeadSize, 50);
    }

    public void SetFixedTarget(Transform from, Transform target)
    {
        this.from = from;
        fixedTarget = target;
        useTrackingTarget = false;
        UpdateArrow();
    }

    public void SetTrackingTarget(Transform from, Vector3 targetPosition)
    {
        this.from = from;
        fixedTarget = null;
        trackingTargetPosition = targetPosition;
        useTrackingTarget = true;
        UpdateArrow();
    }

    private void LateUpdate()
    {
        UpdateArrow();
    }

    private void UpdateArrow()
    {
        if (from == null || lineRenderer == null || outlineLineRenderer == null || arrowHead == null || outlineArrowHead == null)
            return;

        Vector3 start = from.position + Vector3.up * verticalOffset;
        Vector3 end = useTrackingTarget
            ? trackingTargetPosition
            : fixedTarget != null
                ? fixedTarget.position + Vector3.up * verticalOffset
                : start;

        Vector3 direction = end - start;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
            return;

        Vector3 normalizedDirection = direction.normalized;
        start += normalizedDirection * startPadding;
        end -= normalizedDirection * endPadding;

        Vector3 control = GetControlPoint(start, end);
        Vector3[] points = BuildCurvePoints(start, control, end);

        outlineLineRenderer.positionCount = points.Length;
        outlineLineRenderer.SetPositions(points);
        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);

        Vector3 endDirection = (end - control).normalized;
        SetArrowHeadTransform(outlineArrowHead, end, endDirection);
        SetArrowHeadTransform(arrowHead, end, endDirection);
    }

    private Vector3 GetControlPoint(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        return (start + end) * 0.5f + Vector3.up * (arcHeight + distance * distanceArcHeightRatio);
    }

    private Vector3[] BuildCurvePoints(Vector3 start, Vector3 control, Vector3 end)
    {
        int count = Mathf.Max(3, segmentCount);
        Vector3[] points = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            points[i] = CalculateQuadraticBezierPoint(start, control, end, t);
        }

        return points;
    }

    private Vector3 CalculateQuadraticBezierPoint(Vector3 start, Vector3 control, Vector3 end, float t)
    {
        float inverseT = 1f - t;
        return inverseT * inverseT * start
               + 2f * inverseT * t * control
               + t * t * end;
    }

    private void SetArrowHeadTransform(Transform targetArrowHead, Vector3 position, Vector3 direction)
    {
        targetArrowHead.position = position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        targetArrowHead.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private void EnsureLineRenderer(
        ref LineRenderer targetLineRenderer,
        string childName,
        Material material,
        Color color,
        float width,
        int sortingOrder)
    {
        if (targetLineRenderer == null)
        {
            Transform lineTransform = transform.Find(childName);

            if (lineTransform == null)
            {
                GameObject lineObject = new GameObject(childName);
                lineObject.transform.SetParent(transform, false);
                lineTransform = lineObject.transform;
            }

            targetLineRenderer = lineTransform.GetComponent<LineRenderer>();

            if (targetLineRenderer == null)
                targetLineRenderer = lineTransform.gameObject.AddComponent<LineRenderer>();
        }

        targetLineRenderer.useWorldSpace = true;
        targetLineRenderer.positionCount = segmentCount;
        targetLineRenderer.startWidth = width;
        targetLineRenderer.endWidth = width;
        targetLineRenderer.startColor = color;
        targetLineRenderer.endColor = color;
        targetLineRenderer.numCapVertices = 4;
        targetLineRenderer.numCornerVertices = 4;
        targetLineRenderer.sortingOrder = sortingOrder;

        if (material != null)
            targetLineRenderer.material = material;
    }

    private void EnsureArrowHead(
        ref Transform targetArrowHead,
        string childName,
        Material material,
        Color color,
        float size,
        int sortingOrder)
    {
        if (targetArrowHead == null)
            targetArrowHead = transform.Find(childName);

        if (targetArrowHead == null)
        {
            GameObject arrowHeadObject = new GameObject(childName);
            arrowHeadObject.transform.SetParent(transform, false);
            targetArrowHead = arrowHeadObject.transform;
        }

        MeshFilter meshFilter = targetArrowHead.GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = targetArrowHead.gameObject.AddComponent<MeshFilter>();

        MeshRenderer meshRenderer = targetArrowHead.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = targetArrowHead.gameObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = CreateArrowHeadMesh(size);
        meshRenderer.sortingOrder = sortingOrder;

        if (material != null)
            meshRenderer.material = material;

        if (meshRenderer.material != null)
            meshRenderer.material.color = color;
    }

    private Mesh CreateArrowHeadMesh(float size)
    {
        Mesh mesh = new Mesh();
        float halfWidth = size * 0.45f;

        mesh.vertices = new[]
        {
            new Vector3(0f, size * 0.5f, 0f),
            new Vector3(-halfWidth, -size * 0.5f, 0f),
            new Vector3(halfWidth, -size * 0.5f, 0f),
        };
        mesh.triangles = new[] { 0, 1, 2 };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
}
