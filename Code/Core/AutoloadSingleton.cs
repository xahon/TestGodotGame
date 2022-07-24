using Godot;

namespace Core
{
    public class AutoloadSingleton<T> : Node where T : Object
    {
        public static T Instance { get; private set; }

        public override void _Ready()
        {
            foreach (Object node in GetTree().Root.GetChildren())
                if (node is T nodeT)
                {
                    if (Instance != null)
                        GD.PushError("AutoloadSingleton: Multiple instances of " + typeof(T).Name + " found.");
                    Instance = nodeT;
                }

            if (Instance == null)
                GD.PushError("AutoloadSingleton: No instance of type " + typeof(T).Name + " found in scene.");
            else
                GD.Print("AutoloadSingleton: Instance of type " + typeof(T).Name + " is registered.");
        }
    }
}
