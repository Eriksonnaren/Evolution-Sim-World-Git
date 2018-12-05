using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution_Simulator_World
{
    [Serializable]
    public struct Matrix3
    {
        Vector3 I;
        Vector3 J;
        Vector3 K;
        public Matrix3(Vector3 I,Vector3 J,Vector3 K)
        {
            this.I = I;
            this.J = J;
            this.K = K;
        }
        public Matrix3(Matrix3 M)
        {
            I = new Vector3(M.I);
            J = new Vector3(M.J);
            K = new Vector3(M.K);
        }
        public Matrix3(Vector3 V)
        {
            Vector3 Y = new Vector3(0, 0, 1);
            K = V;
            K.Norm();
            J = Vector3.Cross(K, Y);
            J.Norm();
            I = Vector3.Cross(K, J);
            I.Norm();
        }
        public static Vector3 operator *(Matrix3 M, Vector3 V)
        {
            return (M.I * V.X) + (M.J * V.Y) + (M.K * V.Z);
        }
        public static Matrix3 operator *(Matrix3 A, Matrix3 B)
        {
            return new Matrix3(A * B.I, A * B.J, A * B.K);
        }
        public Matrix3 RotX(float Angle)
        {
            float C = Form1.Cos(Angle);
            float S = Form1.Sin(Angle);
            return this * new Matrix3(new Vector3(1, 0, 0), new Vector3(0, C, -S), new Vector3(0, S, C));
        }
        public Matrix3 RotY(float Angle)
        {
            float C = Form1.Cos(Angle);
            float S = Form1.Sin(Angle);
            return this * new Matrix3(new Vector3(C, 0, -S), new Vector3(0, 1, 0), new Vector3(S, 0, C));
        }
        public Matrix3 RotZ(float Angle)
        {
            float C = Form1.Cos(Angle);
            float S = Form1.Sin(Angle);
            return this * new Matrix3(new Vector3(C, -S, 0), new Vector3(S, C, 0), new Vector3(0, 0, 1));
        }
    }
}
