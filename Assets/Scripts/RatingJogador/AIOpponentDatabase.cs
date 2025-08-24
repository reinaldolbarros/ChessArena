using UnityEngine;

public class AIOpponentDatabase : MonoBehaviour
{
    [Header("AI Opponents")]
    [SerializeField] private AIOpponent[] availableOpponents;

    public static AIOpponentDatabase Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreateDefaultOpponents();
    }

    private void CreateDefaultOpponents()
    {
        availableOpponents = new AIOpponent[]
        {
            new AIOpponent
            {
                name = "João Iniciante",
                rating = 800,
                description = "Aprendendo os movimentos básicos",
                personality = AIPersonality.Defensive,
                thinkingTimeMultiplier = 0.5f,
                blunderChance = 0.3f
            },
            new AIOpponent
            {
                name = "Maria Casual",
                rating = 1000,
                description = "Joga por diversão nos fins de semana",
                personality = AIPersonality.Balanced,
                thinkingTimeMultiplier = 0.8f,
                blunderChance = 0.2f
            },
            new AIOpponent
            {
                name = "Pedro Estudioso",
                rating = 1200,
                description = "Estuda aberturas religiosamente",
                personality = AIPersonality.Defensive,
                thinkingTimeMultiplier = 1.2f,
                blunderChance = 0.15f
            },
            new AIOpponent
            {
                name = "Ana Agressiva",
                rating = 1400,
                description = "Ataque é a melhor defesa!",
                personality = AIPersonality.Aggressive,
                thinkingTimeMultiplier = 0.7f,
                blunderChance = 0.12f
            },
            new AIOpponent
            {
                name = "Carlos Calculista",
                rating = 1600,
                description = "Cada movimento é cuidadosamente calculado",
                personality = AIPersonality.Defensive,
                thinkingTimeMultiplier = 1.5f,
                blunderChance = 0.08f
            },
            new AIOpponent
            {
                name = "Sofia Imprevisível",
                rating = 1800,
                description = "Você nunca sabe o que esperar dela",
                personality = AIPersonality.Unpredictable,
                thinkingTimeMultiplier = 1f,
                blunderChance = 0.05f
            },
            new AIOpponent
            {
                name = "Magnus IA",
                rating = 2200,
                description = "Inteligência artificial de elite",
                personality = AIPersonality.Balanced,
                thinkingTimeMultiplier = 2f,
                blunderChance = 0.02f
            }
        };
    }

    public AIOpponent FindOpponent(int playerRating, int maxRatingDifference = 200)
    {
        var suitableOpponents = new System.Collections.Generic.List<AIOpponent>();

        foreach (var opponent in availableOpponents)
        {
            int ratingDiff = Mathf.Abs(opponent.rating - playerRating);
            if (ratingDiff <= maxRatingDifference)
            {
                suitableOpponents.Add(opponent);
            }
        }

        if (suitableOpponents.Count == 0)
        {
            return FindOpponent(playerRating, maxRatingDifference + 100);
        }

        // Retorna CÓPIA para podermos ajustar dificuldade sem mutar o banco
        var baseOpp = suitableOpponents[Random.Range(0, suitableOpponents.Count)];
        return baseOpp.Clone();
    }

    public AIOpponent GetOpponentByName(string opponentName)
    {
        foreach (var opponent in availableOpponents)
        {
            if (opponent.name == opponentName)
                return opponent.Clone();
        }
        return null;
    }

    public AIOpponent[] GetAllOpponents()
    {
        return availableOpponents;
    }
}