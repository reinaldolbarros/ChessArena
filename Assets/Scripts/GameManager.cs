using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Chess Game Components")]
    [SerializeField] private BoardController boardController;
    [SerializeField] private ChessTimer chessTimer;

    [Header("Game Control")]
    [SerializeField] private bool gameActive = true;

    [Header("Current Match Info")]
    [SerializeField] private AIOpponent currentOpponent;
    [SerializeField] private GameMode currentGameMode;

    [Header("UI Feedback")]
    [SerializeField] private GameObject drawOfferPanel;
    [SerializeField] private TextMeshProUGUI drawOfferText;
    [SerializeField] private Button acceptDrawButton;
    [SerializeField] private Button declineDrawButton;
    [SerializeField] private Button surrenderButton;
    [SerializeField] private Button offerDrawButton;

    [Header("Audio")]
    [SerializeField] private AudioSource gameEndSound;
    [SerializeField] private AudioSource drawOfferSound;

    // Chess game variables
    private ChessEngine chessEngine;
    private Vector2Int? selectedSquare = null;
    private PlayerColor currentPlayer;
    private bool isGameOver = false;
    private TimerUI timerUI;

    // Game control variables
    private bool drawOffered = false;
    private bool waitingForPlayerResponse = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        InitializeGame();
        SetupGameControlButtons();
        HideDrawOfferPanel();
    }

    private void InitializeGame()
    {
        try
        {
            // Initialize chess engine first
            chessEngine = new ChessEngine();
            currentPlayer = PlayerColor.White;

            // Find components if not assigned
            if (boardController == null)
            {
                boardController = FindObjectOfType<BoardController>();
                Debug.LogWarning("BoardController não estava assignado! Encontrado automaticamente.");
            }

            if (chessTimer == null)
            {
                chessTimer = FindObjectOfType<ChessTimer>();
                Debug.LogWarning("ChessTimer não estava assignado! Encontrado automaticamente.");
            }

            timerUI = FindObjectOfType<TimerUI>();

            // Initialize board controller if available
            if (boardController != null && chessEngine != null)
            {
                boardController.Initialize(chessEngine);
                Debug.Log("✅ BoardController inicializado com sucesso!");
            }
            else
            {
                Debug.LogError("❌ BoardController ou ChessEngine é null! Verifique as referências.");
                return;
            }

            // Start timer if available
            if (chessTimer != null)
            {
                chessTimer.StartTimer();
                Debug.Log("✅ ChessTimer iniciado!");
            }
            else
            {
                Debug.LogWarning("⚠️ ChessTimer não encontrado! Timer desabilitado.");
            }

            // Set current player in UI
            if (timerUI != null)
            {
                timerUI.SetCurrentPlayer(currentPlayer);
                Debug.Log("✅ TimerUI configurado!");
            }
            else
            {
                Debug.LogWarning("⚠️ TimerUI não encontrado! Interface de timer desabilitada.");
            }

            // Subscribe to timer events
            ChessTimer.OnTimeExpired += HandlePlayerTimeExpired;
            ChessTimer.OnMoveTimeExpired += HandlePlayerMoveTimeExpired;

            Debug.Log("🎯 GameManager inicializado com sucesso!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Erro ao inicializar GameManager: {ex.Message}");
            Debug.LogError($"StackTrace: {ex.StackTrace}");
        }
    }

    private void Update()
    {
        // Teste temporário para UI
        if (Input.GetKeyDown(KeyCode.T))
        {
            ShowDrawOfferFeedback("🤝 Proposta de empate enviada! Aguarde sua decisão.");
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            HideDrawOfferPanel();
        }
    }

    private void OnDestroy()
    {
        ChessTimer.OnTimeExpired -= HandlePlayerTimeExpired;
        ChessTimer.OnMoveTimeExpired -= HandlePlayerMoveTimeExpired;
    }

    #region Chess Game Logic

    public void OnTileSelected(GameObject tile)
    {
        if (isGameOver || !gameActive) return;

        if (chessEngine == null || boardController == null)
        {
            Debug.LogError("❌ ChessEngine ou BoardController é null!");
            return;
        }

        int x = Mathf.RoundToInt(tile.transform.position.x);
        int y = Mathf.RoundToInt(tile.transform.position.z);
        Vector2Int currentPos = new Vector2Int(x, y);

        if (selectedSquare.HasValue)
        {
            if (chessEngine.TryMovePiece(selectedSquare.Value, currentPos))
            {
                boardController.MoveVisualPiece(selectedSquare.Value, currentPos);
                SwitchPlayer();
                CheckForGameOver();
            }
            selectedSquare = null;
            boardController.HideHighlight();
            boardController.HideValidMoves();
        }
        else
        {
            ChessPiece piece = chessEngine.Board[x, y];
            if (piece != null && piece.Color == currentPlayer)
            {
                selectedSquare = currentPos;
                boardController.HighlightSquare(x, y);
                boardController.ShowValidMoves(chessEngine.GetLegalMovesForPieceAt(x, y));
            }
        }
    }

    private void SwitchPlayer()
    {
        currentPlayer = (currentPlayer == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;

        if (chessTimer != null)
        {
            chessTimer.SwitchPlayer();
        }

        if (timerUI != null)
        {
            timerUI.SetCurrentPlayer(currentPlayer);
        }
    }

    private void CheckForGameOver()
    {
        if (chessEngine == null) return;

        if (chessEngine.HasAnyLegalMoves(currentPlayer))
        {
            if (chessEngine.IsKingInCheck(currentPlayer))
                Debug.LogWarning($"XEQUE! Vez do jogador {currentPlayer}.");
        }
        else
        {
            EndGame("Natural");

            if (chessEngine.IsKingInCheck(currentPlayer))
            {
                PlayerColor winner = (currentPlayer == PlayerColor.White ? PlayerColor.Black : PlayerColor.White);
                DeclareWinner(winner, "Xeque-Mate");
                UpdatePlayerRating(winner == PlayerColor.White ? GameResult.Win : GameResult.Loss);
            }
            else
            {
                DeclareDrawResult("Afogamento (Stalemate)");
                UpdatePlayerRating(GameResult.Draw);
            }
        }
    }

    #endregion

    #region Game Control Logic

    private void SetupGameControlButtons()
    {
        try
        {
            if (acceptDrawButton != null)
                acceptDrawButton.onClick.AddListener(AcceptDrawOffer);

            if (declineDrawButton != null)
                declineDrawButton.onClick.AddListener(DeclineDrawOffer);

            if (surrenderButton != null)
                surrenderButton.onClick.AddListener(SurrenderGame);

            if (offerDrawButton != null)
                offerDrawButton.onClick.AddListener(OfferDraw);

            Debug.Log("✅ Botões de controle configurados!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Erro ao configurar botões: {ex.Message}");
        }
    }

    public void OnGameStarted(AIOpponent opponent, GameMode mode)
    {
        currentOpponent = opponent;
        currentGameMode = mode;
        gameActive = true;
        drawOffered = false;
        waitingForPlayerResponse = false;
        isGameOver = false;

        Debug.Log($"🎮 Jogo iniciado contra {opponent?.name ?? "Oponente"} no modo {mode?.name ?? "Padrão"}");
    }

    public void SurrenderGame()
    {
        if (!gameActive || isGameOver) return;

        // DETERMINA VENCEDOR E PERDEDOR
        PlayerColor winner = (currentPlayer == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;
        PlayerColor loser = currentPlayer;

        // DECLARA VITÓRIA OFICIAL
        DeclareWinner(winner, "Vitória por Desistência do Adversário");

        // TERMINA O JOGO
        EndGame("Surrender");

        // PLAYER PERDE RATING
        UpdatePlayerRating(GameResult.Loss);

        Debug.Log($"📉 reinaldolbarros perdeu rating ELO por desistência");

        if (currentOpponent != null)
        {
            Debug.Log($"📈 {currentOpponent.name} ganhou rating ELO por vitória!");
        }

        Invoke(nameof(ReturnToLobby), 3f);
    }

    #endregion

    #region Sistema de Empate - SEM RESPOSTA AUTOMÁTICA DA IA

    public void OfferDraw()
    {
        if (!gameActive || drawOffered || isGameOver || waitingForPlayerResponse) return;

        drawOffered = true;
        waitingForPlayerResponse = true;

        Debug.Log($"🤝 2025-08-23 01:26:07 - reinaldolbarros ofereceu EMPATE!");

        // Desabilita o botão temporariamente
        if (offerDrawButton != null)
            offerDrawButton.interactable = false;

        // ✅ CORREÇÃO: Não simula resposta da IA automaticamente
        // Apenas mostra o painel para o JOGADOR decidir
        string opponentName = currentOpponent != null ? currentOpponent.name : "Oponente";
        ShowDrawOfferFeedback($"🤝 Você ofereceu empate para {opponentName}!\n\nDeseja confirmar esta proposta?");

        Debug.Log($"💭 Aguardando decisão do jogador reinaldolbarros...");
    }

    // ✅ QUANDO JOGADOR CLICA EM ACEITAR (confirma a proposta de empate)
    public void AcceptDrawOffer()
    {
        if (!drawOffered || !waitingForPlayerResponse) return;

        Debug.Log($"✅ 2025-08-23 01:26:07 - reinaldolbarros CONFIRMOU a proposta de empate!");

        // ✅ ESCONDE O PAINEL IMEDIATAMENTE
        HideDrawOfferPanel();

        // Reset das variáveis
        drawOffered = false;
        waitingForPlayerResponse = false;

        // ✅ EMPATE ACEITO - Finaliza o jogo imediatamente
        AcceptDraw();
    }

    // ✅ QUANDO JOGADOR CLICA EM RECUSAR (cancela a proposta de empate)
    public void DeclineDrawOffer()
    {
        if (!waitingForPlayerResponse) return;

        Debug.Log($"❌ 2025-08-23 01:26:07 - reinaldolbarros CANCELOU a proposta de empate!");

        // ✅ ESCONDE O PAINEL IMEDIATAMENTE
        HideDrawOfferPanel();

        // Reset das variáveis
        drawOffered = false;
        waitingForPlayerResponse = false;

        // Reabilita o botão para futuras propostas
        if (offerDrawButton != null)
            offerDrawButton.interactable = true;

        Debug.Log("⚔️ Proposta de empate cancelada! O jogo continua.");
    }

    private void AcceptDraw()
    {
        if (!gameActive || isGameOver) return;

        EndGame("Draw");
        DeclareDrawResult("Empate por Acordo Mútuo");
        UpdatePlayerRating(GameResult.Draw);

        Invoke(nameof(ReturnToLobby), 3f);
    }

    private void DeclareDrawResult(string reason)
    {
        Debug.Log("🤝 ================== EMPATE DECLARADO ================== 🤝");
        Debug.Log($"⚖️ RESULTADO: EMPATE");
        Debug.Log($"📋 Motivo: {reason}");
        Debug.Log($"⏰ Data/Hora: 2025-08-23 01:26:07");
        Debug.Log($"👤 reinaldolbarros: Empate");

        if (currentOpponent != null)
        {
            Debug.Log($"🤖 {currentOpponent.name}: Empate");
        }

        Debug.Log($"📊 Ambos os jogadores mantêm rating similar");
        Debug.Log("🤝 ======================================================= 🤝");
    }

    private void ShowDrawOfferFeedback(string message)
    {
        if (drawOfferPanel != null)
        {
            drawOfferPanel.SetActive(true);
            if (drawOfferText != null)
                drawOfferText.text = message;
        }

        if (drawOfferSound != null)
            drawOfferSound.Play();

        Debug.Log($"💬 Mensagem: {message}");
    }

    private void HideDrawOfferPanel()
    {
        if (drawOfferPanel != null)
        {
            drawOfferPanel.SetActive(false);
            Debug.Log("👁️ Painel de empate escondido");
        }
    }

    #endregion

    #region Timer Events

    private void HandlePlayerTimeExpired(PlayerColor player)
    {
        if (!gameActive || isGameOver) return;

        EndGame("TimeExpired");

        PlayerColor winner = (player == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;
        DeclareWinner(winner, "Vitória por Tempo Esgotado");

        bool playerLost = (player == PlayerColor.White);
        GameResult result = playerLost ? GameResult.Loss : GameResult.Win;
        UpdatePlayerRating(result);

        Invoke(nameof(ReturnToLobby), 3f);
    }

    private void HandlePlayerMoveTimeExpired(PlayerColor player)
    {
        if (!gameActive || isGameOver) return;

        EndGame("MoveTimeExpired");

        PlayerColor winner = (player == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;
        DeclareWinner(winner, "Vitória por Tempo de Movimento Esgotado");

        bool playerLost = (player == PlayerColor.White);
        GameResult result = playerLost ? GameResult.Loss : GameResult.Win;
        UpdatePlayerRating(result);

        Invoke(nameof(ReturnToLobby), 3f);
    }

    #endregion

    #region Game End Logic

    private void EndGame(string reason)
    {
        isGameOver = true;
        gameActive = false;

        if (chessTimer != null)
            chessTimer.StopTimer();

        if (gameEndSound != null)
            gameEndSound.Play();

        // Disable game buttons
        if (surrenderButton != null)
            surrenderButton.interactable = false;
        if (offerDrawButton != null)
            offerDrawButton.interactable = false;

        Debug.Log($"🏁 Jogo terminado por: {reason}");
    }

    private void DeclareWinner(PlayerColor winner, string reason)
    {
        Debug.Log("🎊 ================== PARTIDA FINALIZADA ================== 🎊");
        Debug.Log($"🏆 VENCEDOR: Jogador {winner}");
        Debug.Log($"📋 Motivo: {reason}");
        Debug.Log($"⏰ Data/Hora: 2025-08-23 01:26:07");

        if (winner == PlayerColor.Black && currentOpponent != null)
        {
            Debug.Log($"🤖 {currentOpponent.name} é o CAMPEÃO desta partida!");
        }
        else if (winner == PlayerColor.White)
        {
            Debug.Log($"👤 reinaldolbarros é o CAMPEÃO desta partida!");
        }

        Debug.Log("🎊 ======================================================= 🎊");
    }

    private void UpdatePlayerRating(GameResult result)
    {
        if (PlayerRating.Instance != null && currentGameMode != null && currentGameMode.isRanked)
        {
            int opponentRating = currentOpponent != null ? currentOpponent.rating : 1200;
            PlayerRating.Instance.UpdateRating(result, opponentRating);
        }
    }

    private void ReturnToLobby()
    {
        Debug.Log("🔄 Voltando ao lobby...");
    }

    #endregion

    #region Public Properties

    public bool IsGameOver => isGameOver;
    public bool IsGameActive => gameActive;
    public PlayerColor CurrentPlayer => currentPlayer;

    #endregion
}