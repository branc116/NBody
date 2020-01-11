using Godot;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearAlgebra.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nbody.Gui.src.Core
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
