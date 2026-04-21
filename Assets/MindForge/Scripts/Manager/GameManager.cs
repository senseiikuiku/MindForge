using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private EGameState gameState;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        SetGameState(EGameState.MENU);
    }

    public void SetGameState(EGameState gameState)
    {
        this.gameState = gameState;

        // Thông báo cho tất cả các listener về sự thay đổi trạng thái trò chơi
        IEnumerable<IGameStateListener> gameStateListeners
            = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IGameStateListener>();

        foreach (IGameStateListener dependency in gameStateListeners)
        {
            dependency.GameStateChangedCallback(gameState);
        }
    }

    public void StartGame()
    {
        SetGameState(EGameState.GAME);
    }

    public void NextButtonCallback()
    {
        SceneManager.LoadScene(0);
    }

    public void RetryButtonCallback()
    {
        SceneManager.LoadScene(0);
    }

    public bool IsGame() => gameState == EGameState.GAME;

}
