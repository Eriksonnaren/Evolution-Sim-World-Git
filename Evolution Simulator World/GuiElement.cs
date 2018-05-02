using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Evolution_Simulator_World
{
    class GuiElement
    {
        public enum Type
        {
            TextBox,
            WriteText,
            WriteNum,
            Button
        }
        public Rectangle Rect;
        public string Text;
        public float Num;
        public Type type;
        public bool Selected;
        StringFormat SF = new StringFormat();
        Pen Pe = new Pen(Color.Black, 10);
        public EventHandler ButtonEvent;
        public int ButtonTimer = 0;
        public GuiElement(Type type, Rectangle Rect,string Text)
        {
            this.Text = Text;
            this.Rect = Rect;
            this.type = type;
            SF.Alignment = StringAlignment.Center;
            SF.LineAlignment = StringAlignment.Center;
            Pe.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
        }
        public void Show(Draw D)
        {
            if (ButtonTimer > 0) ButtonTimer--;
            if (type == Type.WriteText && Selected)
            {
                Pe.Color = Color.Orange;
            }
            else
                Pe.Color = Color.Black;
            D.Graphics.DrawRectangle(Pe,Rect);
            Brush BackColor;
            switch (type)
            {
                case Type.TextBox:
                    BackColor = Brushes.SlateGray;
                    break;
                case Type.WriteText:
                    BackColor = Brushes.LightBlue;
                    break;
                case Type.WriteNum:
                    BackColor = Brushes.LightBlue;
                    break;
                case Type.Button:
                    BackColor = ButtonTimer==0?Brushes.DarkRed:Brushes.DarkGreen;
                    break;
                default:
                    BackColor = Brushes.White;
                    break;
            }
            D.Graphics.FillRectangle(BackColor, Rect);
            D.Graphics.DrawString(Text,D.Font,Brushes.White,Rect.X+Rect.Width/2,Rect.Y+Rect.Height/2,SF);

        }
        public bool mouseDown()
        {
            if(type == Type.Button&&ButtonEvent!=null)
            {
                ButtonEvent.Invoke(this, null);
                ButtonTimer = 6;
                return true;
            }else if(type == Type.WriteText)
            {
                Selected = true;
                return true;
            }
            return false;
        }
        public void keyDown(char Key)
        {
            char[] ValidKeys = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890 ".ToArray();
            bool ok = false;
            for (int i = 0; i < ValidKeys.Length&&!ok; i++)
            {
                ok = Key == ValidKeys[i];
            }
            if (ok)
                Text += Key;
            else if(Key==8&&Text.Length>0)
            {
                Text = Text.Remove(Text.Length - 1);
            }
        }
    }
}
