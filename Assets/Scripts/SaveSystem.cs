using UnityEngine;
using System.IO;

public static class SaveSystem
{
    private static string _savePath = Path.Combine(Application.persistentDataPath, "player_progress.json");

    [System.Serializable]
    public class PlayerData
    {
        public int strengthLevel;
        public float strengthExp;
        public int techniqueLevel;
        public float techniqueExp;
        public int currency;
    }

    public static void AutoSave()
    {
        PlayerData data = new PlayerData
        {
            strengthLevel = PlayerStats.StrengthLevel,
            strengthExp = PlayerStats.StrengthExp,
            techniqueLevel = PlayerStats.TechniqueLevel,
            techniqueExp = PlayerStats.TechniqueExp,
            currency = CurrencyManager.Instance != null ? CurrencyManager.Instance.currentCurrency : 0
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_savePath, json);
        Debug.Log($"<color=yellow>Игра сохранена в: {_savePath}</color>");
    }

    public static void LoadGame()
    {
        if (File.Exists(_savePath))
        {
            string json = File.ReadAllText(_savePath);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);

            PlayerStats.StrengthLevel = data.strengthLevel;
            PlayerStats.StrengthExp = data.strengthExp;
            PlayerStats.TechniqueLevel = data.techniqueLevel;
            PlayerStats.TechniqueExp = data.techniqueExp;

            if (CurrencyManager.Instance != null)
                CurrencyManager.Instance.currentCurrency = data.currency;

            Debug.Log("<color=green>Прогресс успешно загружен!</color>");
        }
        else
        {
            Debug.Log("Файл сохранений не найден. Начата новая игра.");
        }
    }
}