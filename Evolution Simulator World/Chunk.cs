using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evolution_Simulator_World
{
    public struct Chunk
    {
        public List<Entity> Entities;
        public List<CollideMove> EntitiesCM;
        public List<CollideStatic> EntitiesCS;

        public float Water;
        public float Nutrients;
        readonly float MaxWater;

        public Chunk(VectorI Pos)
        {
            this.Pos = Pos;
            Entities = new List<Entity>();
            EntitiesCM = new List<CollideMove>();
            EntitiesCS = new List<CollideStatic>();
            Nutrients = 0;
            MaxWater = 100;
            Water = MaxWater;
        }
        
        public void Add(Entity E)
        {
            Entities.Add(E);
            if (E is CollideMove CM)
                EntitiesCM.Add(CM);
            if (E is CollideStatic CS)
                EntitiesCS.Add(CS);
        }
        public void Remove(Entity E)
        {
            Entities.Remove(E);
            if (E is CollideMove CM)
                EntitiesCM.Remove(CM);
            if (E is CollideStatic CS)
                EntitiesCS.Remove(CS);
        }
        public VectorI Pos;
    }
}
