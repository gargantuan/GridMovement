using UnityEngine;
using UnityEditor;

// If Intelisense stops working
// 1. Unity -> Preferences -> Regenerate Project Files
// 2. VSCode -> Cmd P -> reload window
// Give vscode a minute to do it's thing. When you see reference labels
// appear, it's working. 

// Design notes
// create a getGraph mthod that returns a graph object. Each object that needs a
// graph can call this method
// graphObject.getPath(from, to, lookAhead); // returns a subsection of a path

public class GraphBuilder : MonoBehaviour
{

    [SerializeField] bool debugCoordinates;
    private float tileSize = 1;

    public Vector3[,] graph;
    private Bounds levelBounds = new Bounds(Vector3.zero, Vector3.zero);

    void Awake()
    {
        this.GetLevelBounds();
        this.BuildGraph();
        Debug.Log(string.Format("rows: {0} - cols: ${1}", graph.GetLength(0), graph.GetLength(1)));
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDrawGizmos()
    {
        // NOTE: If you're not seeing changes to the Gizmo in unity,
        // run the project, to force it to refresh everything. 
        Gizmos.color = Color.cyan;
        this.GetLevelBounds();
        Gizmos.DrawWireSphere(levelBounds.min, 1f);
        Gizmos.DrawWireSphere(levelBounds.max, 1f);
        this.BuildGraph();

        int rowCount = graph.GetLength(0);
        int colCount = graph.GetLength(1);
        // Debug.Log(string.Format("rows: {0} - cols: ${1}", rowCount, colCount));
        Gizmos.color = Color.green;
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < colCount; col++)
            {
                // C# Doesn't really do null, so we set the vector to float.MaxValue and use
                // that to filter them out. 
                if (graph[row, col].x != float.MaxValue)
                {
                    Gizmos.DrawSphere(graph[row, col], 0.1f);
                    if (debugCoordinates)
                    {
                        Handles.Label(graph[row, col], string.Format("{0},{1}", row, col));
                    }
                }
            }
        }
    }

    void GetLevelBounds()
    {
        levelBounds = new Bounds();
        foreach (Renderer r in FindObjectsOfType(typeof(Renderer)))
        {
            levelBounds.Encapsulate(r.bounds);
        }
    }

    void BuildGraph()
    {
        // Debug.Log(string.Format("start ({0},{1})", Mathf.Floor(levelBounds.min.x), Mathf.Floor(levelBounds.min.z)));
        int rowCount = Mathf.FloorToInt(levelBounds.extents.z * 2);
        int colCount = Mathf.FloorToInt(levelBounds.extents.x * 2);
        float rayDepth = levelBounds.max.y + Mathf.Abs(levelBounds.min.y);

        graph = new Vector3[rowCount, colCount];
        LayerMask mask = LayerMask.GetMask("Walkable");
        LayerMask ignoreMask = LayerMask.GetMask("GraphIgnore");
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < colCount; col++)
            {
                float x = levelBounds.min.x + (col * tileSize) + (tileSize / 2);
                float z = levelBounds.min.z + (row * tileSize) + (tileSize / 2);
                // Debug.Log(string.Format("X = {0}", x));

                RaycastHit hit;
                Vector3 rayPoint = new Vector3(x, levelBounds.max.y + 0.25f, z);
                Ray ray = new Ray(rayPoint, Vector3.down);

                if (Physics.Raycast(rayPoint, Vector3.down, out hit, rayDepth + 3f, ~ignoreMask))
                {
                    if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Walkable"))
                    {
                        // Debug.Log(string.Format("Hit at Coord ${0},${1}", row, col));
                        graph[row, col] = hit.point;
                    }
                    else
                    {
                        graph[row, col] = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                    }
                }
                else
                {
                    // Debug.Log(string.Format("MISS at Coord {0},{1}", row, col));
                    graph[row, col] = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                }
            }
        }
    }
}

