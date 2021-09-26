using UnityEngine;
using UnityEngine.InputSystem;

enum EDirection
{
    Up,
    Down,
    Left,
    Right,
    None
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameObject levelManager;
    // Start is called before the first frame update
    private GraphBuilder graphBuilder;

    private Vector3 currentPosition;
    private int[] currentCoords;
    private int[] targetCoords;
    private Vector3 targetPosition;
    private EDirection direction = EDirection.None;
    private EDirection requestedDirection = EDirection.None;

    // Movement speed in units per second.
    public float speed = 0.1F;

    // Time when the movement started.
    private float startTime;

    // Total distance between the markers.
    private float journeyLength;

    Controls actions;

    void Awake()
    {
        actions = new Controls();
        graphBuilder = levelManager.GetComponent<GraphBuilder>();
    }
    void Start()
    {
        currentCoords = GetCoordsWithVector(transform.position);
        currentPosition = graphBuilder.graph[currentCoords[0], currentCoords[1]];
        targetPosition = graphBuilder.graph[currentCoords[0], currentCoords[1]];
        targetCoords = currentCoords;
        transform.position = targetPosition;
        // print(string.Format("current: {0}", currentPosition));
        // print(string.Format("target: {0}", targetPosition));
    }

    public void OnEnable()
    {
        actions.Enable();
    }

    public void OnDisable()
    {
        actions.Disable();
    }

    int[] GetCoordsWithVector(Vector3 vector)
    {

        float distance = float.MaxValue;
        int[] coords = { 0, 0 };
        for (int row = 0; row < graphBuilder.graph.GetLength(0); row++)
        {
            for (int col = 0; col < graphBuilder.graph.GetLength(1); col++)
            {
                float newDistance = Vector3.Distance(transform.position, graphBuilder.graph[row, col]);
                if (newDistance < distance)
                {
                    coords = new int[] { row, col };
                }
                distance = newDistance < distance ? newDistance : distance;
            }
        }
        // print(string.Format("coords: {0},{1}", coords[0], coords[1]));
        // print(string.Format("positon: {0}", graphBuilder.graph[coords[0], coords[1]]));
        return coords;
    }

    // Update is called once per frame
    void Update()
    {

        if (transform.position.Equals(targetPosition))
        {
            if (direction != requestedDirection)
            {
                direction = requestedDirection;
            }
            int[] nextCoords = GetCoordsWithDirection(direction, currentCoords);
            Vector3? tentativeTarget = GetVectorAtCoords(nextCoords);
            // print(string.Format("[{0},{1}] - {2} - {3}", nextCoords[0], nextCoords[1], tentativeTarget, direction));
            if (tentativeTarget.HasValue)
            {
                currentPosition = transform.position;
                currentCoords = targetCoords;
                targetCoords = nextCoords;
                targetPosition = tentativeTarget.Value;
                startTime = Time.time;
            }
        }
        else
        {
            // Distance moved equals elapsed time times speed..
            float distCovered = Mathf.Clamp01((Time.time - startTime) * speed);

            // Fraction of journey completed equals current distance divided by total distance.
            float fractionOfJourney = distCovered / journeyLength;

            // Set our position as a fraction of the distance between the markers.
            transform.position = Vector3.Lerp(currentPosition, targetPosition, fractionOfJourney);
        }

    }

    Vector3? GetVectorAtCoords(int[] coords)
    {
        try
        {

            Vector3 t = graphBuilder.graph[coords[0], coords[1]];
            if (t.x == float.MaxValue)
            {
                return null;
            }

            return t;
        }
        catch
        {
            return null;
        }
    }

    int[] GetCoordsWithDirection(EDirection dir, int[] current)
    {
        switch (dir)
        {
            case EDirection.Up:
                return new int[] { current[0] + 1, current[1] };
            case EDirection.Down:
                return new int[] { current[0] - 1, current[1] };

            case EDirection.Left:
                return new int[] { current[0], current[1] - 1 };

            case EDirection.Right:
                return new int[] { current[0], current[1] + 1 };

            case EDirection.None:
            default:
                return current;
        }
    }

    public void OnMove(InputValue value)
    {
        Vector2 v2 = value.Get<Vector2>();

        int row = currentCoords[0] + (int)v2.y;
        int col = currentCoords[1] + (int)v2.x;
        // Debug.Log(string.Format("Target coords {0},{1}", row, col));

        if (v2.Equals(Vector2.up))
        {
            requestedDirection = EDirection.Up;
        }
        if (v2.Equals(Vector2.down))
        {
            requestedDirection = EDirection.Down;
        }
        if (v2.Equals(Vector2.left))
        {
            requestedDirection = EDirection.Left;
        }
        if (v2.Equals(Vector2.right))
        {
            requestedDirection = EDirection.Right;
        }
    }
}
