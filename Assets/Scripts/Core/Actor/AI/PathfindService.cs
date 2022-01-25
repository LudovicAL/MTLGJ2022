using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class PathfindService : MonoBehaviour
{
    public List<List<int>> m_Grid;

    // for pathfinding
    private List<AStarTile> m_OpenTiles = new List<AStarTile>();
    private HashSet<Vector2Int> m_ClosedTiles = new HashSet<Vector2Int>();
    
    static Vector2Int
    LEFT = new Vector2Int(-1, 0),
    RIGHT = new Vector2Int(1, 0),
    DOWN = new Vector2Int(0, -1),
    DOWNLEFT = new Vector2Int(-1, -1),
    DOWNRIGHT = new Vector2Int(1, -1),
    UP = new Vector2Int(0, 1),
    UPLEFT = new Vector2Int(-1, 1),
    UPRIGHT = new Vector2Int(1, 1);

    static Vector2Int[] directions = 
      { LEFT, RIGHT, DOWN, DOWNLEFT, DOWNRIGHT, UP, UPLEFT, UPRIGHT };

    public void FindNeighbours(in Vector2Int _node, List<Vector2Int> _outNeighbors, HashSet<Vector2Int> _excludedTiles = null) {
        _outNeighbors.Clear();
        foreach (var direction in directions) 
        {
            Vector2Int neighbourCell = _node + direction;
            if (neighbourCell.x < 0 || neighbourCell.x >= m_Grid.Count)
                continue;

            if (neighbourCell.y < 0 || neighbourCell.y >= m_Grid[0].Count)
                continue;

            if (m_Grid[neighbourCell.x][neighbourCell.y] != 0) // true means obstacle 
                continue;

            if (_excludedTiles != null && _excludedTiles.Contains(neighbourCell))
                continue; 

            _outNeighbors.Add(neighbourCell);
        }
    }

    public static int CellDistance(Vector2Int _cellA, Vector2Int _cellB)
    {
        Vector2Int toTarget = _cellB - _cellA;
        return Mathf.Abs(toTarget.x) + Mathf.Abs(toTarget.y);
    }

    class AStarTile
    {
        public Vector2Int Coord { get; set; }
        public int Cost { get; set; }
        public int Distance { get; set; }
        public int CostDistance => Cost + Distance; // heuristic, try to move closer to goal
        public AStarTile Parent { get; set; }

        //The distance is essentially the estimated distance, ignoring walls to our target. 
        //So how many tiles left and right, up and down, ignoring walls, to get there. 
        public void SetDistance(in Vector2Int _targetCoord)
        {
            Vector2Int toTarget = Coord - _targetCoord;
            this.Distance = CellDistance(Coord, _targetCoord);
        }
    }

    // based on https://dotnetcoretutorials.com/2020/07/25/a-search-pathfinding-algorithm-in-c/
    public bool ComputeAStarPath(Vector2Int _start, Vector2Int _finish, List<Vector2Int> _outPath, HashSet<Vector2Int> _excludedTiles = null)
    {
        _outPath.Clear();
        if (_start == _finish)
            return true;

        AStarTile startTile = new AStarTile();
        startTile.Coord = _start;
        startTile.SetDistance(_finish);
        m_OpenTiles.Add(startTile);

        List<Vector2Int> neighbours = new List<Vector2Int>();
        while (m_OpenTiles.Count != 0)
        {
            AStarTile tile = m_OpenTiles.OrderBy(x => x.CostDistance).First();

            if (tile.Coord == _finish) // done!
            {
                // cleanup
                m_OpenTiles.Clear();
                m_ClosedTiles.Clear();

                // assemble path
                while (tile.Parent != null)
                {
                    _outPath.Add(tile.Coord);
                    tile = tile.Parent;
                }
                _outPath.Reverse();
                return true;
            }

            m_ClosedTiles.Add(tile.Coord);
            m_OpenTiles.RemoveAt(0);

            FindNeighbours(tile.Coord, neighbours, _excludedTiles);
            for (int i = 0; i < neighbours.Count; ++i)
            {
                Vector2Int neighbour = neighbours[i];
                
                if (m_ClosedTiles.Contains(neighbour))
                    continue; // already explored

                int newCost = tile.Cost + 1;

                // see if entry already exists and if we found a shorter path to it
                int existingPathInfoIndex = m_OpenTiles.FindIndex(x => x.Coord == neighbour);
                if (existingPathInfoIndex != -1)
                {
                    if (m_OpenTiles[existingPathInfoIndex].Cost > newCost)
                    {
                        m_OpenTiles[existingPathInfoIndex].Cost = newCost;
                    }
                    continue;
                }

                // add entry to explore
                AStarTile newTile = new AStarTile();
                newTile.Coord = neighbour;
                newTile.Cost = newCost;
                newTile.Parent = tile;
                newTile.SetDistance(_finish);
                m_OpenTiles.Add(newTile);
            }
        }
        
        m_OpenTiles.Clear();
        m_ClosedTiles.Clear();
        return false;
    }
}
