using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;


public class Utils
{
    // Find transform 'name' in descendants of 'current'
    public static Transform FindDescendants(Transform current, string name)
    {
        if (current.name == name)
        {
            return current;
        }
        else
        {
            for (int i = 0; i < current.childCount; ++i)
            {
                Transform found = FindDescendants(current.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
    }

    // Find any transform in 'names' in descendants of 'current'
    public static Transform FindDescendants(Transform current, string[] names)
    {
        foreach (string name in names)
        {
            if (Utils.FindDescendants(current, name))
            {
                return Utils.FindDescendants(current, name);
            }
        }
        return null;
    }

    // Fit least square errors plane to set of points
    public static bool FitPlane(uint numPoints, Vector3[] points, ref float a, ref float b, ref float c, ref float d)
    {
        // Check input
        if (numPoints < 3)
        {
            return false;
        }

        // Compute the mean of the points
        Vector3 mean = new Vector3(0.0f, 0.0f, 0.0f);
        for (uint i = 0; i < numPoints; ++i)
        {
            mean += points[i];
        }
        mean /= numPoints;

        // Compute the linear system matrix and vector elements
        float xxSum = 0.0f, xySum = 0.0f, xzSum = 0.0f, yySum = 0.0f, yzSum = 0.0f;
        for (uint i = 0; i < numPoints; ++i)
        {
            Vector3 diff = points[i] - mean;
            xxSum += diff[0] * diff[0];
            xySum += diff[0] * diff[1];
            xzSum += diff[0] * diff[2];
            yySum += diff[1] * diff[1];
            yzSum += diff[1] * diff[2];
        }

        // Solve the linear system
        float det = xxSum * yySum - xySum * xySum;
        if (det != 0.0f)
        {
            // Compute the fitted plane
            a = (yySum * xzSum - xySum * yzSum) / det;
            b = (xxSum * yzSum - xySum * xzSum) / det;
            c = -1;
            d = -a * mean[0] - b * mean[1] + mean[2];
            return true;
        }
        else
        {
            return false;
        }
    }
}
