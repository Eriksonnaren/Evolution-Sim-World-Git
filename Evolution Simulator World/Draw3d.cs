using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Evolution_Simulator_World
{
    public class Draw3d
    {
        public Graphics Graphics;
        public Size Size;
        public Point Pos;
        public Vector3 CameraPos = new Vector3(0, 0, 1);
        ///<summary> Vector3 that encodes all the rotationaxies </summary>
        public Vector3 angle = new Vector3(0, 0, 0);
        public Vector3 CenterPos = new Vector3();

        double XColor = 1;
        double YColor = 0.95F;
        double ZColor = 0.9F;
        double Xsin = 0;
        double Xcos = 1;
        double Ysin = 0;
        double Ycos = 1;
        double Zsin = 0;
        double Zcos = 1;
        ///<summary> The list of all Polygons that will be displayd when the display() function is run</summary>
        public List<Polygon> Polygons;
        public double CameraDist = 500;
        public double focus = 800;//enlarges the image
        public Draw3d()
        {

            Polygons = new List<Polygon>();
        }
        void FillPolygon(Polygon Pol)
        {
            Vector3[] Points = Pol.Points;
            PointF[] P = new PointF[Points.Length];
            Boolean dead = false;
            for (int i = 0; i < Points.Length; i++)
            {
                P[i] = getPos(Points[i]);//converts from 3d to 2d
                if (P[i].IsEmpty)//the point is empty if it is behind the camera
                    dead = true;
            }
            if (!dead)//ignore if behind
            {
                if (P.Length == 2)
                {
                    Graphics.DrawLine(new Pen(Pol.Col, (float)(focus * Pol.Width / Pol.dist)), P[0], P[1]);
                }
                else
                    Graphics.FillPolygon(new SolidBrush(Pol.Col), P);

            }
        }
        PointF getPos(Vector3 V)
        {
            V.Z = V.Z - CameraDist;
            if (V.Z > 0)//ignore if behind
                return new PointF();
            V = V * (focus / V.Z);//normalize to z, makes z = focus
            return new PointF(Pos.X + (float)(V.X) + Size.Width / 2, Pos.Y + (float)-(V.Y) + Size.Height / 2);//centers it
        }
        ///<summary> Needs to be executed after all polygons have been added </summary> 
        public void display()
        {
            Xcos = Math.Cos(angle.X);
            Ycos = Math.Cos(angle.Y);
            Zcos = Math.Cos(angle.Z);
            Xsin = Math.Sin(angle.X);
            Ysin = Math.Sin(angle.Y);
            Zsin = Math.Sin(angle.Z);
            RotatePolygons();
            foreach (Polygon P in Polygons)
            {
                double min = 0;
                if (!P.dead)
                    for (int i = 0; i < P.Points.Length; i++)
                    {
                        min += (P.Points[i] - CameraPos*CameraDist).Mag();
                    }
                P.dist = min / P.Points.Length;
            }
            Polygons = Polygons.OrderBy(x => -x.dist).ToList();//more distant objects are drawn first
            for (int i = 0; i < Polygons.Count; i++)
            {
                if (!Polygons[i].dead)
                    FillPolygon(Polygons[i]);
            }
            Polygons = new List<Polygon>();
        }
        void RotatePolygons()
        {
            foreach (Polygon P in Polygons)
            {
                for (int i = 0; i < P.Points.Length; i++)
                {
                    P.Points[i] = getRotation(P.Points[i]-CenterPos);
                }
            }
        }
        Vector3 getRotation(Vector3 V)
        {
            V = V.RotZ((float)angle.Z);
            V = V.RotY((float)angle.Y);
            V = V.RotX((float)angle.X);
            return V;
            /*Vector3 V2 = new Vector3();
            V2.X = Ycos * V.X + Ysin * V.Z;
            V2.Y = V.Y;
            V2.Z = -Ysin * V.X + Ycos * V.Z;
            V = new Vector3(V2);
            

            V2.X = V.X;
            V2.Y = Xcos * V.Y - Xsin * V.Z;
            V2.Z = Xsin * V.Y + Xcos * V.Z;
            V = new Vector3(V2);

            V2.X = Zcos * V.X - Zsin * V.Y;
            V2.Y = Zsin * V.X + Zcos * V.Y;
            V2.Z = V.Z;
            V = new Vector3(V2);

            return V;*/
        }
        ///<summary> Rotates all elements in the array around the point Zero</summary>
        public void rotateArray(Polygon[] Polygons, Vector3 angle, Vector3 Zero)
        {
            Vector3 sin = new Vector3(Math.Sin(angle.X), Math.Sin(angle.Y), Math.Sin(angle.Z));
            Vector3 cos = new Vector3(Math.Cos(angle.X), Math.Cos(angle.Y), Math.Cos(angle.Z));
            foreach (Polygon P in Polygons)
            {
                for (int i = 0; i < P.Points.Length; i++)
                {
                    P.Points[i] = getRotation(sin, cos, P.Points[i], Zero);
                }
            }
        }
        public Polygon RotatePolygon(Vector3 sin, Vector3 cos, Polygon P, Vector3 Zero)
        {
            for (int i = 0; i < P.Points.Length; i++)
            {
                P.Points[i] = getRotation(sin, cos, P.Points[i], Zero);
            }
            return P;
        }
        public Vector3 getRotation(Vector3 sin, Vector3 cos, Vector3 V1, Vector3 Zero)
        {
            Vector3 V = new Vector3(V1) - Zero;
            Vector3 V2 = new Vector3();
            V2.X = cos.Y * V.X + sin.Y * V.Z;
            V2.Y = V.Y;
            V2.Z = -sin.Y * V.X + cos.Y * V.Z;
            V = new Vector3(V2);

            V2.X = V.X;
            V2.Y = cos.X * V.Y - sin.X * V.Z;
            V2.Z = sin.X * V.Y + cos.X * V.Z;
            V = new Vector3(V2);

            V2.X = cos.Z * V.X - sin.Z * V.Y;
            V2.Y = sin.Z * V.X + cos.Z * V.Y;
            V2.Z = V.Z;
            V = new Vector3(V2);

            return V + Zero;
        }
        Vector3 getRotationReverse(Vector3 V)
        {
            Vector3 V2 = new Vector3();

            V2.X = Zcos * V.X + Zsin * V.Y;
            V2.Y = -Zsin * V.X + Zcos * V.Y;
            V2.Z = V.Z;
            V = new Vector3(V2);

            V2.X = V.X;
            V2.Y = Xcos * V.Y + Xsin * V.Z;
            V2.Z = -Xsin * V.Y + Xcos * V.Z;
            V = new Vector3(V2);

            V2.X = Ycos * V.X - Ysin * V.Z;
            V2.Y = V.Y;
            V2.Z = Ysin * V.X + Ycos * V.Z;
            V = new Vector3(V2);
            return V;
        }
        ///<summary> Gets the MousePosition as a 3d Vector3 on the XZ plane with Height as Y</summary>
        public Vector3 getMousePos(PointF Pos, double Height)
        {
            Vector3 V = new Vector3(Pos.X - Size.Width / 2, -Pos.Y + Size.Height / 2, 0);
            V = getRotationReverse(V);
            V = (V / (focus)) * (CameraDist);
            Vector3 CV = getRotationReverse(new Vector3(0, 0, -CameraDist));
            double t = (CV.Y - Height) / (CV.Y - V.Y);
            return LerpV(CV, V, t) * (-1);
        }
        double Lerp(double A, double B, double M)
        {
            return A + (B - A) * M;
        }
        Vector3 LerpV(Vector3 A, Vector3 B, double M)
        {
            return new Vector3(Lerp(A.X, B.X, M), Lerp(A.Y, B.Y, M), Lerp(A.Z, B.Z, M));
        }
        ///<summary> Run this instead of Polygons.add() if you want a line, allows for specified width  </summary>
        public void addLine(Color C, double width, Vector3 V1, Vector3 V2)
        {
            Polygon P = new Polygon(C, new Vector3[] { V1, V2 });
            P.Width = width;
            Polygons.Add(P);
        }
        public Polygon[] getCuboid(Color C, Vector3 Corner1, Vector3 Corner2)
        {
            Polygon[] P = new Polygon[6];
            P[0] = (new Polygon(CMult(C, XColor), new Vector3[] {//X1
                new Vector3(Corner1.X,Corner1.Y,Corner1.Z),
                new Vector3(Corner1.X,Corner2.Y,Corner1.Z),
                new Vector3(Corner1.X,Corner2.Y,Corner2.Z),
                new Vector3(Corner1.X,Corner1.Y,Corner2.Z)
            }));
            P[1] = (new Polygon(CMult(C, XColor), new Vector3[] {//X2
                new Vector3(Corner2.X,Corner1.Y,Corner1.Z),
                new Vector3(Corner2.X,Corner2.Y,Corner1.Z),
                new Vector3(Corner2.X,Corner2.Y,Corner2.Z),
                new Vector3(Corner2.X,Corner1.Y,Corner2.Z)
            }));
            P[2] = (new Polygon(CMult(C, YColor), new Vector3[] {//Y1
                new Vector3(Corner1.X,Corner1.Y,Corner1.Z),
                new Vector3(Corner2.X,Corner1.Y,Corner1.Z),
                new Vector3(Corner2.X,Corner1.Y,Corner2.Z),
                new Vector3(Corner1.X,Corner1.Y,Corner2.Z)
            }));
            P[3] = (new Polygon(CMult(C, YColor), new Vector3[] {//Y2
                new Vector3(Corner1.X,Corner2.Y,Corner1.Z),
                new Vector3(Corner2.X,Corner2.Y,Corner1.Z),
                new Vector3(Corner2.X,Corner2.Y,Corner2.Z),
                new Vector3(Corner1.X,Corner2.Y,Corner2.Z)
            }));
            P[4] = (new Polygon(CMult(C, ZColor), new Vector3[] {//Z1
                new Vector3(Corner1.X,Corner1.Y,Corner1.Z),
                new Vector3(Corner2.X,Corner1.Y,Corner1.Z),
                new Vector3(Corner2.X,Corner2.Y,Corner1.Z),
                new Vector3(Corner1.X,Corner2.Y,Corner1.Z)
            }));
            P[5] = (new Polygon(CMult(C, ZColor), new Vector3[] {//Z2
                new Vector3(Corner1.X,Corner1.Y,Corner2.Z),
                new Vector3(Corner2.X,Corner1.Y,Corner2.Z),
                new Vector3(Corner2.X,Corner2.Y,Corner2.Z),
                new Vector3(Corner1.X,Corner2.Y,Corner2.Z)
            }));
            return P;
        }
        public Polygon[] getCuboid(Color C, Vector3 Corner1, Vector3 Corner2, bool[] Sides)
        {
            List<Polygon> P = new List<Polygon>();
            if (Sides[0])
                P.Add(new Polygon(CMult(C, XColor), new Vector3[] {//X1
                new Vector3(Corner1.X,Corner1.Y,Corner1.Z),
                new Vector3(Corner1.X,Corner2.Y,Corner1.Z),
                new Vector3(Corner1.X,Corner2.Y,Corner2.Z),
                new Vector3(Corner1.X,Corner1.Y,Corner2.Z)
            }));
            if (Sides[1])
                P.Add(new Polygon(CMult(C, XColor), new Vector3[] {//X2
                new Vector3(Corner2.X,Corner1.Y,Corner1.Z),
                new Vector3(Corner2.X,Corner2.Y,Corner1.Z),
                new Vector3(Corner2.X,Corner2.Y,Corner2.Z),
                new Vector3(Corner2.X,Corner1.Y,Corner2.Z)
            }));
            if (Sides[2])
                P.Add(new Polygon(CMult(C, YColor), new Vector3[] {//Y1
                new Vector3(Corner1.X,Corner1.Y,Corner1.Z),
                new Vector3(Corner2.X,Corner1.Y,Corner1.Z),
                new Vector3(Corner2.X,Corner1.Y,Corner2.Z),
                new Vector3(Corner1.X,Corner1.Y,Corner2.Z)
            }));
            if (Sides[3])
                P.Add(new Polygon(CMult(C, YColor), new Vector3[] {//Y2
                new Vector3(Corner1.X,Corner2.Y,Corner1.Z),
                new Vector3(Corner2.X,Corner2.Y,Corner1.Z),
                new Vector3(Corner2.X,Corner2.Y,Corner2.Z),
                new Vector3(Corner1.X,Corner2.Y,Corner2.Z)
            }));
            if (Sides[4])
                P.Add(new Polygon(CMult(C, ZColor), new Vector3[] {//Z1
                new Vector3(Corner1.X,Corner1.Y,Corner1.Z),
                new Vector3(Corner2.X,Corner1.Y,Corner1.Z),
                new Vector3(Corner2.X,Corner2.Y,Corner1.Z),
                new Vector3(Corner1.X,Corner2.Y,Corner1.Z)
            }));
            if (Sides[5])
                P.Add(new Polygon(CMult(C, ZColor), new Vector3[] {//Z2
                new Vector3(Corner1.X,Corner1.Y,Corner2.Z),
                new Vector3(Corner2.X,Corner1.Y,Corner2.Z),
                new Vector3(Corner2.X,Corner2.Y,Corner2.Z),
                new Vector3(Corner1.X,Corner2.Y,Corner2.Z)
            }));
            return P.ToArray();
        }
        double Sq(double A)
        { return A * A; }
        public Color CMult(Color C, double M)
        {
            return Color.FromArgb((int)(C.R * M), (int)(C.G * M), (int)(C.B * M));
        }
    }
    public class Polygon
    {
        public Color Col;
        public Vector3[] Points;
        public bool dead = false;
        public double Width = 10;
        public double dist = 1;
        public Polygon(Color Col, Vector3[] Points)
        {
            this.Points = Points;
            this.Col = Col;
        }
    }
    [Serializable]
    public struct Vector3
    {
        public double X;
        public double Y;
        public double Z;
        public Vector3(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }
        public Vector3(Vector3 V)
        {
            X = V.X;
            Y = V.Y;
            Z = V.Z;
        }
        public Vector3(Vector V)
        {
            X = V.X;
            Y = V.Y;
            Z = 0;
        }
        public static Vector3 operator +(Vector3 V1, Vector3 V2)
        {
            return new Vector3(V1.X + V2.X, V1.Y + V2.Y, V1.Z + V2.Z);
        }
        public static Vector3 operator -(Vector3 V1, Vector3 V2)
        {
            return new Vector3(V1.X - V2.X, V1.Y - V2.Y, V1.Z - V2.Z);
        }
        public static Vector3 operator -(Vector3 V1)
        {
            return new Vector3(-V1.X, -V1.Y, -V1.Z);
        }
        public static double operator *(Vector3 V1,Vector3 V2)
        {
            return V1.X * V2.X+ V1.Y * V2.Y+ V1.Z * V2.Z;
        }
        public static Vector3 operator *(Vector3 V1, double M)
        {
            return new Vector3(V1.X * M, V1.Y * M, V1.Z * M);
        }
        public static Vector3 operator *(double M, Vector3 V1)
        {
            return new Vector3(V1.X * M, V1.Y * M, V1.Z * M);
        }
        public static Vector3 operator /(Vector3 V1, double M)
        {
            if (M == 0)
                return new Vector3(0, 0, 0);
            return new Vector3(V1.X / M, V1.Y / M, V1.Z / M);
        }
        public static Vector3 Cross(Vector3 V1, Vector3 V2)
        {
            return new Vector3(V2.Y*V1.Z- V2.Z * V1.Y,-(V2.X * V1.Z - V2.Z * V1.X), V2.X * V1.Y - V2.Y * V1.X);
        }
        public Vector3 RotX(float Angle)
        {
            float C = Form1.Cos(Angle);
            float S = Form1.Sin(Angle);
            return new Vector3(X, Y * C + Z * S, Z * C - Y * S);
        }
        public Vector3 RotY(float Angle)
        {
            float C = Form1.Cos(Angle);
            float S = Form1.Sin(Angle);
            return new Vector3(X * C + Z * S, Y, Z * C - X * S);
        }
        public Vector3 RotZ(float Angle)
        {
            float C = Form1.Cos(Angle);
            float S = Form1.Sin(Angle);
            return new Vector3(X*C+Y*S, Y * C - X * S, Z);
        }
        public double Mag()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }
        public void Norm()
        {
            double M = Mag();
            X /= M;
            Y /= M;
            Z /= M;
        }
        public void Show(Vector3 Start,Draw3d D, Color Col, float W)
        {
            if (X != 0 || Y != 0 || Z != 0)
            {
                double S = 5;
                Vector3 V = new Vector3(this)+Start;
                Vector3 X = new Vector3(this);
                X.Norm();
                Vector3 Y = Cross(X,new Vector3(0, 0, 1));
                Vector3 Z = Cross(X,Y);
                if (Y.Equals(Z))
                {
                    Y = new Vector3(0, 1, 0);
                    Z = new Vector3(1, 0, 0);
                }
                X *= S * 2;
                Y.Norm();
                Z.Norm();
                Y *= S;
                Z *= S;
                D.Polygons.Add(new Polygon(D.CMult(Col, 0.9), new Vector3[] { V - X + Y + Z, V - X + Y - Z, V - X - Y - Z, V - X - Y + Z }));
                D.Polygons.Add(new Polygon(D.CMult(Col, 0.8), new Vector3[] { V, V - X + Y + Z, V - X + Y - Z }));
                D.Polygons.Add(new Polygon(D.CMult(Col, 0.7), new Vector3[] { V, V - X + Y + Z, V - X - Y + Z }));
                D.Polygons.Add(new Polygon(D.CMult(Col, 0.8), new Vector3[] { V, V - X - Y + Z, V - X - Y - Z }));
                D.Polygons.Add(new Polygon(D.CMult(Col, 0.7), new Vector3[] { V, V - X + Y - Z, V - X - Y - Z }));

                D.addLine(Col, W, Start, V-X);
            }
        }
    }
}
