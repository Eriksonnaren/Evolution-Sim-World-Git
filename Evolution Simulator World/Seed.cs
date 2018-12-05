using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Evolution_Simulator_World
{
    [Serializable]
    public class Seed : BaseObject
    {
        public Vector Pos { get; set; }
        public Color Col { get; set; }
        public float Radius { get { return 10; } }
        public float Hue { get; set; }
        public Vector Vel;
        public bool Dead;
        Tree Parent;
        Tree Parent2;
        Tree.Leaf Leaf;
        float SeedTimer = 0;
        const float SeedTimerMax = 30;
        readonly float StartEnergy;
        public readonly bool Edible;
        public Seed(Tree Parent,Tree.Leaf Leaf)
        {
            StartEnergy = 5;
            Col = Color.YellowGreen;
            this.Parent = Parent;
            this.Leaf = Leaf;
            Edible = true;
        }
        public Seed(Tree.Leaf.Flower Fruit,Vector Pos)
        {
            this.Pos=Pos;
            Parent = Fruit.Leaf.Tree;
            Parent2 = Fruit.Pollen.Leaf.Tree;
            StartEnergy = 30;
            Col = Fruit.Col;
            Edible = false;
        }
        public void Fall()
        {
            float Angle = Form1.Rand.Next(0, 360);
            float Rad = (float)(Math.Sqrt(Form1.Rand.NextDouble())*Leaf.Center.Z*Tree.SizeScale2d);
            Vector DeltaPos = new Vector(Form1.Cos(Angle)*Rad, Form1.Sin(Angle) * Rad);
            Pos = new Vector((float)(Leaf.Center.X)*Tree.SizeScale2d, (float)Leaf.Center.Y * Tree.SizeScale2d) + DeltaPos + Leaf.Tree.Pos;
            if (Pos.MagSq() > Form1.ArenaRadius * Form1.ArenaRadius)
            {
                float R = Pos.Mag();
                float S = Form1.ArenaRadius / (2*R-Form1.ArenaRadius);
                Pos = Pos *=-S;

            }
        }
        public void Update()
        {
            Pos += Vel;
            Vel *= 0.5f;
            SeedTimer += 1 / Form1.fps;
            if(Dead)
                Form1.Seeds.Remove(this);
            if (SeedTimer>SeedTimerMax)
            {
                Tree Tree = new Tree(Parent, Pos)
                {
                    Energy = StartEnergy

                };
                Form1.Trees.Add(Tree);
                Form1.Seeds.Remove(this);
            }
        }
        public void Show(Draw D, float x, float y, float Zoom = 1)
        {
            float R = Radius * Zoom;
            float R2 = R * 2;
            D.Graphics.FillEllipse(new SolidBrush(Col), x-R, y-R, R2, R2);
        }
    }
}
