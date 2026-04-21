using System;
using UnityEngine;

public class LevelManager : MonoBehaviour, IGameStateListener
{
    public static LevelManager Instance { get; private set; }

    [Header("Data")]
    [SerializeField] private Level[] levels;
    private const string levelKey = "LevelReached"; // Key để lưu trữ level đã đạt được trong PlayerPrefs
    private int levelIndex;
    public Item[] Items => currentLevel.GetItems();

    [Header("Settings")]
    private Level currentLevel;


    [Header("Action")]
    public static Action<Level> levelSpawned;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        LoadData();
    }

    private void SpawnLevel()
    {
        transform.Clear();

        // Sử dụng toán tử modulo để đảm bảo levelIndex luôn nằm trong phạm vi của mảng levels
        int validatedLevelIndex = levelIndex % levels.Length;

        // Instantiate level mới dựa trên validatedLevelIndex
        currentLevel = Instantiate(levels[validatedLevelIndex], transform);

        // Gọi sự kiện levelSpawned để thông báo rằng một level mới đã được spawn
        levelSpawned?.Invoke(currentLevel);
    }

    private void LoadData()
    {
        levelIndex = PlayerPrefs.GetInt(levelKey);

    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(levelKey, levelIndex);
    }

    // Hàm xử lý khi người chơi hoàn thành một level
    public void GameStateChangedCallback(EGameState gameState)
    {
        if (gameState == EGameState.GAME)
            SpawnLevel();
        else if (gameState == EGameState.LEVELCOMPLETE)
        {
            levelIndex++; // Tăng levelIndex khi hoàn thành level
            SaveData();
        }
    }
}
