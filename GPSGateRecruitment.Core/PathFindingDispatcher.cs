﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;

namespace GPSGateRecruitment.Common;

/// <summary>
/// This class stores the points and delegates computing paths between them. Not thread-safe, so call from a single thread
/// </summary>
public class PathFindingDispatcher
{
    /// <summary>
    /// Raised when the path between two points has been computed. Handler receives the path as a list of points
    /// </summary>
    public event EventHandler<IEnumerable<Point>> LineCreated;

    /// <summary>
    /// Raised when pathfinding between two points has failed. Handler receives the exception
    /// </summary>
    public event EventHandler<Exception> PathFindingFailed;

    /// <summary>
    /// Raised when starting to wait for the start point
    /// </summary>
    public event EventHandler WaitingForStartPoint;
    
    /// <summary>
    /// Raised when starting to wait for the end point
    /// </summary>
    public event EventHandler WaitingForEndPoint;
    
    private readonly IPathFinder _pathFinder;
    
    // Doesn't need to be thread-safe, because paths need to be computed sequentially anyway. Otherwise the lines could cross.
    private readonly Queue<Point> _pointsRequested = new();

    public PathFindingDispatcher(IPathFinder pathFinder)
    {
        _pathFinder = pathFinder;
        WaitingForStartPoint?.Invoke(this, EventArgs.Empty);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pointPoint"></param>
    public void AddPoint(Point pointPoint)
    {
        _pointsRequested.Enqueue(pointPoint);

        if (_pointsRequested.Count % 2 == 0)
        {
            WaitingForStartPoint?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            WaitingForEndPoint?.Invoke(this, EventArgs.Empty);   
        }

        DispatchPathFindingIfReady();
    }

    private void DispatchPathFindingIfReady()
    {
        if (_pointsRequested.Count >= 2)
        {
            var startPoint = _pointsRequested.Dequeue();
            var endPoint = _pointsRequested.Dequeue();
            var pathFindingTask = Task.Run(() => _pathFinder.FindPath(startPoint, endPoint)); // intentionally not awaiting

            pathFindingTask.ContinueWith(task =>
                {
                    PathFindingFailed?.Invoke(this, task.Exception.InnerException);
                },
                default, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.FromCurrentSynchronizationContext());

            pathFindingTask.ContinueWith(task =>
                {
                    LineCreated?.Invoke(this, task.Result);
                },
                default, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.FromCurrentSynchronizationContext());
            
            // in case points were enqueued while pathfinding was in progress
            pathFindingTask.ContinueWith(_ => DispatchPathFindingIfReady());
        }
    }
}