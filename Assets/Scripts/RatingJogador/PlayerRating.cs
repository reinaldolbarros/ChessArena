using UnityEngine;

[System.Serializable]
public class PlayerRating : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private string playerName = "reinaldolbarros";
    [SerializeField] private int currentRating = 1200; // Rating inicial
    [SerializeField] private int gamesPlayed = 0;
    [SerializeField] private int wins = 0;
    [SerializeField] private int losses = 0;
    [SerializeField] private int draws = 0;

    [Header("Rating Settings")]
    [SerializeField] private int kFactor = 32; // Fator K do sistema ELO
    [SerializeField] private int minRating = 100;
    [SerializeField] private int maxRating = 3000;

    public static PlayerRating Instance { get; private set; }

    // Events
    public static System.Action<int> OnRatingChanged;
    public static System.Action<int, int, int> OnStatsChanged; // wins, losses, draws

    // Properties
    public string PlayerName => playerName;
    public int CurrentRating => currentRating;
    public int GamesPlayed => gamesPlayed;
    public int Wins => wins;
    public int Losses => losses;
    public int Draws => draws;
    public float WinRate => gamesPlayed > 0 ? (float)wins / gamesPlayed * 100f : 0f;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadPlayerData();
    }

    public void UpdateRating(GameResult result, int opponentRating)
    {
        gamesPlayed++;

        // Calcula novo rating usando sistema ELO
        float expectedScore = CalculateExpectedScore(currentRating, opponentRating);
        float actualScore = GetActualScore(result);

        int ratingChange = Mathf.RoundToInt(kFactor * (actualScore - expectedScore));
        currentRating = Mathf.Clamp(currentRating + ratingChange, minRating, maxRating);

        // Atualiza estatísticas
        switch (result)
        {
            case GameResult.Win:
                wins++;
                break;
            case GameResult.Loss:
                losses++;
                break;
            case GameResult.Draw:
                draws++;
                break;
        }

        SavePlayerData();

        OnRatingChanged?.Invoke(currentRating);
        OnStatsChanged?.Invoke(wins, losses, draws);

        Debug.Log($"Rating Update: {currentRating - ratingChange} ? {currentRating} ({ratingChange:+#;-#;0})");
    }

    private float CalculateExpectedScore(int playerRating, int opponentRating)
    {
        return 1f / (1f + Mathf.Pow(10f, (opponentRating - playerRating) / 400f));
    }

    private float GetActualScore(GameResult result)
    {
        switch (result)
        {
            case GameResult.Win: return 1f;
            case GameResult.Loss: return 0f;
            case GameResult.Draw: return 0.5f;
            default: return 0f;
        }
    }

    private void SavePlayerData()
    {
        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.SetInt("CurrentRating", currentRating);
        PlayerPrefs.SetInt("GamesPlayed", gamesPlayed);
        PlayerPrefs.SetInt("Wins", wins);
        PlayerPrefs.SetInt("Losses", losses);
        PlayerPrefs.SetInt("Draws", draws);
        PlayerPrefs.Save();
    }

    private void LoadPlayerData()
    {
        playerName = PlayerPrefs.GetString("PlayerName", "reinaldolbarros");
        currentRating = PlayerPrefs.GetInt("CurrentRating", 1200);
        gamesPlayed = PlayerPrefs.GetInt("GamesPlayed", 0);
        wins = PlayerPrefs.GetInt("Wins", 0);
        losses = PlayerPrefs.GetInt("Losses", 0);
        draws = PlayerPrefs.GetInt("Draws", 0);
    }

    public void ResetStats()
    {
        currentRating = 1200;
        gamesPlayed = 0;
        wins = 0;
        losses = 0;
        draws = 0;
        SavePlayerData();

        OnRatingChanged?.Invoke(currentRating);
        OnStatsChanged?.Invoke(wins, losses, draws);
    }

    public string GetRankTitle()
    {
        if (currentRating < 800) return "Iniciante";
        if (currentRating < 1000) return "Aprendiz";
        if (currentRating < 1200) return "Amador";
        if (currentRating < 1400) return "Intermediário";
        if (currentRating < 1600) return "Avançado";
        if (currentRating < 1800) return "Expert";
        if (currentRating < 2000) return "Mestre";
        if (currentRating < 2200) return "Mestre Internacional";
        return "Grande Mestre";
    }
}

public enum GameResult
{
    Win,
    Loss,
    Draw
}