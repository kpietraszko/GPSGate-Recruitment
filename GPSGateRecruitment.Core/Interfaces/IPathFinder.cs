using System.Collections.Generic;

namespace GPSGateRecruitment.Core;

public interface IPathFinder
{
    /// <summary>
    ///     Find a path between 2 given points
    /// </summary>
    /// <param name="start">Start point</param>
    /// <param name="end">End point</param>
    /// <returns>Array of points defining the path</returns>
    public IEnumerable<Point> FindPath(Point start, Point end);
}