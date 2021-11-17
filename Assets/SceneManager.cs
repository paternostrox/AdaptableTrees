using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    [SerializeField] Vector3 size;
    [SerializeField] Vector3 bottomCenter;
    [SerializeField] float unitSize = 1;
    Vector3 unitSizeVec;

    Color freeColor = new Color(0,1,0,.4f);
    Color occupiedColor = new Color(1,0,0,.4f);

    public bool showOccupied = true;
    public bool showFree = false;

    public List<Unit> units = new List<Unit>();

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

    private void Start()
    {
        unitSizeVec = Vector3.one * unitSize;
        ProcessLevel();
    }

    private void ProcessLevel()
    {
        Vector3 halfSize = size / 2;
        halfSize.y = 0;
        Vector3 halfUnitSizeVec = unitSizeVec / 2;
        Vector3 offset = bottomCenter - halfSize + halfUnitSizeVec;
        for (float i=0; i<size.x; i+=unitSize)
        {
            for (float j = 0; j < size.y; j += unitSize)
            {
                for (float k = 0; k < size.z; k += unitSize)
                {
                    Vector3 pos = new Vector3(i, j, k) + offset;
                    Collider[] cols = Physics.OverlapBox(pos, halfUnitSizeVec);
                    units.Add(new Unit(pos, (cols.Length > 0)));
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 center = bottomCenter;
        center.y = bottomCenter.y + size.y / 2;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(center, size);

        if (showOccupied || showFree)
        {
            foreach (Unit u in units)
            {
                if (u.isOccupied && showOccupied)
                {
                    Gizmos.color = occupiedColor;
                    Gizmos.DrawCube(u.position, unitSizeVec);
                }
                else if (showFree)
                {
                    Gizmos.color = freeColor;
                    Gizmos.DrawCube(u.position, unitSizeVec);
                }
            }
        }
    }
}
