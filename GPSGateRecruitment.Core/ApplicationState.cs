using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace GPSGateRecruitment.Common;

/// <summary>
/// This class stores the points and delegates computing paths between them. Not thread-safe.
/// </summary>
public class ApplicationState
{
    /// <summary>
    /// Raised when the path between two points has been computed. Handler receives the path as a list of points
    /// </summary>
    public event EventHandler<IEnumerable<Position>> LineCreated;

    /// <summary>
    /// Raised when pathfinding between two points has failed. Handler receives the exception
    /// </summary>
    public event EventHandler<Exception> PathFindingFailed;
    
    private readonly IPathFinder _pathFinder;
    
    // Doesn't need to be thread-safe, because paths need to be computed sequentially anyway. Otherwise the lines could cross.
    private readonly Queue<Position> _pointsRequested = new();

    public ApplicationState(IPathFinder pathFinder)
    {
        _pathFinder = pathFinder;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pointPosition"></param>
    public void AddPoint(Position pointPosition)
    {
        _pointsRequested.Enqueue(pointPosition);

        if (_pointsRequested.Count >= 2)
        {
            var startPoint = _pointsRequested.Dequeue();
            var endPoint = _pointsRequested.Dequeue();
            var pathFindingTask = Task.Run(() => _pathFinder.FindPath(startPoint, endPoint)); // intentionally not awaiting

            pathFindingTask.ContinueWith(task =>
            {
                PathFindingFailed?.Invoke(this, task.Exception.InnerException);
            }, TaskContinuationOptions.OnlyOnFaulted);

            pathFindingTask.ContinueWith(task =>
            {
                LineCreated?.Invoke(this, task.Result);
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
    
}