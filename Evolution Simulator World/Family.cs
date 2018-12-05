using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Evolution_Simulator_World
{
    [Serializable]
    public class Family
    {
        List<List<Creature>> FamilyTree;
        public Creature Root;
        public Creature Common_Ancestor;
        Creature Hover;
        List<Creature> HoverFamily;
        float ScrollValue = 0;
        float StartScrollValue = 0;
        float MouseStartY = 0;
        bool ScrollHold = false;
        public int CreaturesAlive = 0;

        public Family(Creature Root)
        {
            this.Root = Root;
            Common_Ancestor = Root;
            FamilyTree = new List<List<Creature>>() { new List<Creature>() { Root} };
            CreaturesAlive = Root.Dead?0:1;
            Draw.Families.Add(this);
            Draw.UpdateFamily=true;
        }
        public void Add(Creature Parent, Creature Children)
        {
            CreaturesAlive++;
            Children.Family = this;
            Children.Generation = Parent.Generation + 1;
            if (FamilyTree.Count+Common_Ancestor.Generation <= Children.Generation)
            {
                FamilyTree.Add(new List<Creature>() { Children });
                Children.FamilyPos = 0;
            }else
            {
                int ChildPos;
                int id = Parent.Generation - Common_Ancestor.Generation;
                if (Parent.Children.Count > 0)
                {
                    ChildPos = Parent.Children[Parent.Children.Count-1].FamilyPos+1;
                }
                else
                {
                    int ParentPos = Parent.FamilyPos;
                    while (ParentPos > 0 && FamilyTree[id][ParentPos].Children.Count == 0)
                        ParentPos--;

                    if (FamilyTree[id][ParentPos].Children.Count == 0)
                    {
                        ChildPos = 0;
                    }
                    else
                    {
                        ChildPos = FamilyTree[id][ParentPos].Children[FamilyTree[id][ParentPos].Children.Count - 1].FamilyPos + 1;
                    }
                }
                Children.FamilyPos = ChildPos;
                FamilyTree[id+1].Insert(ChildPos, Children);
                for (int i = ChildPos+1; i < FamilyTree[id+1].Count; i++)
                {
                    FamilyTree[id+1][i].FamilyPos++;
                }
            }
        }
        public void Remove(Creature Cre)
        {
            if (Cre.Children.Count == 0)
            {
                if (Cre != Common_Ancestor)
                {
                    foreach (var C in Cre.Parents)
                    {
                        C.Children.Remove(Cre);
                    }
                    if (Cre.Parents.Count > 0)
                        if (Cre.Parents[0].Dead)
                        {
                            Remove(Cre.Parents[0]);
                        }
                    Cre.Parents.Clear();
                    int id = Cre.Generation - Common_Ancestor.Generation;
                    if (id >= 0)
                    {
                        FamilyTree[id].RemoveAt(Cre.FamilyPos);
                        for (int i = Cre.FamilyPos; i < FamilyTree[id].Count; i++)
                        {
                            FamilyTree[id][i].FamilyPos--;
                        }
                    }
                }
                else
                {
                    Cre.Family = null;
                    Draw.Families.Remove(this);
                    Draw.UpdateFamily = true;
                }
            }
            else if (Cre == Common_Ancestor && Cre.Children.Count == 1)
            {
                Common_Ancestor = Cre.Children[0];
                Cre.Children[0].Parents.Clear();
                Cre.Children.Clear();
                if (Cre.Generation > 0)
                {
                    foreach (var C in Cre.Parents)
                    {
                        C.Children.Remove(Cre);
                    }
                }
                FamilyTree.RemoveAt(0);
                if (Common_Ancestor.Dead)
                    Remove(Common_Ancestor);

            }
        }
        public void Show(Draw D,Rectangle Rect)
        {
            float Zoom = 0.35f;
            int Width = 0;
            int StartX = Rect.X+Rect.Width / 2;
            int dx = 20;
            int dy = 75;
            bool MouseOver = false;
            int Offset = Common_Ancestor.Generation;
            int FullHeight = (FamilyTree.Count+2) * dy;
            int Outside = FullHeight-Rect.Height;
            int Top = dy-(int)(Outside*ScrollValue);
            for (int i = Offset; i < FamilyTree.Count+Offset; i++)
            {
                int FamId = i - Offset;
                int Y = dy * (i-Offset) + Top;
                
                int NextY = Y + dy;
                int NextWidth = 0;
                int NextStartX = 0;
                int Nextdx = 0;
                
                if (FamId + 1 < FamilyTree.Count)
                {
                    NextWidth = (int)(Rect.Width * (1 - 5 / (FamilyTree[FamId + 1].Count + 4f)));
                    NextStartX = Rect.X+(Rect.Width - NextWidth) / 2;
                    if (FamilyTree[FamId + 1].Count > 1)
                        Nextdx = NextWidth / (FamilyTree[FamId + 1].Count - 1);
                    else
                        Nextdx = 0;
                }

                D.Graphics.DrawString(i.ToString(), D.Font, Brushes.Black, StartX-50, Y, D.SF);

                for (int j = 0; j < FamilyTree[FamId].Count; j++)
                {
                    int X = StartX + j * dx;
                    Creature Current = FamilyTree[FamId][j];
                    bool inHoverFamily = false;
                    if (HoverFamily != null && i-Offset < HoverFamily.Count)
                        inHoverFamily = HoverFamily[FamId].FamilyPos == j;
                    for (int k = 0; k < Current.Children.Count; k++)
                    {
                        int X2 = NextStartX + Current.Children[k].FamilyPos * Nextdx;
                        int Y2 = NextY;
                        Color Col = Color.LightGray;
                        bool HoverChildren = false;
                        if(inHoverFamily&&i-Offset<HoverFamily.Count-1)
                        {
                            if(HoverFamily[FamId+1].FamilyPos == Current.Children[k].FamilyPos)
                            {
                                Col = Color.Green;
                                HoverChildren = true;
                            }
                        }
                        D.Graphics.DrawLine(new Pen(Col, HoverChildren? 4:2), X, Y, X2, Y2);
                    }
                    if (!Current.Dead||inHoverFamily||Current.Selected)
                    {
                        float R = Current.Radius * Zoom * 1.5f;
                        Color Col = Color.DarkOrange;
                        if (inHoverFamily)
                            Col = Color.Green;
                        if (Current.Selected)
                            D.Graphics.FillEllipse(Brushes.DarkGreen, X - R, Y - R, R * 2, R * 2);
                        else
                            D.Graphics.DrawEllipse(new Pen(Col,2), X - R, Y - R, R * 2, R * 2);
                    }
                    if (Current.IsEgg != null)
                    {
                        Current.IsEgg.Show(D, X, Y, Zoom);
                    }
                    else
                    {
                        Current.Show(D, X, Y, Zoom);
                    }
                        if (Intersect(X, Y, Current.Radius * Zoom*1.5f))
                        {
                        D.Graphics.DrawString(Current.Name, D.Font, Brushes.Black, X, Y - Current.Radius * Zoom * 1.5f, D.SF);
                        Hover = Current;
                            MouseOver = true;
                        }
                }
                Width = NextWidth;
                StartX = NextStartX;
                dx = Nextdx;
            }
            if(!MouseOver)
            {
                Hover = null;
                HoverFamily = null;
            }
            else
            {
                HoverFamily = new List<Creature>() { Hover };
                int CurrentGen=Hover.Generation-Common_Ancestor.Generation;
                int CurrentPos=Hover.FamilyPos;
                while(CurrentGen>0&&FamilyTree[CurrentGen][CurrentPos].Parents.Count>0)
                {
                    CurrentPos = FamilyTree[CurrentGen][CurrentPos].Parents[0].FamilyPos;
                    CurrentGen--;
                    HoverFamily.Insert(0,FamilyTree[CurrentGen][CurrentPos]);
                }
                if (Form1.MouseHold)
                    D.Selected = D.SelectedCreature = Hover;
            }
            ShowScroll(D);
        }
        bool Intersect(int X,int Y,float Rad)
        {
            return (Form1.MousePos.X - X) * (Form1.MousePos.X - X) + (Form1.MousePos.Y - Y) * (Form1.MousePos.Y - Y) <= Rad * Rad;
        }
        void ShowScroll(Draw D)
        {
            Pen Pe = new Pen(Color.Black,10);
            Pe.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            int X = Form1.PB.Width - 70;
            int Y = 130;
            int Width = 30;
            int Height = Form1.PB.Height - Y - 20;
            D.Graphics.DrawRectangle(Pe, X, Y, Width, Height);
            D.Graphics.FillRectangle(Brushes.LightGray,X,Y,Width,Height);
            int Height2 = 100;
            int X2 = X + 5;
            float Y2 = Y + Lerp(5, Height - Height2-5, ScrollValue);
            int Width2 = Width - 10;
            
            if (Form1.MouseHold)
            {
                if (Form1.MousePos.X > X2 && Form1.MousePos.Y > Y2 && Form1.MousePos.X < X2 + Width2 && Form1.MousePos.Y < Y2 + Height2)
                {
                    if (!ScrollHold)
                    {
                        MouseStartY = Form1.MousePos.Y;
                        ScrollHold = true;
                        StartScrollValue = ScrollValue;
                    }
                }
            }
            else
                ScrollHold = false;
            
            if(ScrollHold)
            {
                float Multiplier = -(Height - Height2);
                ScrollValue = StartScrollValue+(MouseStartY - Form1.MousePos.Y)/Multiplier;
                if (ScrollValue >= 1)
                    ScrollValue = 1;
                else if (ScrollValue <= 0)
                    ScrollValue = 0;
            }
            Y2 = Y + Lerp(5, Height - Height2 - 5, ScrollValue);
            D.Graphics.DrawRectangle(Pe, X2, Y2, Width2, Height2);
            D.Graphics.FillRectangle(Brushes.Red, X2, Y2, Width2, Height2);
        }
        float Lerp(float A, float B,float T)
        {
            return A + (B - A) * T;
        }
    }
}
