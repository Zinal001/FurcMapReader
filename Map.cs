using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Zinal.FurcMapReader
{
    public class Map
    {
        #region Private Static Variables
        private static readonly char[] headerTrimChars = new char[] { '\n', '\t', '\0' };

        #endregion

        #region Private Variables
        private List<String> headerLines = new List<String>();
        private Dictionary<String, String> mapData = new Dictionary<String, String>();

        private String name, patchs, rating;
        private int width, height, revision, patcht;
        private bool allowjs, allowlf, allowfurl, nowho, forcesittable, allowshouts, allowlarge, notab, nonovelty, swearfilter, parentalcontrols, encoded;

        private byte[] mapMatrix, floors, objects, walls, regions, effects;

        private MapTile[,] tiles;
        #endregion

        #region Internal Variables
        internal int bytesLayerCount
        {
            get
            {
                return width * height * 2;
            }
        }
        #endregion
        
        #region Public Variables

        /// <summary>
        /// Get or Set a MapTile object on this Map
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public MapTile this[int x, int y]
        {
            get
            {
                return this.GetMapTile(x, y);
            }
            set
            {
                this.SetObjectAt(value.X, value.Y, value.ObjectNumber);
                this.SetFloorAt(value.X, value.Y, value.FloorNumber);
                this.SetWallAt(value.X, value.Y, value.RealWallNumber);
                this.SetRegionAt(value.X, value.Y, value.RegionNumber);
                this.SetEffectAt(value.X, value.Y, value.EffectNumber);
                this.tiles[x, y] = value;
            }
        }

        /// <summary>
        /// Get the actual width of the map
        /// </summary>
        public int Width
        {
            get { return this.width * 2; }
        }

        /// <summary>
        /// Get the actual height of the map
        /// </summary>
        public int Height
        {
            get { return this.height; }
        }

        /// <summary>
        /// Get or set if this map should use a patch
        /// </summary>
        public PatchSetting UsePatch
        {
            get { return (PatchSetting)this.patcht; }
            set { this.patcht = (int)value; }
        }

        /// <summary>
        /// Allow joining and summoning inside on map
        /// </summary>
        public bool AllowJoinSummon
        {
            get { return this.allowjs; }
            set { this.allowjs = value; }
        }

        /// <summary>
        /// Allow leading and following inside on map
        /// </summary>
        public bool AllowLeadFollow
        {
            get { return this.allowlf; }
            set { this.allowlf = value; }
        }

        /// <summary>
        /// Allow entry with links (furc://)
        /// </summary>
        public bool AllowDreamURL
        {
            get { return this.allowfurl; }
            set { this.allowfurl = value; }
        }

        /// <summary>
        /// Use a swear filter on this map
        /// </summary>
        public bool UseSwearFilter
        {
            get { return this.swearfilter; }
            set { this.swearfilter = value; }
        }

        /// <summary>
        /// Prevent listing of current player on this map
        /// </summary>
        public bool PreventPlayerListing
        {
            get { return this.nowho; }
            set { this.nowho = value; }
        }

        /// <summary>
        /// Allow sitting only on items with the sittable attribute
        /// </summary>
        public bool ForceSitting
        {
            get { return this.forcesittable; }
            set { this.forcesittable = value; }
        }

        /// <summary>
        /// Allow people to shout on this map
        /// </summary>
        public bool AllowShouting
        {
            get { return this.allowshouts; }
            set { this.allowshouts = value; }
        }


        /// <summary>
        /// Allow mapsize above 208x200 tiles (This will require a Group Package)
        /// </summary>
        public bool AllowLargeDreamSize
        {
            get { return this.allowlarge; }
            set { this.allowlarge = value; }
        }

        /// <summary>
        /// Prevent name listing with the TAB key
        /// </summary>
        public bool PreventTabListing
        {
            get { return this.notab; }
            set { this.notab = value; }
        }

        /// <summary>
        /// Prevent the use of seasonal avatars on this map
        /// </summary>
        public bool PreventSeasonalAvatars
        {
            get { return this.nonovelty; }
            set { this.nonovelty = value; }
        }

        /// <summary>
        /// Enforce parental controls
        /// </summary>
        public bool EnforceParentalControls
        {
            get { return this.parentalcontrols; }
            set { this.parentalcontrols = value; }
        }

        /// <summary>
        /// Encode this map on upload
        /// </summary>
        public bool EncodeDream
        {
            get { return this.encoded; }
            set { this.encoded = value; }
        }

        /// <summary>
        /// Get or Set the name of the map
        /// </summary>
        public String Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        /// <summary>
        /// Get or Set the path to the patch
        /// </summary>
        public String PatchArchive
        {
            get { return this.patchs; }
            set { this.patchs = value; }
        }

        /// <summary>
        /// Get or Set the revision of the map
        /// </summary>
        public int Revision
        {   
            get { return this.revision; }
            set { this.revision = value;}
        }

        /// <summary>
        /// Get or Set the standard for the map
        /// </summary>
        public String Rating
        {
            get { return this.rating; }
            set { this.rating = value;}
        }


        #endregion

        #region Constructors
        internal Map()
        {

        }

        /// <summary>
        /// Creates a new empty map with the specified width and height
        /// </summary>
        /// <param name="width">The width of the map</param>
        /// <param name="height">The height of the map</param>
        public Map(int width, int height)
        {
            this.width = width / 2;
            this.height = height;

            floors = new byte[this.bytesLayerCount];
            objects = new byte[this.bytesLayerCount];
            walls = new byte[this.bytesLayerCount];
            regions = new byte[this.bytesLayerCount];
            effects = new byte[this.bytesLayerCount];

            String mapData = String.Format(Properties.Resources.DefaultMapData, this.height, this.width, "");
            String[] mapLines = mapData.Split(new String[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (String line in mapLines)
            {
                this.headerLines.Add(line);
                if (line.Contains("="))
                {
                    String[] vals = line.Split(new char[] { '=' }, 2);
                    this.mapData.Add(vals[0], vals[1]);
                }
                else if (line == "BODY")
                    break;
            }

            SetMapHeaders(this.mapData);

            this.tiles = new MapTile[this.Width, this.Height];

            for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                    this.tiles[x, y] = new MapTile(x, y, this);
            }

            byte[] mapMatrix = new byte[this.bytesLayerCount * 5];
            this.mapMatrix = mapMatrix;
        }

        #endregion

        #region Private Functions
        private void SetMapHeaders(Dictionary<String, String> Values)
        {
            if (Values.ContainsKey("height"))
                this.width = int.Parse(Values["height"].Trim(headerTrimChars));

            if (Values.ContainsKey("width"))
                this.width = int.Parse(Values["width"].Trim(headerTrimChars));

            if (Values.ContainsKey("revision"))
                this.revision = int.Parse(Values["revision"].Trim(headerTrimChars));

            if (Values.ContainsKey("patcht"))
                this.patcht = int.Parse(Values["patcht"].Trim(headerTrimChars));

            if (Values.ContainsKey("name"))
                this.name = Values["name"].Trim(headerTrimChars);

            if (Values.ContainsKey("patchs"))
                this.patchs = Values["patchs"].Trim(headerTrimChars);

            if (Values.ContainsKey("rating"))
                this.rating = Values["rating"].Trim(headerTrimChars);

            if (Values.ContainsKey("allowjs"))
                this.allowjs = Values["allowjs"].Trim(headerTrimChars) == "1";

            if (Values.ContainsKey("allowlf"))
                this.allowlf = Values["allowlf"].Trim(headerTrimChars) == "1";

            if (Values.ContainsKey("allowfurl"))
                this.allowfurl = Values["allowfurl"].Trim(headerTrimChars) == "1";

            if (Values.ContainsKey("swearfilter"))
                this.swearfilter = Values["swearfilter"].Trim(headerTrimChars) == "1";

            if (Values.ContainsKey("nowho"))
                this.nowho = Values["nowho"].Trim(headerTrimChars) == "1";

            if (Values.ContainsKey("forcesittable"))
                this.forcesittable = Values["forcesittable"].Trim(headerTrimChars) == "1";

            if (Values.ContainsKey("allowlarge"))
                this.allowlarge = Values["allowlarge"].Trim(headerTrimChars) == "1";

            if (Values.ContainsKey("allowshouts"))
                this.allowshouts = Values["allowshouts"].Trim(headerTrimChars) == "1";

            if (Values.ContainsKey("notab"))
                this.notab = Values["notab"].Trim(headerTrimChars) == "1";

            if (Values.ContainsKey("nonovelty"))
                this.nonovelty = Values["nonovelty"].Trim(headerTrimChars) == "1";

            if (Values.ContainsKey("parentalcontrols"))
                this.parentalcontrols = Values["parentalcontrols"].Trim(headerTrimChars) == "1";

            if (Values.ContainsKey("encoded"))
                this.encoded = Values["encoded"].Trim(headerTrimChars) == "1";
        }

        private bool ParseMatrix(byte[] matrix)
        {
            this.mapMatrix = matrix;

            for (int i = 0; i < this.bytesLayerCount; i++)
                floors[i] = matrix[i];

            for (int i = 0; i < this.bytesLayerCount; i++)
            {
                objects[i] = matrix[i + this.bytesLayerCount];
            }

            for (int i = 0; i < this.bytesLayerCount; i++)
            {
                walls[i] = matrix[i + (this.bytesLayerCount * 2)];
            }

            if (matrix.Length > this.bytesLayerCount * 3)
            {
                for (int i = 0; i < this.bytesLayerCount; i++)
                {
                    regions[i] = matrix[i + (this.bytesLayerCount * 3)];
                }

                for (int i = 0; i < this.bytesLayerCount; i++)
                {
                    effects[i] = matrix[i + (this.bytesLayerCount * 4)];
                }
            }
            else
            {
                for (int i = 0; i < this.bytesLayerCount; i++)
                {
                    regions[i] = new byte();
                    effects[i] = new byte();
                }
            }

            return true;
        }

        private int getPosFrom(int x, int y)
        {
            return ((this.height * (x / 2) + y) * 2);
        }
        #endregion

        #region Internal Functions

        internal void internal_setFloorAt(int x, int y, ushort nr)
        {
            this.floors[getPosFrom(x, y)] = (byte)nr;
        }

        internal void internal_setObjectAt(int x, int y, ushort nr)
        {
            this.objects[getPosFrom(x, y)] = (byte)nr;
        }

        internal void internal_setWallAt(int x, int y, ushort nr)
        {
            this.walls[this.height * x + y] = (byte)nr;
        }

        internal void internal_setRegionAt(int x, int y, ushort nr)
        {
            this.regions[getPosFrom(x, y)] = (byte)nr;
        }

        internal void internal_setEffectAt(int x, int y, ushort nr)
        {
            this.effects[getPosFrom(x, y)] = (byte)nr;
        }
        #endregion

        #region Public Functions
        /// <summary>
        /// Loads a map from a file
        /// </summary>
        /// <param name="filename">The file to load the map from</param>
        /// <exception cref="InvalidDataException">Thrown if the width and height of the map is not known (corrupt file)</exception>
        /// <returns>The map</returns>
        public static Map LoadFrom(String filename)
        {
            Map m = new Map();
            FileStream fs = new FileStream(filename, FileMode.Open);
            BinaryReader br = new BinaryReader(fs, Encoding.GetEncoding(1252));

            String currentLine = "" + br.ReadChar();
            while (true)
            {
                currentLine += br.ReadChar();

                if (currentLine.EndsWith("\n"))
                {
                    m.headerLines.Add(currentLine.Replace("\n", ""));
                    if (currentLine.Contains("="))
                    {
                        String[] vals = currentLine.Split(new char[] { '=' }, 2);

                        if (!m.mapData.ContainsKey(vals[0]))
                            m.mapData.Add(vals[0], vals[1]);
                        else
                            m.mapData[vals[0]] = vals[1];
                    }
                    else if (currentLine == "BODY\n")
                        break;

                    currentLine = "";
                }
            }

            if (m.mapData.ContainsKey("width") && m.mapData.ContainsKey("height"))
            {
                m.width = int.Parse(m.mapData["width"]);
                m.height = int.Parse(m.mapData["height"]);
            }
            else
            {
                throw new InvalidDataException("Unable to determine width & height of the map");
            }

            m.SetMapHeaders(m.mapData);

            m.floors = new byte[m.bytesLayerCount];
            m.objects = new byte[m.bytesLayerCount];
            m.walls = new byte[m.bytesLayerCount];
            m.regions = new byte[m.bytesLayerCount];
            m.effects = new byte[m.bytesLayerCount];

            List<byte> mapMatrix = new List<byte>();
            int read = 0;
            byte[] buffer = new byte[1024];
            do
            {
                read = br.Read(buffer, 0, buffer.Length);
                for (int i = 0; i < read; i++)
                    mapMatrix.Add(buffer[i]);

            } while (read > 0);

            br.Close();
            fs.Close();

            m.ParseMatrix(mapMatrix.ToArray());

            m.tiles = new MapTile[m.Width, m.Height];

            for (int x = 0; x < m.Width; x++)
            {
                for (int y = 0; y < m.Height; y++)
                {
                    m.tiles[x, y] = new MapTile(x, y, m);
                }
            }

            return m;
        }

        /// <summary>
        /// Get the floor number from a tile
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The floor number</returns>
        public ushort GetFloorAt(int x, int y)
        {
            int pos = getPosFrom(x, y);

            return (ushort)floors[pos];
        }

        /// <summary>
        /// Set the floor number at a tile specified by x and y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="floorNumber"></param>
        public void SetFloorAt(int x, int y, ushort floorNumber)
        {
            int pos = getPosFrom(x, y);

            floors[pos] = (byte)floorNumber;
        }

        /// <summary>
        /// Get the object number from a tile
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The object number</returns>
        public ushort GetObjectAt(int x, int y)
        {
            int pos = getPosFrom(x, y);

            return (ushort)objects[pos];
        }

        /// <summary>
        /// Set the object number at a tile specified by x & y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="floorNumber"></param>
        public void SetObjectAt(int x, int y, ushort objectNumber)
        {
            int pos = getPosFrom(x, y);

            objects[pos] = (byte)objectNumber;
        }

        /// <summary>
        /// Get the wall number from a tile
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The wall number</returns>
        public ushort GetWallAt(int x, int y)
        {
            int pos = (this.height * x + y);

            return (ushort)walls[pos];
        }

        /// <summary>
        /// Set the wall number at a tile specified by x & y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="floorNumber"></param>
        public void SetWallAt(int x, int y, ushort wallNumber)
        {
            int pos = (this.height * x + y);

            walls[pos] = (byte)wallNumber;
        }

        /// <summary>
        /// Get the region number from a tile
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The region number</returns>
        public ushort GetRegionAt(int x, int y)
        {
            int pos = getPosFrom(x, y);

            return (ushort)regions[pos];
        }

        /// <summary>
        /// Set the region number at a tile specified by x & y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="floorNumber"></param>
        public void SetRegionAt(int x, int y, ushort regionNumber)
        {
            int pos = getPosFrom(x, y);

            regions[pos] = (byte)regionNumber;
        }

        /// <summary>
        /// Get the effect number from a tile
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>The effect number</returns>
        public ushort GetEffectAt(int x, int y)
        {
            int pos = getPosFrom(x, y);

            return (ushort)effects[pos];
        }

        /// <summary>
        /// Set the effect number at a tile specified by x & y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="floorNumber"></param>
        public void SetEffectAt(int x, int y, ushort effectNumber)
        {
            int pos = getPosFrom(x, y);

            effects[pos] = (byte)effectNumber;
        }

        /// <summary>
        /// Get a MapTile object from the position specified by x & y
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public MapTile GetMapTile(int x, int y)
        {
            return this.tiles[x, y];
        }

        /// <summary>
        /// Save the map to a file
        /// </summary>
        /// <param name="filename">The filename to save to</param>
        /// <param name="overwrite">If a file with that name already exist, should we overwrite it?</param>
        /// <returns>True if the save was a success, False if not</returns>
        public bool Save(String filename, bool overwrite = true)
        {
            if (File.Exists(filename) && !overwrite)
                return false;

            FileStream fs = new FileStream(filename, FileMode.Create);
            BinaryWriter sw = new BinaryWriter(fs, Encoding.ASCII);

            String headerData = "MAP V01.40 Furcadia\n";
            headerData += "height=" + this.height + "\n";
            headerData += "width=" + this.width + "\n";
            headerData += "revision=" + this.revision + "\n";
            headerData += "patcht=" + this.patcht + "\n";
            headerData += "name=" + this.name + "\n";
            headerData += "patchs=" + this.patchs + "\n";
            headerData += "encoded=" + (this.encoded ? "1" : "0") + "\n";
            headerData += "allowjs=" + (this.allowjs ? "1" : "0") + "\n";
            headerData += "allowlf=" + (this.allowlf ? "1" : "0") + "\n";
            headerData += "allowfurl=" + (this.allowfurl ? "1" : "0") + "\n";
            headerData += "swearfilter=" + (this.swearfilter ? "1" : "0") + "\n";
            headerData += "nowho=" + (this.nowho ? "1" : "0") + "\n";
            headerData += "forcesittable=" + (this.forcesittable ? "1" : "0") + "\n";
            headerData += "allowshouts=" + (this.allowshouts ? "1" : "0") + "\n";
            headerData += "rating=" + this.rating + "\n";
            headerData += "allowlarge=" + (this.allowlarge ? "1" : "0") + "\n";
            headerData += "notab=" + (this.notab ? "1" : "0") + "\n";
            headerData += "nonovelty=" + (this.nonovelty ? "1" : "0") + "\n";
            headerData += "parentalcontrols=" + (this.parentalcontrols ? "1" : "0") + "\n";
            headerData += "BODY\n";

            byte[] headerDataBytes = Encoding.GetEncoding(1252).GetBytes(headerData);

            sw.Write(headerDataBytes);
            sw.Write(this.floors);
            sw.Write(this.objects);
            sw.Write(this.walls);
            sw.Write(this.regions);
            sw.Write(this.effects);

            sw.Close();
            fs.Close();

            return true;
        }
        #endregion

    }

    public static class MapRating
    {
        public const String Everyone = "Everyone";
        public const String Teen = "Teen+";
        public const String Mature = "Mature 16+";
        public const String Adult = "Adult 18+";
        public const String AdultOnly = "Adults Only";
        public const String AOClean = "AOClean";
    }

    public enum PatchSetting
    {
        NoPatch = 0,
        UseLocalPath = 1,
        UseRemotePatch = 2
    }

}
