using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum GameState
{
    Preparing,
    Playing,
    Paused,
    Victory,
    Defeat
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ResourceManager resourceManager;

    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.Preparing;

    // Events
    public event Action<GameState> OnGameStateChanged;

    public GridManager Grid => gridManager;
    public ResourceManager Resources => resourceManager;
    public GameState CurrentState => currentState;

    private void Awake()
    {
        // Singleton 패턴
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 참조 자동 찾기
        if (gridManager == null)
            gridManager = FindAnyObjectByType<GridManager>();
        if (resourceManager == null)
            resourceManager = FindAnyObjectByType<ResourceManager>();
    }

    private void Start()
    {
        Debug.Log("GameManager initialized");

        // 자동으로 게임 시작 (테스트용)
        StartGame();
    }

    private void Update()
    {
        // ESC 키로 일시정지 토글
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (currentState == GameState.Playing)
                PauseGame();
            else if (currentState == GameState.Paused)
                ResumeGame();
        }

        // R 키로 리셋 (테스트용)
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }

    public void StartGame()
    {
        SetState(GameState.Playing);
        Debug.Log("Game Started!");
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;

        Time.timeScale = 0f;
        SetState(GameState.Paused);
        Debug.Log("Game Paused");
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;

        Time.timeScale = 1f;
        SetState(GameState.Playing);
        Debug.Log("Game Resumed");
    }

    public void Victory()
    {
        Time.timeScale = 0f;
        SetState(GameState.Victory);
        Debug.Log("Victory!");
    }

    public void Defeat()
    {
        Time.timeScale = 0f;
        SetState(GameState.Defeat);
        Debug.Log("Defeat!");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        resourceManager?.ResetResources();
        SetState(GameState.Preparing);
        Debug.Log("Game Restarted");

        // 씬을 리로드하거나 그리드 리셋
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    private void SetState(GameState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        OnGameStateChanged?.Invoke(currentState);
    }
}
