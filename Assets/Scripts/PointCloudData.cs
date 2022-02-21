using System.Collections;
using UnityEngine;

[System.Serializable]
public struct PointCloudData
{
    public PointCloudShape cloudShape;

    // For cuboid (box)
    public Vector3 boxSize;

    // For sphere
    public float sphereRadius;

    // For ellipsoid
    public EllipsoidParams ellipsoidParams;

    // For droplet

    // For custom
}

[System.Serializable]
public struct EllipsoidParams
{
    public float a, b, c;
}