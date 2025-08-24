using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI whiteTimeText;
    [SerializeField] private TextMeshProUGUI blackTimeText;
    [SerializeField] private TextMeshProUGUI moveTimeText;
    [SerializeField] private TextMeshProUGUI totalTimeText;
    [SerializeField] private Image whitePanelBackground;
    [SerializeField] private Image blackPanelBackground;
    [SerializeField] private Image movePanelBackground;

    [Header("Colors")]
    [SerializeField] private Color activePlayerColor = new Color(0.3f, 0.8f, 0.3f, 1f); // Verde
    [SerializeField] private Color inactivePlayerColor = new Color(0.53f, 0.81f, 0.92f, 1f); // Azul claro
    [SerializeField] private Color moveTimeNormalColor = new Color(0.86f, 0.08f, 0.24f, 1f); // Vermelho
    [SerializeField] private Color moveTimeLowColor = new Color(1f, 0.5f, 0f, 1f); // Laranja
    [SerializeField] private float lowMoveTimeThreshold = 30f;

    private ChessTimer chessTimer;
    private PlayerColor currentPlayer = PlayerColor.White;

    private void Start()
    {
        chessTimer = FindObjectOfType<ChessTimer>();

        ChessTimer.OnTimeUpdated += UpdateTimeDisplay;
        ChessTimer.OnMoveTimeUpdated += UpdateMoveTimeDisplay;
        ChessTimer.OnTotalTimeUpdated += UpdateTotalTimeDisplay;
        ChessTimer.OnTimeExpired += OnPlayerTimeExpired;
        ChessTimer.OnMoveTimeExpired += OnPlayerMoveTimeExpired;

        UpdateVisualState();
    }

    private void OnDestroy()
    {
        ChessTimer.OnTimeUpdated -= UpdateTimeDisplay;
        ChessTimer.OnMoveTimeUpdated -= UpdateMoveTimeDisplay;
        ChessTimer.OnTotalTimeUpdated -= UpdateTotalTimeDisplay;
        ChessTimer.OnTimeExpired -= OnPlayerTimeExpired;
        ChessTimer.OnMoveTimeExpired -= OnPlayerMoveTimeExpired;
    }

    private void UpdateTimeDisplay(float whiteTime, float blackTime)
    {
        if (whiteTimeText != null)
            whiteTimeText.text = "Brancas: " + chessTimer.FormatTime(whiteTime);

        if (blackTimeText != null)
            blackTimeText.text = "Pretas: " + chessTimer.FormatTime(blackTime);
    }

    private void UpdateMoveTimeDisplay(float moveTime)
    {
        if (moveTimeText != null)
        {
            moveTimeText.text = "Movimento: " + chessTimer.FormatTime(moveTime);

            // Muda cor quando tempo está baixo
            if (movePanelBackground != null)
            {
                movePanelBackground.color = moveTime <= lowMoveTimeThreshold ?
                    moveTimeLowColor : moveTimeNormalColor;
            }
        }
    }

    private void UpdateTotalTimeDisplay(float totalTime)
    {
        if (totalTimeText != null)
            totalTimeText.text = "Total: " + chessTimer.FormatTime(totalTime);
    }

    public void SetCurrentPlayer(PlayerColor player)
    {
        currentPlayer = player;
        UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (whitePanelBackground != null)
        {
            whitePanelBackground.color = (currentPlayer == PlayerColor.White) ?
                activePlayerColor : inactivePlayerColor;
        }

        if (blackPanelBackground != null)
        {
            blackPanelBackground.color = (currentPlayer == PlayerColor.Black) ?
                activePlayerColor : inactivePlayerColor;
        }
    }

    private void OnPlayerTimeExpired(PlayerColor player)
    {
        Debug.LogError($"TEMPO TOTAL ESGOTADO! Jogador {player} perdeu!");
    }

    private void OnPlayerMoveTimeExpired(PlayerColor player)
    {
        Debug.LogError($"TEMPO DE MOVIMENTO ESGOTADO! Jogador {player} perdeu!");
    }
}