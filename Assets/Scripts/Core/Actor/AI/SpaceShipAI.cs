using System.Collections.Generic;
using UnityEngine;

public class SpaceShipAI : MonoBehaviour {
    public float _maxSpeed = 200f;
    public float _maxAcceleration = 10f;
    public float _brakeDistance = 1f;
    public float _timeToReachDesiredVelocity = 0.3f;
    public float _avoidanceRadius = 0.4f;
    public float _avoidanceImportance = 3f;
    public float _avoidanceBreadth = 0.3f;
    public float _idealDistanceToTarget = 5f;
    public float _minAsteroidSizeToCareAbout = 2f;
    private Rigidbody2D _rigidbody;
    private PathfindService _pathfind;
    private AvatarController _player;
    private List<Vector2Int> _path = new List<Vector2Int>();
    [HideInInspector] public Vector2 _aimDirection = Vector2.right;
    private Vector2 _avoidanceOffset = Vector2.zero;
    private float _lockDirectionTimer = 0f;
    private Vector3 _lockedAccel = Vector3.zero;
    private Vector2 _debugTestPoint = Vector2.zero;
	private Transform spriteRendererTransform;

	void Awake() {
		spriteRendererTransform = transform.Find("Renderer");
	}

	//CHRISC
	struct GridPoint
    {
        public int x;
        public int y;

        public GridPoint(int newX, int newY)
        {
            x = newX;
            y = newY;
        }
    }
    private List<List<Vector2>> m_LocalGridPositions = new List<List<Vector2>>();
    private List<List<Vector2>> m_WorldGridPositions = new List<List<Vector2>>();
    public List<List<int>> m_CollisionGridTests = new List<List<int>>();
    //MAKE PUBLIC LATER!
    private int CHRIS_xPoints = 13;
    private int CHRIS_yPoints = 13;
    private float CHRIS_xDistanceBetweenPoints = 2.0f;
    private float CHRIS_yDistanceBetweenPoints = 2.0f;
    private Vector2 CHRIS_gridOffset = new Vector2(0.0f, 4.0f);
    //Debug TEST
    //private Collider2D[] asteroidsInGridToDraw;

    List<GridPoint> CHRIS_GetNeighbors(GridPoint myGridPosition)
    {
        List<GridPoint> GridPositions = new List<GridPoint>();
        
        //West
        if (myGridPosition.x != 0)
        {
            GridPoint gridPoint = new GridPoint(myGridPosition.x - 1, myGridPosition.y);
            GridPositions.Add(gridPoint);
        }
        //North West
        if (myGridPosition.x != 0 && myGridPosition.y != 0)
        {
            GridPoint gridPoint = new GridPoint(myGridPosition.x - 1, myGridPosition.y - 1);
            GridPositions.Add(gridPoint);
        }
        //North
        if (myGridPosition.y != 0)
        {
            GridPoint gridPoint = new GridPoint(myGridPosition.x, myGridPosition.y - 1);
            GridPositions.Add(gridPoint);
        }
        //North East
        if (myGridPosition.x != (CHRIS_xPoints - 1) && myGridPosition.y != 0)
        {
            GridPoint gridPoint = new GridPoint(myGridPosition.x + 1, myGridPosition.y - 1);
            GridPositions.Add(gridPoint);
        }
        // East
        if (myGridPosition.x != (CHRIS_xPoints - 1))
        {
            GridPoint gridPoint = new GridPoint(myGridPosition.x + 1, myGridPosition.y);
            GridPositions.Add(gridPoint);
        }
        //South East
        if (myGridPosition.x != (CHRIS_xPoints - 1) && myGridPosition.y != (CHRIS_yPoints - 1))
        {
            GridPoint gridPoint = new GridPoint(myGridPosition.x + 1, myGridPosition.y + 1);
            GridPositions.Add(gridPoint);
        }
        //South
        if (myGridPosition.y != (CHRIS_yPoints - 1))
        {
            GridPoint gridPoint = new GridPoint(myGridPosition.x, myGridPosition.y + 1);
            GridPositions.Add(gridPoint);
        }
        //South West
        if (myGridPosition.x != 0 && myGridPosition.y != (CHRIS_yPoints - 1))
        {
            GridPoint gridPoint = new GridPoint(myGridPosition.x - 1, myGridPosition.y + 1);
            GridPositions.Add(gridPoint);
        }

        return GridPositions;
    }
    // Start is called before the first frame update
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _player = FindObjectOfType<AvatarController>();

        //CHRISC
        for (int x = 0; x < CHRIS_xPoints; ++x)
        {
            List<Vector2> columnLocalPositions = new List<Vector2>();
            List<Vector2> columnWorldPositions = new List<Vector2>();
            List<int> columnCollisionTests = new List<int>();
            for (int y = 0; y < CHRIS_yPoints; ++y)
            {
                columnLocalPositions.Add(new Vector2(((float)x - (float)(CHRIS_xPoints-1) / 2.0f) * CHRIS_xDistanceBetweenPoints, ((float)y - (float)(CHRIS_yPoints-1) / 2.0f) * CHRIS_yDistanceBetweenPoints));
                columnWorldPositions.Add(Vector2.zero);
                columnCollisionTests.Add(0);
            }
            m_LocalGridPositions.Add(columnLocalPositions);
            m_WorldGridPositions.Add(columnWorldPositions);
            m_CollisionGridTests.Add(columnCollisionTests);
        }

        _pathfind = GetComponent<PathfindService>();
        if (_pathfind)
            _pathfind.m_Grid = m_CollisionGridTests;
    }

	void Update() {
		Vector2 bodyAim = (_player.transform.position - transform.position);
		spriteRendererTransform.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.up, bodyAim));
	}

	public bool CanShoot()
    {
        Vector3 shipPosition = transform.position;
        Vector3 playerPosition = _player.transform.position;
        Vector3 toTarget = (playerPosition - shipPosition);

        // test range
        float minShootRange = _idealDistanceToTarget * _idealDistanceToTarget * 0.8f;
        float maxShootRange = _idealDistanceToTarget * _idealDistanceToTarget * 1.2f;
        float distSqrToTarget = toTarget.sqrMagnitude;

        return distSqrToTarget <= maxShootRange && distSqrToTarget >= minShootRange;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //if (_path.Count > 12)
        //{
        //    _rigidbody.velocity = Vector2.zero;
        //    return;
        //}

        //CHRISC
        //Vector3 targetPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector3 shipPosition = transform.position;
        Vector3 playerPosition = _player.transform.position;
        Vector3 toTarget = (playerPosition - shipPosition);
        Vector3 targetPos = playerPosition - toTarget.normalized * _idealDistanceToTarget;
        _aimDirection = toTarget.normalized;

        Vector3 directionVector = (targetPos - shipPosition).normalized;
        //Vector3 shipDirection = (Vector3)GetComponent<Rigidbody2D>().velocity.normalized;
        float angle = Mathf.Atan2(directionVector.x, directionVector.y) * Mathf.Rad2Deg;
        Quaternion gridRotation = Quaternion.AngleAxis(-angle, Vector3.forward);
        Vector2 size = new Vector2(CHRIS_xPoints * CHRIS_xDistanceBetweenPoints, CHRIS_yPoints * CHRIS_yDistanceBetweenPoints);
        LayerMask mask = 1 << 7 | 1 << 10; // 7 = asteroid layer, 10 = AI
        Collider2D[] asteroidsInGrid = Physics2D.OverlapBoxAll((Vector2)shipPosition + (Vector2)(gridRotation * (Vector3)CHRIS_gridOffset), size, -angle, mask); // 7 = asteroid layer
        //asteroidsInGridToDraw = Physics2D.OverlapBoxAll((Vector2)transform.position + (Vector2)(gridRotation * (Vector3)CHRIS_gridOffset), size, -angle, 1 << 7); // 7 = asteroid layer

        //Debug.Log(angle);
        //m_LocalGridPositions
        //m_WorldGridPositions
        //m_CollisionGridTests

        
        //CHRISC
        for (int x = 0; x < CHRIS_xPoints; ++x)
        {
            for (int y = 0; y < CHRIS_yPoints; ++y)
            {
                m_CollisionGridTests[x][y] = 0;
                m_WorldGridPositions[x][y] = (Vector2)(gridRotation * (m_LocalGridPositions[x][y] + CHRIS_gridOffset)) + (Vector2)transform.position;
            }
        }

        //CHRISC
        foreach (Collider2D colliderHit in asteroidsInGrid)
        {
            // avoid self
            if (colliderHit.transform == transform)
                continue;

            // ignore small asteroids
            GK.BreakableSurface breakable = colliderHit.transform.GetComponent<GK.BreakableSurface>();
            if (breakable)
            {
                float area = breakable.Area;
                if (area <= _minAsteroidSizeToCareAbout)
                    continue;
            }

            for (int x = 0; x < CHRIS_xPoints; ++x)
            {
                for (int y = 0; y < CHRIS_yPoints; ++y)
                {
                    if (m_CollisionGridTests[x][y] == 0)
                    {
                        m_CollisionGridTests[x][y] = colliderHit.OverlapPoint(m_WorldGridPositions[x][y]) ? 1 : 0;
                    }
                }
            }
        }


        for (int x = 0; x < CHRIS_xPoints; ++x)
        {
            for (int y = 0; y < CHRIS_yPoints; ++y)
            {
                if (m_CollisionGridTests[x][y] == 1)
                {
                    if (IsCoordValid(new Vector2Int(x-1, y)) && m_CollisionGridTests[x-1][y] != 1) m_CollisionGridTests[x-1][y] = 2;
                    if (IsCoordValid(new Vector2Int(x, y-1)) && m_CollisionGridTests[x][y-1] != 1) m_CollisionGridTests[x][y-1] = 2;
                    if (IsCoordValid(new Vector2Int(x+1, y)) && m_CollisionGridTests[x+1][y] != 1) m_CollisionGridTests[x+1][y] = 2;
                    if (IsCoordValid(new Vector2Int(x, y+1)) && m_CollisionGridTests[x][y+1] != 1) m_CollisionGridTests[x][y+1] = 2;
                }
            }
        }

        bool canUpdateAccel = false;
        if (_lockDirectionTimer >= 0f)
        {
            _lockDirectionTimer -= Time.fixedDeltaTime;
            if (_lockDirectionTimer <= 0f)
            {            
                _lockDirectionTimer = 0.2f; // small timer for less wobbles
                canUpdateAccel = true;
            }
        }

        if (canUpdateAccel)
        {
            Vector3 destination = ComputeNextPathDestination(targetPos);
            Vector2 accel = Helper.ComputeAccelerationToReachDestination(destination, transform, _maxSpeed, _brakeDistance, _rigidbody, _timeToReachDesiredVelocity);

            if (accel.magnitude > _maxAcceleration)
            {
                accel.Normalize();
                accel *= _maxAcceleration;
            }

            _lockedAccel = accel;
        }

        _rigidbody.AddForce(_lockedAccel);
    }

    bool IsCoordValid(Vector2Int coord)
    {
        if (coord.x < 0 || coord.x >= m_WorldGridPositions.Count
         || coord.y < 0 || coord.y >= m_WorldGridPositions[0].Count)
            return false;

        return true;
    }

    private Vector2 ComputeNextPathDestination(Vector2 destination)
    {
        if (!_pathfind)
            return Vector2.zero;

        _path.Clear();
        Vector2Int start = WorldToGrid(transform.position, true);
        Vector2Int goal =  WorldToGrid(destination, true);
        bool success = _pathfind.ComputeAStarPath(start, goal, _path);

        if (!success || _path.Count == 0)
        {
            if (!success) Debug.Log(start + " to " + goal + " failed");
            return transform.position;
        }

        Vector2 nextPathPos = GridToWorld(_path[0]);
        _debugTestPoint = nextPathPos;
        //Debug.Log(start + " to " + goal + " next " + nextPathPos + " pathSize " + path.Count);
        return nextPathPos;
    }
    
    private Vector2 GridToWorld(Vector2Int gridCoord)
    {
        if (gridCoord.x < 0 || gridCoord.x >= m_WorldGridPositions.Count
         || gridCoord.y < 0 || gridCoord.y >= m_WorldGridPositions[0].Count)
        {
            Debug.LogWarning("Invalid grid coord " + gridCoord);
            return Vector2.zero;
        }

        return m_WorldGridPositions[gridCoord.x][gridCoord.y];
    }

    private Vector2Int WorldToGrid(Vector2 worldPos, bool freeSpaceOnly)
    {
        Vector2Int closestGrid = Vector2Int.zero;
        float closestDistSqr = float.MaxValue;

        // there's for sure a much smarter way to do this using x/y grid size but this will work
        for (int x = 0; x < CHRIS_xPoints; ++x)
        {
            for (int y = 0; y < CHRIS_yPoints; ++y)
            {
                if (freeSpaceOnly && m_CollisionGridTests[x][y] != 0) // true means there's an obstacle
                    continue;

                float distSqr = (worldPos - m_WorldGridPositions[x][y]).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    closestGrid = new Vector2Int(x, y);
                }
            }
        }

        return closestGrid;
    }

    float Project(Vector2 a, Vector2 b)
    {
        // projection of b onto a = (A dot B) / mag(A)
        return Vector2.Dot(a, b) / a.magnitude;
    }

    public void OnDrawGizmos()
    {
        // don't run in editor
        if (_player == null)
            return;
        
        Vector3 shipVelocity = (Vector3)GetComponent<Rigidbody2D>().velocity;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + (Vector3)_avoidanceOffset, 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + shipVelocity);

        if (_debugTestPoint != Vector2.zero)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(_debugTestPoint, 0.6f);
        }

        Gizmos.color = Color.white;
        //Vector3 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector3 targetPos = _player.transform.position;
        Gizmos.DrawLine(targetPos, (Vector2)transform.position);

        for (int x = 0; x < m_CollisionGridTests.Count; ++x)
        {
            for (int y = 0; y < m_CollisionGridTests[x].Count; ++y)
            {
                if (m_CollisionGridTests[x][y] == 1)
                {
                    Gizmos.color = Color.red;
                }
                else if (m_CollisionGridTests[x][y] == 2)
                {
                    Gizmos.color = new Color(1, 0.5f, 0);
                }
                else
                {
                    Gizmos.color = Color.white;
                }

                if (m_WorldGridPositions.Count != 0)
                    Gizmos.DrawCube(m_WorldGridPositions[x][y], new Vector3(0.5f, 0.5f, 0.0f));
                //Gizmos.DrawLine(m_TestPoints[x][y] + (Vector2)transform.position, (Vector2)transform.position);
            }
        }

        /*
        Gizmos.color = Color.red;
        foreach (Collider2D colliderHit in asteroidsInGridToDraw)
        {
            Gizmos.DrawCube(colliderHit.transform.position, new Vector3(1.0f, 1.0f, 0.0f));
        }
        */

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)_aimDirection * 20f);

        // draw path
        Gizmos.color = Color.green;
        Vector3 prevPos = transform.position;
        foreach (Vector2Int point in _path)
        {
            Vector3 pos = m_WorldGridPositions[point.x][point.y];
            Gizmos.DrawWireSphere(pos, 0.6f);
            Gizmos.DrawLine(prevPos, pos);
            prevPos = pos;
        }

        Gizmos.color = Color.red;
        Vector3 directionVector = (targetPos - transform.position).normalized;
        float angle = Mathf.Atan2(directionVector.x, directionVector.y) * Mathf.Rad2Deg;
        Vector2 size = new Vector2(CHRIS_xPoints * CHRIS_xDistanceBetweenPoints, CHRIS_yPoints * CHRIS_yDistanceBetweenPoints);

        Gizmos.matrix = Matrix4x4.TRS(transform.position, Quaternion.AngleAxis(-angle, Vector3.forward), Vector3.one);
        Gizmos.DrawWireCube(CHRIS_gridOffset, size);
    }
}