using Core;
using Godot;
using Godot.Collections;

namespace Game.Gameplay
{
    public class GameUi : Control
    {
        [Export] private NodePath[] DieControlPaths { get; set; }
        [Export] private NodePath RerollButtonPath { get; set; }

        [Export] private Texture IconRoll { get; set; }
        [Export] private Texture IconNone { get; set; }
        [Export] private Texture IconRed_1 { get; set; }
        [Export] private Texture IconRed_2 { get; set; }
        [Export] private Texture IconRed_3 { get; set; }
        [Export] private Texture IconRed_4 { get; set; }
        [Export] private Texture IconRed_5 { get; set; }
        [Export] private Texture IconRed_6 { get; set; }
        [Export] private Texture IconBlue_1 { get; set; }
        [Export] private Texture IconBlue_2 { get; set; }
        [Export] private Texture IconBlue_3 { get; set; }
        [Export] private Texture IconBlue_4 { get; set; }
        [Export] private Texture IconBlue_5 { get; set; }
        [Export] private Texture IconBlue_6 { get; set; }
        [Export] private Texture IconYellow_1 { get; set; }
        [Export] private Texture IconYellow_2 { get; set; }
        [Export] private Texture IconYellow_3 { get; set; }
        [Export] private Texture IconYellow_4 { get; set; }
        [Export] private Texture IconYellow_5 { get; set; }
        [Export] private Texture IconYellow_6 { get; set; }

        [Signal] public delegate void OnDieCaptured(DieControl dieControl);
        [Signal] public delegate void OnDieReleased(DieControl dieControl);
        [Signal] public delegate void OnReroll();

        private Array<DieControl> DieControls { get; set; } = new Array<DieControl>();
        private Button RerollButton { get; set; }

        public enum DieColor
        {
            Red = 0,
            Blue,
            Yellow
        }
        
        private readonly Dictionary<DieColor, Dictionary<int, Texture>> DieTexturesMapping = new Dictionary<DieColor, Dictionary<int, Texture>>();
        private readonly Dictionary<DieColor, string> DieLabelsMapping = new Dictionary<DieColor, string>();

        public override void _Ready()
        {
            RerollButton = this.GetNodeChecked<Button>(RerollButtonPath);
            foreach (NodePath dieControlPath in DieControlPaths)
            {
                DieControl dieControl = this.GetNodeChecked<DieControl>(dieControlPath);
                DieControls.Add(dieControl);
            }

            RerollButton.Connect("pressed", this, nameof(_OnRerollButtonPressed));
            foreach (DieControl dieControl in DieControls)
            {
                dieControl.Connect(nameof(DieControl.OnDieCaptured), this, nameof(_OnDieCaptured));
                dieControl.Connect(nameof(DieControl.OnDieReleased), this, nameof(_OnDieReleased));
            }

            DieLabelsMapping.Add(DieColor.Red, "Attack");
            DieLabelsMapping.Add(DieColor.Blue, "Teleport");
            DieLabelsMapping.Add(DieColor.Yellow, "Move");

            DieTexturesMapping.Add(DieColor.Red, new Dictionary<int, Texture>());
            DieTexturesMapping.Add(DieColor.Blue, new Dictionary<int, Texture>());
            DieTexturesMapping.Add(DieColor.Yellow, new Dictionary<int, Texture>());

            foreach (Dictionary<int, Texture> innerDict in DieTexturesMapping.Values)
                for (int i = 1; i <= 6; i++)
                    innerDict.Add(i, null);

            DieTexturesMapping[DieColor.Red][1] = IconRed_1;
            DieTexturesMapping[DieColor.Red][2] = IconRed_2;
            DieTexturesMapping[DieColor.Red][3] = IconRed_3;
            DieTexturesMapping[DieColor.Red][4] = IconRed_4;
            DieTexturesMapping[DieColor.Red][5] = IconRed_5;
            DieTexturesMapping[DieColor.Red][6] = IconRed_6;
            DieTexturesMapping[DieColor.Blue][1] = IconBlue_1;
            DieTexturesMapping[DieColor.Blue][2] = IconBlue_2;
            DieTexturesMapping[DieColor.Blue][3] = IconBlue_3;
            DieTexturesMapping[DieColor.Blue][4] = IconBlue_4;
            DieTexturesMapping[DieColor.Blue][5] = IconBlue_5;
            DieTexturesMapping[DieColor.Blue][6] = IconBlue_6;
            DieTexturesMapping[DieColor.Yellow][1] = IconYellow_1;
            DieTexturesMapping[DieColor.Yellow][2] = IconYellow_2;
            DieTexturesMapping[DieColor.Yellow][3] = IconYellow_3;
            DieTexturesMapping[DieColor.Yellow][4] = IconYellow_4;
            DieTexturesMapping[DieColor.Yellow][5] = IconYellow_5;
            DieTexturesMapping[DieColor.Yellow][6] = IconYellow_6;
        }

        public void RandomizeDice()
        {
            foreach (DieControl dieControl in DieControls)
            {
                DieColor color = (DieColor)(GD.Randi() % 3);
                int value = (int)(GD.Randi() % 6 + 1);
                Texture dieTexture = DieTexturesMapping[color][value];

                dieControl.SetState(value, color);
                dieControl.SetIcon(dieTexture);
                dieControl.SetOverrideIcon(null);
                dieControl.SetLabel(DieLabelsMapping[color]);
                dieControl.SetInteractable(true);
            }
        }

        public void ConsumeDie(DieControl dieControl)
        {
            dieControl.SetIcon(IconNone);
            dieControl.SetLabel("...");
            dieControl.SetInteractable(false);
        }

        public void SetInteractable(bool interactable)
        {
            foreach (DieControl dieControl in DieControls)
                dieControl.SetInteractable(interactable);
            RerollButton.Disabled = !interactable;
        }
        
        private void _OnDieCaptured(DieControl dieControl)
        {
            EmitSignal(nameof(OnDieCaptured), dieControl);
            dieControl.SetOverrideIcon(IconNone);
        }
        
        private void _OnDieReleased(DieControl dieControl)
        {
            EmitSignal(nameof(OnDieReleased), dieControl);
            dieControl.SetOverrideIcon(null);
        }

        private async void _OnRerollButtonPressed()
        {
            foreach (DieControl dieControl in DieControls)
            {
                dieControl.SetIcon(IconRoll);
                dieControl.SetLabel("...");
                dieControl.SetInteractable(false);
            }

            Timer sleepTimer = new Timer();
            sleepTimer.OneShot = true;
            AddChild(sleepTimer);
            sleepTimer.Start(1);

            await ToSignal(sleepTimer, "timeout");

            EmitSignal(nameof(OnReroll));
            RandomizeDice();
        }
    }
}
