namespace NBody.Gui
{
    public static class SourceOfTruth
    {
        public static string InputFile = "PlanetSystem.json";
        [PropEdit]
        public static Core.PlanetSystem System;
        [PropEdit]
        public static int StepsPerFrame;
        [PropEdit]
        public static bool RestartStepPerFrame = true;
        public static bool IsDebugShown;
        [PropEdit]
        public static bool ShowLights = true;
        public static bool RestartRequested = false;
        public static bool ShowOpenPlanetSystemDialog = false;
        [PropEdit]
        public static int DebugPlanetListScrollValue = 0;
        public static bool Paused = false;
    }
}
