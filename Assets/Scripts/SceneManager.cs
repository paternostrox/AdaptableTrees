using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : Singleton<SceneManager>
{
    Camera camera;

    [SerializeField] Vector3Int size;
    [SerializeField] Vector3 bottomCenter;
    [SerializeField] float unitSize = 1;
    Vector3 offset;

    Color freeColor = new Color(0,1,0,.4f);
    Color occupiedColor = new Color(1,0,0,.4f);

    public bool showOccupied = true;
    public bool showFree = false;
    public bool showLastCrown = false;

    public Unit[] units;
    public Unit[] lastCrown;

    public float treeHeight = 10f;
    public float treeSize = 2f;
    public GameObject treePrefab;

    public float nodeKillDistance = .2f;
    public float nodeAttractionDistance = 10f;
    public float nodeSegmentLength = .1f;
    // jitter to avoid getting stuck
    public float nodeJitterAmount = .02f;

    public int attractorsAmount = 100;

    private void Start()
    {
        camera = Camera.main;
        units = new Unit[(int) (size.x * size.y * size.z / unitSize)];
        Vector3 halfSize = size / 2;
        halfSize.y = 0;
        offset = bottomCenter - halfSize;
        ProcessLevel();
    }

    private void ProcessLevel()
    {
        float halfUnitSize = unitSize / 2;
        Vector3 halfUnitSizeVec = Vector3.one * halfUnitSize * .99f;
        for (float i = halfUnitSize; i < size.x; i+=unitSize)
        {
            for (float j = halfUnitSize; j < size.y; j += unitSize)
            {
                for (float k = halfUnitSize; k < size.z; k += unitSize)
                {
                    Vector3 pos = new Vector3(i, j, k) + offset;
                    Collider[] cols = Physics.OverlapBox(pos, halfUnitSizeVec);
                    Unit unit = new Unit(pos, (cols.Length > 0));
                    units[WorldToGrid(pos)] = unit;
                }
            }
        }
    }

    private void Update()
    {
        MouseInteraction();
    }

    private void MouseInteraction()
    {
        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit))
            {
                print("HIT at " + hit.point);
                TryGenerateTree(hit.point + Vector3.up * .001f);
            }
        }
    }

    private void TryGenerateTree(Vector3 position)
    {
        Vector3 fixedPos = units[WorldToGrid(position)].position;
        Vector3 crownPos = fixedPos + Vector3.up * treeHeight;
        Unit[] crownUnits = GetFreeUnits(crownPos, Vector3.one * treeSize);
        Tree tree = Instantiate(treePrefab, transform).GetComponent<Tree>();
        tree.SetParams(nodeKillDistance, nodeAttractionDistance, nodeSegmentLength, attractorsAmount, unitSize / 2);
        tree.Init(fixedPos, crownUnits);
        lastCrown = crownUnits;
    }

    private Unit[] GetFreeUnits(Vector3 position, Vector3 halfExtents)
    {
        List<Unit> freeUnits = new List<Unit>();

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
        return Mathf.FloorToInt(worldPos.x) + size.x * (Mathf.FloorToInt(worldPos.y) + size.y * Mathf.FloorToInt(worldPos.z)); // Tried to divide by unit size, didnt work
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
        Vector3 center = bottomCenter;
        Vector3 unitSizeVec = unitSize * Vector3.one;
        center.y = bottomCenter.y + size.y / 2;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(center, size);

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

        if (showLastCrown && lastCrown != null)
        {
            for (int i = 0; i < lastCrown.Length; i++)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawCube(lastCrown[i].position, unitSizeVec);
            }
        }
    }
}
