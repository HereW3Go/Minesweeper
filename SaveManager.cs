using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

public class Save
{
    public int SaveNum{ get; set; } // basically the ID
    public DateTime SaveDateTime { get; set; }
    public enum GameDifficulty
    {
        Easy = 10,
        Medium = 15,
        Hard = 20,
        VeryHard = 40,
        Unknown = 0
    }
    public GameDifficulty Difficulty { get; set; }
    public TimeSpan TimeTaken { get; set; }
    public int NoOfClicks { get; set; }
    public bool IsWin { get; set; }


    public override string ToString()
    {
        return
            $"Game #{SaveNum} | DateTime: {SaveDateTime:dd/MMMM/yyyy, HH:mm:ss} | Difficulty: {Difficulty} | Game Time: {TimeTaken:m\\:ss\\.fff} | No. Of Clicks: {NoOfClicks} | {(IsWin ? "Game Won" : "Game Lost")}";
    }
}

public static class SaveManager
{
    private static string savesFilePath = ProjectSettings.GlobalizePath("user://minesweeperSaves.json");
    

    public static void AddSave(Save gameToAdd)
    {
        List<Save> saves = GetSaves();
        gameToAdd.SaveNum = saves.Count + 1;
        saves.Add(gameToAdd);

        string jsonEquivalent = JsonSerializer.Serialize(saves);

        File.WriteAllText(savesFilePath, jsonEquivalent);

        Debug.Print($"Save stored to {savesFilePath}.");
    }

    private static List<Save> GetSaves()
    {
        List<Save> saves = new List<Save>();
        if(File.Exists(savesFilePath))
        {
            string savePlainText = File.ReadAllText(savesFilePath);
            if(!String.IsNullOrWhiteSpace(savePlainText))
            {
                saves = JsonSerializer.Deserialize<List<Save>>(savePlainText);
            }
        }
        return saves;
    }

    public static List<Save> GetSavesFiltered(Func<Save, bool> filter) // thanks to chatgpt, cause i didnt wanna write out so many methods
    {
        return GetSaves().FindAll(save => filter(save));
    }

    public static List<Save> GetSavesFiltered(List<Save> listOfSaves, Func<Save, bool> filter) // thanks to chatgpt, cause i didnt wanna write out so many methods
    {
        return listOfSaves.FindAll(save => filter(save));
    }
}
