using Godot;
using System;
using System.Diagnostics;

public partial class MainMenu : Control
{
	private PopupMenu PUMSelectDifficulty;
	private Button BtnStartGame;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		PUMSelectDifficulty = GetNode<PopupMenu>("PUMSelectDifficulty");
        BtnStartGame = GetNode<Button>("BtnStartGame");

		Debug.Print($"BtnStartGame Has Theme Override for \"normal\"?: {BtnStartGame.HasThemeColorOverride("normal")}\n");

		Debug.Print($"BtnStartGame has a StyleBox for \"normal\"?: {BtnStartGame.HasThemeStylebox("normal")}\n");

        BtnStartGame.Set("theme_override_styles/normal/bg_color", new StyleBoxFlat()
		{
			BgColor = new Color(0.5f, 0.5f, 0.5f)
		}); // YES FINALLY

        BtnStartGame.Set("theme_override_colors/font_color", new Color(0, 0, 0));

    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _OnBtnStartGamePressed()
	{
		PUMSelectDifficulty.Popup(); // does this differ to .Show(); ?; chatgpt answered yes, as Show simply makes it visible, popup makes it visible with intended features available to work.

		CentrePUM();

		// start ChatGPT aided code
        PUMSelectDifficulty.Clear();

        PUMSelectDifficulty.AddItem("Easy (10 Mines)", 0);
        PUMSelectDifficulty.AddItem("Medium (15 Mines)", 1);
        PUMSelectDifficulty.AddItem("Hard (20 Mines)", 2);
        PUMSelectDifficulty.AddItem("Very Hard (40 Mines)", 3);
        // end ChatGPT aided code

        CentrePUM();
    }

    private void CentrePUM()
	{
        Rect2 currentWindowRect = this.GetRect();
        Rect2 PUMSDSize = PUMSelectDifficulty.GetVisibleRect();
        int PUMSDPositionX = (int)((currentWindowRect.Size.X / 2) - PUMSDSize.Size.X / 2), PUMSDPositionY = (int)((currentWindowRect.Size.Y / 2) - PUMSDSize.Size.Y / 2);

        Debug.Print($"PUMSDPositionX: {PUMSDPositionX}, PUMSDPositionY: {PUMSDPositionY}");

        PUMSelectDifficulty.Position = new Vector2I(PUMSDPositionX, PUMSDPositionY);
    }

    public void _OnPUMSelectDifficultyIDPressed(int id)
	{
		Debug.Print($"PUMSelectDifficultyID: {id}");

        switch (id)
		{
			case 0:	Global.Instance.SetNoOfMinesForNextGame(10); break;
			case 1:	Global.Instance.SetNoOfMinesForNextGame(15); break;
			case 2:	Global.Instance.SetNoOfMinesForNextGame(20); break;
			case 3:	Global.Instance.SetNoOfMinesForNextGame(40); break;
			default: Debug.Fail($"Unhandled ID in '_OnPUMSelectDifficultyIDPressed({id})'."); return;
		}

		GetTree().ChangeSceneToFile("GameScreen.tscn");
    }

	public void _OnBtnStatisticsPressed()
	{
        GetTree().ChangeSceneToFile("StatisticsScreen.tscn");

    }
}
