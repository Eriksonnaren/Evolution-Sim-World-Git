﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Evolution_Simulator_World
{
    [Serializable]
    public struct Vector
    {
        public float X;
        public float Y;
        public Vector(float X,float Y)
        {
            this.X = X;
            this.Y = Y;
        }
        public static Vector operator +(Vector V1,Vector V2)
        {
            return new Vector(V1.X + V2.X,V1.Y+V2.Y);
        }
        public static Vector operator -(Vector V1, Vector V2)
        {
            return new Vector(V1.X - V2.X, V1.Y - V2.Y);
        }
        public static Vector operator *(Vector V1, float d)
        {
            return new Vector(V1.X *d, V1.Y *d);
        }
        public static Vector operator *(float d, Vector V1)
        {
            return new Vector(V1.X * d, V1.Y * d);
        }
        public static Vector operator /(Vector V1, float d)
        {
            return new Vector(V1.X / d, V1.Y / d);
        }
        public Vector Rot(Vector R)
        {
            return new Vector(X * R.X - Y * R.Y, Y * R.X + X * R.Y);
        }
        public float Mag()
        {
            return Form1.Sqrt(MagSq());
        }
        public float MagSq()
        {
            return X*X +Y * Y;
        }
        public PointF ToPoint()
        {
            return new PointF(X,Y);
        }
    }
}