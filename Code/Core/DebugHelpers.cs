using System.Diagnostics;

namespace Core
{
    public static class DebugHelpers
    {
        [DebuggerHidden, DebuggerStepThrough]
        public static void AssertDebugBreak(bool condition, string message = "")
        {
#if DEBUG
            if (Debugger.IsAttached && !condition)
                Debugger.Break();
            else
                Debug.Assert(condition, message);
#endif
        }
    }
}
