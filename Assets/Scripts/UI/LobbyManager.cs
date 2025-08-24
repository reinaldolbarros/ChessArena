using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LobbyManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject matchmakingPanel;
    [SerializeField] private GameObject gamePanel;

    [Header("Lobby UI")]
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI currentRatingText;
    [SerializeField] private TextMeshProUGUI rankTitleText;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private Button[] gameModeButtons;

    [Header("Matchmaking UI")]
    [SerializeField] private TextMeshProUGUI searchStatusText;
    [SerializeField] private Button cancelSearchButton;
    [SerializeField] private Slider searchProgressSlider;
    [SerializeField] private GameObject opponentFoundPanel;
    [SerializeField] private TextMeshProUGUI opponentNameText;
    [SerializeField] private TextMeshProUGUI opponentRatingText;
    [SerializeField] private TextMeshProUGUI opponentDescriptionText;
    [SerializeField] private Image opponentAvatarImage;
    [SerializeField] private Button playButton;
    [SerializeField] private TextMeshProUGUI gameModeInfoText;

    [Header("Difficulty (opcional)")]
    [SerializeField] private TMP_Dropdown difficultyDropdown; // arraste no Inspector (valores: Iniciante/Intermediário/Avançado/Mestre)

    [Header("Audio")]
    [SerializeField] private AudioSource buttonClickSound;
    [SerializeField] private AudioSource matchFoundSound;

    public static LobbyManager Instance { get; private set; }

    private GameMode selectedGameMode;
    private AIOpponent foundOpponent;
    private bool isSearching = false;
    private Coroutine progressCoroutine;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        InitializeUI();
    }

    private void OnEnable()
    {
        MatchmakingManager.OnMatchmakingStarted += OnMatchmakingStarted;
        MatchmakingManager.OnMatchmakingCancelled += OnMatchmakingCancelled;
        MatchmakingManager.OnMatchmakingStatusChanged += OnMatchmakingStatusChanged;
        MatchmakingManager.OnMatchFound += OnMatchFound;
    }

    private void OnDisable()
    {
        MatchmakingManager.OnMatchmakingStarted -= OnMatchmakingStarted;
        MatchmakingManager.OnMatchmakingCancelled -= OnMatchmakingCancelled;
        MatchmakingManager.OnMatchmakingStatusChanged -= OnMatchmakingStatusChanged;
        MatchmakingManager.OnMatchFound -= OnMatchFound;
    }

    private void Start()
    {
        SetupButtons();
        UpdatePlayerInfo();
    }

    private void InitializeUI()
    {
        ShowLobby();
        SetupGameModeButtons();
        if (opponentFoundPanel != null) opponentFoundPanel.SetActive(false);
        if (playButton != null) playButton.gameObject.SetActive(false);
        if (searchProgressSlider != null) searchProgressSlider.value = 0f;
    }

    private void SetupButtons()
    {
        if (cancelSearchButton != null) cancelSearchButton.onClick.AddListener(CancelSearch);
        if (playButton != null) playButton.onClick.AddListener(StartGame);
    }

    private void SetupGameModeButtons()
    {
        var modes = MatchmakingManager.Instance != null
            ? MatchmakingManager.Instance.GetAvailableGameModes()
            : null;

        if (modes == null)
        {
            foreach (var btn in gameModeButtons) if (btn != null) btn.gameObject.SetActive(false);
            return;
        }

        for (int i = 0; i < gameModeButtons.Length; i++)
        {
            if (i < modes.Length)
            {
                var button = gameModeButtons[i];
                var mode = modes[i];
                button.gameObject.SetActive(true);

                var texts = button.GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length >= 2)
                {
                    texts[0].text = mode.name;
                    texts[1].text = mode.description;
                }

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => StartMatchmaking(mode));
            }
            else
            {
                if (gameModeButtons[i] != null)
                    gameModeButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void UpdatePlayerInfo()
    {
        if (PlayerRating.Instance != null)
        {
            playerNameText.text = PlayerRating.Instance.PlayerName;
            currentRatingText.text = $"Rating: {PlayerRating.Instance.CurrentRating}";
            rankTitleText.text = PlayerRating.Instance.GetRankTitle();

            float winRate = PlayerRating.Instance.WinRate;
            int totalGames = PlayerRating.Instance.Wins + PlayerRating.Instance.Losses + PlayerRating.Instance.Draws;
            statsText.text = $"Partidas: {totalGames} | Vitórias: {PlayerRating.Instance.Wins} | Taxa: {winRate:F1}%";
        }
        else
        {
            playerNameText.text = "Jogador";
            currentRatingText.text = "Rating: 1000";
            rankTitleText.text = "Iniciante";
            statsText.text = "Partidas: 0 | Vitórias: 0 | Taxa: 0%";
        }
    }

    public void StartMatchmaking(GameMode gameMode)
    {
        selectedGameMode = gameMode;
        PlayButtonSound();

        ShowMatchmaking();

        // Obtém dificuldade do dropdown (fallback: Intermediário)
        AIDifficulty difficulty = AIDifficulty.Intermediate;
        if (difficultyDropdown != null)
        {
            int idx = Mathf.Clamp(difficultyDropdown.value, 0, 3);
            difficulty = (AIDifficulty)idx;
        }

        MatchmakingManager.Instance.StartMatchmaking(selectedGameMode, difficulty);
    }

    // Eventos do MatchmakingManager
    private void OnMatchmakingStarted()
    {
        isSearching = true;
        if (searchProgressSlider != null) searchProgressSlider.value = 0f;
        if (opponentFoundPanel != null) opponentFoundPanel.SetActive(false);
        if (playButton != null) playButton.gameObject.SetActive(false);

        // Anima barra enquanto procura
        if (progressCoroutine != null) StopCoroutine(progressCoroutine);
        progressCoroutine = StartCoroutine(AnimateProgress());
    }

    private void OnMatchmakingStatusChanged(string msg)
    {
        if (searchStatusText != null) searchStatusText.text = msg;
    }

    private void OnMatchmakingCancelled()
    {
        isSearching = false;
        if (progressCoroutine != null) { StopCoroutine(progressCoroutine); progressCoroutine = null; }
        ShowLobby();
    }

    private void OnMatchFound(AIOpponent opponent, GameMode mode)
    {
        isSearching = false;
        if (progressCoroutine != null) { StopCoroutine(progressCoroutine); progressCoroutine = null; }

        foundOpponent = opponent;
        selectedGameMode = mode;
        ShowOpponentFound();
    }

    private IEnumerator AnimateProgress()
    {
        if (searchProgressSlider == null) yield break;

        float t = 0f;
        while (isSearching)
        {
            t += Time.deltaTime;
            // ping-pong 0..1
            searchProgressSlider.value = 0.5f * (1f + Mathf.Sin(t * 2.5f));
            yield return null;
        }
        searchProgressSlider.value = 1f;
    }

    public void CancelSearch()
    {
        PlayButtonSound();
        MatchmakingManager.Instance.CancelMatchmaking();
    }

    private void ShowOpponentFound()
    {
        if (matchFoundSound != null) matchFoundSound.Play();

        if (opponentFoundPanel != null) opponentFoundPanel.SetActive(true);
        if (playButton != null) playButton.gameObject.SetActive(true);

        if (opponentNameText != null) opponentNameText.text = foundOpponent != null ? foundOpponent.name : "Oponente";
        if (opponentRatingText != null) opponentRatingText.text = foundOpponent != null ? $"Rating: {foundOpponent.rating}" : "Rating: -";
        if (opponentDescriptionText != null) opponentDescriptionText.text = foundOpponent != null ? foundOpponent.description : "";

        if (gameModeInfoText != null && selectedGameMode != null)
        {
            string timeControl = $"{selectedGameMode.timeControlMinutes}+{selectedGameMode.incrementSeconds}";
            gameModeInfoText.text = $"Modo: {selectedGameMode.name} ({timeControl})";
        }
    }

    public void StartGame()
    {
        PlayButtonSound();

        lobbyPanel.SetActive(false);
        matchmakingPanel.SetActive(false);
        gamePanel.SetActive(true);

        var chessTimer = FindObjectOfType<ChessTimer>();
        if (chessTimer != null && selectedGameMode != null)
        {
            chessTimer.SetGameTime(selectedGameMode.timeControlMinutes);
            chessTimer.SetIncrement(selectedGameMode.incrementSeconds);
            chessTimer.StartTimer();
        }

        GameEvents.OnGameStarted?.Invoke(foundOpponent, selectedGameMode);
    }

    public void ReturnToLobby()
    {
        ShowLobby();

        var chessTimer = FindObjectOfType<ChessTimer>();
        if (chessTimer != null)
        {
            chessTimer.StopTimer();
        }
    }

    private void ShowLobby()
    {
        lobbyPanel.SetActive(true);
        matchmakingPanel.SetActive(false);
        gamePanel.SetActive(false);
        isSearching = false;
    }

    private void ShowMatchmaking()
    {
        lobbyPanel.SetActive(false);
        matchmakingPanel.SetActive(true);
        gamePanel.SetActive(false);
    }

    private void PlayButtonSound()
    {
        if (buttonClickSound != null)
            buttonClickSound.Play();
    }
}