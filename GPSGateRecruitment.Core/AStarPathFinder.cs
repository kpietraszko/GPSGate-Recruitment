using System;
using System.Collections.Generic;
using System.Threading;

namespace GPSGateRecruitment.Common;

// Based on pseudo-code here: https://en.wikipedia.org/wiki/A*_search_algorithm
// TODO: Could be optimized by extending this with Jump Point Search method, which skips empty regions
public class AStarPathFinder : IPathFinder
{
    private readonly int _gridWidth;
    private readonly int _gridHeight;
    
    // <nodeIndex, fScore(priority)>
    private PriorityQueue<int, float> _openSet;
    private readonly Dictionary<int, int> _cameFrom;

    public AStarPathFinder(int gridWidth, int gridHeight)
    {
        _gridWidth = gridWidth;
        _gridHeight = gridHeight;
        _cameFrom = new Dictionary<int, int>(_gridWidth * _gridHeight);
    }

    public Position[] FindPath(Position start, Position end)
    {
        Thread.Sleep(5000);
        
        _openSet.Enqueue(GetNodeIndex(start), float.MaxValue);
        
        // <nodeIndex, nodeIndex of parent node>

        // g score is the cost of the path from start to current node
        var gScorePerNode = new Dictionary<int, float>(_gridWidth * _gridHeight);
        gScorePerNode[GetNodeIndex(start)] = 0;
        
        // f score is g score + heuristic (estimate of cost from current node to end node)
        var fScorePerNode = new Dictionary<int, float>(_gridWidth * _gridHeight);
        fScorePerNode[GetNodeIndex(start)] = GetHeuristic(start, end);

        while (_openSet.TryDequeue(out var currentNodeIndex, out _))
        {
            if (currentNodeIndex == GetNodeIndex(end))
            {
                // Found path, go through parents backward from end to retrieve it
                return ReconstructPath(currentNodeIndex);
            }
        }

        return new Position[0];
    }

    private Position[] ReconstructPath(int currentNodeIndex)
    {
        throw new NotImplementedException();
    }

    private float GetHeuristic(Position from, Position to) => GetManhattanHeuristic(from, to);

    // Manhattan-distance heuristic
    private float GetManhattanHeuristic(Position from, Position to) => Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);

    // This maps a 2D position to the index identifying the node, as if you were flattening a 2D array
    private int GetNodeIndex(int x, int y) => x + y * _gridWidth;
    private int GetNodeIndex(Position pos) => GetNodeIndex(pos.X, pos.Y);

    private IEnumerable<int> GetNeighboursIndices(Position pos)
    {
        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue; // ignore node itself

                yield return GetNodeIndex(x, y);
            }
        }
    }
}