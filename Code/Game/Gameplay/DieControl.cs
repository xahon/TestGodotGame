using Core;
using Godot;

namespace Game.Gameplay
{
    public class DieControl : Control
    {
        [Export] private NodePath TextureRectPath { get; set; }
        [Export] private NodePath LabelPath { get; set; }
        
        [Signal] public delegate void OnDieCaptured(DieControl dieControl);
        [Signal] public delegate void OnDieReleased(DieControl dieControl);

        public int Value { get; private set; }
        public GameUi.DieColor DieColor { get; private set; }
        
        private TextureRect TextureRect { get; set;}
        private Label Label { get; set; }
        private TextureRect MovableControl { get; set; }
        private Texture OriginalIcon { get; set; }
        private Texture OverrideIcon { get; set; }
        
        private bool MouseIsOver { get; set; }
        private bool IsCapturedByMouse { get; set; }
        private bool Interactable { get; set; }
        private Vector2 RelativeMousePosition { get; set; }

        public override void _Ready()
        {
            TextureRect = this.GetNodeChecked<TextureRect>(TextureRectPath);
            Label = this.GetNodeChecked<Label>(LabelPath);
            
            SetProcessInput(true);
            SetProcess(true);
            
            Connect("mouse_entered", this, nameof(OnMouseEntered));
            Connect("mouse_exited", this, nameof(OnMouseExited));
            Connect(nameof(OnDieCaptured), this, nameof(_OnDieCaptured));
            Connect(nameof(OnDieReleased), this, nameof(_OnDieReleased));
        }

        private void OnMouseEntered()
        {
            MouseIsOver = true;
        }
        
        private void OnMouseExited()
        {
            MouseIsOver = false;
        }
        
        public override void _Process(float delta)
        {
            if (Input.IsActionJustPressed("DieHold") && MouseIsOver && Interactable)
            {
                IsCapturedByMouse = true;
                EmitSignal(nameof(OnDieCaptured), this);
            }
            else if (Input.IsActionJustReleased("DieHold") && IsCapturedByMouse)
            {
                IsCapturedByMouse = false;
                EmitSignal(nameof(OnDieReleased), this);
            }

            if (IsCapturedByMouse)
            {
                MovableControl.RectGlobalPosition = GetGlobalMousePosition() - RelativeMousePosition;
            }
        }

        private void _OnDieCaptured(DieControl dieControl)
        {
            dieControl.RelativeMousePosition = GetLocalMousePosition();

            // SetIconVisibility(false);
        }

        private void _OnDieReleased(DieControl dieControl)
        {
            dieControl.MovableControl.RectPosition = Vector2.Zero;
            dieControl.RelativeMousePosition = Vector2.Zero;

            // SetIconVisibility(true);
        }

        public void SetIconVisibility(bool visible)
        {
            // hide/unhide its own icon object without changing actual layout
            Color modulationColor = visible ? Colors.White : Colors.Transparent;
            TextureRect.Modulate = modulationColor;
        }

        public void SetState(int value, GameUi.DieColor color)
        {
            Value = value;
            DieColor = color;
        }

        public void SetIcon(Texture icon)
        {
            TextureRect.Texture = icon;
            OriginalIcon = icon;
        }
        
        public void SetOverrideIcon(Texture icon)
        {
            OverrideIcon = icon;
            if (OverrideIcon == null)
                TextureRect.Texture = OriginalIcon;
            else
                TextureRect.Texture = icon;
        }

        public Texture GetIcon()
        {
            return TextureRect.Texture;
        }

        public void SetLabel(string label)
        {
            Label.Text = label;
        }

        public void SetInteractable(bool interactable)
        {
            Interactable = interactable;
        }

        public void SetTargetMovableControl(TextureRect control)
        {
            MovableControl = control;
        }
    }
}
