using System;
using System.Collections.Generic;

namespace GPSGateRecruitment.Core.Extensions;

public static class PointExtensions
{
    /// <returns>Distance between 2 points</returns>
    /// <remarks>Distance calculated as magnitude of the vector "target minus this"</remarks>
    public static float DistanceTo(this Point thisPoint, Point target) =>
        (float)Math.Sqrt(Math.Pow(target.X - thisPoint.X, 2) + Math.Pow(target.Y - thisPoint.Y, 2));

    /// <returns>Enumerable of 8 neighbours around the given point</returns>
    public static IEnumerable<Point> GetNeighbours(this Point thisPoint)
    {
        for (var neighbourXOffset = -1; neighbourXOffset <= 1; neighbourXOffset++)
        {
            for (var neighbourYOffset = -1; neighbourYOffset <= 1; neighbourYOffset++)
            {
                if (neighbourXOffset == 0 && neighbourYOffset == 0)
                    continue; // ignore the node itself

                yield return new Point(thisPoint.X + neighbourXOffset, thisPoint.Y + neighbourYOffset);
            }
        }
    }
}