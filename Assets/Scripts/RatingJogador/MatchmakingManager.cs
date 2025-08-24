using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MatchmakingManager : MonoBehaviour
{
    [Header("Matchmaking Settings")]
    [SerializeField] private float searchTimeMin = 2f;
    [SerializeField] private float searchTimeMax = 8f;
    [SerializeField] private int maxRatingDifference = 150;

    [Header("Game Modes")]
    [SerializeField] private GameMode[] availableGameModes;

    [Header("AI Difficulty")]
    [SerializeField] private AIDifficulty defaultDifficulty = AIDifficulty.Intermediate;

    public static MatchmakingManager Instance { get; private set; }

    // Events
    public static System.Action OnMatchmakingStarted;
    public static System.Action<AIOpponent, GameMode> OnMatchFound;
    public static System.Action OnMatchmakingCancelled;
    public static System.Action<string> OnMatchmakingStatusChanged;

    // State
    private bool isSearching = false;
    private Coroutine searchCoroutine;
    private GameMode selectedGameMode;
    private AIDifficulty selectedDifficulty;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (availableGameModes == null || availableGameModes.Length == 0)
            CreateDefaultGameModes();
    }

    private void CreateDefaultGameModes()
    {
        availableGameModes = new GameMode[]
        {
            new GameMode
            {
                name = "Blitz",
                description = "5 minutos por jogador",
                timeControlMinutes = 5f,
                maxMoveTimeSeconds = 120f,
                incrementSeconds = 0f,
                isRanked = true
            },
            new GameMode
            {
                name = "Rápida",
                description = "10 minutos por jogador",
                timeControlMinutes = 10f,
                maxMoveTimeSeconds = 180f,
                incrementSeconds = 0f,
                isRanked = true
            },
            new GameMode
            {
                name = "Bullet",
                description = "1 minuto por jogador",
                timeControlMinutes = 1f,
                maxMoveTimeSeconds = 30f,
                incrementSeconds = 1f,
                isRanked = true
            },
            new GameMode
            {
                name = "Casual",
                description = "15 minutos, sem rating",
                timeControlMinutes = 15f,
                maxMoveTimeSeconds = 300f,
                incrementSeconds = 0f,
                isRanked = false
            }
        };
    }

    // Compatível com chamadas antigas
    public void StartMatchmaking(GameMode gameMode)
    {
        StartMatchmaking(gameMode, defaultDifficulty);
    }

    // Novo: com dificuldade
    public void StartMatchmaking(GameMode gameMode, AIDifficulty difficulty)
    {
        if (isSearching) return;

        selectedGameMode = gameMode;
        selectedDifficulty = difficulty;
        isSearching = true;

        OnMatchmakingStarted?.Invoke();
        OnMatchmakingStatusChanged?.Invoke("Procurando oponente...");

        searchCoroutine = StartCoroutine(SearchForMatch());

        Debug.Log($"Iniciando busca por partida: {gameMode.name} | Dificuldade: {difficulty}");
    }

    public void CancelMatchmaking()
    {
        if (!isSearching) return;

        if (searchCoroutine != null)
        {
            StopCoroutine(searchCoroutine);
            searchCoroutine = null;
        }

        isSearching = false;
        OnMatchmakingCancelled?.Invoke();
        OnMatchmakingStatusChanged?.Invoke("Busca cancelada");

        Debug.Log("Matchmaking cancelado");
    }

    private IEnumerator SearchForMatch()
    {
        float searchTime = Random.Range(searchTimeMin, searchTimeMax);
        float elapsed = 0f;

        while (elapsed < searchTime)
        {
            elapsed += 0.1f;

            int dots = Mathf.FloorToInt(elapsed * 2) % 4;
            string status = "Procurando oponente" + new string('.', dots);
            OnMatchmakingStatusChanged?.Invoke(status);

            yield return new WaitForSeconds(0.1f);
        }

        // Busca IA base por rating
        int playerRating = PlayerRating.Instance != null ? PlayerRating.Instance.CurrentRating : 1000;
        AIOpponent opponent = null;

        if (AIOpponentDatabase.Instance != null)
        {
            opponent = AIOpponentDatabase.Instance.FindOpponent(playerRating, maxRatingDifference);
        }

        // Fallback absoluto (sem database)
        if (opponent == null)
        {
            opponent = new AIOpponent
            {
                name = "Bot Genérico",
                rating = playerRating,
                description = "Oponente gerado",
                personality = AIPersonality.Balanced,
                thinkingTimeMultiplier = 1f,
                blunderChance = 0.08f
            };
        }

        // Aplica dificuldade sobre uma cópia segura
        opponent = AIOpponentFactory.CreateForDifficulty(opponent, selectedDifficulty);

        isSearching = false;
        OnMatchFound?.Invoke(opponent, selectedGameMode);
        OnMatchmakingStatusChanged?.Invoke($"Partida encontrada vs {opponent.name}!");

        Debug.Log($"Partida encontrada: {opponent.name} (Rating: {opponent.rating}) | Dif.: {selectedDifficulty}");
    }

    public GameMode[] GetAvailableGameModes()
    {
        return availableGameModes;
    }

    public bool IsSearching => isSearching;
}

[System.Serializable]
public class GameMode
{
    public string name;
    public string description;
    public float timeControlMinutes;
    public float maxMoveTimeSeconds;
    public float incrementSeconds;
    public bool isRanked;

    public string GetTimeControlText()
    {
        string timeText = $"{timeControlMinutes}min";
        if (incrementSeconds > 0)
            timeText += $" + {incrementSeconds}s";
        return timeText;
    }
}