using Core;
using Game.Gameplay;
using Godot;

namespace Game.Scenes
{
    public class GameScene : Node
    {
        [Export] private NodePath GameUiPath { get; set; }
        [Export] private NodePath PlaygroundGridPath { get; set; }
        [Export] private NodePath ActiveDieTextureRectPath { get; set; }

        private GameUi GameUi { get; set; }
        private PlaygroundGrid PlaygroundGrid { get; set; }
        private TextureRect ActiveDieTextureRect { get; set; }

        public override void _Ready()
        {
            GameUi = this.GetNodeChecked<GameUi>(GameUiPath);
            PlaygroundGrid = this.GetNodeChecked<PlaygroundGrid>(PlaygroundGridPath);
            ActiveDieTextureRect = this.GetNodeChecked<TextureRect>(ActiveDieTextureRectPath);

            GameUi.Connect(nameof(GameUi.OnDieCaptured), this, nameof(_OnDieCaptured));
            GameUi.Connect(nameof(GameUi.OnDieReleased), this, nameof(_OnDieReleased));

            ActiveDieTextureRect.Visible = false;
            GameUi.RandomizeDice();
            PlaygroundGrid.RandomizeLevel();
        }

        private void _OnDieCaptured(DieControl dieControl)
        {
            ActiveDieTextureRect.Visible = true;
            ActiveDieTextureRect.Texture = dieControl.GetIcon();
            dieControl.SetTargetMovableControl(ActiveDieTextureRect);
        }
        
        private void _OnDieReleased(DieControl dieControl)
        {
            ActiveDieTextureRect.Visible = false;
        }
    }
}
