using System.Collections;
using UnityEngine;
public class Unit
{
    public Vector3 position;
    public bool occupied;
    public bool visited;

    public Unit(Vector3 position, bool isOccupied)
    {
        this.position = position;
        this.occupied = isOccupied;
        this.visited = false;
    }

    public Unit(Vector3 position)
    {
        this.position = position;
        this.occupied = true;
        this.visited = false;
    }
}