using System;
using System.Linq;
using Godot;

namespace Game.Gameplay
{
    public class PlaygroundGridState
    {
        public int SizeX { get; private set; }
        public int SizeY { get; private set; }
        private int[] Array { get; set; }

        public int InvalidValue { get; set; } = -1;

        public PlaygroundGridState(int sizeX, int sizeY)
        {
            SizeX = sizeX;
            SizeY = sizeY;

            Array = new int[SizeX * SizeY];
            ClearAllCells();
        }

        public bool IsCellWithinBounds(int x, int y)
        {
            return x >= 0 && x < SizeX && y >= 0 && y < SizeY;
        }

        public bool CellEmpty(int x, int y)
        {
            return IsCellWithinBounds(x, y) && GetCell(x, y) == InvalidValue;
        }

        public bool CellIsOneOf(int x, int y, params int[] oneOfValues)
        {
            if (!IsCellWithinBounds(x, y))
                return false;

            int cellValue = GetCell(x, y);
            return oneOfValues.Any(value => cellValue == value);
        }

        public bool CellIsNoneOf(int x, int y, params int[] noneOfValues)
        {
            if (!IsCellWithinBounds(x, y))
                return false;

            int cellValue = GetCell(x, y);
            return noneOfValues.All(value => cellValue != value);
        }

        public bool PathEmpty(int x0, int y0, int x1, int y1, bool ignoreSelf = true)
        {
            return PathIsOneOf(x0, y0, x1, y1, ignoreSelf, InvalidValue);
        }

        public bool PathIsOneOf(int x0, int y0, int x1, int y1, bool ignoreSelf = true, params int[] oneOfValues)
        {
            if (x0 != x1 && y0 != y1)
            {
                throw new Exception("PlaygroundGridState.IsPathAvailable(): Paths must be horizontal or vertical.");
            }

            if (ignoreSelf)
            {
                int diffX = x1 - x0;
                int diffY = y1 - y0;
                x0 += Mathf.Sign(diffX);
                y0 += Mathf.Sign(diffY);
            }

            int xMin = Math.Min(x0, x1);
            int xMax = Math.Max(x0, x1);
            int yMin = Math.Min(y0, y1);
            int yMax = Math.Max(y0, y1);

            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    if (!IsCellWithinBounds(x, y))
                        return false;
                    if (!CellIsOneOf(x, y, oneOfValues))
                        return false;
                }
            }
            return true;
        }

        public bool PathIsNoneOf(int x0, int y0, int x1, int y1, bool ignoreSelf = true, params int[] noneOfValues)
        {
            if (x0 != x1 && y0 != y1)
            {
                throw new Exception("PlaygroundGridState.IsPathAvailable(): Paths must be horizontal or vertical.");
            }

            if (ignoreSelf)
            {
                int diffX = x1 - x0;
                int diffY = y1 - y0;
                x0 += Mathf.Sign(diffX);
                y0 += Mathf.Sign(diffY);
            }

            int xMin = Math.Min(x0, x1);
            int xMax = Math.Max(x0, x1);
            int yMin = Math.Min(y0, y1);
            int yMax = Math.Max(y0, y1);

            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    if (!IsCellWithinBounds(x, y))
                        return false;
                    if (!CellIsNoneOf(x, y, noneOfValues))
                        return false;
                }
            }
            return true;
        }

        public void ClearPath(int x0, int y0, int x1, int y1, bool ignoreSelf = true)
        {
            if (x0 != x1 && y0 != y1)
            {
                throw new Exception("PlaygroundGridState.IsPathAvailable(): Paths must be horizontal or vertical.");
            }

            if (ignoreSelf)
            {
                int diffX = x1 - x0;
                int diffY = y1 - y0;
                x0 += Mathf.Sign(diffX);
                y0 += Mathf.Sign(diffY);
            }

            int xMin = Math.Min(x0, x1);
            int xMax = Math.Max(x0, x1);
            int yMin = Math.Min(y0, y1);
            int yMax = Math.Max(y0, y1);

            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    if (!IsCellWithinBounds(x, y))
                        continue;
                    ClearCell(x, y);
                }
            }
        }

        public int GetCell(int x, int y)
        {
            if (!IsCellWithinBounds(x, y))
                throw new Exception("PlaygroundGridState.GetCell(): Cell is out of bounds.");
            return Array[x + y * SizeX];
        }

        public void SetCell(int x, int y, int objectType)
        {
            if (!IsCellWithinBounds(x, y))
                throw new Exception("PlaygroundGridState.SetCell(): Cell is out of bounds.");
            Array[x + y * SizeX] = objectType;
        }

        public void ClearCell(int x, int y)
        {
            if (!IsCellWithinBounds(x, y))
                throw new Exception("PlaygroundGridState.ClearCell(): Cell is out of bounds.");
            Array[x + y * SizeX] = InvalidValue;
        }

        public void ClearAllCells()
        {
            for (int i = 0; i < Array.Length; i++)
                Array[i] = InvalidValue;
        }
    }
}
