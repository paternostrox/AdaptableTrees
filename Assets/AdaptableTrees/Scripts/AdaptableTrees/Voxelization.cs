using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if(UNITY_EDITOR)

[ExecuteInEditMode]
public class Voxelization : MonoBehaviour
{
    [HideInInspector] public Vector3Int size;
    [HideInInspector] public float unitSize = 1;
    [HideInInspector] public Vector3 floodStartPos;
    [HideInInspector] public Bounds regionBounds;
    Vector3 halfUnitSizeVec;
    Vector3 offset;

    Color freeColor = new Color(0,1,0,.4f);
    Color occupiedColor = new Color(1,0,0,.4f);

    [HideInInspector] public bool showOccupied = true;
    [HideInInspector] public bool showFree = false;

    private Unit[] units;

    [HideInInspector] public int treeTubeVertexAmount = 5;
    [HideInInspector] public float treeHeight = 10f;
    public PointCloudData pointCloudData;
    [HideInInspector] public bool abortCollidingBranches = true;
    [HideInInspector] public bool animateGrowth = false;

    [HideInInspector] public float nodeKillDistance = .2f;
    [HideInInspector] public float nodeAttractionDistance = 10f;
    [HideInInspector] public float nodeSegmentLength = .1f;
    [HideInInspector] public int attractorsAmount = 100;
    [HideInInspector] public float treeBaseThickness = .05f;
    [HideInInspector] public float treePerChildThickness = .2f;
    [HideInInspector] public float treeMaxDiffThickness = .6f;
    [HideInInspector] public Material treeMaterial;

    [HideInInspector]
    public bool useCustomVolume = false;
    [HideInInspector]
    public GameObject treeGenVolume;
    [HideInInspector]
    public LayerMask treeGenVolumeMask;

    private void Awake()
    {
        ProcessLevel();
    }

    public void ProcessLevel()
    {
        units = new Unit[Mathf.RoundToInt(size.x * size.y * size.z / Mathf.Pow(unitSize, 3))];
        Vector3 halfSize = size;
        halfSize = halfSize / 2f;
        offset = transform.position - new Vector3(halfSize.x, 0, halfSize.z);
        float halfUnitSize = unitSize / 2f;
        regionBounds = new Bounds(transform.position + Vector3.up * halfSize.y, size);
        //Vector3 halfUnitSizeVec = Vector3.one * halfUnitSize;
        halfUnitSizeVec = Vector3.one * halfUnitSize * .99f;
        for (float i = halfUnitSize; i < size.x; i+=unitSize)
        {
            for (float j = halfUnitSize; j < size.y; j += unitSize)
            {
                for (float k = halfUnitSize; k < size.z; k += unitSize)
                {
                    Vector3 pos = new Vector3(i, j, k) + offset;
                    //bool isOccupied = Physics.CheckBox(pos, halfUnitSizeVec);
                    Unit unit = new Unit(pos);
                    units[WorldToGrid(pos)] = unit;
                }
            }
        }

        //Vector3 floodStartPos = levelBounds.center + Vector3.up * (size.y/2f - halfUnitSize);
        //Vector3 floodStartPos = levelBounds.max - halfUnitSizeVec;
        FloodFill(floodStartPos);

        AdaptableTree[] childTrees = transform.GetComponentsInChildren<AdaptableTree>();
        foreach(AdaptableTree t in childTrees)
        {
            t.voxelization = this;
        }
    }

    private void FloodFill(Vector3 pos)
    {
        bool[] visitedTable = new bool[Mathf.RoundToInt(size.x * size.y * size.z / Mathf.Pow(unitSize, 3))];
        Queue<Vector3> toFill = new Queue<Vector3>();
        toFill.Enqueue(pos);

        while(toFill.Count > 0)
        {
            Vector3 currPos = toFill.Dequeue();

            if (!regionBounds.Contains(currPos))
                continue;

            if (visitedTable[WorldToGrid(currPos)])
                continue;

            visitedTable[WorldToGrid(currPos)] = true;

            if (Physics.CheckBox(currPos, halfUnitSizeVec))
                continue;

            units[WorldToGrid(currPos)].occupied = false;

            toFill.Enqueue(currPos + unitSize * Vector3.right);
            toFill.Enqueue(currPos + unitSize * Vector3.left);
            toFill.Enqueue(currPos + unitSize * Vector3.up);
            toFill.Enqueue(currPos + unitSize * Vector3.down);
            toFill.Enqueue(currPos + unitSize * Vector3.forward);
            toFill.Enqueue(currPos + unitSize * Vector3.back);
        }

    }

    float lastEventTime = 0f;

    public void MouseInteraction()
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (Time.time - lastEventTime < 0.05f)
                return;
            RaycastHit hit;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if(Physics.Raycast(ray, out hit))
            {
                TryGenerateTree(hit.point + Vector3.up * .001f);
            }
            lastEventTime = Time.time;
        }
    }

    private void TryGenerateTree(Vector3 position)
    {
        //Unit rootUnit = units[WorldToGrid(position)];
        //if (rootUnit.isOccupied)
        //{
        //    Debug.Log("Cannot place tree roots on obstacles!");
        //    return;
        //}
        float halfUnitSize = unitSize / 2f;
        GameObject go = new GameObject("Tree");
        go.transform.SetParent(transform);
        AdaptableTree tree = go.AddComponent<AdaptableTree>();
        tree.Init(this, position, pointCloudData, treeMaterial, treeHeight, nodeKillDistance, 
            nodeAttractionDistance, nodeSegmentLength, attractorsAmount, abortCollidingBranches, animateGrowth, 
            treeTubeVertexAmount, treeBaseThickness, treePerChildThickness, treeMaxDiffThickness, unitSize / 2f);
        tree.TreeRegen();
    }

    // Note: 'position' is the position of trunk highest point
    public Unit[] GetFreeUnitsFloodFill(Vector3 position, PointCloudData shapeData)
    {
        Unit[] freeUnits = null;
        switch (shapeData.cloudShape)
        {
            case PointCloudShape.Box:
                freeUnits = GetFreeUnitsCuboid(position, shapeData.boxSize);
                break;
            case PointCloudShape.Sphere:
                freeUnits = GetFreeUnitsSphere(position, shapeData.sphereRadius);
                break;
            case PointCloudShape.Ellipsoid:
                freeUnits = GetFreeUnitsEllipsoid(position, shapeData.ellipsoidSize);
                break;
        }


        return freeUnits;
    }

    public Unit[] GetFreeUnitsSphere(Vector3 position, float radius)
    {
        bool[] visitedTable = new bool[Mathf.RoundToInt(size.x * size.y * size.z / Mathf.Pow(unitSize, 3))];
        Queue<Vector3> toFill = new Queue<Vector3>();
        Vector3 bottomCenter = position + Vector3.up * halfUnitSizeVec.y;
        toFill.Enqueue(bottomCenter);

        Vector3 sphereCenter = position + Vector3.up * radius;

        List<Unit> freeUnits = new List<Unit>();

        while (toFill.Count > 0)
        {
            Vector3 currPos = toFill.Dequeue();

            if (!isInsideSphere(currPos, sphereCenter, radius) || !regionBounds.Contains(currPos) || visitedTable[WorldToGrid(currPos)])
                continue;

            visitedTable[WorldToGrid(currPos)] = true;

            Unit unit = GetUnit(currPos);

            if (unit.occupied)
                continue;

            freeUnits.Add(unit);

            toFill.Enqueue(currPos + unitSize * Vector3.right);
            toFill.Enqueue(currPos + unitSize * Vector3.left);
            toFill.Enqueue(currPos + unitSize * Vector3.up);
            toFill.Enqueue(currPos + unitSize * Vector3.down);
            toFill.Enqueue(currPos + unitSize * Vector3.forward);
            toFill.Enqueue(currPos + unitSize * Vector3.back);
        }

        return freeUnits.ToArray();
    }

    public Unit[] GetFreeUnitsEllipsoid(Vector3 position, Vector3 ellipsoidSize)
    {
        bool[] visitedTable = new bool[Mathf.RoundToInt(size.x * size.y * size.z / Mathf.Pow(unitSize, 3))];
        Queue<Vector3> toFill = new Queue<Vector3>();
        Vector3 bottomCenter = position + Vector3.up * halfUnitSizeVec.y;
        toFill.Enqueue(bottomCenter);

        Vector3 ellipsoidCenter = position + Vector3.up * ellipsoidSize.y;

        List<Unit> freeUnits = new List<Unit>();

        while(toFill.Count > 0)
        {
            Vector3 currPos = toFill.Dequeue();

            if (!isInsideEllipsoid(currPos, ellipsoidCenter, ellipsoidSize) || !regionBounds.Contains(currPos) || visitedTable[WorldToGrid(currPos)])
                continue;

            visitedTable[WorldToGrid(currPos)] = true;

            Unit unit = GetUnit(currPos);

            if (unit.occupied)
                continue;

            freeUnits.Add(unit);

            toFill.Enqueue(currPos + unitSize * Vector3.right);
            toFill.Enqueue(currPos + unitSize * Vector3.left);
            toFill.Enqueue(currPos + unitSize * Vector3.up);
            toFill.Enqueue(currPos + unitSize * Vector3.down);
            toFill.Enqueue(currPos + unitSize * Vector3.forward);
            toFill.Enqueue(currPos + unitSize * Vector3.back);
        }

        return freeUnits.ToArray();
    }

    public Unit[] GetFreeUnitsCuboid(Vector3 position, Vector3 treeSize)
    {
        bool[] visitedTable = new bool[Mathf.RoundToInt(size.x * size.y * size.z / Mathf.Pow(unitSize, 3))];
        Queue<Vector3> toFill = new Queue<Vector3>();
        Vector3 bottomCenter = position + Vector3.up * halfUnitSizeVec.y;
        toFill.Enqueue(bottomCenter);

        Vector3 center = position + Vector3.up * (treeSize.y / 2f);
        Bounds treeBounds = new Bounds(center, treeSize);
        List<Unit> freeUnits = new List<Unit>();

        while (toFill.Count > 0)
        {
            Vector3 currPos = toFill.Dequeue();

            if (!treeBounds.Contains(currPos) || !regionBounds.Contains(currPos) || visitedTable[WorldToGrid(currPos)])
                continue;

            visitedTable[WorldToGrid(currPos)] = true;

            Unit unit = GetUnit(currPos);

            if (unit.occupied)
                continue;

            freeUnits.Add(unit);

            toFill.Enqueue(currPos + unitSize * Vector3.right);
            toFill.Enqueue(currPos + unitSize * Vector3.left);
            toFill.Enqueue(currPos + unitSize * Vector3.up);
            toFill.Enqueue(currPos + unitSize * Vector3.down);
            toFill.Enqueue(currPos + unitSize * Vector3.forward);
            toFill.Enqueue(currPos + unitSize * Vector3.back);
        }

        return freeUnits.ToArray();
    }

    public bool isInsideSphere(Vector3 position, Vector3 sphereCenter, float radius)
    {
        Vector3 checkPosition = position - sphereCenter;
        return checkPosition.x * checkPosition.x + checkPosition.y * checkPosition.y + checkPosition.z * checkPosition.z <= radius * radius;
    }

    public bool isInsideEllipsoid(Vector3 position, Vector3 ellipsoidCenter, Vector3 ellipsoidSize)
    {
        Vector3 checkPosition = position - ellipsoidCenter;
        float ratioX = (checkPosition.x * checkPosition.x) / (ellipsoidSize.x * ellipsoidSize.x);
        float ratioY = (checkPosition.y * checkPosition.y) / (ellipsoidSize.y * ellipsoidSize.y);
        float ratioZ = (checkPosition.z * checkPosition.z) / (ellipsoidSize.z * ellipsoidSize.z);
        return ratioX + ratioY + ratioZ <= 1;
    }

    public Unit[] GetFreeUnits(Vector3 position, Vector3 halfExtents)
    {
        List<Unit> freeUnits = new List<Unit>();

        if (useCustomVolume)
            treeGenVolume.transform.position = position;

        for(float i =-halfExtents.x; i < halfExtents.x; i += unitSize)
        {
            for (float j = -halfExtents.y; j < halfExtents.y; j += unitSize)
            {
                for (float k = -halfExtents.z; k < halfExtents.z; k += unitSize)
                {
                    Vector3 fixedPos = position + new Vector3(i, j, k);
                    Unit unit = GetUnit(fixedPos);
                    if(!unit.occupied)
                    {
                        if(useCustomVolume)
                        {
                            if(!Physics.CheckBox(fixedPos, halfUnitSizeVec))
                            {
                                continue;
                            }
                        }
                        freeUnits.Add(unit);
                    }
                }
            }
        }

        return freeUnits.ToArray();
    }

    public Unit GetUnit(Vector3 worldPosition)
    {
        return units[WorldToGrid(worldPosition)];
    }

    private int WorldToGrid(Vector3 worldPosition)
    {
        worldPosition -= offset;
        return Mathf.FloorToInt(worldPosition.x / unitSize) + Mathf.RoundToInt(size.x / unitSize) * (Mathf.FloorToInt(worldPosition.y / unitSize) 
            + Mathf.RoundToInt(size.y / unitSize) * Mathf.FloorToInt(worldPosition.z / unitSize));
    }

    private void OnDrawGizmos()
    {
        Vector3 center = transform.position;
        Vector3 unitSizeVec = unitSize * Vector3.one;
        center.y += size.y / 2f;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(center, size);

        if (units == null)
            return;

        if (showOccupied || showFree)
        {
            for(int i=0; i<units.Length;i++)
            {
                if (units[i].occupied && showOccupied)
                {
                    Gizmos.color = occupiedColor;
                    Gizmos.DrawCube(units[i].position, unitSizeVec);
                }
                else if (showFree)
                {
                    Gizmos.color = freeColor;
                    Gizmos.DrawCube(units[i].position, unitSizeVec);
                }
            }
        }
    }
}

#endif
