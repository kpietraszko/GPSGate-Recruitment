using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using GPSGateRecruitment.Common.Extensions;

namespace GPSGateRecruitment.Common;

// Based on https://en.wikipedia.org/wiki/A*_search_algorithm
// TODO: Could be optimized by extending this with Jump Point Search method, which skips empty regions
public class AStarPathFinder : IPathFinder
{
    private readonly int _gridWidth;
    private readonly int _gridHeight;
    private HashSet<Point> _obstacles = new();

    public AStarPathFinder(int gridWidth, int gridHeight)
    {
        _gridWidth = gridWidth;
        _gridHeight = gridHeight;
    }
    
    public IEnumerable<Point> FindPath(Point start, Point end)
    {
        if (start == end)
        {
            return new[] { start };
        }

        ValidateStartAndEndPoints(start, end);

        // <node, parent node of that node>
        var cameFrom = new Dictionary<Point, Point>(_gridWidth * _gridHeight);

        // g score is the lowest currently known cost of the path from start to this node
        var gScorePerNode = new Dictionary<Point, float>(_gridWidth * _gridHeight);
        gScorePerNode[start] = 0;

        // using this for O(1) lookup
        var openNodesPerFScore = new SortedDictionary<float, List<Point>>();
        openNodesPerFScore.AddToList(float.MaxValue, start);

        while (openNodesPerFScore.Any())
        {
            var currentNode = openNodesPerFScore.First().Value.First(); // first node in the list of nodes with the smallest f score
            var currentNodeFScore = openNodesPerFScore.First().Key; // only necessary for removal
            
            if (currentNode == end)
            {
                // Found path, go through parents backwards from end to retrieve it
                // TODO: remember to store found path as obstacle for the future
                var path = ReconstructPath(currentNode, cameFrom);
                SavePathAsObstacle(path);
                return path;
            }

            openNodesPerFScore.RemoveFromList(currentNodeFScore, currentNode);

            foreach (var neighbour in currentNode.GetNeighbours())
            {
                if (_obstacles.Contains(neighbour))
                {
                    continue;
                }
                
                var tentativeGScore = gScorePerNode[currentNode] + currentNode.DistanceTo(neighbour);
                var gScoreBetterThanCurrentlySaved = !gScorePerNode.ContainsKey(neighbour) || tentativeGScore < gScorePerNode[neighbour];
                
                if (gScoreBetterThanCurrentlySaved)
                {
                    cameFrom[neighbour] = currentNode;
                    gScorePerNode[neighbour] = tentativeGScore;
                    var fScore = tentativeGScore + GetHeuristic(neighbour, end);
                    openNodesPerFScore.AddToList(fScore, neighbour);
                }
            }
        }

        // open set is empty but end point wasn't reached
        throw new ArgumentException($"Impossible to find path between {start} and {end}");
    }

    private void ValidateStartAndEndPoints(Point start, Point end)
    {
        if (_obstacles.Contains(start))
        {
            throw new ArgumentException("Start point can't be placed on an existing line");
        }

        if (_obstacles.Contains(end))
        {
            throw new ArgumentException("End point can't be placed on an existing line");
        }
    }

    private IEnumerable<Point> ReconstructPath(Point currentNode, Dictionary<Point, Point> cameFrom)
    {
        yield return currentNode;
        
        // this returns the path from end to start, but doesn't really matter
        while(cameFrom.TryGetValue(currentNode, out var parent))
        {
            yield return currentNode;
            currentNode = parent;
        }
    }
    
    private void SavePathAsObstacle(IEnumerable<Point> path)
    {
        foreach (var point in path)
        {
            _obstacles.Add(point);
            
            // Adding additional border around the path to 1. make sure diagonal paths don't cross each other
            // and 2. make close paths more discernible
            foreach (var neighbour in point.GetNeighbours())
            {
                _obstacles.Add(neighbour);
            }
        }
    }

    private float GetHeuristic(Point from, Point to) => GetManhattanHeuristic(from, to);

    // Manhattan-distance heuristic
    private float GetManhattanHeuristic(Point from, Point to) => Math.Abs(from.X - to.X) + Math.Abs(from.Y - to.Y);
}