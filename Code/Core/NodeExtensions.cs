using Godot;

namespace Core
{
    public static class NodeExtensions
    {
        public static T GetNodeChecked<T>(this Node node, NodePath nodePath) where T: Node
        {
            DebugHelpers.AssertDebugBreak(nodePath != null && !string.IsNullOrEmpty(nodePath));
            T child = node.GetNode<T>(nodePath);
            System.Diagnostics.Debug.Assert(child != null, "child != null");
            return child;
        }

        public static T GetParentChecked<T>(this Node node) where T : Node
        {
            T parent = node.GetParent() as T;
            System.Diagnostics.Debug.Assert(parent != null, "parent != null");
            return parent;
        }
    }
}
