using System.Collections;
using UnityEngine;
public class Unit
{
    public Vector3 position;
    public bool occupied;

    public Unit(Vector3 position, bool isOccupied)
    {
        this.position = position;
        this.occupied = isOccupied;
    }

    public Unit(Vector3 position)
    {
        this.position = position;
        this.occupied = true;
    }
}