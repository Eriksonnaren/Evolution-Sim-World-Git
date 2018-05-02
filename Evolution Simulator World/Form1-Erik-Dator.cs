using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Evolution_Simulator_World
{
    public partial class Form1 : Form
    {
        public static List<Creature> Creatures = new List<Creature>();
        public static Random Rand = new Random();
        BufferedGraphics BG;
        PictureBox PB;
        Draw Draw;
        Timer T=new Timer();
        static float[] SinArray;
        static float[] CosArray;
        static int SinAmount = 1000;
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
            T.Interval = 20;
            T.Tick += T_Tick;
            T.Start();
            Creatures.Add(new Creature(new Vector(0, 0)));
            SinArray = new float[SinAmount];
            CosArray = new float[SinAmount];
            for (int i = 0; i < SinAmount; i++)
            {
                double angle = Math.PI*2 * i / (float)SinAmount;
                SinArray[i] = (float)Math.Sin(angle);
                CosArray[i] = (float)Math.Cos(angle);
            }
        }
        private void T_Tick(object sender, EventArgs e)
        {
            BG.Graphics.Clear(Color.Gray);
            Draw.MainDraw();
            BG.Render();
        }
        public static float Sin(float angle)
        {
            int i = (int)(SinAmount * angle / 360);
            i = i % SinAmount;
            if(i<0)
                i+=SinAmount;
            return SinArray[i];
        }
        public static float Cos(float angle)
        {
            int i = (int)(SinAmount * angle / 360);
            i = i % SinAmount;
            if (i < 0)
                i += SinAmount;
            return CosArray[i];
        }
        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            
        }
    }
}
