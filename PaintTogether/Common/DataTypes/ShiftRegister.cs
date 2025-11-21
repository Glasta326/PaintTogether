using System.Collections;
using System.Collections.Generic;

namespace PaintTogether.Common.DataTypes
{
    public class ShiftRegister<T>(int capacity)
    {
        public T[] data { get; private set; } = new T[capacity];


        public T this[int index] => data[index];

        public bool HasData => data.Length > 0;

        public void Push(T content)
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

        public T Pop()
        {
            T result = data[0];
            MoveBack();
            return result;
        }

        private void MoveBack()
        {
            for (int i = 1; i < data.Length - 1; i++)
            {
                data[i - 1] = data[i];
            }
        }

        public void Clear()
        {
            data = new T[capacity];
        }

        public T Last()
        {
            return data[^1];
        }

        public int Length()
        {
            return data.Length;
        }
    }
}
