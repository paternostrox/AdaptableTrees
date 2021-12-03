using System.Collections;
using UnityEngine;
public struct Unit
{
    public Unit(Vector3 position, bool isOccupied)
    {
        this.position = position;
        this.isOccupied = isOccupied;
    }

    public Vector3 position;
    public bool isOccupied;
}