using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace Evolution_Simulator_World
{
    class FileHandler
    {
        List<GuiElement> FileButtons = new List<GuiElement>();
        GuiElement CancelButton;
        GuiElement SaveButton;
        GuiElement LoadButton;
        GuiElement TextBox;
        public static bool SaveState;
        public static bool LoadState;
        int AutoSave_Timer = 0;
        int AutoSave_Interval=(int)(120*Form1.fps);
        string FilePath;
        public FileHandler()
        {
            SaveButton = new GuiElement(GuiElement.Type.Button, new Rectangle(Form1.PB.Size.Width - 140, 20, 120, 30), "Save");
            LoadButton = new GuiElement(GuiElement.Type.Button, new Rectangle(Form1.PB.Size.Width - 140, 70, 120, 30), "Load");
            CancelButton = new GuiElement(GuiElement.Type.Button, new Rectangle(Form1.PB.Size.Width - 280, 20, 120, 30), "Cancel");
            TextBox = new GuiElement(GuiElement.Type.WriteText, new Rectangle(Form1.PB.Size.Width - 140, 70, 120, 30), "");
            SaveButton.ButtonEvent = SaveEvent;
            LoadButton.ButtonEvent = LoadEvent;
            CancelButton.ButtonEvent = CancelEvent;
            FilePath = Path.Combine(Form1.StartPath, "Saves");
            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }
            
        }
        public void Show(Draw D)
        {
            AutoSave_Timer++;
            if (AutoSave_Timer > AutoSave_Interval)
            {
                SaveButton.ButtonTimer = 20;
                Save(Path.Combine(FilePath, "Autosave"));
                AutoSave_Timer = 0;
            }
            if (LoadState||SaveState)
                CancelButton.Show(D);
            if (!LoadState)
                SaveButton.Show(D);
            if(!(SaveState||LoadState))
                LoadButton.Show(D);
            if(SaveState)
                TextBox.Show(D);
            foreach (GuiElement E in FileButtons)
            {
                E.Show(D);
            }
        }
        public bool MousePress()
        {
            if(LoadState||SaveState)
                if (getPressButton(CancelButton))
                    if (CancelButton.mouseDown())
                        return true;
            if (!LoadState)
                if (getPressButton(SaveButton))
                    if (SaveButton.mouseDown())
                        return true;
            if (!(SaveState || LoadState))
                if (getPressButton(LoadButton))
                    if (LoadButton.mouseDown())
                        return true;
            if (SaveState)
                if (getPressButton(TextBox))
                    if (TextBox.mouseDown())
                        return true;
            TextBox.Selected = false;
            foreach (GuiElement E in FileButtons)
            {
                E.Selected = false;
                if (getPressButton(E))
                    if (E.mouseDown())
                        return true;
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
            if(TextBox.Selected)
                TextBox.keyDown(Key);
        }
        void SaveEvent(object sender,EventArgs e)
        {
            if(SaveState)
            {
                if (TextBox.Text.Length > 0)
                {
                    Save(Path.Combine(FilePath, TextBox.Text));
                    FileButtons = new List<GuiElement>();
                    SaveState = false;
                }
            }else
            {
                CreateFileButtons(120);
                SaveState = true;
                TextBox.Selected = true;
            }
            CancelButton.ButtonTimer = 0;
        }
        void LoadEvent(object sender, EventArgs e)
        {
            LoadState=CreateFileButtons(20);
            CancelButton.ButtonTimer = 0;
        }
        void FileEvent(object sender, EventArgs e)
        {
            GuiElement Pressed = sender as GuiElement; 
            if(LoadState)
            {
                Load(Path.Combine(FilePath,Pressed.Text));
                LoadState = false;
                LoadButton.ButtonTimer = 0;
                FileButtons = new List<GuiElement>();
            }
            else if(SaveState)
            {
                TextBox.Text = Pressed.Text;
            }
            CancelButton.ButtonTimer = 0;
        }
        void CancelEvent(object sender, EventArgs e)
        {
            FileButtons = new List<GuiElement>();
            SaveState = LoadState = false;
            LoadButton.ButtonTimer = 0;
            
        }
        bool CreateFileButtons(int Y)
        {
            string[] Files = Directory.GetFiles(FilePath);
            for (int i = 0; i < Files.Length; i++)
            {
                string Text = Path.GetFileName(Files[i]);
                GuiElement G = new GuiElement(GuiElement.Type.Button, new Rectangle(Form1.PB.Width - 140, Y + i * 50, 120, 30), Text);
                FileButtons.Add(G);
                G.ButtonEvent = FileEvent;
            }
            return Files.Length > 0;
        }
        void Save(string Path)
        {
            using (Stream stream = File.Open(Path, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                object[] O = new object[] { Form1.Creatures, Form1.Eggs, Form1.Trees, Form1.Seeds, Form1.Foods };
                binaryFormatter.Serialize(stream, O);
            }

        }
        void Load(string Path)
        {
            using (Stream stream = File.Open(Path, FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                object[] O = binaryFormatter.Deserialize(stream) as object[];
                Form1.Creatures = O[0] as List<Creature>;
                Form1.Eggs = O[1] as List<Egg>;
                Form1.Trees = O[2] as List<Tree>;
                Form1.Seeds = O[3] as List<Seed>;
                Form1.Foods = O[4] as List<Food>;
            }
            Draw.Families=FindFamilies();
            Draw.UpdateFamily = true;
        }
        List<Family> FindFamilies()
        {
            List<Family> Families=new List<Family>();
            foreach (var C in Form1.Creatures)
            {
                if(C.Family!=null)
                {
                    if(!Families.Contains(C.Family))
                    {
                        Families.Add(C.Family);
                    }
                }
            }
            return Families;
        }
    }
}
