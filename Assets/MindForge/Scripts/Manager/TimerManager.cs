using UnityEngine;
using TMPro;
using System;

public class TimerManager : MonoBehaviour, IGameStateListener
{
    [Header("Elements")]
    [SerializeField] private TextMeshProUGUI timerText;
    private int currentTimer;

    private void Awake()
    {
        LevelManager.levelSpawned += OnLevelSpawned;
    }


    private void OnDestroy()
    {
        LevelManager.levelSpawned -= OnLevelSpawned;
    }

    private void OnLevelSpawned(Level level)
    {
        currentTimer = level.Duration;
        UpdateTImerText();

        StartTimer();
    }

    private void StartTimer()
    {
        InvokeRepeating("UpdateTimer", 0, 1); // Gọi hàm UpdateTimer mỗi giây
    }

    private void UpdateTimer()
    {
        currentTimer--;
        UpdateTImerText();

        if (currentTimer <= 0)
            TimerFinished();
    }

    private void UpdateTImerText()
    {
        timerText.text = SecondsToString(currentTimer);
    }

    private void TimerFinished()
    {
        StopTimer(); // Dừng việc gọi hàm UpdateTimer
        GameManager.Instance.SetGameState(EGameState.GAMEOVER); // Thay đổi trạng thái game thành GAMEOVER
    }

    private string SecondsToString(int seconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
        return timeSpan.ToString().Substring(3); // Lấy phần MM:SS
    }

    public void GameStateChangedCallback(EGameState gameState)
    {
        if (gameState == EGameState.LEVELCOMPLETE || gameState == EGameState.GAMEOVER)
        {
            StopTimer();// Dừng việc gọi hàm UpdateTimer khi hoàn thành cấp độ hoặc kết thúc trò chơi
        }
    }

    private void StopTimer()
    {
        CancelInvoke("UpdateTimer");
    }
}


