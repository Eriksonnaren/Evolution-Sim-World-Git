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
        PointF CameraPosition;
        SizeF Size;
        float CameraAngle;
        float CameraAngleCos;
        float CameraAngleSin;
        float Zoom;
        PointF WorldToScreen(float x, float y)
        {
            return new PointF(x-Size.Width/2, y-Size.Height/2);
        }
        public Draw(Graphics Graphics,SizeF Size)
        {
            this.Graphics = Graphics;
            this.Size = Size;
            CameraAngle = 0;
            CameraAngleCos = 1;
            CameraAngleSin = 0;
        }
        public void MainDraw()
        {
            DrawWorld();
            DrawGUI();
        }
        void DrawWorld()
        {
            foreach (var C in Form1.Creatures)
            {
                PointF P = WorldToScreen(C.Pos.X, C.Pos.Y);
                C.Show(this,P.X,P.Y);
            }
        }
        void DrawGUI()
        {

        }
    }
}
