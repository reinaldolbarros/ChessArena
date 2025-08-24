using System;

public static class GameEvents
{
    // Mantemos os eventos sem redefinir enums que j� existem no projeto.
    public static Action<AIOpponent, GameMode> OnGameStarted;
    public static Action<string> OnGameEnded; // Use uma string/raz�o para evitar conflito com GameResult existente
    public static Action OnPlayerMove;
    public static Action OnAIMove;
}