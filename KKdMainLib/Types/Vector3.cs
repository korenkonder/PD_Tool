using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KKdMainLib.MessagePack;

namespace KKdMainLib.Types
{
    public class Vector3<T>
    {
        public T X;
        public T Y;
        public T Z;

        public Vector3()
        { X = default(T); Y = default(T); Z = default(T); }

        public Vector3(T X, T Y, T Z)
        { this.X = X; this.Y = Y; this.Z = Z; }
    }
}
