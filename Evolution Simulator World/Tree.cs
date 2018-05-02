using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Evolution_Simulator_World
{
    [Serializable]
    public class Tree:BaseObject
    {
        public Vector Pos { get; set; }
        public float Radius { get { return 90; } }
        float EnergyRadius;
        float BaseEnergyRadius=180;
        float EnergyRadiusPerBranch=80;
        float EnergyGainMultiplier=1;
        public int BranchAmount;
        public List<Branch> Branches = new List<Branch>();
        public List<Tree> NearbyTrees;
        bool Touch = false;
        float EnergyPerFood = 75;
        float LifeTime;
        float LifeTimeMax = 300;//seconds
        float Energy=10;
        float CurrentMaxEnergy;
        int currentBranch;
        public Color Col { get; set; }
        public float Hue { get; set; }
        public float FoodHue;
        public bool Dead = false;
        public float SeedGrowthPercent = 0.15f;//how high is the chanse of a food producing a seed
        public Tree(Vector Pos,int BranchAmount = 0)
        {
            Hue = (float)Form1.Rand.NextDouble();
            FoodHue = (float)Form1.Rand.NextDouble();
            Col = Form1.ColorFromHue(Hue);
            this.Pos = Pos;
            if(BranchAmount==0)
                BranchAmount = Form1.Rand.Next(2, 7);
            this.BranchAmount = BranchAmount;
            float a = Form1.Rand.Next(360);
            for (int i = 0; i < BranchAmount; i++)
            {
                float angle = a + (i * 360 + Form1.Rand.Next(-90, 90)) / (float)BranchAmount;
                float BRadius = Form1.Rand.Next(45, 55);
                Branches.Add(new Branch(this, new Vector(Form1.Cos(angle), Form1.Sin(angle)) * (Radius + BRadius * 1.5f), BRadius));
            }
            EnergyRadius = BaseEnergyRadius + EnergyRadiusPerBranch*BranchAmount;
            UpdateArea();
        }
        float getCoveredArea(float Rad1, float Rad2, float Dist)
        {
            if (Dist < Math.Abs(Rad1 - Rad2))//one is inside the other
            {
                float R = Math.Min(Rad1, Rad2);
                return R * R;
            }
            float D1 = (Dist * Dist + Rad1 * Rad1 - Rad2 * Rad2) / (2 * Dist);//from object 1 to intersect line
            float D2 = Dist - D1;//from object 2 to intersect line
            float A = getChordArea(Rad1, D1) + getChordArea(Rad2, D2);
            return A;
        }
        float getChordArea(float Rad, float Dist)
        {
            return Rad * Rad * (float)Math.Acos(Dist / Rad) - Dist * Form1.Sqrt(Rad * Rad - Dist * Dist);
        }
        public void UpdateArea()
        {
            Touch = false;
            NearbyTrees = new List<Tree>();
            EnergyGainMultiplier = 1;
            foreach (Tree T in Form1.Trees)
            {
                if (T != this)
                {
                    float dist = (Pos - T.Pos).Mag();
                    if (dist < T.EnergyRadius + EnergyRadius && T.EnergyRadius > 0)//if their energy circles are touching
                    {
                        NearbyTrees.Add(T);
                        if(!T.NearbyTrees.Contains(this))
                            T.NearbyTrees.Add(this);
                        Touch = true;
                        float A = getCoveredArea(EnergyRadius, T.EnergyRadius, dist);
                        EnergyGainMultiplier *= 1 - ( A/ ((float)Math.PI * (EnergyRadius * EnergyRadius))) / 2;
                        T.Touch = true;
                        T.EnergyGainMultiplier *= 1 - (A / ((float)Math.PI * (T.EnergyRadius * T.EnergyRadius))) / 2;
                    }
                }
            }
        }
        public void Update()
        {
            Energy += addEnergy();
            Energy -= removeBaseEnergy();
            Energy -= removeFoodEnergy();
            CurrentMaxEnergy = Math.Max(CurrentMaxEnergy, Energy);
            float s = Energy / CurrentMaxEnergy;
            if (Energy < 0)
            {
                Dead = true;
            }
            LifeTime += 1/Form1.fps;
            if (Branches[currentBranch].Energy > EnergyPerFood)
            {
                Branches[currentBranch].SpawnFood();
                int c = Form1.Rand.Next(0, Branches.Count - 1);
                if (c >= currentBranch)
                    c++;
                currentBranch = c;
            }
        }
        
        float addEnergy()
        {
            float L = LifeTime / LifeTimeMax;
            return 1 * (1 - (L * L)) * (1 + Branches.Count / 20.0f) * EnergyGainMultiplier;
        }
        float removeBaseEnergy()
        {
            return 0.9f * (1 + EnergyGainMultiplier) / 2;
        }
        float removeFoodEnergy()
        {
            float E = Math.Max(Energy - EnergyPerFood, 0) * 0.01f;
            Branches[currentBranch].Energy += E;
            return E;
        }
        public void ShowBelow(Draw D, float X, float Y, float Zoom)
        {
            //Color C = Touch ? Color.Purple:Color.Blue;
            //C = Color.FromArgb(50,C);
            //D.Graphics.FillEllipse(new SolidBrush(C), X-EnergyRadius * Zoom, Y-EnergyRadius * Zoom, EnergyRadius * 2 * Zoom, EnergyRadius * 2 * Zoom);
            foreach (Branch B in Branches)
            {
                B.Show(D, X, Y, Zoom);
            }
        }
        public void Show(Draw D,float X,float Y,float Zoom=1)
        {
            float R = Radius * Zoom;
            float R2 = R * 2;
            D.Graphics.FillEllipse(new SolidBrush(Col),X-R,Y-R,R2,R2);
            //D.Graphics.DrawString(Energy.ToString("0.0"), new Font("Arial", 10), Brushes.Black, X, Y);
        }
        [Serializable]
        public class Branch
        {
            public Vector Pos;
            public float Radius = 50;
            public float Energy;
            Tree Parent;
            public Food NextFood;
            public int NextAngle;
            public Branch(Tree Parent, Vector Pos, float Radius)
            {
                NextAngle = Form1.Rand.Next(0, 360);
                this.Radius = Radius;
                this.Parent = Parent;
                this.Pos = Pos;
                float Angle = Form1.Rand.Next(0, 360);
                Vector Pos2 = Parent.Pos + this.Pos + new Vector(Form1.Cos(Angle), Form1.Sin(Angle)) * Form1.Rand.Next(0, (int)Radius);
                NextFood = new Food(Pos2, Parent, Parent.EnergyPerFood, Form1.Rand.NextDouble()<Parent.SeedGrowthPercent);
            }
            public void Update()
            {

            }
            public void SpawnFood()
            {
                float Angle = Form1.Rand.Next(0, 360);
                Form1.Foods.Add(NextFood);
                Energy = 0;
                Vector Pos = Parent.Pos + this.Pos + new Vector(Form1.Cos(Angle), Form1.Sin(Angle)) * Form1.Rand.Next(0, (int)Radius);
                NextFood = new Food(Pos, Parent, Parent.EnergyPerFood, Form1.Rand.NextDouble() < Parent.SeedGrowthPercent);
            }
            public void Show(Draw D, float X,float Y,float Zoom)
            {
                Vector P = Pos * Zoom;
                float R = Radius * Zoom;
                float R2 = R * 2;
                D.Graphics.DrawLine(new Pen(Color.Brown, R), X, Y, X+P.X, Y+P.Y);
                D.Graphics.FillEllipse(new SolidBrush(Parent.Col), X+P.X-R, Y+P.Y-R, R2, R2);
                if(Energy>0)
                {
                    Vector FP = (NextFood.Pos - Parent.Pos)*Zoom;
                    float FR = NextFood.Radius * Zoom;
                    float FR2 = FR * 2;
                    D.Graphics.FillEllipse(new SolidBrush(LerpCol(Parent.Col,NextFood.Col, Energy / Parent.EnergyPerFood)),X+FP.X-FR,Y+FP.Y-FR,FR2,FR2);
                }
            }
            int Lerp(int a,int b,float t)
            {
                return (int)(a + (b - a) * t);
            }
            Color LerpCol(Color a,Color b,float t)
            {
                return Color.FromArgb(Lerp(a.R,b.R,t), Lerp(a.G, b.G, t), Lerp(a.B, b.B, t));
            }
        }
    }
}
