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
    public class Creature:MovingEntity, Visible,CollideMove, SelectableObject,FamilyMember
    {
        //Creature
        public int DrawLayer { get; } = 5;
        public Vector Pos { get; set; }
        public VectorI ChunkPos { get; set; }
        public Vector MouthPos=new Vector(0.6f,0);//percentage of radius
        float MouthRadius = 0.7f;//percentage of radius
        public float Friction { get; } = 0.1f;
        float AngleFriction = 0.1f;
        public float BaseSpeed = 2f;
        public float BaseAngleSpeed = 0.5f;
        public Vector Vel { get; set; }
        public float Mass { get; } = 10;
        public bool EnableCollide { get; set; } = true;
        public float AngleVel;
        public float Angle;//in degrees
        public float Radius { get; } = 40;
        public Vector AngleVector;
        public float Energy=500;
        public float StartEnergy = 500;
        float EggEnergy=500;
        public bool Selected { get; set; }
        bool SelectedPrev = false;
        int Age = 0;
        public bool Dead { get; set; } = false;
        public string Name { get; set; }
        float MaxSpeed;
        public Tree.Leaf.Flower Pollen;
        //Eyes
        public const float EyeRadius=800;
        //float EyeRadiusExtra=100;
        public List<Eye> Eyes = new List<Eye>();
        public Color Col { get; set; }
        public float Hue { get; set; }
        public float EggHue;
        //Brain
        public NeuralNetwork Brain;
        [Serializable]
        class Food
        {
            public float Energy;
            public bool Seed;
            public float MaxEnergy;
            public Color Col;
            public Tree.Leaf.Flower Parent;
            public Food(Tree.Leaf.Flower Parent)
            {
                this.Parent = Parent;
                Seed = Form1.Rand.NextDouble() < 0.6;
                Col = Parent.Leaf.Tree.FruitColor;
                MaxEnergy = Energy = 250;
            }
            
        }
        List<Food> EatenFood = new List<Food>();
        float[] Inputs;
        float[] Outputs;
        bool TurnWithOutput = false;//if true out[0]=trottle, out[1]=turn
        int ConstantId;
        int EnergyId;
        int SpeedId;
        int ListenId;
        int PollenId;
        int ManualSpd = 0;
        int ManualRot = 0;
        bool UseSpike;
        public float TalkValue;
        double MaxEatLeafHeight = 5;
        int LeafEatTimer = 0;
        const int LeafEatTimerMax = 100;
        //Family
        public List<FamilyMember> Parents { get; set; }
        public List<FamilyMember> Children { get; set; }
        public int Generation { get; set; }
        public int FamilyPos { get; set; }
        public Family Family { get; set; }
        public Egg IsEgg;

        bool Collided;

        public Creature()
        {
            Children = new List<FamilyMember>();
            MaxSpeed = ((1 - Friction)*BaseSpeed) / Friction;
            Name = GenerateName();
            Hue = (float)Form1.Rand.NextDouble();
            EggHue = (float)Form1.Rand.NextDouble();
            Col = Form1.ColorFromHue(Hue);
            Angle = Form1.Rand.Next(0, 360);
            AngleVector = new Vector(Form1.Cos(Angle), Form1.Sin(Angle));

            int EyeAmount = 3;
            int OutputCount = 4;
            int InputCount = EyeAmount * 3 + 5;
            //Brain = new RecurrentNetwork(InputCount, OutputCount,7, Form1.Rand.Next(10, 35));
            //Brain = new SpikingNetwork(InputCount, OutputCount,1,10, Form1.Rand.Next(30, 100));
            Brain = new MemoryNetwork(InputCount, OutputCount, 7);
            Brain.GenerateRandom(-1,1);
            //Brain = new FeedForwardNetwork(InputCount, OutputCount,7,3);
            
            Inputs = new float[InputCount];
            Outputs = new float[OutputCount];
            ConstantId = InputCount - 1;
            Inputs[ConstantId] = 1;
            EnergyId = InputCount - 2;
            SpeedId = InputCount - 3;
            ListenId = InputCount - 4;
            PollenId = InputCount - 5;
            for (int i = 0; i < EyeAmount; i++)
            {
                float angle = Form1.Rand.Next(0, 360);
                Eyes.Add(new Eye(this, new Vector(Form1.Cos(angle), Form1.Sin(angle)) * Radius * 1f,(float)Form1.Rand.NextDouble()));
            }
        }
        
        public void Update()
        {
            foreach (Eye E in Eyes)
            {
                E.Update();
            }
            updateBrain();
            UseSpike = Outputs[2]>0.5;
            Move();
            UpdateEnergy();
            if(Energy>EggEnergy+StartEnergy)
            {
                Energy -= EggEnergy*1.25f;
                UpdateWorld.AddEntity(new Egg(this),Pos,ChunkPos);
            }
            Age++;
            if (LeafEatTimer > 0)
                LeafEatTimer--;
        }
        void Move()
        {
            if (!(Selected && Form1.ManualControl))//auto
            {
                float Rot, Spd;
                if (TurnWithOutput)
                {
                    Rot = Outputs[1];
                    Spd = Outputs[0];
                }
                else
                {
                    Rot = (Outputs[1] - Outputs[0]) / 2;
                    Spd = (Outputs[1] + Outputs[0]) / 2;
                }
                AngleVel += BaseAngleSpeed * Rot;
                Vel += AngleVector * BaseSpeed * Spd;
            }
            else
            {
                ManualSpd = ManualRot = 0;
                if (Form1.HoldKeys.Contains(System.Windows.Forms.Keys.W))
                {
                    ManualSpd = 1;
                }
                if (Form1.HoldKeys.Contains(System.Windows.Forms.Keys.S))
                {
                    ManualSpd = -1;
                }
                if (Form1.HoldKeys.Contains(System.Windows.Forms.Keys.D))
                {
                    ManualRot = 1;
                }
                if (Form1.HoldKeys.Contains(System.Windows.Forms.Keys.A))
                {
                    ManualRot = -1;
                }
                Vel += BaseSpeed * AngleVector * ManualSpd;
                AngleVel += BaseAngleSpeed * ManualRot;
            }
            Angle += AngleVel;
            AngleVector.X = Form1.Cos(Angle);
            AngleVector.Y = Form1.Sin(Angle);
            AngleVel *= (1 - AngleFriction);
            SelectedPrev = Selected;
            Selected = false;
        }
        void UpdateEnergy()
        {
            float PassiveDrain = 0.15f;
            float ActiveDrain = 0.2f;
            Energy -= ActiveDrain * (Math.Abs(Outputs[0]) + Math.Abs(Outputs[1])) / 2 + PassiveDrain * (1 + Age / (400 * Form1.fps));
            if (EatenFood.Count > 0)
            {
                float TotalFoodEnergy = 0;
                for (int i = 0; i < EatenFood.Count; i++)
                {
                    TotalFoodEnergy += EatenFood[i].Energy;
                }
                float FoodGain = (TotalFoodEnergy * 0.005f)+0.1f;
                EatenFood[0].Energy -= FoodGain;
                Energy += FoodGain;
                if(EatenFood[0].Energy<=0.01f)
                {
                    if (EatenFood[0].Seed)
                    {
                        UpdateWorld.AddEntity(new Seed(EatenFood[0].Parent, Pos, ChunkPos),Pos,ChunkPos);
                    }
                    EatenFood.RemoveAt(0);

                }
            }
            if (Energy <= 0)
            {
                Kill();
            }
        }
        public void Kill()
        {
            Dead = true;
            Energy = 0;
            foreach (var E in Eyes)
            {
                E.LookAt = null;
            }
            if (Family != null)
            {
                Family.Remove(this);
            }
            UpdateWorld.RemoveEntity(this);
        }
        public void Show(Draw D, float x, float y, float Zoom = 1)
        {
            if(IsEgg!=null)
            {
                IsEgg.Show(D,x,y,Zoom);
            }else
            {
                ShowThis(D, x, y, Zoom);
            }
            
        }
        void ShowThis(Draw D, float x, float y, float Zoom)
        {

            //Body
            float R = Radius * Zoom;
            float MR = R * MouthRadius;
            Vector MP = MouthPos * R;
            float MR2 = MR * 2;
            float R2 = R * 2;
            Matrix M = D.Graphics.Transform.Clone();
            D.Graphics.TranslateTransform(x, y);
            D.Graphics.RotateTransform(Angle);
            D.Graphics.FillEllipse(Brushes.White, -MR + MP.X, -MR + MP.Y, MR2, MR2);
            if(Pollen!=null)
            {
                float Rad = R / 5;
                D.Graphics.FillEllipse(new SolidBrush(Pollen.Col), -Rad + R, -Rad, Rad*2, Rad*2);
            }
            Boolean Extend;
            if(!(Selected && Form1.ManualControl))
            {
                Extend = UseSpike;
            }
            else
            {
                Extend = Form1.HoldKeys.Contains(System.Windows.Forms.Keys.Space);
            }
            if (Extend)
            {
                PointF[] Points = new PointF[] {
                    new PointF(-MR + MP.X, -MR),
                    new PointF(-MR + MP.X, +MR),
                    new PointF(MR*1.2f + MP.X, 0) };
                D.Graphics.FillPolygon(Brushes.Black,Points);
            }
            if(Collided)
                D.Graphics.FillEllipse(Brushes.Red, -R, -R, R2, R2);
            else
                D.Graphics.FillEllipse(new SolidBrush(Col), -R, -R, R2, R2);
            Collided = false;

            //thrusters
            float ThrustR = R * 0.75f;
            float ThrustR2 = ThrustR * 2;
            float ThrustAngle = 40;
            float ThrusterWidth = Zoom * 10;
            float ThrustRight, ThrustLeft;
            if (Form1.ManualControl && SelectedPrev)
            {
                ThrustLeft = Clamp(ManualSpd+ManualRot);
                ThrustRight = Clamp(ManualSpd - ManualRot);
            }
            else
            {
                if (TurnWithOutput)
                {
                    ThrustRight = Clamp(Outputs[0] - Outputs[1]);
                    ThrustLeft = Clamp(Outputs[0] + Outputs[1]);
                }
                else
                {
                    ThrustRight = Outputs[0];
                    ThrustLeft = Outputs[1];
                }
            }
            D.Graphics.DrawArc(new Pen(Color.Black, ThrusterWidth), -ThrustR, -ThrustR, ThrustR2, ThrustR2,90- ThrustAngle, ThrustAngle * 2);
            D.Graphics.DrawArc(new Pen(Color.Black, ThrusterWidth), -ThrustR, -ThrustR, ThrustR2, ThrustR2, -90 - ThrustAngle, ThrustAngle * 2);
            if (Math.Abs(ThrustRight) > 0.1)
                D.Graphics.DrawArc(new Pen(Color.White, ThrusterWidth), -ThrustR, -ThrustR, ThrustR2, ThrustR2, 90, ThrustAngle*ThrustRight);
            if(Math.Abs(ThrustLeft) >0.1)
                D.Graphics.DrawArc(new Pen(Color.White, ThrusterWidth), -ThrustR, -ThrustR, ThrustR2, ThrustR2, -90, -ThrustAngle * ThrustLeft);

            //FoodDisplay
            float W = R / 7;
            float W2 = W * 2;
            float H = R*0.59f;
            D.Graphics.FillRectangle(Brushes.Black,-H,-W,H*2,W*2);

            if (EatenFood.Count > 0)
            {
                float Offset = W2*(1-EatenFood[0].Energy / EatenFood[0].MaxEnergy);
                for (int i = 0; i < EatenFood.Count&&i<4; i++)
                {
                    D.Graphics.FillEllipse(new SolidBrush(EatenFood[i].Col), -H + W2 * i-Offset, -W, W2, W2);
                }
            }

            //EnergyMeter
            Color EnergyBackground = Energy < StartEnergy ? Color.Black : Color.Orange;
            Color EnergyForground = Energy < StartEnergy ? Color.Orange : Color.DeepPink;
            float EnergyPercent = Energy < StartEnergy ? Energy / StartEnergy : (Energy - StartEnergy) / EggEnergy;
            if(EnergyPercent > 1)EnergyPercent = 1;
            else if (EnergyPercent < 0.02f) EnergyPercent = 0.02f;
            D.Graphics.DrawArc(new Pen(EnergyBackground, ThrusterWidth), -ThrustR, -ThrustR, ThrustR2, ThrustR2, 180-ThrustAngle, ThrustAngle*2);
            D.Graphics.DrawArc(new Pen(EnergyForground, ThrusterWidth), -ThrustR, -ThrustR, ThrustR2, ThrustR2, 180 - ThrustAngle*EnergyPercent, ThrustAngle * 2*EnergyPercent);



            //Eyes
            foreach (Eye E in Eyes)
            {
                E.Show(D,Zoom);
            }
            //D.Graphics.DrawEllipse(new Pen(Col, 2 * Zoom), -EyeRadius*Zoom, -EyeRadius * Zoom, EyeRadius * Zoom * 2, EyeRadius * Zoom * 2);

            //float ER = (EyeRadius + Radius + EyeRadiusExtra)*Zoom;
            D.Graphics.Transform = M;

            /*foreach (Eye E in Eyes)
            {
                if (E.LookAt != null)
                {
                    Vector P = (E.Pos * Zoom).Rot(AngleVector);
                    D.Graphics.DrawLine(new Pen(E.Col, 1), x + P.X, y + P.Y, x + E.RelPos.X * Zoom, y + E.RelPos.Y * Zoom);
                }
            }*/

            if (Selected&&D.EyeRings)
            {
                foreach (Eye E in Eyes)
                {
                    if (E.LookAt!=null)
                    {
                        //PointF Po = D.WorldToScreen(Pos + E.Pos.Rot(AngleVector));
                        PointF Po = D.WorldToScreen(ChunkPos, Pos + E.Pos.Rot(AngleVector));
                        float Dist = EyeRadius * Zoom * (1 - E.value);

                        D.Graphics.DrawEllipse(new Pen(E.LookAt.Col, 2 * Zoom), Po.X - Dist, Po.Y - Dist, Dist * 2, Dist * 2);
                    }
                }
            }

            

            if (Selected && D.EyeLines)
            {
                foreach (Eye E in Eyes)
                {
                    if (E.value > 0)
                    {
                        PointF Po = D.WorldToScreen(Pos + E.Pos.Rot(AngleVector));
                        PointF Po2 = D.WorldToScreen(E.LookAt.Pos);

                        D.Graphics.DrawLine(new Pen(E.LookAt.Col, 2 * Zoom),Po,Po2);
                    }
                }
            }
            if(Dead)
                Selected = false;
        }
        /*
        void UpdateNearby()
        {
            PreviousNearbyUpdateTicks++;
            if ((PreviousNearbyUpdatePos - Pos).MagSq() > EyeRadiusExtra * EyeRadiusExtra||PreviousNearbyUpdateTicks>PreviousNearbyUpdateTicksMax)
            {
                PreviousNearbyUpdateTicks = 0;
                PreviousNearbyUpdatePos = Pos;
                NearbyObjects = new List<BaseObject>();
                float R = EyeRadius + EyeRadiusExtra*2 + Radius;
                float RR = R * R;
                foreach (Creature C in Form1.Creatures)
                {
                    if (C != this)
                        if ((C.Pos - Pos).MagSq() < RR)
                            NearbyObjects.Add(C);
                        
                }
                foreach (var S in Form1.Seeds)
                {
                    if ((S.Pos - Pos).MagSq() < RR)
                        NearbyObjects.Add(S);
                }
                foreach (Tree T in Form1.Trees)
                {
                    if ((T.Pos - Pos).MagSq() < RR)
                        NearbyObjects.Add(T);
                }
                foreach (Egg E in Form1.Eggs)
                {
                    if ((E.Pos - Pos).MagSq() < RR)
                        NearbyObjects.Add(E);
                }
                foreach (var T in Form1.Trees)
                {
                    foreach (var F in T.GetAllFlowers())
                    {
                        if ((F.Pos - Pos).MagSq() < RR)
                            NearbyObjects.Add(F);
                    }
                }
            }
        }*/
        public void UpdateVision(Visible V)
        {
            foreach (var E in Eyes)
            {
                E.Add(V);
            }
        }
        void updateBrain()
        {
            //input
            for (int i = 0; i < Eyes.Count; i++)
            {
                Inputs[i * 3] = Eyes[i].Col.R / 255.0f;
                Inputs[i * 3 + 1] = Eyes[i].Col.G / 255.0f;
                Inputs[i * 3 + 2] = Eyes[i].Col.B / 255.0f;
            }
            Inputs[ListenId] = 0;
            /*foreach (var item in NearbyObjects)
            {
                if(item is Creature)
                {
                    Creature C = item as Creature;
                    float Ratio = (C.Pos - Pos).MagSq() / (EyeRadius * EyeRadius);
                    if (Ratio<=1)
                    {
                        Inputs[ListenId] += (1 - Ratio) * C.TalkValue;
                    }
                }
            }*/
            Inputs[ListenId] = Clamp01(Inputs[ListenId]);
            Inputs[EnergyId] = Clamp01(Energy / (StartEnergy + EggEnergy));
            Inputs[SpeedId] = Clamp01(Vel.Mag()/MaxSpeed);
            Inputs[PollenId] = Pollen != null?1:0;
            Brain.SetInputs(Inputs);
            Brain.Update();
            //output
            Outputs=Brain.GetOutputs();
            TalkValue = Clamp01(Outputs[3]);
        }
        
        float Clamp(float x)
        {
            if (x > 1)
                return 1;
            else if (x < -1)
                return -1;
            else
                return x;
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
        /*void Collide()
        {
            bool Extend;
            if (!(Selected && Form1.ManualControl))
            {
                Extend = UseSpike;
            }
            else
            {
                Extend = Form1.HoldKeys.Contains(System.Windows.Forms.Keys.Space);
            }
            for (int i = NearbyObjects.Count-1; i >= 0; i--)
            {
                var I = NearbyObjects[i];
                float R = I.Radius + Radius;
                Vector RelPos = I.Pos - Pos;
                if ((I.Pos-Pos).MagSq()<R*R)//intersect
                {
                    if (I is Tree)
                    {
                        Vector Push = RelPos * -((Vel.Mag() + 2) / RelPos.Mag());
                        Vel += Push;
                    }
                    else if (I is Creature C)
                    {
                        float R2 = C.Radius + MouthRadius * Radius;
                        float mult = 1;
                        if (Extend && (C.Pos - (Pos + (MouthPos * Radius).Rot(AngleVector))).MagSq() < R2 * R2)
                        {
                            mult = 1.5f;
                            EatCreature(C);
                        }
                        if (!C.Dead)
                        {
                            Vector Push = RelPos * mult*-(((Vel - C.Vel).Mag() + 2) / (2 * RelPos.Mag()));
                            Vel += Push;
                            C.Vel -= Push;
                        }
                    }
                    else if (I is Seed S)
                    {
                        float R2 = S.Radius + MouthRadius * Radius;
                        if (!Extend && (S.Pos - (Pos + (MouthPos * Radius).Rot(AngleVector))).MagSq() < R2 * R2&&S.Edible)
                        {
                            Energy += 125;
                            S.Dead = true;
                            NearbyObjects.Remove(S);
                        }
                        else
                        {
                            Vector Push = RelPos * -((Vel.Mag() + 2) / (2 * RelPos.Mag()));
                            Vel += Push;
                            S.Vel -= Push * 2;
                        }
                    }
                    else if (I is Tree.Leaf.Flower F)
                    {
                        if(F.Pollen !=null&&F.FruitDone)
                        {
                            Eat(F);
                        }else
                            F.CollideWith(this);
                    }
                }
            }
            for (int i = NearbyObjects.Count-1; i >= 0; i--)
            {
                if (NearbyObjects[i] is Creature)
                {
                    Creature Cre = NearbyObjects[i] as Creature;
                    if (Cre.Dead)
                    {
                        NearbyObjects.RemoveAt(i);
                    }
                }
            }
        }*/
        void EatCreature(Creature C)
        {
            float EnergyGain = 150;
            if (C.Energy < EnergyGain)
                EnergyGain = C.Energy;
            C.Energy -= EnergyGain*1.25f;
            Energy += EnergyGain;
        }
        void Eat(Tree.Leaf.Flower Flower)
        {
            Flower.Leaf.CurrentFlower = null;
            EatenFood.Add(new Food(Flower));
            UpdateWorld.Entities.Remove(Flower);
        }
        void Eat(Entity E)
        {
            bool Extend;
            if (!(Draw.Selected == this && Form1.ManualControl))
            {
                Extend = UseSpike;
            }
            else
            {
                Extend = Form1.HoldKeys.Contains(System.Windows.Forms.Keys.Space);
            }
            if (E is Seed S&&S.EnableCollide&&!Extend)
            {
                Energy += 125;
                S.Dead = true;
                UpdateWorld.RemoveEntity(S);
            }else if (E is Creature C&&Extend)
            {
                EatCreature(C);
            }else if (E is Tree.Leaf.Flower F)
            {
                if (F.Pollen != null && F.FruitDone)
                {
                    Eat(F);
                }
                else
                    F.CollideWith(this);
            }
            else if(E is Tree T && Extend)
            {
                if(T.Leaves.Count>0&&LeafEatTimer==0)
                {
                    Tree.Leaf L =T.Leaves[Form1.Rand.Next(T.Leaves.Count)];
                    if (L.Center.Z < MaxEatLeafHeight)
                    {
                        L.Kill();
                        Energy += 200;
                    }
                }
            }
        }
        public void Mutate()
        {
            Name=MutateName(Name);
            Brain.Mutate(0.1f);
            Hue += (float)(1 - Form1.Rand.NextDouble() * 2) * 0.05f;
            EggHue += (float)(1 - Form1.Rand.NextDouble() * 2) * 0.05f;
            Hue = Clamp01(Hue);
            EggHue = Clamp01(EggHue);
            Col = Form1.ColorFromHue(Hue);
        }
        string GenerateName()
        {
            int Length = Form1.Rand.Next(4, 9);
            string Consonants = "bcdfghjklmnpqrstvwxz";
            string Vowels = "aeiouy";
            string Name = "";
            for (int i = 0; i < Length; i++)
            {
                if ((i & 1) == 0)
                    Name += Consonants[Form1.Rand.Next(Consonants.Length)];
                else
                    Name += Vowels[Form1.Rand.Next(Vowels.Length)];
                if (i == 0)
                {
                    Name = Name.ToUpper();
                }
            }
            return Name;
        }
        string MutateName(string Name)
        {
            string StartName = Name;
            string Consonants = "bcdfghjklmnpqrstvwxz";
            string Vowels = "aeiouy";
            if (Form1.Rand.NextDouble() < 0.1)
            {
                if((Form1.Rand.NextDouble()<0.5&& Name.Length<=9)|| !(Name.Length>4))
                {
                    if ((Name.Length & 1) == 0)
                        Name += Consonants[Form1.Rand.Next(Consonants.Length)].ToString();
                    else
                        Name += Vowels[Form1.Rand.Next(Vowels.Length)].ToString();
                }
                else
                {
                    Name = Name.Remove(Name.Length-1,1);
                }
            }
            else
            {
                int i = Form1.Rand.Next(Name.Length);
                string c;
                if ((i & 1) == 0)
                    c = Consonants[Form1.Rand.Next(Consonants.Length)].ToString();
                else
                    c = Vowels[Form1.Rand.Next(Vowels.Length)].ToString();
                if (i == 0)
                    c = c.ToUpper();
                Name = Name.Remove(i, 1);
                Name = Name.Insert(i, c);
            }
            while (Name == StartName)
                Name = MutateName(Name);
            return Name;
        }
        public void OnCollision(CollideTrigger Other,Vector RelVector)
        {
            Vector V = MouthPos.Rot(AngleVector) * Radius;
            float R = (MouthRadius * Radius + Other.Radius);
            if ((RelVector - V).MagSq() < R*R)
                Eat(Other);
        }

        [Serializable]
        public class Eye
        {
            public Vector Pos;
            Vector PosNorm;
            Creature Parent;
            float Radius = 5;
            public float Fov;
            public Color Col;
            public Color FullColor;
            public float value;
            public float Hue;
            float Dist;
            Vector BodyPos;
            Vector WorldPos;
            public Visible LookAt;
            public Vector RelPos;
            public Eye(Creature Parent,Vector Pos,float Fov)
            {
                this.Fov = Fov;
                this.Parent = Parent;
                this.Pos = Pos;
                PosNorm = Pos/Pos.Mag();
            }
            public void Update()
            {
                if (LookAt != null)
                {
                    Dist = Form1.Sqrt(Dist);
                    value = 1 - (Dist / EyeRadius);
                    Col = LookAt.Col;
                    Col = Color.FromArgb((int)(Col.R * value), (int)(Col.G * value), (int)(Col.B * value));
                    Hue = LookAt.Hue;
                }
                else
                {
                    Hue = 0;
                    LookAt = null;
                    Col = Color.Black;
                    value = 0;
                }
                BodyPos = PosNorm.Rot(Parent.AngleVector);
                WorldPos = Parent.Pos + BodyPos;
                Dist = EyeRadius * EyeRadius;
                LookAt = null;
                RelPos = new Vector();
            }
            public void Add(Visible V)
            {
                RelPos=UpdateWorld.GetRelativePosition(Parent.ChunkPos,Parent.Pos, V.ChunkPos, V.Pos);
                Vector V2 = RelPos - BodyPos;
                float Dot = BodyPos * (V2 / V2.Mag());
                float A = 1 - Fov * Fov;
                if (Dot > A)
                {
                    float D = (V.Pos - WorldPos).MagSq();//(float)Math.Sqrt((Dot-A)/(1-A));
                    if (D < Dist)
                    {
                        Dist = D;
                        LookAt = V;
                    }
                }
            }
            public void Show(Draw D, float Zoom)
            {
                float R = Radius * Zoom;
                float R2 = R * 2;
                Vector P = Pos * Zoom;
                D.Graphics.FillEllipse(new SolidBrush(Col), P.X - R, P.Y - R, R2, R2);
                D.Graphics.DrawEllipse(new Pen(Color.DarkGray, 2 * Zoom), P.X - R, P.Y - R, R2, R2);
                
            }
        }
    }
}
