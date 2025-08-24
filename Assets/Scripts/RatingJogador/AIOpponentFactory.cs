using UnityEngine;

public static class AIOpponentFactory
{
    public static AIOpponent CreateForDifficulty(AIOpponent baseOpponent, AIDifficulty difficulty)
    {
        if (baseOpponent == null) return null;

        var ai = baseOpponent.Clone();

        switch (difficulty)
        {
            case AIDifficulty.Beginner:
                ai.rating = Mathf.Clamp(ai.rating - 300, 600, 2600);
                ai.blunderChance = Mathf.Clamp01(Mathf.Max(ai.blunderChance, 0.12f));
                ai.thinkingTimeMultiplier = Mathf.Max(0.8f, ai.thinkingTimeMultiplier * 0.9f);
                break;

            case AIDifficulty.Intermediate:
                ai.rating = Mathf.Clamp(ai.rating, 800, 2700);
                ai.blunderChance = Mathf.Clamp01(Mathf.Max(ai.blunderChance, 0.08f));
                ai.thinkingTimeMultiplier = Mathf.Clamp(ai.thinkingTimeMultiplier, 0.9f, 1.3f);
                break;

            case AIDifficulty.Advanced:
                ai.rating = Mathf.Clamp(ai.rating + 150, 1000, 2800);
                ai.blunderChance = Mathf.Clamp01(Mathf.Min(ai.blunderChance, 0.05f));
                ai.thinkingTimeMultiplier = Mathf.Clamp(ai.thinkingTimeMultiplier * 1.15f, 1.0f, 1.6f);
                break;

            case AIDifficulty.Master:
                ai.rating = Mathf.Clamp(ai.rating + 300, 1200, 2900);
                ai.blunderChance = Mathf.Clamp01(Mathf.Min(ai.blunderChance, 0.02f));
                ai.thinkingTimeMultiplier = Mathf.Clamp(ai.thinkingTimeMultiplier * 1.3f, 1.1f, 1.8f);
                break;
        }

        return ai;
    }
}