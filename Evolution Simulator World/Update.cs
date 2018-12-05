using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution_Simulator_World
{
    public class UpdateWorld
    {
        public static int MinCreatures = 60;
        public static int MinTrees = 120;
        public static int Speed=1;
        int AverageCreatures = 0;
        int AverageTrees = 0;
        int GraphTimer=0;
        int GraphTimerMax = 300;
        int SpawnTimer = 0;
        public void MainUpdate()
        {
            for (int s = 0; s < Speed; s++)
            {
                if (Form1.Creatures.Count < MinCreatures||SpawnTimer>=30)
                {
                    SpawnTimer = 0;
                    float angle = Form1.Rand.Next(0, 360);
                    float Dist = Form1.Sqrt((float)Form1.Rand.NextDouble()) * (Form1.ArenaRadius - 200);
                    float X = Form1.Cos(angle) * Dist;
                    float Y = Form1.Sin(angle) * Dist;
                    Form1.Creatures.Add(new Creature(new Vector(X, Y)));
                }
                if (Form1.Trees.Count < MinTrees)
                {
                    float angle = Form1.Rand.Next(0, 360);
                    float Dist = Form1.Sqrt((float)Form1.Rand.NextDouble()) * (Form1.ArenaRadius - 200);
                    float X = Form1.Cos(angle) * Dist;
                    float Y = Form1.Sin(angle) * Dist;
                    Form1.Trees.Add(new Tree(new Vector(X, Y)));
                }
                foreach (Creature C in Form1.Creatures)
                {
                    C.Update();
                }
                
                foreach (var T in Form1.Trees)
                {
                    T.Update();
                }
                for (int i = Form1.Eggs.Count - 1; i >= 0; i--)
                {
                    Form1.Eggs[i].Update();
                }
                for (int i = Form1.Seeds.Count - 1; i >= 0; i--)
                {
                    Form1.Seeds[i].Update();
                }
                for (int i = Form1.Creatures.Count - 1; i >= 0; i--)
                {
                    if (Form1.Creatures[i].Dead)
                        Form1.Creatures.RemoveAt(i);
                }
                for (int i = Form1.Trees.Count - 1; i >= 0; i--)
                {
                    if (Form1.Trees[i].Dead)
                        Form1.Trees.RemoveAt(i);
                }
                AverageCreatures += Form1.Creatures.Count;
                AverageTrees += Form1.Trees.Count;
                GraphTimer++;
                if (GraphTimer >= GraphTimerMax)
                {
                    GraphTimer = 0;
                    Draw.CreatureGraph.Add(AverageCreatures/GraphTimerMax);
                    Draw.TreeGraph.Add(AverageTrees / GraphTimerMax);
                    AverageCreatures = AverageTrees = 0;
                }
            }
        }
        public void Reset()
        {
            GraphTimer = 0;
            AverageCreatures = AverageTrees = 0;
        }
    }
}
