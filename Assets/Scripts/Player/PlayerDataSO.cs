using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData", order = 1)]
public class PlayerDataSO : ScriptableObject
{
    [Header("Base Stats")]
    public int baseMaxHP = 10;
    public float baseMoveSpeed = 5f;
    public float baseJumpForce = 10f;
    public float baseDashSpeed = 20f;
    public float baseAttackDamage = 1f;

    // [Leveling]
    // XP Formula: 100 * (1.15 ^ (Level - 1))
    
    [Header("Movement Limits (Asymptotic Scaling)")]
    public float limitMoveSpeed = 10f;
    public float limitJumpForce = 18f;
    public float limitDashSpeed = 40f;
    
    // [RPG Growth (Percentage)]
    // Uses formula: Base * (1 + (Level / 100))
    // No specific limits, just infinite scaling based on level
    // We don't need explicit fields for these as they depend on Base Stats.


    /// <summary>
    /// Calculates a stat that approaches a mathematical limit as Level increases.
    /// Formula: Limit + (Base - Limit) / (1 + (Level * 0.05))
    /// </summary>
    public float CalculateLimitStat(float baseVal, float limitVal, int level)
    {
        // Avoid division by zero, though 1 + ... is safe for positive levels.
        float denominator = 1f + (level * 0.05f);
        return limitVal + ((baseVal - limitVal) / denominator);
    }

    /// <summary>
    /// Calculates an RPG hp stat (Integer, Ceiled).
    /// </summary>
    public int CalculateRPGStat(int baseVal, int level)
    {
        // Formula: Base * (1 + (Level - 1) * 0.1)
        // Wait, user said "Increase 0.1 per level".
        // If Level 1 = Base (1.0x)
        // Level 2 = Base * 1.01? Or 1.1?
        // User said: "Level 10 suppose to Increase HP by 1.0" (which is 10 * 0.1).
        // Since Level 1->10 is 9 levels gained, 9 * 0.1 = 0.9 increase?
        // Or did they mean Level 11?
        // "Level 10 suppose to Increase HP by 1.0"
        // If Base is 10. Increase by 1.0 means Result is 11.
        // If formula is 1 + (Level/100):
        // Lvl 10 -> 1 + 0.1 = 1.1x multiplier. 10 * 1.1 = 11.
        // But user said "it didn't".
        
        // Let's look at previous code:
        // float multiplier = 1f + (level / 100f);
        // return Mathf.CeilToInt(baseVal * multiplier);
        
        // At Level 10: 1 + 0.1 = 1.1. Base(10) * 1.1 = 11. Ceil(11) = 11.
        // So at Level 10 it SHOULD be 11.
        
        // Maybe the issue is at Level 1?
        // Prev: Lvl 1 -> 1 + 0.01 = 1.01. 10 * 1.01 = 10.1 -> Ceil(10.1) = 11.
        // So Level 1 starts at 11 ??
        // User probably wants Level 1 = 10.
        
        // New Formula plan: 1 + ((level - 1) / 100f)
        // Lvl 1 -> 1 + 0 = 1.0. 10 * 1.0 = 10. Correct.
        // Lvl 11 -> 1 + 0.1 = 1.1. 10 * 1.1 = 11.
        // Lvl 10 -> 1 + 0.09 = 1.09. 10 * 1.09 = 10.9 -> Floor(10.9) = 10.
        
        // Wait, user said "Level 10 suppose to Increase by 1.0".
        // If they mean 1 full Point.
        // 10.9 is almost 11.
        
        // If we use RoundToInt? 10.9 -> 11.
        // If we use FloorToInt? 10.
        
        // If the goal is "Increase 0.1 per level" and "Level 10 increases by 1.0".
        // Base 10. 
        // Level 1: 10
        // ...
        // Level 10: 11
        // (11 is 1.0 increase from 10).
        
        // If we use (Level / 100):
        // Lvl 10: 10/100 = 0.1. Multiplier 1.1. 10*1.1 = 11.
        
        // The problem likely IS the start point.
        // Old: Level 1 -> 1.01 -> 10.1 -> Ceil -> 11.
        // So Level 1 Was 11.
        // Level 10 -> 1.1 -> 11.
        // So from Lvl 1 to Lvl 10, it stayed 11? No increase.
        
        // Fix: Use (Level - 1) / 100f to start at 1.0 for Level 1.
        // And regarding rounding:
        // Lvl 11 (10 increase) -> 1 + 0.1 = 1.1 -> 11.
        
        // Use FloorToInt to prevent premature jumps?
        // Lvl 10 (9 increase) -> 1 + 0.09 = 1.09 -> 10.9. Floor -> 10.
        
        // User asked: "Level 10 suppose to increase by 1.0".
        // This implies at Level 10, the value should be 11 (if Base 10).
        // 0.1 * 10 levels = 1.0.
        // Does Level 1 count as a level? Usually Level 1 is base.
        // So Level 10 is +9 levels.
        
        // Maybe user means "Increase stats by 0.1 (value) per level"?
        // Base Attack 1.0. Lvl 2 -> 1.1?
        // Base HP 10. Lvl 2 -> 10.1? (But HP is int).
        
        // Let's stick to (Level - 1) / 100.
        // At Base 10.
        // Lvl 1: 1.0 * 10 = 10.
        // Lvl 11: 1.1 * 10 = 11.
        // This gives exactly 1 HP per 10 levels.
        
        // If User wants 1 HP at Level 10 (which is +9 steps), they need slightly faster scaling?
        // Or they accept Level 11 is the milestone.
        
        float multiplier = 1f + ((level - 1) / 50f);
        return Mathf.FloorToInt(baseVal * multiplier);
    }

    /// <summary>
    /// Calculates Attack Damage as float with 1 decimal place.
    /// Formula: Base * (1 + (Level / 100)) -> Rounded to 1 decimal.
    /// </summary>
    public float CalculateAttackStat(float baseVal, int level)
    {
        float multiplier = 1f + ((level - 1) / 10f);
        float val = baseVal * multiplier;
        return Mathf.Round(val * 10f) / 10f;
    }

    /// <summary>
    /// Calculates XP required for the NEXT level.
    /// Formula: 100 * (1.15 ^ (Level - 1))
    /// </summary>
    public int CalculateXPRequired(int level)
    {
        int baseReq = 100;
        float growthFactor = 1.15f;
        float req = baseReq * Mathf.Pow(growthFactor, level - 1);
        return Mathf.RoundToInt(req);
    }
}
