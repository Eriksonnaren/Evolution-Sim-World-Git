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
    public class NewTree : BaseObject,SelectableObject
    {
        //Tree
        public Vector Pos { get; set; }
        public Branch Root;
        public float Radius { get; set; } = 50;
        public float Hue { get; set; }
        public Color Col { get; set; }
        public List<Branch> Branches = new List<Branch>();
        public List<Leaf> Leaves = new List<Leaf>();
        
        Dictionary<NewTree, float> NearbyTrees = new Dictionary<NewTree, float>();
        public const float SizeScale3d = 15;
        public const float SizeScale2d = 120;
        public float TotalHeight;
        public int Generation;

        //Energy
        public float Energy;
        public bool Dead { get; set; } = false;
        Vector3 Sun;
        const float NeuronEnergyScale = 100;

        const float EnergyGainScale = 3f;
        const float BranchCost = 10;
        const float LeafCost = 20;
        const float SeedCost = 40;
        const float FlowerCost = 8;
        const float PollenCost = 4;
        const float FruitCost = 40;
        const float FruitPrepareTime = 40;//seconds
        const float FruitSpoilTime = 60;//seconds
        const float LeafDrain = 0.4f;
        const float BranchDrain = 0.2f;

        float EnergyRadius = 300;
        public float AreaMultiplier = 1;

        //Neurons
        public readonly NeuralNetwork NeuralNetwork;
        public float[] Inputs;
        int Age = 0;
        int Timer;
        public readonly int MaxTimer = 200;//slows down the tree
        const float AgeScale = 1000;//how many seconds before age = 1 = dead
        public bool Selected;
        public float MutationRate = 0.3f;

        //Flowers
        public readonly float FlowerHue;
        public readonly float FruitHue;
        public readonly Color FlowerColor = Color.Blue;
        public readonly Color FruitColor=Color.OrangeRed;
        
        public NewTree(Vector Pos,float StartEnergy=5)
        {
            Timer = Form1.Rand.Next(MaxTimer);
            Energy = StartEnergy;
            //Inputs: TreeAge,TreeEnergy,IsBranch,Layer,ChildBranches,ChildLeaves,LocalEnergy(if branch sum of all children),LocalAge,HasFlower,FruitRatio
            //Outputs: SpawnLeaf(if below -1, drop instead),LeafAngle1,LeafAngle2,LeafAngle3,SpawnBranch,BranchAngle1,BranchAngle2,ThickerBranch,LongerBranch,SpawnSeed,SpawnFlower,FruitRate
            int InputCount = 10;
            int OutputCount = 12;
            NeuralNetwork = new FeedForwardNetwork(InputCount,OutputCount,8,1,NeuralNetwork.OutputType.Linear);
            Inputs = new float[InputCount];
            FeedForwardNetwork N = NeuralNetwork as FeedForwardNetwork;

            /*for (int i = 0; i < N.WeightMatrix[0].Length; i++)
            {
                N.WeightMatrix[0][i][0] = 0;
            }
            for (int i = 0; i < N.WeightMatrix[1].Length; i++)
            {
                N.WeightMatrix[1][i][9] = 0;
            }*/
            
            N.WeightMatrix[N.ToIndex(0,3,0)] = 0.1f;
            N.WeightMatrix[N.ToIndex(0, 1, 0)] = 2;
            N.WeightMatrix[N.ToIndex(0, 10, 0)] = -2;
            N.WeightMatrix[N.ToIndex(1, 0, 9)] = 1;

            //N.WeightMatrix[0][0][0] = 1;
            N.WeightMatrix[N.ToIndex(0, 5, 1)] = -1;
            N.WeightMatrix[N.ToIndex(0, 3, 1)] = -0.5f;
            N.WeightMatrix[N.ToIndex(0, 10, 1)] = -0.3f;
            N.WeightMatrix[N.ToIndex(0, 1, 1)] = 1;
            N.WeightMatrix[N.ToIndex(1, 1, 0)] = 1;
            N.WeightMatrix[N.ToIndex(1, 1, 4)] = 1;

            N.WeightMatrix[N.ToIndex(0, 6, 2)] = 2;
            N.WeightMatrix[N.ToIndex(1, 8, 1)] = 0.5f;
            N.WeightMatrix[N.ToIndex(1, 2, 2)] = 2f;

            //N.WeightMatrix[1][8][9] = -1f;

            N.Mutate(MutationRate);


            this.Pos = Pos;
            Root = new Branch(this, null, 3, 0, 0);
            AddLeaf(Root, new Leaf(Root, 40, Form1.Rand.Next(0, 360), 0));
            Branches.Add(Root);
            Col = Color.ForestGreen;
            UpdateArea();
        }
        public NewTree(NewTree Parent,Vector Pos)
        {
            Timer = Form1.Rand.Next(MaxTimer);
            Generation = Parent.Generation+1;
            NeuralNetwork = new FeedForwardNetwork(Parent.NeuralNetwork as FeedForwardNetwork);

            MutationRate = Parent.MutationRate*(1 + NeuralNetwork.Gaussian() * 0.2f);
            LimitMax(ref MutationRate, 0.01f);
            NeuralNetwork.Mutate(MutationRate);
            

            Inputs = new float[Parent.Inputs.Length];
            this.Pos = Pos;
            Root = new Branch(this, null, 3, 0, Form1.Rand.Next(0, 360));
            AddLeaf(Root, new Leaf(Root, 40, Form1.Rand.Next(0, 360), 0));

            Branches.Add(Root);
            Col = Color.ForestGreen;
            UpdateArea();
        }
        float GetCoveredArea(float Rad1, float Rad2, float Dist)
        {
            if (Dist < Math.Abs(Rad1 - Rad2))//one is inside the other
            {
                float R = Math.Min(Rad1, Rad2);
                return R * R;
            }
            float D1 = (Dist * Dist + Rad1 * Rad1 - Rad2 * Rad2) / (2 * Dist);//from object 1 to intersect line
            float D2 = Dist - D1;//from object 2 to intersect line
            float A = GetChordArea(Rad1, D1) + GetChordArea(Rad2, D2);
            return A;
        }
        float GetChordArea(float Rad, float Dist)
        {
            return Rad * Rad * (float)Math.Acos(Dist / Rad) - Dist * Form1.Sqrt(Rad * Rad - Dist * Dist);
        }
        public void UpdateArea()
        {
            
            AreaMultiplier = 1;
            foreach (NewTree T in Form1.NewTrees)
            {
                if (T != this)
                {
                    float dist = (Pos - T.Pos).Mag();
                    if (dist < T.EnergyRadius + EnergyRadius && T.EnergyRadius > 0)//if their energy circles are touching
                    {
                        
                        float A = GetCoveredArea(EnergyRadius, T.EnergyRadius, dist);
                        float M1 = 1 - (A / ((float)Math.PI * (EnergyRadius * EnergyRadius))) / 2;
                        AreaMultiplier *= M1;
                        float M2 = 1 - (A / ((float)Math.PI * (T.EnergyRadius * T.EnergyRadius))) / 2;
                        T.AreaMultiplier *= M2;
                        NearbyTrees.Add(T,M1);
                        T.NearbyTrees.Add(this, M2);
                    }
                }
            }
        }
        public void Update()
        {
            Timer++;
            Age++;
            if(Age/(AgeScale*Form1.fps)>1)
            {
                Kill();
            }
            if (Timer>MaxTimer)
            {
                if (Selected)
                {

                }
                Sun = new Vector3(0, 0, 1).RotX(-Form1.Rand.Next(0, 90)).RotZ(-Form1.Rand.Next(0, 360));
                Timer = 0;
                UpdateEnergy();
                UpdateNeurons();
                if (Energy < 0)
                    Kill();
            }
            Selected = false;
        }
        void Kill()
        {
            Dead = true;
            foreach (var T in NearbyTrees)
            {
                T.Key.AreaMultiplier /= T.Value;
                T.Key.NearbyTrees.Remove(this);
            }
            NearbyTrees.Clear();
        }
        void LimitMax(ref float Input,float L)
        {
            if (Input < L)
                Input = L;
        }
        void UpdateEnergy()
        {
            List<NewTree> NearbyList = new List<NewTree>() { this };
            double SunSlope = Sun.Z*Sun.Z/(Sun.X*Sun.X+Sun.Y*Sun.Y+1);
            float S = SizeScale2d * SizeScale2d;
            foreach (var T in Form1.NewTrees)
            {
                if (T != this)
                {
                    Vector DPos = T.Pos - Pos;
                    if (T.TotalHeight * T.TotalHeight * S > SunSlope * DPos.MagSq())
                    {
                        NearbyList.Add(T);
                    }
                }
            }
            foreach (var L in Leaves)
            {
                Energy += AreaMultiplier*EnergyGainScale*MaxTimer*L.GetSunEnergy(Sun, NearbyList)/Form1.fps;
            }
            Branches[0].GetLocalEnergy();
            Energy -= (Leaves.Count * LeafDrain+Branches.Count*BranchDrain) * MaxTimer / Form1.fps;
        }
        void UpdateNeurons()
        {
            Inputs[0]=Age/(AgeScale*Form1.fps);
            Inputs[1]=Energy/(NeuronEnergyScale);

            for (int i = Branches.Count-1; i >=0; i--)
            {
                Branches[i].Update(NeuralNetwork);
            }
            for (int i = Leaves.Count - 1; i >= 0; i--)
            {
                Leaves.ElementAt(i).Update(NeuralNetwork);
            }
        }
        public void AddLeaf(Branch B,Leaf L)
        {
            B.Leaves.Add(L,"");
            Leaves.Add(L);
        }
        public bool PointOnTree(Vector P)
        {
            foreach (var B in Branches)
            {
                if (B.PointOnBranch(P))
                    return true;
            }
            foreach (var L in Leaves)
            {
                if (L.PointInLeaf(P))
                    return true;
            }
            if ((P - Pos).MagSq() < 5000)
                return true;
            return false;
        }
        public List<BaseObject> GetAllFlowers()
        {
            List<BaseObject> Objects = new List<BaseObject>();
            foreach (Leaf L in Leaves)
            {
                if (L.CurrentFlower != null)
                    Objects.Add(L.CurrentFlower);
            }
            return Objects;
        }
        public void Show2D(Draw D, float X, float Y, float Zoom = 1)
        {
            Root.Show2D(D, X, Y, Zoom);
        }
        public void ShowSelected(Draw D,Draw3d D3)
        {
            Root.Show3D(D3,Sun);
        }
        [Serializable]
        public class Branch
        {
            public SortedList<Leaf,string> Leaves = new SortedList<Leaf, string>();
            public List<Branch> Branches=new List<Branch>();
            readonly float Pitch;
            readonly float Yaw;
            public float Length;
            public float Width = 0.3f;
            public Matrix3 RotMatrix;
            public Vector3 Base;
            public Vector3 Tip;
            public NewTree Tree;
            public Branch Parent;
            public int Layer;
            int Age = 0;
            float LocalEnergy = 0;

            public Branch(NewTree Tree, Branch Parent, float Length, float Pitch, float Yaw)
            {
                
                
                this.Parent = Parent;
                
                this.Tree = Tree;
                this.Pitch = Pitch;
                this.Yaw = Yaw;
                
                this.Length = Length;
                Tip = new Vector3(0, 0, Length);
                Tip = Tip.RotY(Pitch).RotZ(Yaw);
                if (Parent != null)
                {
                    Layer = Parent.Layer + 1;
                    Base = Parent.Tip;
                    Tip=Parent.RotMatrix * Tip;
                }
                else
                {
                    Base = new Vector3();
                }
                
                if(Tip.X==0&&Tip.Y==0)
                {
                    float S = Form1.Sin(Yaw);
                    float C = Form1.Cos(Yaw);
                    Vector3 I = new Vector3(C, -S, 0);
                    Vector3 J = new Vector3(S, C, 0);
                    Vector3 K = new Vector3(0, 0, 1);
                    RotMatrix = new Matrix3(I,J,K);
                }else
                {
                    RotMatrix = new Matrix3(Tip);
                }
                Tip += Base;
                if (Tip.Z > Tree.TotalHeight)
                    Tree.TotalHeight = (float)Tip.Z;
            }
            public void Update(NeuralNetwork Network)
            {
                Age++;
                float[] Inputs = Tree.Inputs;
                Inputs[2] = 1;
                Inputs[3] = Layer / 3f;
                Inputs[4] = Branches.Count / 2f;
                Inputs[5] = Leaves.Count / 2f;
                Inputs[6] = Tree.Leaves.Count == 0 ? 0 : LocalEnergy / Tree.Leaves.Count;
                Inputs[7] = Age / (AgeScale * Form1.fps);
                Inputs[8] = Inputs[9] = 0;
                Network.SetInputs(Inputs);
                Network.Update();
                float[] Outputs = Network.GetOutputs();
                if(Outputs[0]>0)
                {
                    if (Tree.Energy > LeafCost)
                    {
                        Tree.Energy -= LeafCost;
                        Leaf L = new Leaf(this, (Network.Clamp(Outputs[1],-1,1)) * 90, (Outputs[2] + 1) * 180, Outputs[3] * 90);
                        Tree.AddLeaf(this, L);
                    }else
                    {
                        Tree.Energy -= LeafCost / 20f;
                    }
                }
                if(Outputs[4] > 0)
                {
                    if (Tree.Energy > BranchCost)
                    {
                        Tree.Energy -= BranchCost;
                        Branch B = new Branch(Tree, this, 3, (Network.Clamp(Outputs[5],-1,1))*90, Outputs[6] * 180);
                        Branches.Add(B);
                        Tree.Branches.Add(B);
                    }else
                    {
                        Tree.Energy -= BranchCost / 20f;
                    }
                }

            }
            public float GetLocalEnergy()
            {
                LocalEnergy = 0;
                foreach (var B in Branches)
                {
                    LocalEnergy += B.GetLocalEnergy();
                }
                foreach (var L in Leaves.Keys)
                {
                    LocalEnergy += L.EnergyFlow;
                }
                return LocalEnergy;
            }
            public bool PointOnBranch(Vector V)
            {
                Vector V1 = new Vector(Tip* SizeScale2d) + Tree.Pos;
                Vector V2 = new Vector(Base* SizeScale2d) + Tree.Pos;
                float T = (V1 - V2) * (V - V2)/ (V1 - V2).MagSq();
                Vector Point = Vector.Lerp(V1, V2, T);
                float Length2 = (V - Point).MagSq();
                float W = Width * SizeScale2d*2;
                return T < 1 && T > 0 && Length2 < W * W;
            }
            public void Show2D(Draw D, float X, float Y, float Zoom)
            {
                Pen pen = new Pen(Color.SaddleBrown)
                {
                    EndCap = System.Drawing.Drawing2D.LineCap.Round,
                    StartCap = System.Drawing.Drawing2D.LineCap.Round
                };
                foreach (var I in Branches)
                {
                    I.Show2D(D, X, Y, Zoom);
                }
                foreach (var L in Leaves.Keys)
                {
                    L.Show2D(D, X, Y, Zoom);
                }
                float Scale = SizeScale2d * Zoom;
                pen.Width = Width * Scale;
                if(Pitch==0)
                {
                    float R = pen.Width / 2;
                    D.Graphics.FillEllipse(new SolidBrush(pen.Color), X + (float)Base.X-R, Y + (float)Base.Y-R,R*2,R*2);
                }else
                D.Graphics.DrawLine(pen,X+ (float)Base.X * Scale, Y+ (float)Base.Y * Scale, X + (float)Tip.X * Scale, Y + (float)Tip.Y * Scale);
            }
            public void Show3D(Draw3d D,Vector3 Sun)
            {
                
                foreach (var I in Branches)
                {
                    I.Show3D(D,Sun);
                }
                foreach (var I in Leaves.Keys)
                {
                    I.Show3D(D);
                    //(Sun * 15*5).Show(I.Center*15,D,Color.Orange,3);
                }
                D.addLine(Color.SaddleBrown, Width * SizeScale3d, Base * SizeScale3d, Tip * SizeScale3d);
            }
        }
        [Serializable]
        public class Leaf:IComparable<Leaf>
        {
            readonly float Pitch;
            readonly float Yaw;
            readonly float Roll;
            public Vector3 Base;
            public Vector3 Tip1;
            public Vector3 Tip2;
            public Vector3 Center;
            Vector3 RayPoint;
            Vector3 Norm;
            Matrix3 RotMatrix;
            readonly Branch Parent;
            public readonly NewTree Tree;
            public float EnergyFlow=0.5f;
            Color Col=Color.ForestGreen;
            int Age;
            NewSeed CurrentSeed;
            public Flower CurrentFlower;
            float SeedTimer = 0;
            const float SeedTimerMax = 20;
            public Leaf(Branch Parent,float Pitch, float Yaw, float Roll)
            {
                this.Parent = Parent;
                this.Pitch = Pitch;
                this.Yaw = Yaw;
                this.Roll = Roll;
                Tree = Parent.Tree;
                Base = Parent.Tip;
                RotMatrix = new Matrix3(Parent.RotMatrix);
                //RotMatrix = RotMatrix.RotZ(Yaw).RotX(Pitch).RotZ(Roll);
                RotMatrix = RotMatrix.RotZ(Yaw).RotY(Pitch).RotZ(Roll);
                Tip1 = RotMatrix * new Vector3(0,1,2);
                Tip2 = RotMatrix * new Vector3(0, -1, 2);
                Norm = Vector3.Cross(Tip1, Tip2);
                Norm.Norm();
                Tip1 += Base;
                Tip2 += Base;
                Center = (Base + Tip1 + Tip2) / 3;
                if (Center.Z > Tree.TotalHeight)
                    Tree.TotalHeight = (float)Center.Z;
            }
            float Sign(Vector p1, Vector p2, Vector p3)
            {
                return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
            }

            public bool PointInLeaf(Vector pt)
            {
                Vector v1 = new Vector(Base * SizeScale2d) + Tree.Pos;
                Vector v2 = new Vector(Tip1 * SizeScale2d) + Tree.Pos;
                Vector v3 = new Vector(Tip2 * SizeScale2d) + Tree.Pos;
                bool b1, b2, b3;
                b1 = Sign(pt, v1, v2) < 0.0f;
                b2 = Sign(pt, v2, v3) < 0.0f;
                b3 = Sign(pt, v3, v1) < 0.0f;

                return ((b1 == b2) && (b2 == b3));
            }
            public void Show2D(Draw D, float X, float Y, float Zoom)
            {
                float Scale = SizeScale2d * Zoom;
                PointF[] Points = new PointF[]
                {
                    new PointF((float)(X+Base.X*Scale),(float)(Y+Base.Y*Scale)),
                    new PointF((float)(X+Tip1.X*Scale),(float)(Y+Tip1.Y*Scale)),
                    new PointF((float)(X+Tip2.X*Scale),(float)(Y+Tip2.Y*Scale)),
                };
                D.Graphics.FillPolygon(new SolidBrush(Col),Points);
                if(CurrentSeed!=null)
                {
                    CurrentSeed.Show(D,(float)(X+Center.X*Scale), (float)(Y + Center.Y * Scale),Zoom);
                }
                if(CurrentFlower!=null)
                {
                    CurrentFlower.Show(D, (float)(X + Center.X * Scale), (float)(Y + Center.Y * Scale), Zoom);
                }
            }
            public void Show3D(Draw3d D)
            {
                Vector3[] Points = new Vector3[] {Base*SizeScale3d,Tip1 * SizeScale3d, Tip2 * SizeScale3d };
                Polygon P = new Polygon(Col,Points);
                D.Polygons.Add(P);
                if (CurrentSeed != null)
                {
                    float Radius = SizeScale3d * CurrentSeed.Radius/SizeScale2d;
                    Vector3 V = new Vector3(Radius, Radius, Radius);
                    Polygon[] Polys = D.getCuboid(CurrentSeed.Col, Center * SizeScale3d - V,Center * SizeScale3d + V);
                    D.Polygons.AddRange(Polys);
                }
                if(CurrentFlower!=null)
                {
                    CurrentFlower.Show3d(D);
                }
            }
            public void Update(NeuralNetwork Network)
            {
                Age++;
                if(CurrentSeed != null)
                {
                    SeedTimer += Tree.MaxTimer / Form1.fps;
                    if(SeedTimer>SeedTimerMax)
                    {
                        Form1.NewSeeds.Add(CurrentSeed);
                        CurrentSeed.Fall();
                        CurrentSeed = null;
                    }
                }
                float L1 = (float)Form1.Rand.NextDouble();
                float L2 = (float)Form1.Rand.NextDouble();
                if(L1+L2>1)
                {
                    L1 = 1 - L1;
                    L2 = 1 - L2;
                }
                RayPoint = Base + (Tip1 - Base) * L1+ (Tip2 - Base) * L2;

                float[] Inputs = Tree.Inputs;
                Inputs[2] = 0;
                Inputs[3] = (Parent.Layer+1) / 3f;
                Inputs[4] = 0;
                Inputs[5] = CurrentSeed==null?0:1;
                Inputs[6] = EnergyFlow;
                Inputs[7] = Age / (AgeScale * Form1.fps);
                Inputs[8] = CurrentFlower != null ? 1 : 0;
                Inputs[9] = CurrentFlower != null ? (CurrentFlower.CurrentEnergy/FruitCost): 0;
                Network.SetInputs(Inputs);
                Network.Update();
                float[] Outputs = Network.GetOutputs();
                if(CurrentFlower!=null)
                {
                    CurrentFlower.Update(Network.Clamp(Outputs[11],0,1));
                }
                if (Outputs[0] < -1)
                {
                    Tree.Energy += LeafCost / 2;
                    Tree.Leaves.Remove(this);
                    Parent.Leaves.Remove(this);
                }
                else
                {
                    if (Outputs[9] > 0)
                    {
                        if (CurrentSeed == null && CurrentFlower == null && Tree.Energy > SeedCost)
                        {
                            Tree.Energy -= SeedCost;
                            CurrentSeed = new NewSeed(Tree,this);

                        }
                        else
                        {
                            Tree.Energy -= SeedCost / 20;
                        }
                    }
                    if (Outputs[10] > 0)
                    {
                        if (CurrentSeed == null && CurrentFlower == null && Tree.Energy > FlowerCost)
                        {
                            Tree.Energy -= FlowerCost;
                            CurrentFlower = new Flower(this);
                        }
                        else
                        {
                            Tree.Energy -= FlowerCost / 20;
                        }
                    }
                }
            }
            public float GetSunEnergy(Vector3 SunVector,List<NewTree> Trees)
            {
                if (Center.Z < 0)
                {
                    AverageEnergy(0);
                    return 0;
                }
                foreach (NewTree Tree in Trees)
                {
                    if (TreeBlockSun(SunVector,Tree))
                    {
                        AverageEnergy(0);
                        return 0;
                    }
                }
                float CurrentEnergy = Math.Abs((float)(Norm * SunVector));
                AverageEnergy(CurrentEnergy);
                return CurrentEnergy;
            }
            void AverageEnergy(float NextEnergy)
            {
                EnergyFlow = EnergyFlow + (NextEnergy-EnergyFlow) / 5;
                Col = Color.Red.Lerp(Color.ForestGreen,Math.Min(EnergyFlow*2,1));
            }
            public bool TreeBlockSun(Vector3 SunVector,NewTree Tree)
            {
                foreach (var item in Tree.Branches)
                {
                    if (BranchBlockSun(SunVector, item))
                    {
                        return true;
                    }
                }
                foreach (var item in Tree.Leaves)
                {
                    if (LeafBlockSun(SunVector, item))
                    {
                        return true;
                    }
                }
                return false;
            }
            bool BranchBlockSun(Vector3 SunVector,Branch Branch)
            {
                #region Math
                /*L1(t) = Branch.Base + BranchVector*t
                  L2(t) = RayPoint + SunVector*t

                  (L1(s)-L2(r)) dot BranchVector=0
                  (L1(s)-L2(r)) dot SunVector=0

                  (Branch.Base - RayPoint + BranchVector*s - SunVector*r) dot BranchVector = 0
                  (Branch.Base - RayPoint + BranchVector*s - SunVector*r) dot SunVector = 0

                  a1*s - b1*r = c1
                  a2*s - b2*r = c2

                  L1(s)-L2(r) is distance at closest point
              */
                #endregion
                Vector3 RelPos = new Vector3(Tree.Pos - Branch.Tree.Pos);

                Vector3 BranchVector = (Branch.Tip - Branch.Base)*SizeScale2d;
                Vector3 L1(double t) { return Branch.Base * SizeScale2d+RelPos + BranchVector * t; }
                Vector3 L2(double t) { return RayPoint * SizeScale2d + SunVector * t; }
                double a1 = BranchVector * BranchVector;
                double b1 = -SunVector * BranchVector;
                double c1 = -(((Branch.Base - RayPoint) * SizeScale2d+RelPos) * BranchVector);
                double a2 = BranchVector * SunVector;
                double b2 = -SunVector * SunVector;
                double c2 = -(((Branch.Base - RayPoint) * SizeScale2d+RelPos) * SunVector);

                double det = a1 * b2 - a2 * b1;
                double r = (a1 * c2 - a2 * c1) / det;
                double s = (b2 * c1 - b1 * c2) / det;
                if (det == 0)
                    return false;
                if (s > 1 || s < 0||r<0)
                    return false;
                double V = (L1(s) - L2(r)).Mag();
                return V <= Branch.Width * SizeScale2d / 2;
                
            }
            bool LeafBlockSun(Vector3 SunVector, Leaf Leaf)
            {
                Vector3 RelPos = new Vector3(Tree.Pos-Leaf.Tree.Pos);
                if (Leaf == this)
                    return false;
                Vector3 B1 = (Leaf.Tip1 - Leaf.Base) * SizeScale2d;
                Vector3 B2 = (Leaf.Tip2 - Leaf.Base) * SizeScale2d;
                Vector3 CB = (RayPoint -Leaf.Base) * SizeScale2d+RelPos;
                double Det = -(SunVector * Vector3.Cross(B1, B2));
                double ts = Vector3.Cross(B1,B2)*CB / Det;
                double t1 = -(Vector3.Cross(B2, SunVector) * CB) / Det;
                double t2 = -(Vector3.Cross(SunVector, B1) * CB) / Det;
                return ts>-0.1&&t1<1&&t1>0&& t2 < 1 && t2 > 0&&t1+t2<1;
            }

            public int CompareTo(Leaf L)
            {
                if (Center.Y == L.Center.Y)
                    return 1;
                return (Center.Y).CompareTo(L.Center.Y);
            }

            [Serializable]
            public class Flower:BaseObject
            {
                public Vector Pos { get; set; }
                public Color Col { get; set; }
                public float Hue { get; set; }
                public float Radius { get; set; }
                public Leaf Leaf;
                float Angle;
                public Flower Pollen;
                public float CurrentEnergy;
                public bool FruitDone;
                float SpoilTimer = 0;
                public Flower(Leaf Leaf)
                {
                    Pos = Leaf.Tree.Pos + new Vector(Leaf.Center) * SizeScale2d;
                    Hue = 0.85f;
                    Col = Leaf.Tree.FlowerColor;
                    Radius = 10;
                    this.Leaf = Leaf;
                    Angle = Form1.Rand.Next(360);
                }
                public void CollideWith(Creature C)
                {
                    if (Pollen==null)
                    {
                        if (C.Pollen != null)
                        {
                            if (C.Pollen != this)
                            {
                                Pollen = C.Pollen;
                                C.Pollen = null;
                            }
                        }
                        else
                        {
                            C.Pollen = this;
                            Leaf.Tree.Energy -= PollenCost;
                        }
                    }
                }
                public void Update(float FruitRate)
                {
                    if(FruitDone)
                    {
                        SpoilTimer += Leaf.Tree.MaxTimer/Form1.fps;
                        if (SpoilTimer > FruitSpoilTime)
                            Leaf.CurrentFlower = null;
                    }
                    else
                    if (Pollen!=null)
                    {
                        float EnergyChange = FruitRate * FruitCost* Leaf.Tree.MaxTimer / (FruitPrepareTime * Form1.fps);
                        CurrentEnergy += EnergyChange;
                        Leaf.Tree.Energy -= EnergyChange;
                        if(CurrentEnergy>=FruitCost)
                        {
                            FruitDone = true;
                            CurrentEnergy = FruitCost;
                        }
                        float Ratio = CurrentEnergy / FruitCost;
                        Col = Leaf.Tree.FlowerColor.Lerp(Leaf.Tree.FruitColor, Ratio*Ratio);
                    }
                }
                public void Show(Draw D, float x, float y, float zoom)
                {
                    float Ratio = CurrentEnergy / FruitCost;
                    float Rad1 = Radius * zoom;
                    float Rad2 = Rad1 * 0.8f;
                    Vector[] RootOfUnity = { new Vector(1, 0), new Vector(-0.5f, 0.866f), new Vector(-0.5f, -0.866f) };

                    Matrix M = D.Graphics.Transform.Clone();
                    D.Graphics.TranslateTransform(x, y);
                    D.Graphics.RotateTransform(Angle);
                    for (int i = 0; i < 3; i++)
                    {
                        Vector V = RootOfUnity[i] * Rad1 * Lerp(0.8f,0.5f,Ratio);
                        D.Graphics.FillEllipse(Brushes.White, V.X - Rad2, V.Y - Rad2, 2 * Rad2, 2 * Rad2);
                    }
                    Rad1 *= Lerp(1, 1.5f, Ratio);
                    D.Graphics.FillEllipse(new SolidBrush(Col), -Rad1, -Rad1, 2 * Rad1, 2 * Rad1);
                    D.Graphics.Transform = M;

                }
                public void Show3d(Draw3d D3)
                {
                    float Radius = SizeScale3d * this.Radius / SizeScale2d;
                    Vector3 V = new Vector3(Radius, Radius, Radius);
                    Polygon[] Polys = D3.getCuboid(Col, Leaf.Center * SizeScale3d - V, Leaf.Center * SizeScale3d + V);
                    D3.Polygons.AddRange(Polys);
                }
                public float Lerp(float A, float B, float T) => A + (B - A) * T;
            }
        }
    }
}
