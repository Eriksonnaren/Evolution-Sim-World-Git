using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Evolution_Simulator_World
{
    [Serializable]
    public class Seed:BaseObject
    {
        public Vector Pos { get; set; }
        public Color Col { get; set; }
        public float Radius { get { return 20; } }
        public float Hue { get; set; }
        float Angle = 0;
        float Time = 0;
        float maxTime = 10;//sec
        float AngleSpeed1;
        float AngleSpeed2;
        Tree Parent;
        public Seed(Tree Parent,Vector Pos)
        {
            this.Pos = Pos;
            this.Parent = Parent;
            Hue = Parent.Hue;
            Col = Parent.Col;
            AngleSpeed1 = Form1.Rand.Next(20,70) / 10f;
            AngleSpeed2 = Form1.Rand.Next(20, 70) / 10f;
            //AngleSpeed2 = 1;
            AngleSpeed2 = AngleSpeed2 / AngleSpeed1;
        }
        public void Update()
        {
            Angle += AngleSpeed1;
            Time += 1/Form1.fps;
            if(Time>maxTime)
            {
                Tree T = new Tree(Pos, Parent.BranchAmount);
                Form1.Trees.Add(T);
                T.Hue = Parent.Hue+(float)(1 - Form1.Rand.NextDouble() * 2) * 0.03f;
                T.FoodHue = Parent.FoodHue + (float)(1 - Form1.Rand.NextDouble() * 2) * 0.03f;
                T.Hue = Clamp01(T.Hue);
                T.FoodHue = Clamp01(T.FoodHue);
                T.Col = Form1.ColorFromHue(T.Hue);
                Form1.Seeds.Remove(this);
                foreach (var B in T.Branches)
                {
                    B.NextFood.Hue = T.FoodHue;
                    B.NextFood.Col = Form1.ColorFromHue(T.FoodHue);
                }
            }
        }
        float Clamp01(float x)
        {
            if (x > 1)
                return 1;
            else if (x < 0)
                return 0;
            else
                return x;
        }
        public void Show(Draw D, float x, float y, float Zoom = 1)
        {
            float R = Radius * Zoom;
            float R2 = R * 2;
            float R3 = R * 0.75f;
            float R4 = R * 1.75f;
            Matrix M = D.Graphics.Transform.Clone();
            D.Graphics.TranslateTransform(x, y);
            D.Graphics.RotateTransform(Angle);

            PointF[] P = new PointF[]
            {
                new PointF(0,R3),
                new PointF(R4,0),
                new PointF(0,-R3),
                new PointF(-R4,0),
            };
            D.Graphics.FillPolygon(Brushes.Brown,P);
            D.Graphics.RotateTransform(-Angle * (1 + AngleSpeed2));
            D.Graphics.FillPolygon(Brushes.Brown, P);
            D.Graphics.FillEllipse(new SolidBrush(Col), -R, -R, R2, R2);
            D.Graphics.Transform = M;
        }
    }
}
