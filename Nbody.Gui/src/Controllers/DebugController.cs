using Godot;
using Nbody.Gui.src.Attributes;
using NBody.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nbody.Gui.src.Controllers
{
    [ButtonCommand(Name ="")]
    public class DebugController
    {
        public void Restart(Node node)
        {
            SourceOfTruth.RestartRequested = true;
        }
        public void Pause(Node node)
        {
            SourceOfTruth.Paused = true;
            (node as Button).Text = node.Name = "Resume";
        }
        public void Resume(Node node)
        {
            SourceOfTruth.Paused = false;
            (node as Button).Text = node.Name = "Pause";
        }
        public void Step(Node node)
        {
            SourceOfTruth.System.Step();
        }
        public void NSteps(Node node)
        {
            SourceOfTruth.System.Step(SourceOfTruth.StepsPerFrame);
        }
        [ButtonCommand(Name ="Open")]
        public void OpenFileDialog(Node node)
        {
            SourceOfTruth.ShowOpenPlanetSystemDialog = true;
        }
    }
}
