using System.Collections.Generic;
using System.Text.RegularExpressions;
using Godot;

namespace Core
{
    public static class RecursiveDirectoryWalker
    {
        public delegate void FileFoundDelegate(string filePath);

        public static void FindFilesRecursive(string rootDir, string fileRegex, FileFoundDelegate fileFoundDelegate)
        {
            Directory dir = new Directory();

            if (dir.Open(rootDir) != Error.Ok)
            {
                GD.Print("SceneManager: Failed to open res://");
                return;
            }

            Stack<(string, int)> subdirsStack = new Stack<(string, int)>();
            subdirsStack.Push((".", 0));
            int currentDepth = -1;

            while (subdirsStack.Count > 0)
            {
                (string nextDir, int nextDepth) = subdirsStack.Pop(); 

                while (currentDepth >= nextDepth)
                {
                    dir.ChangeDir("..");
                    currentDepth--;
                }

                if (dir.ChangeDir(nextDir) != Error.Ok)
                {
                    GD.PushError("SceneManager: Failed to change dir " + dir.GetCurrentDir() + " to " + nextDir);
                    return;
                }
                currentDepth = nextDepth;
            
                dir.ListDirBegin();

                for (string entry = dir.GetNext(); !string.IsNullOrEmpty(entry); entry = dir.GetNext())
                {
                    if (entry == "." || entry == "..")
                        continue;

                    if (dir.FileExists(entry))
                    {
                        if (Regex.IsMatch(entry, fileRegex))
                        {
                            string resPath = System.IO.Path.Combine(dir.GetCurrentDir(), entry);
                            resPath = resPath.Replace("\\", "/");
                            fileFoundDelegate(resPath);
                        }
                    }
                    else
                    {
                        subdirsStack.Push((entry, currentDepth + 1));
                    }
                }
            
                dir.ListDirEnd();
            }
        }
    }
}
