using UnityEngine;

public static class PlayerStats
{
    public static int StrengthLevel = 1;
    public static float StrengthExp = 0;
    public static int TechniqueLevel = 1;
    public static float TechniqueExp = 0;

    private static PlayerMovement _player;

    public static void Init(PlayerMovement player)
    {
        _player = player;
        SaveSystem.LoadGame();
    }

    public static void AddExp(string statName, float amount)
    {
        if (statName == "strength")
        {
            StrengthExp += amount;
            if (StrengthExp >= 100) { StrengthLevel++; StrengthExp = 0; }
        }
        else if (statName == "technique")
        {
            TechniqueExp += amount;
            if (TechniqueExp >= 100) { TechniqueLevel++; TechniqueExp = 0; }
        }
        
        SaveSystem.AutoSave();
    }

    public static void ConsumeStamina(float amount)
    {
        if (_player != null)
        {
            _player.Stamina -= amount;
            if (_player.Stamina < 0) _player.Stamina = 0;
            _player.RestoreStamina(0); 
        }
    }
}