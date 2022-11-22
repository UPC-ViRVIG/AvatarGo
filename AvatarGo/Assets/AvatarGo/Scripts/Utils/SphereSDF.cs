using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereSDF : MonoBehaviour, ISDF
{
    public float Radius = 1.0f;

    public float Distance(Vector3 p)
    {
        return (p - transform.position).magnitude - Radius;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }
}
