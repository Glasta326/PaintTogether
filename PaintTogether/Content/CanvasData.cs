using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PaintTogether.Common.Utilities;

namespace PaintTogether.Content
{
    public class CanvasData
    {
        public readonly uint Width;
        public readonly uint Height;

        public Color[] Data { get; private set; }

        public Color this[int x, int y]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            get
            {
                if (x < 0 || y < 0 || x > Width || y > Height)
                {
                    throw new IndexOutOfRangeException();
                }
                else
                {
                    return Data[y * Height + x];
                }
            }
            set
            {
                if (x < 0 || y < 0 || x > Width || y > Height)
                {
                    throw new IndexOutOfRangeException();
                }

                Data[y * Height + x] = value;
            }
        }

        public Color this[Vector2 pos]
        {
            get => this[(int)pos.X, (int)pos.Y];
            set => this[(int)pos.X, (int)pos.Y] = value;
        }

        public Color this[Point pos]
        {
            get => this[pos.X, pos.Y];
            set => this[pos.X, pos.Y] = value;
        }

        public void ClearCanvas()
        {
            Data = new Color[Height * Width];
        }

        internal CanvasData(uint width, uint height)
        {
            Width = width;
            Height = height;
            Data = new Color[height * width];
        }



    }
}