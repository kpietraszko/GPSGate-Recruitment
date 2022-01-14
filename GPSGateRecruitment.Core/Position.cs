using System;
using System.Collections.Generic;

namespace GPSGateRecruitment.Common;

public record Position(int X, int Y)
{
    // Distance calculated as magnitude of the vector "target minus this"
    public float DistanceTo(Position target) => (float)Math.Sqrt(Math.Pow(target.X - this.X, 2) + Math.Pow(target.Y - this.Y, 2));
    
    public IEnumerable<Position> GetNeighbours()
    {
        for (var neighbourXOffset = -1; neighbourXOffset <= 1; neighbourXOffset++)
        {
            for (var neighbourYOffset = -1; neighbourYOffset <= 1; neighbourYOffset++)
            {
                if (neighbourXOffset == 0 && neighbourYOffset == 0)
                    continue; // ignore the node itself

                yield return new Position(this.X + neighbourXOffset, this.Y + neighbourYOffset);
            }
        }
    }

    public override string ToString() => $"({X},{Y})";
}