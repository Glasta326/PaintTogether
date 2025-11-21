using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PaintTogether.Common.DataTypes
{
    public class ShiftRegisterList<T>(int capacity)
    {
        private List<T> _Data { get; set; } = new List<T>(capacity);


        public T this[int index] => _Data[index];


        public void Add(T content)
        {
            MoveAlong();
            _Data[0] = content;
        }

        private void MoveAlong()
        {
            for (int i = _Data.Count - 1; i > 0; i--)
            {
                _Data[i] = _Data[i - 1];
            }
        }

        public void Clear()
        {
            _Data.Clear();;
        }

        public T Last()
        {
            return _Data[^1];
        }

        public T Pop()
        {
            T result = _Data.Last();
            _Data.RemoveAt(_Data.Count - 1);
            return result;
        }
    }
}
