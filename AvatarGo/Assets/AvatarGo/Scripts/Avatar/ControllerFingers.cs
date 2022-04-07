using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ControllerFingers : MonoBehaviour
{
    [Header("Capsule - Body Controller")]
    public Vector3 StartPoint;
    public Vector3 EndPoint;
    public float Radius;
    [Header("Plane - Thumb Center")]
    public Vector3 PlanePoint;
    public Vector3 PlaneNormal = Vector3.right;
    [Header("Plane - Thumb / Fingers Separator")]
    public Vector3 PlaneSeparatorPoint;
    public Vector3 PlaneSeparatorNormal = Vector3.up;

    private void Start()
    {
        PlaneNormal = PlaneNormal.normalized;
        PlaneSeparatorNormal = PlaneSeparatorNormal.normalized;
    }

    // Capsule + Plane
    public float GetThumbPlaneDistance(Vector3 pos)
    {
        return PlaneDistance(pos);
    }

    // Separator Plane
    public float GetSDFSeparatorPlane(Vector3 pos)
    {
        return SeparatorPlaneSDF(pos);
    }

    // Capsule
    public float GetControllerSDF(Vector3 pos)
    {
        return SDFCapsule(pos);
    }

    private float SDFCapsule(Vector3 pos)
    {
        pos = transform.InverseTransformPoint(pos);
        Vector3 posStart = pos - StartPoint;
        Vector3 endStart = EndPoint - StartPoint;
        float h = Mathf.Clamp(Vector3.Dot(posStart, endStart) / Vector3.Dot(endStart, endStart), 0, 1);
        float distance = Vector3.Magnitude(posStart - endStart * h) - Radius - 0.005f; // "radius" fingers -> 0.005f
        return distance;
    }

    private float PlaneDistance(Vector3 pos)
    {
        Vector3 center = transform.TransformPoint(PlanePoint);
        Vector3 n = transform.TransformDirection(PlaneNormal);
        return Mathf.Abs(Vector3.Dot(pos - center, n));
    }

    private float SeparatorPlaneSDF(Vector3 pos)
    {
        Vector3 center = transform.TransformPoint(PlaneSeparatorPoint);
        Vector3 n = transform.TransformDirection(PlaneSeparatorNormal);
        return Vector3.Dot(pos - center, n);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector3 worldStart = transform.TransformPoint(StartPoint);
        Vector3 worldEnd = transform.TransformPoint(EndPoint);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(worldStart, 0.005f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(worldEnd, 0.005f);
        // Plane
        const float scale = 0.25f;
        Gizmos.color = Color.green;
        Vector3 center = transform.TransformPoint(PlanePoint);
        Vector3 n = transform.TransformDirection(PlaneNormal.normalized);
        Vector3 v1 = (new Vector3(-n.y, n.x, 0)).normalized;
        Vector3 v2 = Vector3.Cross(n, v1);
        Gizmos.DrawLine(v1 * scale + center, v2 * scale + center);
        Gizmos.DrawLine(-v1 * scale + center, -v2 * scale + center);
        Gizmos.DrawLine(v2 * scale + center, -v1 * scale + center);
        Gizmos.DrawLine(-v2 * scale + center, v1 * scale + center);
        // Plane Separator
        Gizmos.color = Color.magenta;
        Vector3 centerSeparator = transform.TransformPoint(PlaneSeparatorPoint);
        Vector3 nSeparator = transform.TransformDirection(PlaneSeparatorNormal.normalized);
        Vector3 v1Separator = (new Vector3(-nSeparator.y, nSeparator.x, 0)).normalized;
        Vector3 v2Separator = Vector3.Cross(nSeparator, v1Separator);
        Gizmos.DrawLine(v1Separator * scale + centerSeparator, v2Separator * scale + centerSeparator);
        Gizmos.DrawLine(-v1Separator * scale + centerSeparator, -v2Separator * scale + centerSeparator);
        Gizmos.DrawLine(v2Separator * scale + centerSeparator, -v1Separator * scale + centerSeparator);
        Gizmos.DrawLine(-v2Separator * scale + centerSeparator, v1Separator * scale + centerSeparator);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(ControllerFingers))]
public class ControllerFingersEditor : Editor
{
    private void OnSceneGUI()
    {
        ControllerFingers script = (ControllerFingers)target;
        Vector3 worldStart = script.transform.TransformPoint(script.StartPoint);
        Vector3 worldEnd = script.transform.TransformPoint(script.EndPoint);
        Gizmos.color = Color.red;
        DrawWireCapsule(worldStart, worldEnd, script.Radius);
    }
    public static void DrawWireCapsule(Vector3 p1, Vector3 p2, float radius)
    {
        // Special case when both points are in the same position
        if (p1 == p2)
        {
            // DrawWireSphere works only in gizmo methods
            Gizmos.DrawWireSphere(p1, radius);
            return;
        }
        using (new UnityEditor.Handles.DrawingScope(Gizmos.color, Gizmos.matrix))
        {
            Quaternion p1Rotation = Quaternion.LookRotation(p1 - p2);
            Quaternion p2Rotation = Quaternion.LookRotation(p2 - p1);
            // Check if capsule direction is collinear to Vector.up
            float c = Vector3.Dot((p1 - p2).normalized, Vector3.up);
            if (c == 1f || c == -1f)
            {
                // Fix rotation
                p2Rotation = Quaternion.Euler(p2Rotation.eulerAngles.x, p2Rotation.eulerAngles.y + 180f, p2Rotation.eulerAngles.z);
            }
            // First side
            UnityEditor.Handles.DrawWireArc(p1, p1Rotation * Vector3.left, p1Rotation * Vector3.down, 180f, radius);
            UnityEditor.Handles.DrawWireArc(p1, p1Rotation * Vector3.up, p1Rotation * Vector3.left, 180f, radius);
            UnityEditor.Handles.DrawWireDisc(p1, (p2 - p1).normalized, radius);
            // Second side
            UnityEditor.Handles.DrawWireArc(p2, p2Rotation * Vector3.left, p2Rotation * Vector3.down, 180f, radius);
            UnityEditor.Handles.DrawWireArc(p2, p2Rotation * Vector3.up, p2Rotation * Vector3.left, 180f, radius);
            UnityEditor.Handles.DrawWireDisc(p2, (p1 - p2).normalized, radius);
            // Lines
            UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.down * radius, p2 + p2Rotation * Vector3.down * radius);
            UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.left * radius, p2 + p2Rotation * Vector3.right * radius);
            UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.up * radius, p2 + p2Rotation * Vector3.up * radius);
            UnityEditor.Handles.DrawLine(p1 + p1Rotation * Vector3.right * radius, p2 + p2Rotation * Vector3.left * radius);
        }
    }

    private static void DrawLine(float arg1, float arg2, float forward)
    {
        Handles.DrawLine(new Vector3(arg1, arg2, 0f), new Vector3(arg1, arg2, forward));
    }
}
#endif