using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if(UNITY_EDITOR)

[ExecuteInEditMode]
public class SceneManager : MonoBehaviour
{
    [SerializeField] Vector3Int size;
    [SerializeField] float unitSize = 1;
    Bounds levelBounds;
    Vector3 halfUnitSizeVec;
    Vector3 offset;

    Color freeColor = new Color(0,1,0,.4f);
    Color occupiedColor = new Color(1,0,0,.4f);

    public bool showOccupied = true;
    public bool showFree = false;

    public Unit[] units;

    public int treeTubeVertexAmount = 5;
    public float treeHeight = 10f;
    public Vector3 treeSize = Vector3.one;
    public float treeBaseThickness = .05f;
    public float treeStepThickness = .2f;
    public float treeMaxDiffThickness = .6f;
    public Material treeMaterial;

    public float nodeKillDistance = .2f;
    public float nodeAttractionDistance = 10f;
    public float nodeSegmentLength = .1f;

    public int attractorsAmount = 100;

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
        levelBounds = new Bounds(transform.position + Vector3.up * halfSize.y, size);
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
        Vector3 floodStartPos = levelBounds.max - halfUnitSizeVec;
        FloodFill(floodStartPos);

        AdaptableTree[] childTrees = transform.GetComponentsInChildren<AdaptableTree>();
        foreach(AdaptableTree t in childTrees)
        {
            t.getFreeUnits = GetFreeUnitsFloodFill;
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

            if (!levelBounds.Contains(currPos))
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

    public void MouseInteraction()
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            print("LEFT CLICK IDNTF");
            RaycastHit hit;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if(Physics.Raycast(ray, out hit))
            {
                print("HIT at " + hit.point);
                TryGenerateTree(hit.point + Vector3.up * .001f);
            }
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
        tree.Init(position, GetFreeUnitsFloodFill, treeMaterial, treeSize, treeHeight, nodeKillDistance, 
            nodeAttractionDistance, nodeSegmentLength, attractorsAmount, treeTubeVertexAmount,
            treeBaseThickness, treeStepThickness, treeMaxDiffThickness, unitSize / 2f);
        tree.TreeRegen();
    }

    public Unit[] GetFreeUnitsFloodFill(Vector3 position, Vector3 treeSize)
    {
        List<Unit> freeUnits = new List<Unit>();

        bool[] visitedTable = new bool[Mathf.RoundToInt(size.x * size.y * size.z / Mathf.Pow(unitSize, 3))];
        Queue<Vector3> toFill = new Queue<Vector3>();

        Vector3 bottomCenter = position - Vector3.up * ((treeSize.y / 2) - halfUnitSizeVec.y);
        toFill.Enqueue(bottomCenter);

        Bounds treeBounds = new Bounds(position, treeSize);

        if (useCustomVolume)
            treeGenVolume.transform.position = position;


        while (toFill.Count > 0)
        {
            Vector3 currPos = toFill.Dequeue();

            if (!treeBounds.Contains(currPos) || !levelBounds.Contains(currPos))
                continue;

            if (visitedTable[WorldToGrid(currPos)])
                continue;

            visitedTable[WorldToGrid(currPos)] = true;

            Unit unit = units[WorldToGrid(currPos)];

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
                    Unit unit = units[WorldToGrid(fixedPos)];
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

    private int WorldToGrid(Vector3 worldPos)
    {
        worldPos -= offset;
        return Mathf.FloorToInt(worldPos.x / unitSize) + Mathf.RoundToInt(size.x / unitSize) * (Mathf.FloorToInt(worldPos.y / unitSize) 
            + Mathf.RoundToInt(size.y / unitSize) * Mathf.FloorToInt(worldPos.z / unitSize));
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
