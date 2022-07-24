using Core;
using Godot;
using Godot.Collections;

namespace Game.Managers
{
    public class SceneManager : AutoloadSingleton<SceneManager>
    {
        private readonly Dictionary<string, PackedScene> _cachedScenes = new Godot.Collections.Dictionary<string, PackedScene>();
        private readonly Array<Node> _pushedScenes = new Array<Node>();

        public override void _Ready()
        {
            base._Ready();

            RecursiveDirectoryWalker.FindFilesRecursive("res://Scenes", ".*\\.tscn", filePath =>
            {
                PackedScene scene = GD.Load<PackedScene>(filePath);

                if (_cachedScenes.ContainsKey(filePath))
                {
                    GD.PushError($"SceneManager: Scene with name \"{filePath}\" is already loaded");
                    return;
                }
                System.IO.FileInfo fileInfo = new System.IO.FileInfo(filePath);
                string fileName = fileInfo.Name;
                string fileNameWithoutExtension = fileName.Substring(0, fileName.Length - fileInfo.Extension.Length);
            
                _cachedScenes.Add(fileNameWithoutExtension, scene);
            });
        }

        public void ChangeSceneTo(string name, bool keepPushedScenes = false)
        {
            if (!_cachedScenes.TryGetValue(name, out PackedScene packedScene))
            {
                GD.PushError($"SceneManager: Scene with name \"{name}\" is not registered");
                return;
            }

            if (!keepPushedScenes)
                while (PopScene()) { }
        
            GetTree().ChangeSceneTo(packedScene);
        }

        public void PushScene(string name)
        {
            if (!_cachedScenes.TryGetValue(name, out PackedScene packedScene))
            {
                GD.PushError($"SceneManager: Scene with name \"{name}\" is not registered");
                return;
            }

            Node sceneInstance = packedScene.Instance();
            _pushedScenes.Add(sceneInstance);
        
            GetTree().Root.CallDeferred("add_child", sceneInstance);
        }
    
        public bool PopScene()
        {
            if (_pushedScenes.Count == 0)
                return false;

            Node sceneInstance = _pushedScenes[_pushedScenes.Count - 1];
            sceneInstance.QueueFree();

            _pushedScenes.RemoveAt(_pushedScenes.Count - 1);
            return _pushedScenes.Count > 0;
        }
    }
}
