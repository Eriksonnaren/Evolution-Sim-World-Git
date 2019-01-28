using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Evolution_Simulator_World
{
    [Serializable]
    public class Seed : MovingEntity, SelectableObject, CollideMove, Visible
    {
        public int DrawLayer { get; } = 0;
        public Vector Pos { get; set; }
        public VectorI ChunkPos { get; set; }
        public Color Col { get; set; }
        public float Radius { get; } = 10;
        public float Hue { get; set; }
        public Vector Vel { get; set; }
        public float Friction { get; } = 0.1f;
        public bool Dead { get; set; }
        public float Mass { get; } = 5;
        public bool EnableCollide { get; set; } = false;
        readonly Tree Parent;
        readonly Tree Parent2;
        Tree.Leaf Leaf;
        float SeedTimer = 0;
        const float SeedTimerMax = 60;
        readonly float StartEnergy;
        public readonly bool Edible;
        public Seed(Tree Parent,Tree.Leaf Leaf)
        {
            Pos = Leaf.Pos;
            ChunkPos = Leaf.ChunkPos;

            StartEnergy = 5;
            Col = Color.YellowGreen;
            this.Parent = Parent;
            this.Leaf = Leaf;
            Edible = true;
        }
        public Seed(Tree.Leaf.Flower Fruit,Vector Pos,VectorI Chunk)
        {
            this.Pos=Pos;
            ChunkPos = Chunk;
            Parent = Fruit.Leaf.Tree;
            Parent2 = Fruit.Pollen.Leaf.Tree;
            StartEnergy = 30;
            Col = Fruit.Col;
            Edible = false;
        }
        public void Fall()
        {
            EnableCollide = true;
            float Angle = Form1.Rand.Next(0, 360);
            float Rad = (float)(Math.Sqrt(Form1.Rand.NextDouble())*Leaf.Center.Z*Tree.SizeScale2d);
            Vector DeltaPos = new Vector(Form1.Cos(Angle)*Rad, Form1.Sin(Angle) * Rad);
            //Vector DeltaPos = new Vector(UpdateWorld.ChunkSize,0);
            //Pos = new Vector((float)(Leaf.Center.X)*Tree.SizeScale2d, (float)Leaf.Center.Y * Tree.SizeScale2d) + DeltaPos + Leaf.Tree.Pos;
            //ChunkPos = Leaf.Tree.ChunkPos;
            Pos += DeltaPos;
            //Vector OldPos = Pos;
            //VectorI OldChunk = ChunkPos;
            UpdateWorld.ConstrainEntityInChunk(this);
            if (!UpdateWorld.Chunks[ChunkPos.X][ChunkPos.Y].Entities.Contains(this))
            {
                throw new Exception("Entity has wrong chunk");
            }
        }
        public void Update()
        {
            if (EnableCollide)
            {
                SeedTimer += 1 / Form1.fps;
                if (Dead)
                    UpdateWorld.RemoveEntity(this);
                if (SeedTimer > SeedTimerMax)
                {
                    Tree Tree = new Tree(Parent, Pos, ChunkPos)
                    {
                        Energy = StartEnergy
                    };
                    UpdateWorld.AddEntity(Tree, Pos, ChunkPos);
                    UpdateWorld.RemoveEntity(this);
                }
            }
        }
        public void Show(Draw D, float x, float y, float Zoom = 1)
        {
            float R = Radius * Zoom;
            float R2 = R * 2;
            D.Graphics.FillEllipse(new SolidBrush(Col), x - R, y - R, R2, R2);
        }

        public void OnCollision(CollideTrigger Other, Vector RelVector)
        {
            
        }
    }
}
