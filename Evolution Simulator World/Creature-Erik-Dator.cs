using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Evolution_Simulator_World
{
    public class Creature
    {
        public Vector Pos;
        public Vector MouthPos=new Vector(0.6f,0);//percentage of radius
        float MouthRadius = 0.7f;//percentage of radius
        Vector Vel;
        float Angle;//in degrees
        float Radius = 30;
        Vector AngleVector;
        public Creature(Vector Pos)
        {
            this.Pos = Pos;
        }
        public void Update()
        {

        }
        public void Show(Draw D, float x,float y, float Zoom = 1)
        {
            float R = Radius * Zoom;
            float MR = Radius * MouthRadius;
            Vector MP = MouthPos * R;
            float MR2 = MR * 2;
            float R2 = R * 2;
            Matrix M = D.Graphics.Transform.Clone();
            D.Graphics.TranslateTransform(-x,-y);
            D.Graphics.RotateTransform(Angle);
            D.Graphics.FillEllipse(Brushes.White, -MR+ MP.X, -MR+MP.Y, MR2, MR2);
            D.Graphics.FillEllipse(Brushes.Blue, -R, -R, R2, R2);
            D.Graphics.Transform = M;
        }
    }
}
