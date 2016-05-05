// C# example
using UnityEditor;

class PC_Builder
{
  static void PerformBuild()
  {
    string[] scenes = { "Assets/main.unity" };
    BuildPipeline.BuildPlayer(scenes, "build/bioblox.exe", BuildTarget.StandaloneWindows64, BuildOptions.None);
  }
}
