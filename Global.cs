using Godot;
using System;

public partial class Global : Node
{
    // https://www.jetbrains.com/guide/gamedev/tutorials/singletons-autoloads-godot-csharp/
    // https://docs.godotengine.org/en/latest/tutorials/scripting/singletons_autoload.html
    /*
     * The Enable column (Project Settings -> Autoload -> Add script to list) has no effect in C# code. However, if the singleton is a C# script, a similar effect can be achieved by including a static property called Instance and assigning it in _Ready():
    */

    public static Global Instance { get; private set; } // needs to be called "Instance" so Godot can look for it, & needs to be set in the _Ready() method.
    private readonly object @lock = new();
    public int NoOfMines { get; private set; } = 0;

    //[Signal]
    //public delegate void ScoreChangedEventHandler(int score);

    public override void _Ready()
    {
        Instance = this;
    }

    public int SetNoOfMinesForNextGame(int noOfMines)
    {
        lock (@lock)
        {
            NoOfMines = noOfMines;
            //EmitSignal(SignalName.ScoreChanged, Score);
            return NoOfMines;
        }
    }
}
