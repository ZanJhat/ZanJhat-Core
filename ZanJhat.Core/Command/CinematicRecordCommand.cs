using System;
using Engine;
using Game;

namespace ZanJhat.Core
{
    public class CinematicRecordCommand : Command
    {
        public override string Name => "rec";
        public override string Usage => "/rec <bool enable>";

        protected override CmdArgument[] Arguments => new[]
        {
            new CmdArgument("enable", CmdArgType.Bool)
        };

        protected override void ExecuteCore(SubsystemConsole subsystemConsole, ComponentConsole executor, object[] args)
        {
            bool enable = (bool)args[0];

            ComponentPlayer componentPlayer = executor.m_componentPlayer;
            if (componentPlayer == null)
            {
                subsystemConsole.AddMessage(null, MessageType.Error, "System", "Only players can use this command.");
                return;
            }

            try
            {
                if (enable)
                {
                    // Resolution quay (có thể chỉnh)
                    int width = 1280;
                    int height = 720;

                    bool success = CinematicRecorderManager.StartRecording(componentPlayer, width, height);
                    if (success)
                        subsystemConsole.AddMessage(null, MessageType.Info, "System", $"Cinematic recording started ({width}x{height})");
                    else
                        subsystemConsole.AddMessage(null, MessageType.Warning, "System", "Camera is already in use by someone else!");
                }
                else
                {
                    bool success = CinematicRecorderManager.StopRecording(componentPlayer);
                    if (success)
                        subsystemConsole.AddMessage(null, MessageType.Info, "System", "Cinematic recording stopped");
                    else
                        subsystemConsole.AddMessage(null, MessageType.Warning, "System", "You don't have permission to stop this recording or it's not running.");
                }
            }
            catch (Exception ex)
            {
                subsystemConsole.AddMessage(null, MessageType.Error, "System", $"Recorder error: {ex.Message}");
            }
        }
    }
}
