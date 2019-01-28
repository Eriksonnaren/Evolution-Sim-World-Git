using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution_Simulator_World
{
    public class UpdateWorld
    {
        public struct ResourceContainer
        {
            public float Energy;
            public float Water;
            public float Nutrients;
            public ResourceContainer(float energy, float water, float nutrients)
            {
                Energy = energy;
                Water = water;
                Nutrients = nutrients;
            }
            public static ResourceContainer operator +(ResourceContainer A,ResourceContainer B)
            {
                return new ResourceContainer(A.Energy+B.Energy,A.Water+B.Water,A.Nutrients+B.Nutrients);
            }
            public static ResourceContainer operator -(ResourceContainer A, ResourceContainer B)
            {
                return new ResourceContainer(A.Energy - B.Energy, A.Water - B.Water, A.Nutrients - B.Nutrients);
            }
            public static ResourceContainer operator *(ResourceContainer A, float M)
            {
                return new ResourceContainer(A.Energy * M, A.Water * M, A.Nutrients * M);
            }
            public static ResourceContainer operator /(ResourceContainer A, float M)
            {
                return new ResourceContainer(A.Energy / M, A.Water / M, A.Nutrients / M);
            }
        }
        public static List<Entity> Entities = new List<Entity>();
        public static List<Creature> Creatures = new List<Creature>();
        public static List<Tree> Trees = new List<Tree>();
        public static Chunk[][] Chunks;
        public static int MinCreatures = 60;
        public static int MinTrees = 200;
        public static int Speed=1;
        public const float ChunkSize = 1000;
        public const int ChunkAmount=21;
        int AverageCreatures = 0;
        int AverageTrees = 0;
        int GraphTimer=0;
        int GraphTimerMax = 300;
        int SpawnTimer = 0;
        public UpdateWorld()
        {
            Chunks = new Chunk[ChunkAmount][];
            for (int i = 0; i < ChunkAmount; i++)
            {
                Chunks[i] = new Chunk[ChunkAmount];
                for (int j = 0; j < ChunkAmount; j++)
                {
                    Chunks[i][j] = new Chunk(new VectorI(i,j));
                }
            }
            for (int i = 0; i < MinCreatures; i++)
            {
                GetRandomPosition(out Vector Pos, out VectorI Chunk);
                AddEntity(new Creature(), Pos, Chunk);
            }
            for (int i = 0; i < MinTrees; i++)
            {
                GetRandomPosition(out Vector Pos, out VectorI Chunk);
                AddEntity(new Tree(Pos, Chunk), Pos, Chunk);
            }

        }
        public void MainUpdate()
        {
            //(Entities[0] as Creature).Selected = true;
            for (int s = 0; s < Speed; s++)
            {

                /*if (Form1.Creatures.Count < MinCreatures||SpawnTimer>=30)
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
                }*/
                Creatures = Entities.OfType<Creature>().ToList();
                Trees = Entities.OfType<Tree>().ToList();
                if (Creatures.Count < MinCreatures)
                {
                    GetRandomPosition(out Vector Pos, out VectorI Chunk);
                    AddEntity(new Creature(),Pos,Chunk);
                }
                if (Trees.Count < MinTrees)
                {
                    GetRandomPosition(out Vector Pos, out VectorI Chunk);
                    AddEntity(new Tree(Pos, Chunk), Pos, Chunk);
                }
                for (int i = Entities.Count-1; i >= 0; i--)
                {
                    while (i >= Entities.Count - 1)
                        i--;
                    Entity E = Entities[i];
                    E.Update();
                    if(E is MovingEntity M)
                    {
                        M.Pos += M.Vel;
                        M.Vel *= (1 - M.Friction);
                        ConstrainEntityInChunk(M);
                    }
                }
                Collide();
                AverageCreatures += Creatures.Count;
                AverageTrees += Trees.Count;
                GraphTimer++;
                if (GraphTimer >= GraphTimerMax)
                {
                    GraphTimer = 0;
                    Draw.CreatureGraph.Add((AverageCreatures+ GraphTimerMax/2) /GraphTimerMax);
                    Draw.TreeGraph.Add((AverageTrees+ GraphTimerMax/2) / GraphTimerMax);
                    AverageCreatures = AverageTrees = 0;
                }
            }
            CheckValidChunks();
        }
        public void Reset()
        {
            for (int i = 0; i < ChunkAmount; i++)
            {
                for (int j = 0; j < ChunkAmount; j++)
                {
                    Chunks[i][j].Entities.Clear();
                    Chunks[i][j].EntitiesCM.Clear();
                    Chunks[i][j].EntitiesCS.Clear();
                }
            }
            foreach (var E in Entities)
            {
                Chunks[E.ChunkPos.X][E.ChunkPos.Y].Add(E);
            }
            GraphTimer = 0;
            AverageCreatures = AverageTrees = 0;
        }
        public static Vector GetRelativePosition(Entity E1,Entity E2)
        {
            return GetRelativePosition(E1.ChunkPos, E1.Pos, E2.ChunkPos, E2.Pos);
        }
        public static Vector GetRelativePosition(VectorI Chunk1,Vector V1,VectorI Chunk2,Vector V2)
        {
            VectorI RelativeChunk = Chunk2 - Chunk1;
            Vector RelativeInChunk = V2 - V1;
            float X = (RelativeChunk.X) * ChunkSize;
            if (RelativeChunk.X > 0)
            {
                if (RelativeChunk.X * 2 > ChunkAmount)
                    X -= ChunkAmount * ChunkSize;
            }
            else
            {
                if (RelativeChunk.X * 2 < -ChunkAmount)
                    X += ChunkAmount * ChunkSize;
            }
            X += RelativeInChunk.X;
            float Y = (RelativeChunk.Y) * ChunkSize;
            if (RelativeChunk.Y > 0)
            {
                if (RelativeChunk.Y * 2 > ChunkAmount)
                    Y -= ChunkAmount * ChunkSize;
            }
            else
            {
                if (RelativeChunk.Y * 2 < -ChunkAmount)
                    Y += ChunkAmount * ChunkSize;
            }
            Y += RelativeInChunk.Y;
            return new Vector(X,Y);
        }
        public void GetRandomPosition(out Vector V, out VectorI C)
        {
            V = new Vector(Form1.Rand.Next((int)ChunkSize), Form1.Rand.Next((int)ChunkSize));
            C = new VectorI(Form1.Rand.Next(ChunkAmount), Form1.Rand.Next(ChunkAmount));
        }
        public static void AddEntity(Entity E,Vector V, VectorI C)
        {
            int Pos = 0;
            bool KeepGoing = true;
            while (KeepGoing)//perform insertion sort so that the entities gets drawn in the correct order
            {
                if (Pos >= Entities.Count-1)
                {
                    Entities.Add(E);
                    KeepGoing = false;
                }else
                if (Entities[Pos].DrawLayer>=E.DrawLayer)
                {
                    Entities.Insert(Pos,E);
                    KeepGoing = false;
                }
                Pos++;
            }
            
            Chunks[C.X][C.Y].Add(E);
            E.ChunkPos = C;
            E.Pos = V;
        }
        public static void ConstrainVectorInChunk(ref Vector Vector,ref VectorI Chunk)
        {
            bool Done;
            do
            {
                Done = true;
                if (Vector.X > ChunkSize)
                {
                    Vector -= new Vector(ChunkSize, 0);
                    Chunk = new VectorI((Chunk.X + 1) % ChunkAmount, Chunk.Y);
                    Done = false;
                }
                else
                if (Vector.X < 0)
                {
                    Vector += new Vector(ChunkSize, 0);
                    Chunk = new VectorI((Chunk.X + ChunkAmount - 1) % ChunkAmount, Chunk.Y);
                    Done = false;
                }
                if (Vector.Y > ChunkSize)
                {
                    Vector -= new Vector(0, ChunkSize);
                    Chunk = new VectorI(Chunk.X, (Chunk.Y + 1) % ChunkAmount);
                    Done = false;
                }
                else
                if (Vector.Y < 0)
                {
                    Vector += new Vector(0, ChunkSize);
                    Chunk = new VectorI(Chunk.X, (Chunk.Y + ChunkAmount - 1) % ChunkAmount);
                    Done = false;
                }
            }
            while (!Done);
        }
        public static void ConstrainEntityInChunk(Entity E)
        {
            if (E.Pos.X > ChunkSize)
            {
                E.Pos -= new Vector(ChunkSize, 0);
                Chunks[E.ChunkPos.X][E.ChunkPos.Y].Remove(E);
                E.ChunkPos = new VectorI((E.ChunkPos.X + 1) % ChunkAmount, E.ChunkPos.Y);
                Chunks[E.ChunkPos.X][E.ChunkPos.Y].Add(E);
            }
            else
            if (E.Pos.X < 0)
            {
                E.Pos += new Vector(ChunkSize, 0);
                Chunks[E.ChunkPos.X][E.ChunkPos.Y].Remove(E);
                E.ChunkPos = new VectorI((E.ChunkPos.X + ChunkAmount - 1) % ChunkAmount, E.ChunkPos.Y);
                Chunks[E.ChunkPos.X][E.ChunkPos.Y].Add(E);
            }
            if (E.Pos.Y > ChunkSize)
            {
                E.Pos -= new Vector(0, ChunkSize);
                Chunks[E.ChunkPos.X][E.ChunkPos.Y].Remove(E);
                E.ChunkPos = new VectorI(E.ChunkPos.X, (E.ChunkPos.Y + 1) % ChunkAmount);
                Chunks[E.ChunkPos.X][E.ChunkPos.Y].Add(E);
            }
            else
            if (E.Pos.Y < 0)
            {
                E.Pos += new Vector(0, ChunkSize);
                Chunks[E.ChunkPos.X][E.ChunkPos.Y].Remove(E);
                E.ChunkPos = new VectorI(E.ChunkPos.X, (E.ChunkPos.Y + ChunkAmount - 1) % ChunkAmount);
                Chunks[E.ChunkPos.X][E.ChunkPos.Y].Add(E);
            }
        }
        public static void RemoveEntity(Entity E)
        {
            Chunks[E.ChunkPos.X][E.ChunkPos.Y].Remove(E);
            Entities.Remove(E);
        }
        void CheckValidChunks()
        {
            foreach (var E in Entities)
            {
                if(!Chunks[E.ChunkPos.X][E.ChunkPos.Y].Entities.Contains(E))
                {
                    throw new Exception("Entity has wrong chunk");
                }
            }
        }
        void Collide()
        {
            //int Checks = 0;
            CollideMove M1;
            Chunk C1;
            Chunk C2;
            for (int ChunkX = 0; ChunkX < ChunkAmount; ChunkX++)
            {
                for (int ChunkY = 0; ChunkY < ChunkAmount; ChunkY++)
                {
                    for (int Entity1 = 0; Entity1 < Chunks[ChunkX][ChunkY].EntitiesCM.Count; Entity1++)
                    {
                        C1 = Chunks[ChunkX][ChunkY];
                        M1 = C1.EntitiesCM[Entity1];
                        for (int Entity2 = Entity1+1; Entity2 < C1.EntitiesCM.Count; Entity2++)
                        {
                            //Checks++;
                            CollidePair(M1, C1.EntitiesCM[Entity2]);
                        }
                        for (int Entity2 = 0; Entity2 < C1.EntitiesCS.Count; Entity2++)
                        {
                            //Checks++;
                            CollidePair(M1, C1.EntitiesCS[Entity2]);
                        }
                        for (int Chunk2 = 1; Chunk2 < 4; Chunk2++)
                        {
                            int i = Chunk2 / 2;
                            int j = Chunk2 - 2 * i;
                            i = (i + ChunkX) % ChunkAmount;
                            j = (j + ChunkY) % ChunkAmount;
                            C2 = Chunks[i][j];
                            for (int Entity2 = Entity1 + 1; Entity2 < C2.EntitiesCM.Count; Entity2++)
                            {
                                //Checks++;
                                CollidePair(M1, C2.EntitiesCM[Entity2]);
                            }
                            for (int Entity2 = 0; Entity2 < C2.EntitiesCS.Count; Entity2++)
                            {
                                //Checks++;
                                CollidePair(M1, C2.EntitiesCS[Entity2]);
                            }
                        }
                    }
                }
            }
            //Console.WriteLine(Checks);
        }
        /*void CheckPair(Entity E1, Entity E2)
        {
            Vector RelPos = GetRelativePosition(E1, E2);
            float DistSq = RelPos.MagSq();
            if (DistSq < Sq(Creature.EyeRadius))
            {
                if (E1 is Creature C1 && E2 is Visible V2)
                {
                    C1.UpdateVision(V2);
                }
                if (E2 is Creature C2 && E1 is Visible V1)
                {
                    C2.UpdateVision(V1);
                }
            }
            if (E1 is CollideTrigger T1 && E2 is CollideTrigger T2)
            {
                if (DistSq < Sq(T1.Radius + T2.Radius))
                {
                    T1.OnCollision(T2, RelPos);
                    T2.OnCollision(T1, -RelPos);
                    if(E1 is CollideMove M1&&M1.EnableCollide)
                    {
                        if (E2 is CollideMove M2 && M2.EnableCollide)
                        {
                            CollidePair(M1, M2, RelPos, DistSq);
                        }
                        else if(E2 is CollideStatic S2)
                        {
                            CollidePair(M1, S2, RelPos, DistSq);
                        }
                    }else if(E1 is CollideStatic S1)
                    {
                        if (E2 is CollideMove M2 && M2.EnableCollide)
                        {
                            CollidePair(M2, S1, -RelPos, DistSq);
                        }
                    }
                }
            }
        }*/
        void CollidePair(CollideMove E1, CollideMove E2)
        {

            Vector RelPos = GetRelativePosition(E1, E2);
            float DistSq = RelPos.MagSq();
            if (DistSq < Sq(Creature.EyeRadius))
            {
                if (E1 is Creature C1 && E2 is Visible V2)
                {
                    C1.UpdateVision(V2);
                }
                if (E2 is Creature C2 && E1 is Visible V1)
                {
                    C2.UpdateVision(V1);
                }
            }
            if (DistSq < Sq(E1.Radius + E2.Radius))
            {
                E1.OnCollision(E2, RelPos);
                E2.OnCollision(E1, -RelPos);
                if (E1.EnableCollide && E2.EnableCollide)
                {
                    if (((E2.Vel - E1.Vel) * RelPos) < 0)
                    {
                        float Denominator = (E1.Mass + E2.Mass) * DistSq;
                        Vector dV1 = ((2 * E2.Mass * ((E1.Vel - E2.Vel) * RelPos)) / Denominator) * RelPos;
                        Vector dV2 = ((2 * E1.Mass * ((E2.Vel - E1.Vel) * RelPos)) / Denominator) * RelPos;
                        E1.Vel -= dV1;
                        E2.Vel -= dV2;
                        Vector P = RelPos * (DistSq - Sq(E1.Radius + E2.Radius)) / (2 * DistSq);
                        E1.Pos += P;
                        E2.Pos -= P;

                    }
                }
            }
        }
        void CollidePair(CollideMove E1, CollideStatic E2)
        {
            Vector RelPos = GetRelativePosition(E1, E2);
            float DistSq = RelPos.MagSq();
            if (DistSq < Sq(Creature.EyeRadius))
            {
                if (E1 is Creature C1 && E2 is Visible V2)
                {
                    C1.UpdateVision(V2);
                }
            }
            if (DistSq < Sq(E1.Radius + E2.Radius))
            {
                E1.OnCollision(E2, RelPos);
                E2.OnCollision(E1, -RelPos);
                if (E1.EnableCollide)
                {
                    if ((-E1.Vel * RelPos) < 0)
                    {
                        float Denominator = DistSq;
                        Vector dV1 = ((2 * ((E1.Vel) * RelPos)) / Denominator) * RelPos;
                        //Vector dV2 = ((2 * E1.Mass * ((E2.Vel - E1.Vel) * RelPos)) / Denominator) * RelPos;
                        E1.Vel -= dV1;
                        Vector P = RelPos * (DistSq - Sq(E1.Radius + E2.Radius)) / (DistSq);
                        E1.Pos += P;
                    }
                }
            }
        }
        float Sq(float x) => x * x;
    }
}
