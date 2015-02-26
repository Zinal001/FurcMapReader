using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zinal.FurcMapReader
{
    public class MapPosition
    {
        public ushort FloorNumber { get; private set; }
        public ushort ObjectNumber { get; private set; }
        public ushort WallNumber { get; private set; }
        public ushort RegionNumber { get; private set; }
        public ushort EffectNumber { get; private set; }

        public int X { get; private set; }
        public int Y { get; private set; }

        public MapPosition(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public MapPosition(int x, int y, Map map)
        {
            this.X = x;
            this.Y = y;
            this.FloorNumber = map.GetFloorAt(x, y);
            this.ObjectNumber = map.GetObjectAt(x, y);
            this.WallNumber = map.GetWallAt(x, y);
            this.RegionNumber = map.GetRegionAt(x, y);
            this.EffectNumber = map.GetEffectAt(x, y);
        }
    }
}
