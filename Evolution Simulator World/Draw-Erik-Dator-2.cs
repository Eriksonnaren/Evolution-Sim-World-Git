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
        bool BlindMode = false;
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
            this.Graphics = Graphics;
            this.Size = Size;
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
                Graphics.RotateTransform(-Selected.Angle-90);
                CameraPosition = Selected.Pos.ToPoint();
                if (BlindMode)
                {
                    PointF P = WorldToScreen(Selected.Pos.X, Selected.Pos.Y);
                    Selected.Show(this, P.X, P.Y, Zoom, true);
                }
            }
            if(Selected==null||!BlindMode)
            {
                foreach (var T in Form1.Trees)
                {
                    PointF P = WorldToScreen(T.Pos.X, T.Pos.Y);
                    T.ShowBelow(this, P.X, P.Y, Zoom);
                }
                
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
            }
            Graphics.ResetTransform();
            
        }
        void DrawGUI()
        {

        }
    }
}
