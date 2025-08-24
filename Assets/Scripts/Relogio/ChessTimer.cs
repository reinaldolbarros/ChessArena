using UnityEngine;
using System;

[System.Serializable]
public class ChessTimer : MonoBehaviour
{
    [Header("Configurações de Tempo")]
    [SerializeField] private float gameTimeInMinutes = 10f; // Tempo total por jogador em minutos
    [SerializeField] private float maxMoveTimeInSeconds = 120f; // Tempo máximo por movimento em segundos
    [SerializeField] private float incrementPerMove = 0f; // Incremento por jogada (Fischer increment)

    [Header("Estado do Timer")]
    [SerializeField] private float whiteTimeRemaining;
    [SerializeField] private float blackTimeRemaining;
    [SerializeField] private float currentMoveTimeRemaining;
    [SerializeField] private float totalGameTime;
    [SerializeField] private bool isGameActive = false;
    [SerializeField] private PlayerColor currentTimingPlayer;

    // Events para atualizar a UI
    public static event Action<float, float> OnTimeUpdated; // white time, black time
    public static event Action<float> OnMoveTimeUpdated; // current move time
    public static event Action<float> OnTotalTimeUpdated;
    public static event Action<PlayerColor> OnTimeExpired;
    public static event Action<PlayerColor> OnMoveTimeExpired; // NOVO EVENT

    public float WhiteTimeRemaining => whiteTimeRemaining;
    public float BlackTimeRemaining => blackTimeRemaining;
    public float CurrentMoveTimeRemaining => currentMoveTimeRemaining;
    public float TotalGameTime => totalGameTime;
    public bool IsGameActive => isGameActive;
    public float MaxMoveTimeInSeconds => maxMoveTimeInSeconds;

    private void Start()
    {
        InitializeTimer();
    }

    private void Update()
    {
        if (isGameActive)
        {
            UpdateTimer();
        }
    }

    public void InitializeTimer()
    {
        whiteTimeRemaining = gameTimeInMinutes * 60f;
        blackTimeRemaining = gameTimeInMinutes * 60f;
        currentMoveTimeRemaining = maxMoveTimeInSeconds;
        totalGameTime = 0f;
        currentTimingPlayer = PlayerColor.White;
        isGameActive = false;

        OnTimeUpdated?.Invoke(whiteTimeRemaining, blackTimeRemaining);
        OnMoveTimeUpdated?.Invoke(currentMoveTimeRemaining);
        OnTotalTimeUpdated?.Invoke(totalGameTime);
    }

    public void StartTimer()
    {
        isGameActive = true;
        currentTimingPlayer = PlayerColor.White;
        currentMoveTimeRemaining = maxMoveTimeInSeconds;
    }

    public void StopTimer()
    {
        isGameActive = false;
    }

    public void SwitchPlayer()
    {
        if (!isGameActive) return;

        // Adiciona incremento ao jogador que acabou de jogar
        if (incrementPerMove > 0)
        {
            if (currentTimingPlayer == PlayerColor.White)
                whiteTimeRemaining += incrementPerMove;
            else
                blackTimeRemaining += incrementPerMove;
        }

        // Troca o jogador atual
        currentTimingPlayer = (currentTimingPlayer == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;

        // NOVO: Reseta o tempo do movimento
        currentMoveTimeRemaining = maxMoveTimeInSeconds;

        OnTimeUpdated?.Invoke(whiteTimeRemaining, blackTimeRemaining);
        OnMoveTimeUpdated?.Invoke(currentMoveTimeRemaining);
    }

    private void UpdateTimer()
    {
        float deltaTime = Time.deltaTime;
        totalGameTime += deltaTime;

        // NOVO: Reduz o tempo do movimento atual
        currentMoveTimeRemaining -= deltaTime;
        if (currentMoveTimeRemaining <= 0)
        {
            currentMoveTimeRemaining = 0;
            OnMoveTimeExpired?.Invoke(currentTimingPlayer);
            StopTimer();
            return;
        }

        // Reduz o tempo total do jogador atual
        if (currentTimingPlayer == PlayerColor.White)
        {
            whiteTimeRemaining -= deltaTime;
            if (whiteTimeRemaining <= 0)
            {
                whiteTimeRemaining = 0;
                OnTimeExpired?.Invoke(PlayerColor.White);
                StopTimer();
                return;
            }
        }
        else
        {
            blackTimeRemaining -= deltaTime;
            if (blackTimeRemaining <= 0)
            {
                blackTimeRemaining = 0;
                OnTimeExpired?.Invoke(PlayerColor.Black);
                StopTimer();
                return;
            }
        }

        OnTimeUpdated?.Invoke(whiteTimeRemaining, blackTimeRemaining);
        OnMoveTimeUpdated?.Invoke(currentMoveTimeRemaining);
        OnTotalTimeUpdated?.Invoke(totalGameTime);
    }

    public void SetGameTime(float minutes)
    {
        gameTimeInMinutes = minutes;
        InitializeTimer();
    }

    public void SetMoveTime(float seconds)
    {
        maxMoveTimeInSeconds = seconds;
        InitializeTimer();
    }

    public void SetIncrement(float seconds)
    {
        incrementPerMove = seconds;
    }

    public string FormatTime(float timeInSeconds)
    {
        if (timeInSeconds < 0) timeInSeconds = 0;

        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);

        if (timeInSeconds < 60f)
            return $"{seconds:00}";
        else if (timeInSeconds < 3600f)
            return $"{minutes:00}:{seconds:00}";
        else
        {
            int hours = Mathf.FloorToInt(timeInSeconds / 3600f);
            minutes = Mathf.FloorToInt((timeInSeconds % 3600f) / 60f);
            return $"{hours:00}:{minutes:00}:{seconds:00}";
        }
    }
}