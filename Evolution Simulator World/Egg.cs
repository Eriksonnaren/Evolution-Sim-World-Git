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
    public class Egg:BaseObject
    {
        public List<Creature> Parents;
        public Vector Pos { get; set; }
        float Time;
        float maxTime = 10;
        public float Radius { get { return 20; } }
        public Color Col { get; set; }
        public float Hue { get; set; }
        float Angle = 0;
        Creature Child;
        public Egg(Creature Parent)
        {
            Hue = Parent.EggHue;
            Col = Form1.ColorFromHue(Hue);
            Pos = Parent.Pos;
            Parents = new List<Creature> { Parent };
            Child = new Creature(Pos);
            Child.IsEgg = this;
            Child.Name = "";
            if (Parents[0].Family == null)
            {
                Parents[0].Family = new Family(Parents[0]);
            }
            Parents[0].Family.Add(Parents[0], Child);

            Child.Parents = new List<Creature> { Parent };

            foreach (Creature C in Parents)
            {
                C.Children.Add(Child);
            }
        }
        public void Update()
        {
            Angle += 5;
            Time+=1/Form1.fps;
            if(Time>=maxTime)
            {
                Hatch();
            }
        }
        public void Show(Draw D,float x,float y,float Zoom = 1)
        {
            float R = Radius * Zoom;
            float R2 = R * 2;
            float sqrt2 = 0.707f;
            float R21 = R * sqrt2;
            float R22 = R21 * 2;
            Matrix M = D.Graphics.Transform.Clone();
            D.Graphics.TranslateTransform(x, y);
            D.Graphics.RotateTransform(Angle);
            D.Graphics.FillEllipse(new SolidBrush(Col),-R,-R,R2,R2);
            float m = Time / maxTime;
            D.Graphics.FillRectangle(Brushes.White, -R21, -R21, R22, R22);
            D.Graphics.FillRectangle(new SolidBrush(Parents[0].Col), -R21*m, -R21*m, R22*m, R22*m);

            D.Graphics.Transform = M;
        }
        void Hatch()
        {
            Child.IsEgg = null;
            Child.Brain = Parents[0].Brain.Copy();
            int EyeMin = Parents[0].Eyes.Count;
            int EyeMax = EyeMin;
            for (int i2 = 1; i2 < Parents.Count; i2++)
            {
                int C = Parents[i2].Eyes.Count;
                if (C > EyeMax) EyeMax = C;
                if (C < EyeMin) EyeMin = C;
            }
            int EyeAmount = Form1.Rand.Next(EyeMin, EyeMax + 1);
            Child.Eyes = new List<Creature.Eye>();
            for (int i = 0; i < EyeAmount; i++)
            {
                int Id = Form1.Rand.Next(Parents.Count);
                while (Parents[Id].Eyes.Count < i + 1)
                    Id = (Id + 1) % Parents.Count;
                Child.Eyes.Add(new Creature.Eye(Child, Parents[Id].Eyes[i].Pos));
            }

            int EyeId = Form1.Rand.Next(Child.Eyes.Count);
            double Angle = (Form1.Rand.NextDouble() * 2 - 1) * Math.PI * 0.1;
            float Sin = (float)Math.Sin(Angle);
            float Cos = (float)Math.Cos(Angle);
            Vector OldPos = Child.Eyes[EyeId].Pos;
            Child.Eyes[EyeId].Pos = new Vector(OldPos.X * Cos + OldPos.Y * Sin, OldPos.Y * Cos - OldPos.X * Sin);
            //mutate
            Child.Name = Parents[0].Name;
            Child.Hue = Parents[0].Hue;
            Child.EggHue = Parents[0].EggHue;
            Child.Mutate();

            
            Form1.Creatures.Add(Child);
            Form1.Eggs.Remove(this);
        }
    }
}
