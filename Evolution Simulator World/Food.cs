using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Evolution_Simulator_World
{
    [Serializable]
    public class Food:BaseObject
    {
        public Tree Parent;
        public Vector Pos { get; set; }
        public float Radius { get { return 12; } }
        public Color Col { get; set; }
        public float Hue { get; set; }
        public Vector Vel;
        public bool Dead = false;
        public float Energy;
        public float MaxEnergy;
        public bool Seed;
        float time;
        float EnergyMultiplier=1.5f;
        public Food(Vector Pos,Tree Parent,float Energy,bool Seed)
        {
            time = 75 + Form1.Rand.Next(50);
            this.Seed = Seed;
            this.Parent = Parent;
            this.Energy = MaxEnergy = Energy*EnergyMultiplier;
            Hue = Parent.FoodHue;
            Col = Form1.ColorFromHue(Hue);
            this.Pos = Pos;
            Collide();
        }
        public void Update()
        {
            if (Vel.MagSq() > 0.1)
            {
                Pos += Vel;
                Vel *= 0.5f;
                Collide();
            }
            time -= 1 / Form1.fps;
            if (time <= 0)
                Dead = true;
        }
        public void Collide()
        {
            foreach (Tree T in Form1.Trees)
            {
                float R = Radius + T.Radius;
                if ((T.Pos - Pos).MagSq() < R * R)
                {
                    Vector RelPos = T.Pos - Pos;
                    Vector Push = RelPos * ((Vel.Mag() + 1) / RelPos.Mag());
                    Vel -= Push;
                }
            }
            foreach (Food F in Form1.Foods)
            {
                float R = Radius + F.Radius;
                if ((F.Pos - Pos).MagSq() < R * R)
                {
                    Vector RelPos = F.Pos - Pos;
                    Vector Push = RelPos * -(((Vel - F.Vel).Mag() + 1) / (2 * RelPos.Mag()));
                    Vel += Push;
                    F.Vel -= Push;
                }
            }
        }
        public void Show(Draw D, float x, float y, float Zoom = 1)
        {
            float R = Radius * Zoom;
            float R2 = R * 2;
            System.Drawing.Drawing2D.Matrix M = D.Graphics.Transform;
            D.Graphics.TranslateTransform(x, y);

            D.Graphics.FillEllipse(new SolidBrush(Col),-R,-R,R2,R2);

            D.Graphics.Transform = M;
        }
    }
}
