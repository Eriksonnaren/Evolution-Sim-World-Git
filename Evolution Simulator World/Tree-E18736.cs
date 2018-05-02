using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Evolution_Simulator_World
{
    public class Tree
    {
        public Vector Pos;
        float Radius=90;
        float EnergyRadius;
        int BranchAmount;
        List<Branch> Branches = new List<Branch>();
        public Tree(Vector Pos)
        {
            this.Pos = Pos;
            BranchAmount = Form1.Rand.Next(2, 6);
            float a = Form1.Rand.Next(360);
            for (int i = 0; i < BranchAmount; i++)
            {
                float angle = a+(i * 360+Form1.Rand.Next(-90,90)) / (float)BranchAmount;
                float BRadius = Form1.Rand.Next(40, 60);
                Branches.Add(new Branch(this, new Vector(Form1.Cos(angle), Form1.Sin(angle))*(Radius+BRadius*1.5f),BRadius));
            }
            
        }
        public void Update()
        {

        }
        public void Show(Draw D,float X,float Y,float Zoom=1)
        {
            float R = Radius * Zoom;
            float R2 = R * 2;
            System.Drawing.Drawing2D.Matrix M = D.Graphics.Transform;
            D.Graphics.TranslateTransform(X, Y);
            foreach (Branch B in Branches)
            {
                B.Show(D, Zoom);
            }
            D.Graphics.FillEllipse(Brushes.LawnGreen,-R,-R,R2,R2);
            D.Graphics.Transform = M;
        }
        class Branch
        {
            public Vector Pos;
            public float Radius;
            Tree Parent;
            public Branch(Tree Parent,Vector Pos,float Radius)
            {
                this.Radius = Radius;
                this.Parent = Parent;
                this.Pos = Pos;
            }
            public void Update()
            {

            }
            public void Show(Draw D, float Zoom)
            {
                Vector P = Pos * Zoom;
                float R = Radius * Zoom;
                float R2 = R * 2;
                D.Graphics.DrawLine(new Pen(Color.Brown,R),0,0,P.X,P.Y);
                D.Graphics.FillEllipse(Brushes.LawnGreen, P.X-R, P.Y-R, R2, R2);
            }
        }
    }
}
