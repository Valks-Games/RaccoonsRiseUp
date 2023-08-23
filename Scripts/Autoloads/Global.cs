global using Godot;
global using GodotUtils;
global using System;
global using System.Collections.Generic;
global using System.Collections.Concurrent;
global using System.Diagnostics;
global using System.Linq;
global using System.Runtime.CompilerServices;
global using System.Threading;
global using System.Text.RegularExpressions;
global using System.Threading.Tasks;

#region Using Definitions
global using JobDict = System.Collections.Generic.Dictionary<RRU.JobType, int>;
global using StructureDict = System.Collections.Generic.Dictionary<Godot.StringName, int>;
global using ResourceDict = System.Collections.Generic.Dictionary<RRU.ResourceType, double>;
#endregion

namespace RRU;

public partial class Global : Node
{
    public static Action QuitAction { get; private set; }
    public static NodePath GetNodePath => "/root/Global";

    public event Action OnQuitRequest;

    public override void _Ready()
    {
        // For 'CommandExit' to work
        QuitAction = Quit;

        // Gradually fade out all SFX whenever the scene is changed
        SceneManager.SceneChanged += name => AudioManager.FadeOutSFX();
    }

    public override void _PhysicsProcess(double delta)
    {
        Logger.Update();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
            Quit();
    }

    public void Quit()
    {
        // Handle cleanup here
        OptionsManager.SaveOptions();
        OptionsManager.SaveHotkeys();

        OnQuitRequest.Invoke();
        GetTree().Quit();
    }
}
