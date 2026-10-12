using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class StatisticsScreen : Control
{
	private RichTextLabel LblStats;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
        LblStats = GetNode<RichTextLabel>("LblStats");
        LblStats.Text = TextLblStats();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _OnBtnReturnToMainMenuPressed()
	{
		GetTree().ChangeSceneToFile("MainMenu.tscn");
	}


    private string TextLblStats() {
		string builtLblText = "";


		IEnumerable<Save> saves = SaveManager.GetSavesFiltered(save => { return save.IsWin; })
				.OrderBy(save => { return save.SaveNum; });
        builtLblText += $"Most Recent Win: \t\t\t\t\t\t\t\t\t\t\t\t\t {(!saves.Any() ? "No such game exists." : saves.Last())}";

        
        saves = SaveManager.GetSavesFiltered(save => { return !save.IsWin; })
				.OrderBy(save => { return save.SaveNum; });
		builtLblText += $"\nMost Recent Loss: \t\t\t\t\t\t\t\t\t\t\t\t\t {(!saves.Any() ? "No such game exists." : saves.Last())}";


        saves = SaveManager.GetSavesFiltered((save) => { return save.IsWin; })
				.OrderBy((save) => { return save.TimeTaken; });
		builtLblText += $"\nQuickest Win: \t\t\t\t\t\t\t\t\t\t\t\t\t\t\t {(!saves.Any() ? "No such game exists." : saves.First())}";


        saves = SaveManager.GetSavesFiltered((save) => { return true; })
                .OrderBy((save) => { return save.SaveNum; });
        builtLblText += $"\nFirst Game Played: \t\t\t\t\t\t\t\t\t\t\t\t\t {(!saves.Any() ? "No such game exists." : saves.First())}";

        saves = SaveManager.GetSavesFiltered((save) => { return save.IsWin; })
                .OrderBy((save) => { return save.SaveNum; });
        builtLblText += $"\nFirst Game Won: \t\t\t\t\t\t\t\t\t\t\t\t\t\t {(!saves.Any() ? "No such game exists." : saves.First())}";

        builtLblText += "\n"; // --------------------------------------------------------------------------------------------------------------------------------------------------
        builtLblText += "\n"; // --------------------------------------------------------------------------------------------------------------------------------------------------

        saves = SaveManager.GetSavesFiltered((save) => { return save.Difficulty == Save.GameDifficulty.Easy; });
        builtLblText += $"\nTotal No. of Easy Games Played: \t\t\t\t\t\t\t {(!saves.Any() ? "No games on Easy difficulty exist." : saves.Count())}";

        saves = SaveManager.GetSavesFiltered((save) => { return save.IsWin && save.Difficulty == Save.GameDifficulty.Easy; });
        builtLblText += $"\nTotal No. of Easy Games Won: \t\t\t\t\t\t\t {(!saves.Any() ? "No wins on Easy difficulty exist." : saves.Count())}";

        saves = SaveManager.GetSavesFiltered((save) => { return save.IsWin && save.Difficulty == Save.GameDifficulty.Easy; })
				.OrderBy((save) => { return save.NoOfClicks; });
        builtLblText += $"\nWin with Least Clicks on Easy Difficulty: \t\t\t {(!saves.Any() ? "No such game exists." : saves.First())}";


        saves = SaveManager.GetSavesFiltered((save) => { return save.IsWin && save.Difficulty == Save.GameDifficulty.Easy; })
                .OrderBy((save) => { return save.TimeTaken; });
        builtLblText += $"\nFastest Win on Easy Difficulty: \t\t\t\t\t\t\t {(!saves.Any() ? "No such game exists." : saves.First())}";

        builtLblText += "\n"; // --------------------------------------------------------------------------------------------------------------------------------------------------
        builtLblText += "\n"; // --------------------------------------------------------------------------------------------------------------------------------------------------

        saves = SaveManager.GetSavesFiltered((save) => { return save.Difficulty == Save.GameDifficulty.Medium; });
        builtLblText += $"\nTotal No. of Medium Games Played: \t\t\t\t\t {(!saves.Any() ? "No games on Medium difficulty exist." : saves.Count())}";

        saves = SaveManager.GetSavesFiltered((save) => { return save.IsWin && save.Difficulty == Save.GameDifficulty.Medium; });
        builtLblText += $"\nTotal No. of Medium Games Won: \t\t\t\t\t\t {(!saves.Any() ? "No wins on Medium difficulty exist." : saves.Count())}";

        saves = SaveManager.GetSavesFiltered((save) => { return save.IsWin && save.Difficulty == Save.GameDifficulty.Medium; })
                .OrderBy((save) => { return save.NoOfClicks; });
        builtLblText += $"\nWin with Least Clicks on Medium Difficulty: \t {(!saves.Any() ? "No such game exists." : saves.Last())}";


        saves = SaveManager.GetSavesFiltered((save) => { return save.IsWin && save.Difficulty == Save.GameDifficulty.Medium; })
                .OrderBy((save) => { return save.TimeTaken; });
        builtLblText += $"\nFastest Win on Medium Difficulty: \t\t\t\t\t\t {(!saves.Any() ? "No such game exists." : saves.Last())}";

        builtLblText += "\n"; // --------------------------------------------------------------------------------------------------------------------------------------------------
        builtLblText += "\n"; // --------------------------------------------------------------------------------------------------------------------------------------------------

        saves = SaveManager.GetSavesFiltered((save) => { return save.Difficulty == Save.GameDifficulty.Hard; });
        builtLblText += $"\nTotal No. of Hard Games Played: \t\t\t\t\t\t {(!saves.Any() ? "No games on Hard difficulty exist." : saves.Count())}";

        saves = SaveManager.GetSavesFiltered((save) => { return save.IsWin && save.Difficulty == Save.GameDifficulty.Hard; });
        builtLblText += $"\nTotal No. of Hard Games Won: \t\t\t\t\t\t\t {(!saves.Any() ? "No wins on Hard difficulty exist." : saves.Count())}";

        saves = SaveManager.GetSavesFiltered((save) => { return save.IsWin && save.Difficulty == Save.GameDifficulty.Hard; })
				.OrderBy((save) => { return save.NoOfClicks; });
        builtLblText += $"\nWin with Least Clicks on Hard Difficulty: \t\t\t {(!saves.Any() ? "No such game exists." : saves.First())}";


        saves = SaveManager.GetSavesFiltered((save) => { return save.IsWin && save.Difficulty == Save.GameDifficulty.Hard; })
                .OrderBy((save) => { return save.TimeTaken; });
        builtLblText += $"\nFastest Win on Hard Difficulty: \t\t\t\t\t\t\t {(!saves.Any() ? "No such game exists." : saves.First())}";

        builtLblText += "\n"; // --------------------------------------------------------------------------------------------------------------------------------------------------
        builtLblText += "\n"; // --------------------------------------------------------------------------------------------------------------------------------------------------

        saves = SaveManager.GetSavesFiltered((save) => { return save.Difficulty == Save.GameDifficulty.VeryHard; });
        builtLblText += $"\nTotal No. of Very Hard Games Played: \t\t\t\t {(!saves.Any() ? "No games on Very Hard difficulty exist." : saves.Count())}";

        saves = SaveManager.GetSavesFiltered((save) => { return save.IsWin && save.Difficulty == Save.GameDifficulty.VeryHard; });
        builtLblText += $"\nTotal No. of Very Hard Games Won: \t\t\t\t\t {(!saves.Any() ? "No wins on Very Hard difficulty exist." : saves.Count())}";

        saves = SaveManager.GetSavesFiltered((save) => { return save.IsWin && save.Difficulty == Save.GameDifficulty.VeryHard; })
				.OrderBy((save) => { return save.NoOfClicks; });
        builtLblText += $"\nWin with Least Clicks on Very Hard Difficulty:  {(!saves.Any() ? "No such game exists." : saves.First())}";


        saves = SaveManager.GetSavesFiltered((save) => { return save.IsWin && save.Difficulty == Save.GameDifficulty.VeryHard; })
                .OrderBy((save) => { return save.TimeTaken; });
        builtLblText += $"\nFastest Win on Very Hard Difficulty: \t\t\t\t\t {(!saves.Any() ? "No such game exists." : saves.First())}";



        return builtLblText;
    }
}
