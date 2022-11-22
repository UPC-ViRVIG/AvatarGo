using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class CubeSDF : MonoBehaviour, ISDF
{
    public Vector3 Size = Vector3.one;

    public float Distance(Vector3 p)
    {
        p = Quaternion.Inverse(transform.parent == null ? transform.rotation : transform.parent.rotation) * (p - transform.position);
        float3 q = math.abs((float3)p) - (float3)Size / 2;
        return math.length(math.max(q, 0.0f)) + math.min(math.max(q.x, math.max(q.y, q.z)), 0.0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Size);
    }
}
