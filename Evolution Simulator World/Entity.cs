using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Evolution_Simulator_World
{
    public interface Entity
    {
        /// <summary>
        /// Relative to the current chunk
        /// </summary>
        Vector Pos { get; set; }
        void Update();
        void Show(Draw D, float x, float y, float Zoom = 1);
        VectorI ChunkPos { get; set; }
        /// <summary>
        /// Lower values are drawn below higher values
        /// </summary>
        int DrawLayer { get; }
    }
    public interface SelectableObject : Entity
    {
        float Radius { get; }
        bool Dead { get; set; }
    }
    public interface MovingEntity : Entity
    {
        Vector Vel { get; set; }
        float Friction { get; }
    }
    public interface Visible : Entity
    {
        Color Col { get; set; }
        float Hue { get; set; }
    }
    public interface CollideTrigger : Entity
    {
        float Radius { get; }
        void OnCollision(CollideTrigger Other,Vector Vector);
    }
    public interface CollideMove : CollideTrigger,MovingEntity
    {
        float Mass { get; }
        bool EnableCollide { get; set; }
    }
    public interface CollideStatic : CollideTrigger
    {
        
    }
    public interface FamilyMember : SelectableObject
    {
        List<FamilyMember> Parents { get; set; }
        List<FamilyMember> Children { get; set; }
        int Generation { get; set; }
        int FamilyPos { get; set; }
        Family Family { get; set; }
        bool Selected { get; set; }
        string Name { get; set; }
    }
}
