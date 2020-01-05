using System;

namespace NBody.Gui.Run
{
    class Program
    {
        static readonly string GodotExePath = @"C:\Users\Branimir\Source\Tools\Godot_v3.1.2-stable_mono_win64\Godot_v3.1.2-stable_mono_win64\Godot_v3.1.2-stable_mono_win64.exe";
        static readonly string ProjectFile = @"C:\Users\Branimir\Source\repos\nbody\Nbody.Godot\";
        static void Main(string[] args)
        {
            var proc = System.Diagnostics.Process.Start(GodotExePath, $"--path {ProjectFile}");
            proc.WaitForExit();
        }
    }
}
