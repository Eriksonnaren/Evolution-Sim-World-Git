using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Evolution_Simulator_World
{
    public class Draw
    {
        public Graphics Graphics;
        public PointF CameraPosition;
        SizeF Size;
        public float Zoom=1;
        public Creature Selected;
        public bool SelectedFamily;
        bool BlindMode = false;
        public bool EyeRings = false;
        public bool EyeLines = false;
        bool Lock = false;
        public Font Font = new Font("Arial Black",15);
        public StringFormat SF = new StringFormat();

        GuiElement SpeedText=new GuiElement(GuiElement.Type.TextBox, new Rectangle(20, 20, 120, 30), "Speed:1");
        GuiElement SpeedSub = new GuiElement(GuiElement.Type.Button, new Rectangle(190, 20, 30, 30), "-");
        GuiElement SpeedAdd = new GuiElement(GuiElement.Type.Button, new Rectangle(150, 20, 30, 30), "+");

        

        public static Graph CreatureGraph = new Graph(new Rectangle(Form1.PB.Width - 400, Form1.PB.Height - 200, 350, 150), Color.Blue, UpdateWorld.MinCreatures);
        public static Graph TreeGraph = new Graph(new Rectangle(Form1.PB.Width - 400, Form1.PB.Height - 420, 350, 150), Color.DarkGreen, UpdateWorld.MinTrees);

        GuiElement CreatureText = new GuiElement(GuiElement.Type.TextBox, new Rectangle(Form1.PB.Width - (400-95), Form1.PB.Height - 245, 160, 30), "Creatures:");
        GuiElement TreeText = new GuiElement(GuiElement.Type.TextBox, new Rectangle(Form1.PB.Width - (400 - 115), Form1.PB.Height - 465, 120, 30), "Trees:");

        List<GuiElement> SelectedElements;
         GuiElement FamilyButton = new GuiElement(GuiElement.Type.Button, new Rectangle(20, 300, 120, 30), "Family");
        GuiElement BlindButton= new GuiElement(GuiElement.Type.Button, new Rectangle(20, 350, 120, 30), "Blind");
        GuiElement LockButton = new GuiElement(GuiElement.Type.Button, new Rectangle(20, 400, 120, 30), "Lock");
        GuiElement EyeLinesButton = new GuiElement(GuiElement.Type.Button, new Rectangle(20, 450, 120, 30), "EyeLines");
       


        List<GuiElement> Elements;
        public static List<Family> Families=new List<Family>();

        List<GuiElement> FamilyButtons=new List<GuiElement>();
        public static bool UpdateFamily = false;

        public PointF WorldToScreen(float x, float y)
        {
            return new PointF((x-CameraPosition.X)*Zoom, (y - CameraPosition.Y) * Zoom);
        }
        public PointF WorldToScreen(Vector V)
        {
            return new PointF((V.X - CameraPosition.X) * Zoom, (V.Y - CameraPosition.Y) * Zoom);
        }
        public Vector ScreenToWorld(float x,float y)
        {
            return new Vector((x-Size.Width/2) / Zoom + CameraPosition.X, (y - Size.Height / 2) / Zoom + CameraPosition.Y);
        }
        public Draw(Graphics Graphics,SizeF Size)
        {
            SF.Alignment = StringAlignment.Center;
            SF.LineAlignment = StringAlignment.Center;
            this.Graphics = Graphics;
            this.Size = Size;
            SelectedElements = new List<GuiElement>() {BlindButton,LockButton,EyeLinesButton };
            FamilyButton.ButtonEvent = FamilyPress;
            Elements = new List<GuiElement>() { SpeedText, SpeedAdd, SpeedSub};
            SpeedAdd.ButtonEvent = SpeedAddPress;
            SpeedSub.ButtonEvent = SpeedSubPress;
            BlindButton.ButtonEvent = BlindButtonEvent;
            LockButton.ButtonEvent = LockButtonEvent;
            EyeLinesButton.ButtonEvent = EyeLinesButtonEvent;
        }
        public void MainDraw()
        {
            DrawWorld();
            DrawGUI();
        }
        void DrawWorld()
        {
            Graphics.TranslateTransform(Size.Width/2,Size.Height/2);
            if (Selected != null)
            {
                Selected.Selected = true;
                if(Lock)
                    Graphics.RotateTransform(-Selected.Angle-90);
                CameraPosition = Selected.Pos.ToPoint();
                if (BlindMode)
                {
                    PointF P = WorldToScreen(Selected.Pos.X, Selected.Pos.Y);
                    Selected.Show(this, P.X, P.Y, Zoom);
                }
            }
            if(Selected==null||!BlindMode)
            {
                foreach (var T in Form1.Trees)
                {
                    PointF P = WorldToScreen(T.Pos.X, T.Pos.Y);
                    T.ShowBelow(this, P.X, P.Y, Zoom);
                }
                foreach (var E in Form1.Eggs)
                {
                    PointF P = WorldToScreen(E.Pos.X, E.Pos.Y);
                    E.Show(this, P.X, P.Y, Zoom);
                }
                Font FontC = new Font(Font.Name,15*Zoom);
                foreach (var C in Form1.Creatures)
                {
                    PointF P = WorldToScreen(C.Pos.X, C.Pos.Y);
                    C.Show(this, P.X, P.Y, Zoom);
                    
                }
                foreach (var T in Form1.Trees)
                {
                    PointF P = WorldToScreen(T.Pos.X, T.Pos.Y);
                    T.Show(this, P.X, P.Y, Zoom);
                }
                foreach (var F in Form1.Foods)
                {
                    PointF P = WorldToScreen(F.Pos.X, F.Pos.Y);
                    F.Show(this, P.X, P.Y, Zoom);
                }
                foreach (var S in Form1.Seeds)
                {
                    PointF P = WorldToScreen(S.Pos.X, S.Pos.Y);
                    S.Show(this, P.X, P.Y, Zoom);
                }
                foreach (var C in Form1.Creatures)
                {
                    PointF P = WorldToScreen(C.Pos.X, C.Pos.Y);
                    Graphics.DrawString(C.Name, FontC, Brushes.Black, P.X, P.Y - C.Radius * Zoom * 1.5f, SF);
                }
            }
            Graphics.ResetTransform();
        }
        void DrawGUI()
        {
            if (UpdateFamily)
                UpdateFamilyList();
            CreatureText.Text = "Creatures:" + Form1.Creatures.Count.ToString();
            TreeText.Text = "Trees:" + Form1.Trees.Count.ToString();
            if (Selected!=null)
            {
                Rectangle BrainRect = new Rectangle(20, 70, 190, 350);
                //DrawGuiBox(Brushes.LightGray, BrainRect);
                

                FamilyButton.Rect.Y = BrainRect.Y + BrainRect.Height + 15;

                if (Selected.Family != null)
                {
                    if (SelectedFamily)
                    {
                        Graphics.FillRectangle(new SolidBrush(Color.FromArgb(150, Color.Black)), 0, 0, Size.Width, Size.Height);
                        Selected.Family.Show(this,new Rectangle(BrainRect.Width+BrainRect.X-50,0,Form1.PB.Width-(200+ BrainRect.Width + BrainRect.X),Form1.PB.Height));
                    }
                    FamilyButton.Show(this);
                }
                else
                    SelectedFamily = false;

                for (int i = 0; i < SelectedElements.Count; i++)
                {
                    GuiElement E = SelectedElements[i];
                    E.Rect.Y= BrainRect.Y + BrainRect.Height + 65+i*50;
                    E.Show(this);
                }
                    Selected.Brain.Show(Graphics, BrainRect);
            }
            foreach (var E in Elements)
            {
                E.Show(this);
            }
            if (!(FileHandler.LoadState || FileHandler.SaveState))
            {
                int i = -1;
                foreach (var E in FamilyButtons)
                {
                    E.Show(this);
                    if(i>=0)
                    E.Text = Families[i].Root.Name+":"+Families[i].CreaturesAlive.ToString();
                    i++;
                }
            }
            if(!SelectedFamily)
            {
                CreatureGraph.Show(this);
                TreeGraph.Show(this);
                CreatureText.Show(this);
                TreeText.Show(this);
            }
        }
        
        
        public bool MousePress()
        {
            if (Selected != null)
                if (Selected.Family != null)
                {
                    if (getPressButton(FamilyButton))
                        if (FamilyButton.mouseDown())
                            return true;
                }
            foreach (GuiElement E in Elements)
            {
                E.Selected = false;
                if (getPressButton(E))
                    if (E.mouseDown())
                        return true;
            }
            if (!(FileHandler.LoadState || FileHandler.SaveState))
                foreach (var E in FamilyButtons)
                {
                    E.Selected = false;
                    if (getPressButton(E))
                        if (E.mouseDown())
                            return true;
                }
            if (Selected != null)
            {
                foreach (GuiElement E in SelectedElements)
                {
                    E.Selected = false;
                    if (getPressButton(E))
                        if (E.mouseDown())
                            return true;
                }
            }
            return false;
        }
        bool getPressButton(GuiElement E)
        {
            return (Form1.MousePos.X >= E.Rect.X && Form1.MousePos.X <= E.Rect.X + E.Rect.Width &&
                    Form1.MousePos.Y >= E.Rect.Y && Form1.MousePos.Y <= E.Rect.Y + E.Rect.Height);
        }
        public void KeyPress(char Key)
        {
            foreach (GuiElement E in Elements)
            {
                if (E.Selected)
                    E.keyDown(Key);
            }
        }
        void SpeedAddPress(object o,EventArgs e)
        {
            if (UpdateWorld.Speed < 128)
                UpdateWorld.Speed *= 2;
            SpeedText.Text = "Speed:" + UpdateWorld.Speed.ToString();
        }
        void SpeedSubPress(object o, EventArgs e)
        {
            if(UpdateWorld.Speed>1)
                UpdateWorld.Speed /= 2;
            SpeedText.Text = "Speed:" + UpdateWorld.Speed.ToString();
        }
        void FamilyPress(object o,EventArgs e)
        {
            SelectedFamily = !SelectedFamily;
        }
        void DrawGuiBox(Brush B,Rectangle Rect)
        {
            Pen Pe=new Pen(Color.Black,10);
            Pe.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            Graphics.DrawRectangle(Pe, Rect);
            Graphics.FillRectangle(B, Rect);
        }
        int ILerp(int A,int B, float T)
        {
            return A + (int)((B - A) * T);
        }
        Color CLerp(Color A,Color B,float T)
        {
            return Color.FromArgb(ILerp(A.R,B.R,T), ILerp(A.G, B.G, T), ILerp(A.B, B.B, T));
        }
        void UpdateFamilyList()
        {
            UpdateFamily = false;
            FamilyButtons.Clear();
            
            if (Families.Count > 0)
            {
                int X = Form1.PB.Size.Width - 300;
                int Y = 20;
                int dy = 50;
                int i = 1;
                FamilyButtons.Add(new GuiElement(GuiElement.Type.TextBox,new Rectangle(X, Y, 140, 30),"Families:"));
                foreach (var F in Families)
                {
                    GuiElement Button = new GuiElement(GuiElement.Type.Button, new Rectangle(X, Y + dy * i, 140, 30), F.Root.Name+ ":"+F.CreaturesAlive.ToString());
                    FamilyButtons.Add(Button);
                    Button.ButtonEvent = FamilyButtonEvent;
                    i++;
                }
            }
        }
        void FamilyButtonEvent(object sender,EventArgs e)
        {
            var G= sender as GuiElement;
            int id = FamilyButtons.IndexOf(G);
            var F = Families[id-1];
            Selected = F.Root;
            SelectedFamily=true;
        }
        void BlindButtonEvent(object sender, EventArgs e)
        {
            BlindMode = !BlindMode;
        }
        void LockButtonEvent(object sender, EventArgs e)
        {
            Lock = !Lock;
        }
        void EyeLinesButtonEvent(object sender, EventArgs e)
        {
            EyeLines = !EyeLines;
        }
    }
    public class Graph
    {
        Rectangle Rect;
        int[] Array;
        Color Color;
        float Average;
        int max = 0;
        int min = 0;
        int spacing = 2;
        public Graph(Rectangle Rect,Color color,int Start)
        {
            Color = color;
            this.Rect = Rect;
            Array = new int[Rect.Width/spacing];
            for (int i = 0; i < Array.Length; i++)
            {
                Array[i] = Start;
            }
            Average = Start;
        }
        public void Add(int a)
        {
            Average =max =min=a;
            
            for (int i = 0; i < Array.Length-1; i++)
            {
                Array[i] = Array[i+1];
                Average += Array[i];
                max = max > Array[i] ? max : Array[i];
                min = min < Array[i] ? min : Array[i];
            }
            Array[Array.Length - 1] = a;
            Average = Average / Array.Length;
        }
        public void Show(Draw D)
        {
            Pen Pe = new Pen(Color.Black, 10);
            Pe.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            D.Graphics.DrawRectangle(Pe, Rect);
            D.Graphics.FillRectangle(Brushes.LightGray, Rect);
            PointF[] Points = new PointF[Array.Length];
            float slope;
            float offset;
            //if ((max - min) - 2 >= 10)
            {
                slope = Rect.Height / (float)(max - min+2);
                offset = -(max+1)*slope+ Rect.Height / 2;
            }
            //else
            {
                //slope = Rect.Height / 10;
                //offset = -Average*slope;
            }
            for (int i = 0; i < Points.Length; i++)
            {
                Points[i] = new PointF(Rect.X+i*spacing,Rect.Y+Rect.Height/2-(Array[i]*slope + offset));
            }
            D.Graphics.DrawLines(new Pen(Color,3),Points);
        }
    }
}
