using System.Collections.Generic;

namespace PaintTogether.Common.DataTypes
{
    public class ShiftRegister<T>(int capacity)
    {
        public T[] data { get; private set; } = new T[capacity];


        public T this[int index] => data[index];


        public void Add(T content)
        {
            MoveAlong();
            data[0] = content;
        }

        private void MoveAlong()
        {
            for (int i = data.Length - 1; i > 0; i--)
            {
                data[i] = data[i - 1];
            }
        }
    }
}
