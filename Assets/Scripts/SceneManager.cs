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
    Vector3 halfUnitSizeVec;
    Vector3 offset;

    Color freeColor = new Color(0,1,0,.4f);
    Color occupiedColor = new Color(1,0,0,.4f);

    public bool showOccupied = true;
    public bool showFree = false;

    public Unit[] units;

    public float treeHeight = 10f;
    public Vector3 treeHalfExtents = Vector3.one;
    public float treeThickness = .05f;
    public int treeTubeVertexAmount = 5;
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
        Vector3 halfSize = size / 2;
        halfSize.y = 0;
        offset = transform.position - halfSize;
        float halfUnitSize = unitSize / 2f;
        //Vector3 halfUnitSizeVec = Vector3.one * halfUnitSize;
        halfUnitSizeVec = Vector3.one * halfUnitSize * .99f;
        for (float i = halfUnitSize; i < size.x; i+=unitSize)
        {
            for (float j = halfUnitSize; j < size.y; j += unitSize)
            {
                for (float k = halfUnitSize; k < size.z; k += unitSize)
                {
                    Vector3 pos = new Vector3(i, j, k) + offset;
                    bool isOccupied = Physics.CheckBox(pos, halfUnitSizeVec);
                    Unit unit = new Unit(pos, isOccupied);
                    units[WorldToGrid(pos)] = unit;
                }
            }
        }

        Tree[] childTrees = transform.GetComponentsInChildren<Tree>();
        foreach(Tree t in childTrees)
        {
            t.getFreeUnits = GetFreeUnits;
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
        Tree tree = go.AddComponent<Tree>();
        tree.Init(position, GetFreeUnits, treeMaterial, treeHalfExtents, treeHeight, nodeKillDistance, 
            nodeAttractionDistance, nodeSegmentLength, attractorsAmount, treeThickness, treeTubeVertexAmount, unitSize / 2f);
        tree.TreeRegen();
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
                    if(!unit.isOccupied)
                    {
                        if(useCustomVolume)
                        {
                            if(!Physics.CheckBox(fixedPos, halfUnitSizeVec, Quaternion.identity))
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
        center.y += size.y / 2;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(center, size);

        if (units == null)
            return;

        if (showOccupied || showFree)
        {
            for(int i=0; i<units.Length;i++)
            {
                if (units[i].isOccupied && showOccupied)
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
