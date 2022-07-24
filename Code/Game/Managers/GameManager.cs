using Core;
using Godot;

namespace Game.Managers
{
    public class GameManager : AutoloadSingleton<GameManager>
    {
        public override void _Ready()
        {
            GD.Randomize();
            SceneManager.Instance.ChangeSceneTo("GameScene");
        }
    }
}
