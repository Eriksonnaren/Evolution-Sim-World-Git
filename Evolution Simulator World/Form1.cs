using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Evolution_Simulator_World
{
    public partial class Form1 : Form
    {
        public static float ArenaRadius = 10000;
        public static List<Creature> Creatures = new List<Creature>();
        public static List<Tree> Trees = new List<Tree>();
        public static List<Food> Foods = new List<Food>();
        public static List<Egg> Eggs = new List<Egg>();
        public static List<Seed> Seeds = new List<Seed>();
        public static Random Rand = new Random();
        BufferedGraphics BG;
        public static PictureBox PB;
        Draw Draw;
        UpdateWorld update;
        FileHandler FileHandler;
        Timer T = new Timer();
        static float[] SinArray;
        static float[] CosArray;
        static int SinAmount = 360;
        public static PointF MousePos;
        public PointF MousePosPrev;
        public static float fps;
        public static bool ManualControl = false;
        public static string StartPath=Application.StartupPath;
        public static List<Keys> HoldKeys = new List<Keys>();
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            MouseWheel += Form1_MouseWheel;
            PB = new PictureBox();
            PB.Parent = this;
            PB.Location = new Point(0, 0);
            WindowState = FormWindowState.Maximized;
            PB.Dock = DockStyle.Fill;
            BufferedGraphicsContext Context = BufferedGraphicsManager.Current;
            BG = Context.Allocate(PB.CreateGraphics(), PB.DisplayRectangle);
            Draw = new Draw(BG.Graphics, PB.Size);
            update = new UpdateWorld();
            T.Interval = 20;
            fps = 1000 / T.Interval;
            T.Tick += T_Tick;
            KeyDown += Form1_KeyDown;
            KeyUp += Form1_KeyUp;
            T.Start();
            FileHandler = new FileHandler();
            SinArray = new float[SinAmount];
            CosArray = new float[SinAmount];
            for (int i = 0; i < SinAmount; i++)
            {
                double angle = Math.PI * 2 * i / (float)SinAmount;
                SinArray[i] = (float)Math.Sin(angle);
                CosArray[i] = (float)Math.Cos(angle);
            }
            for (int i = 0; i < UpdateWorld.MinTrees; i++)
            {
                float angle = Rand.Next(0, 360);
                float Dist = Sqrt((float)Rand.NextDouble()) * (Form1.ArenaRadius - 200);
                float X = Cos(angle) * Dist;
                float Y = Sin(angle) * Dist;
                Trees.Add(new Tree(new Vector(X, Y)));
            }
            for (int i = 0; i < UpdateWorld.MinCreatures; i++)
            {
                float angle = Rand.Next(0, 360);
                float Dist = Sqrt((float)Rand.NextDouble()) * (Form1.ArenaRadius - 200);
                float X = Cos(angle) * Dist;
                float Y = Sin(angle) * Dist;
                Creatures.Add(new Creature(new Vector(X, Y)));
            }
            
        }



        private void T_Tick(object sender, EventArgs e)
        {
            MouseStuff();
            BG.Graphics.Clear(Color.Gray);
            update.MainUpdate();
            Draw.MainDraw();
            FileHandler.Show(Draw);

            BG.Render();
        }
        public static Color ColorFromHue(float h)
        {
            int R = (int)(hueValue(h) * 255);
            int G = (int)(hueValue(h + 0.333f) * 255);
            int B = (int)(hueValue(h + 0.666f) * 255);
            return Color.FromArgb(R, G, B);
        }
        static float hueValue(float h)
        {
            float a = (2 * ((h) % 1) - 1);
            if (a < 0) a = -a;
            a = 3 * a - 1;
            if (a > 1) return 1;
            else if (a < 0) return 0;
            else return a;
        }
        public static float Sin(float angle)
        {
            int i = (int)(SinAmount * angle / (360));
            i = i % SinAmount;
            if (i < 0)
                i += SinAmount;
            return SinArray[i];
        }
        public static float Sqrt(float In)
        {
            float Out = 1;
            for (int i = 0; i < 20; i++)
            {
                float O = (Out + In / Out) / 2;
                if (O == Out && i > 2)
                    return Out;
                Out = O;
            }
            return Out;
        }
        public static float Cos(float angle)
        {
            int i = (int)(SinAmount * angle / (360));
            i = i % SinAmount;
            if (i < 0)
                i += SinAmount;
            return CosArray[i];
        }
        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                Draw.CameraPosition.X -= ((PB.Width / 2 - MousePos.X) / Draw.Zoom);
                Draw.CameraPosition.Y -= ((PB.Height / 2 - MousePos.Y) / Draw.Zoom);
                Draw.Zoom *= 1.2f;
                Draw.CameraPosition.X += ((PB.Width / 2 - MousePos.X) / Draw.Zoom);
                Draw.CameraPosition.Y += ((PB.Height / 2 - MousePos.Y) / Draw.Zoom);
            }
            else if (e.Delta < 0)
            {
                Draw.CameraPosition.X -= ((PB.Width / 2 - MousePos.X) / Draw.Zoom);
                Draw.CameraPosition.Y -= ((PB.Height / 2 - MousePos.Y) / Draw.Zoom);
                Draw.Zoom /= 1.2f;
                Draw.CameraPosition.X += ((PB.Width / 2 - MousePos.X) / Draw.Zoom);
                Draw.CameraPosition.Y += ((PB.Height / 2 - MousePos.Y) / Draw.Zoom);
            }
        }
        MouseButtons PrevButton;
        public static bool MouseHold;
        void MouseStuff()
        {
            if (PrevButton != MouseButtons.None && MouseButtons == MouseButtons.None)
                MouseRelease(PrevButton);
            MousePosPrev = MousePos;
            MousePos = PointToClient(MousePosition);
            if (MouseButtons == MouseButtons.Right)
            {
                Draw.CameraPosition.X -= (MousePos.X - MousePosPrev.X) / Draw.Zoom;
                Draw.CameraPosition.Y -= (MousePos.Y - MousePosPrev.Y) / Draw.Zoom;
            }
            PrevButton = MouseButtons;
            MouseHold = (MouseButtons != MouseButtons.None);
        }
        void MouseRelease(MouseButtons B)
        {
            if (!(Draw.MousePress()||FileHandler.MousePress()||Draw.SelectedFamily))
            {
                Vector WorldPos = Draw.ScreenToWorld(MousePos.X, MousePos.Y);
                Creature C = Creatures.Find(x => (x.Pos - WorldPos).MagSq() < (x.Radius * x.Radius) * 4);
                Draw.Selected = C;
                ManualControl = false;
            }
        }
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            while(HoldKeys.Remove(e.KeyData));
        }
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            string S = ((char)e.KeyValue).ToString();
            if(ModifierKeys == Keys.Shift)
            {
                S=S.ToUpper();
            }else
                S = S.ToLower();
            char C = S[0];
            //Console.WriteLine((int)C);
            Draw.KeyPress(C);
            FileHandler.KeyPress(C);
            HoldKeys.Add(e.KeyData);
            if(e.KeyData==Keys.Enter)
            {
                ManualControl = !ManualControl;
            }
        }
    }
}
