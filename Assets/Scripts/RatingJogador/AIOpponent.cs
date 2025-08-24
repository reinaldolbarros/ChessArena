using UnityEngine;

[System.Serializable]
public class AIOpponent
{
    public string name;
    public int rating;
    public string description;
    public Sprite avatar;
    public AIPersonality personality;

    [Range(0.1f, 3f)]
    public float thinkingTimeMultiplier = 1f; // Simula tempo de resposta

    [Range(0f, 1f)]
    public float blunderChance = 0.1f; // Chance de fazer erro

    // Evita mutar o objeto do banco ao aplicar dificuldade
    public AIOpponent Clone()
    {
        return new AIOpponent
        {
            name = this.name,
            rating = this.rating,
            description = this.description,
            avatar = this.avatar,
            personality = this.personality,
            thinkingTimeMultiplier = this.thinkingTimeMultiplier,
            blunderChance = this.blunderChance
        };
    }
}

[System.Serializable]
public enum AIPersonality
{
    Aggressive,   // Joga rápido e agressivo
    Defensive,    // Joga defensivo e lento
    Balanced,     // Equilibrado
    Unpredictable // Estilo aleatório
}