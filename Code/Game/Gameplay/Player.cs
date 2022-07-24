using Core;
using Godot;

namespace Game.Gameplay
{
    public class Player : KinematicBody2D
    {
        [Export] private NodePath CollisionShapePath { get; set; }
        
        private CollisionShape2D CollisionShape { get; set; }

        public override void _Ready()
        {
            CollisionShape = this.GetNodeChecked<CollisionShape2D>(CollisionShapePath);
        
            SetAttackMode(false);
        }
        
        public override void _PhysicsProcess(float delta)
        {
            if (!CollisionShape.Disabled)
            {
                KinematicCollision2D collision = MoveAndCollide(Vector2.Zero);
                ((Node2D)collision?.Collider)?.QueueFree();
            }
        }
        
        public void SetAttackMode(bool enabled)
        {
            CollisionShape.SetDeferred("disabled", !enabled);
        }
    }
}
