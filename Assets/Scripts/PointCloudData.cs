﻿using System.Collections;
using UnityEngine;

[System.Serializable]
public struct PointCloudData
{
    public PointCloudShape cloudShape;

    // For cuboid
    public Vector3 cuboidSize;

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