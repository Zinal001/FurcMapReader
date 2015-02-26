using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zinal.FurcMapReader
{
    public enum WallPosition
    {
        NorthWest,
        NorthEast
    }

    public class MapTile
    {
        #region Private Variables
        private ushort floornumber, objectnumber, wallnenumber, wallnwnumber, regionnumber, effectnumber, exactwallnumber;
        private Map map;
        #endregion

        #region Public Variables

        /// <summary>
        /// Get a value determening if this tile position has a North-Western wall or a North-Eastern wall
        /// </summary>
        public WallPosition WallPositionOnTile
        {
            get
            {
                if (X % 2 == 0)
                    return WallPosition.NorthWest;

                return WallPosition.NorthEast;
            }
        }

        /// <summary>
        /// Get or Set the floor number on this tile
        /// </summary>
        public ushort FloorNumber
        {
            get { return floornumber; }
            set
            {
                floornumber = value;
                map.internal_setFloorAt(this.X, this.Y, value);
            }
        }

        /// <summary>
        /// Get or Set the Object number on this tile
        /// </summary>
        public ushort ObjectNumber
        {
            get { return objectnumber; }
            set
            {
                objectnumber = value;
                map.internal_setObjectAt(this.X, this.Y, value);
            }
        }

        /// <summary>
        /// Get or Set the North-Eastern wall of this tile
        /// </summary>
        public ushort WallNENumber
        {
            get { return wallnenumber; }
            set
            {
                wallnenumber = value;
                map.internal_setWallAt(this.X, this.Y, value);
            }
        }

        /// <summary>
        /// Get or Set the North-Western wall of this tile
        /// </summary>
        public ushort WallNWNumber
        {
            get { return wallnwnumber; }
            set
            {
                wallnwnumber = value;
                map.internal_setWallAt(this.X, this.Y, value);
            }
        }

        /// <summary>
        /// Get or Set the Region number of this tile
        /// </summary>
        public ushort RegionNumber
        {
            get { return regionnumber; }
            set
            {
                regionnumber = value;
                map.internal_setRegionAt(this.X, this.Y, value);
            }
        }

        /// <summary>
        /// Get or Set the Effect number of this tile
        /// </summary>
        public ushort EffectNumber
        {
            get { return effectnumber; }
            set
            {
                effectnumber = value;
                map.internal_setEffectAt(this.X, this.Y, value);
            }
        }

        /// <summary>
        /// Get or Set the wall on this position
        /// (X position determinds where the wall should be placed)
        /// </summary>
        public ushort RealWallNumber
        {
            get
            {
                if (this.X % 2 == 0)
                    return this.WallNWNumber;
                else
                    return this.WallNENumber;
            }
            set
            {
                if (this.X % 2 == 0)
                {
                    this.wallnwnumber = value;
                    map.internal_setWallAt(this.X + 1, this.Y, value);
                }
                else
                {
                    this.wallnenumber = value;
                    map.internal_setWallAt(this.X - 1, this.Y, value);
                }
            }
        }

        /// <summary>
        /// Get the X position of this tile
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Get the X position of this tile
        /// </summary>
        public int Y { get; private set; }
        #endregion

        /// <summary>
        /// Create a new MapTile object
        /// </summary>
        /// <param name="x">The X position of this tile</param>
        /// <param name="y">The Y position of this tile</param>
        /// <param name="map">The Map this MapTile belongs to</param>
        public MapTile(int x, int y, Map map)
        {
            this.map = map;
            this.X = x;
            this.Y = y;
            this.FloorNumber = map.GetFloorAt(x, y);
            this.ObjectNumber = map.GetObjectAt(x, y);
            this.RegionNumber = map.GetRegionAt(x, y);
            this.EffectNumber = map.GetEffectAt(x, y);

            if (x % 2 == 0)
            {
                this.WallNENumber = map.GetWallAt(x + 1, y);
                this.WallNWNumber = map.GetWallAt(x, y);
            }
            else
            {
                this.WallNENumber = map.GetWallAt(x, y);
                this.WallNWNumber = map.GetWallAt(x - 1, y);
            }
        }
    }
}
