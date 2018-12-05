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
    public class Creature:BaseObject, SelectableObject
    {
        //Creature
        public Vector Pos { get; set; }
        public Vector MouthPos=new Vector(0.6f,0);//percentage of radius
        float MouthRadius = 0.7f;//percentage of radius
        float Friction = 0.2f;
        float AngleFriction = 0.1f;
        public float BaseSpeed = 4f;
        public float BaseAngleSpeed = 0.5f;
        public Vector Vel;
        public float AngleVel;
        public float Angle;//in degrees
        public float Radius { get { return 40; }}
        public Vector AngleVector;
        public float Energy=500;
        public float StartEnergy = 500;
        float EggEnergy=500;
        public bool Selected = false;
        bool SelectedPrev = false;
        int Age = 0;
        public bool Dead { get; set; } = false;
        public string Name;
        float MaxSpeed;
        public NewTree.Leaf.Flower Pollen;
        //Eyes
        float EyeRadius=800;
        float EyeRadiusExtra=100;
        List<BaseObject> NearbyObjects;
        Vector PreviousNearbyUpdatePos;
        int PreviousNearbyUpdateTicks;
        int PreviousNearbyUpdateTicksMax=100;
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
            public NewTree.Leaf.Flower Parent;
            public Food(NewTree.Leaf.Flower Parent)
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
        //Family
        public List<Creature> Parents;
        public List<Creature> Children=new List<Creature>();
        public int Generation=0;
        public int FamilyPos;
        public Family Family;
        public Egg IsEgg;

        public Creature(Vector Pos)
        {
            MaxSpeed = ((1 - Friction)*BaseSpeed) / Friction;
            Name = GenerateName();
            Hue = (float)Form1.Rand.NextDouble();
            EggHue = (float)Form1.Rand.NextDouble();
            Col = Form1.ColorFromHue(Hue);
            PreviousNearbyUpdatePos = new Vector(Form1.ArenaRadius*2,0);
            AngleVector = new Vector(1,0);
            this.Pos = Pos;

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
            UpdateNearby();
            foreach (Eye E in Eyes)
            {
                E.Update();
            }
            updateBrain();
            UseSpike = Outputs[2]>0.5;
            Collide();
            Move();
            UpdateEnergy();
            if(Energy>EggEnergy+StartEnergy)
            {
                Energy -= EggEnergy*1.25f;
                Form1.Eggs.Add(new Egg(this));
            }
            Age++;
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
            Pos += Vel;
            if (Pos.MagSq() > Form1.ArenaRadius * Form1.ArenaRadius)
            {
                Pos = Pos * ((100 - Form1.ArenaRadius) / Form1.ArenaRadius);
            }
            Vel *= (1 - Friction);
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
                        Form1.NewSeeds.Add(new NewSeed(EatenFood[0].Parent,Pos));
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
            NearbyObjects.Clear();
            Dead = true;
            Energy = 0;
            foreach (var E in Eyes)
            {
                E.LookAt = null;
            }
            if (Family != null)
            {
                Family.CreaturesAlive--;
                Family.Remove(this);
            }
        }
        public void Show(Draw D, float x, float y, float Zoom = 1)
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
            D.Graphics.FillEllipse(new SolidBrush(Col), -R, -R, R2, R2);

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
            float ER = (EyeRadius + Radius + EyeRadiusExtra)*Zoom;
            D.Graphics.Transform = M;
            if (Selected&&D.EyeRings)
            {
                foreach (Eye E in Eyes)
                {
                    if (E.value > 0)
                    {
                        PointF Po = D.WorldToScreen(Pos + E.Pos.Rot(AngleVector));
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
                foreach (var S in Form1.NewSeeds)
                {
                    if ((S.Pos - Pos).MagSq() < RR)
                        NearbyObjects.Add(S);
                }
                /*foreach (Food F in Form1.Foods)
                {
                    if ((F.Pos - Pos).MagSq() < RR)
                        NearbyObjects.Add(F);
                }*/
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
                foreach (var T in Form1.NewTrees)
                {
                    foreach (var F in T.GetAllFlowers())
                    {
                        if ((F.Pos - Pos).MagSq() < RR)
                            NearbyObjects.Add(F);
                    }
                }
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
            foreach (var item in NearbyObjects)
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
            }
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
        void Collide()
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
                    /*else if (I is Food Fod)
                    {
                        float R2 = Fod.Radius + MouthRadius * Radius;
                        if (!Extend && (Fod.Pos - (Pos + (MouthPos * Radius).Rot(AngleVector))).MagSq() < R2 * R2)
                        {
                            Eat(Fod);
                            Fod.Dead = true;
                        }
                        else
                        {
                            Vector Push = RelPos * -((Vel.Mag() + 2) / (2 * RelPos.Mag()));
                            Vel += Push;
                            Fod.Vel -= Push * 2;
                        }
                    }*/
                    else if (I is NewSeed S)
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
                    else if (I is NewTree.Leaf.Flower F)
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
                /*if(NearbyObjects[i] is Food Fod)
                {
                    if(Fod.Dead)
                    {
                        NearbyObjects.RemoveAt(i);
                    }
                }else */
                if (NearbyObjects[i] is Creature)
                {
                    Creature Cre = NearbyObjects[i] as Creature;
                    if (Cre.Dead)
                    {
                        NearbyObjects.RemoveAt(i);
                    }
                }
            }
        }
        void EatCreature(Creature C)
        {
            float EnergyGain = 150;
            C.Energy -= EnergyGain*4/3f;
            Energy += EnergyGain;
        }
        void Eat(NewTree.Leaf.Flower Flower)
        {
            Flower.Leaf.CurrentFlower = null;
            EatenFood.Add(new Food(Flower));
            NearbyObjects.Remove(Flower);
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
            public BaseObject LookAt;
            public Eye(Creature Parent,Vector Pos,float Fov)
            {
                this.Fov = Fov;
                this.Parent = Parent;
                this.Pos = Pos;
                PosNorm = Pos/Pos.Mag();
            }
            public void Update()
            {
                Vector BodyPos = PosNorm.Rot(Parent.AngleVector);
                Vector WorldPos = Parent.Pos + BodyPos;
                BaseObject NearestObj=null;
                float Dist = Parent.EyeRadius* Parent.EyeRadius;
                foreach (BaseObject B in Parent.NearbyObjects)
                {
                    Vector RelPos = B.Pos - Parent.Pos;
                    Vector V2 = RelPos - BodyPos;
                    float Dot = BodyPos * (V2/V2.Mag());
                    if (Dot > 1-Fov*Fov)
                    {
                        float D = (B.Pos - WorldPos).MagSq();
                        if (D < Dist)
                        {
                            Dist = D;
                            NearestObj = B;
                        }
                    }
                }
                if (NearestObj != null)
                {
                    LookAt = NearestObj;
                    Dist = Form1.Sqrt(Dist);
                    value = 1 - (Dist / Parent.EyeRadius);
                    Col = NearestObj.Col;
                    Col = Color.FromArgb((int)(Col.R * value), (int)(Col.G * value), (int)(Col.B * value));
                    Hue = NearestObj.Hue;
                }
                else
                {
                    Hue = 0;
                    LookAt = null;
                    Col = Color.Black;
                    value = 0;
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
