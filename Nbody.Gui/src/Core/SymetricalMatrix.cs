using Godot;
using System;

namespace NBody.Gui.src.Core
{
    public struct Vector3Formatable : IEquatable<Vector3Formatable>, IFormattable
    {
        public Vector3 vector;
        public bool Equals(Vector3Formatable other)
        {
            return other.vector == vector;
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return vector.ToString();
        }
    }
}
