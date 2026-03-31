using UnityEngine;
using System.Collections.Generic;
using System;
public class SkillManager : MonoBehaviour
{
    [Header("Dependencies")]
    public Player player;

    [Header("Skills Database")]
    public SkillBase skill1_ThrowKnife; 
    public SkillBase skill2_Invisible;
    public SkillBase skill3_Ultimate;

    public SkillBase activeSlot1 { get; private set; }
    public SkillBase activeSlot2 { get; private set; }
    public SkillBase activeSlot3 { get; private set; }
    
    private bool unlockSkill1 = false;
    private bool unlockSkill2 = false;
    private bool unlockSkill3 = false;

    public event Action<int, SkillBase> OnSkillUnlocked;
    void Start()
    {
        if (player == null) player = GetComponent<Player>();

        
        if (skill1_ThrowKnife) skill1_ThrowKnife.Initialize(player);
        if (skill2_Invisible) skill2_Invisible.Initialize(player);
        if (skill3_Ultimate) skill3_Ultimate.Initialize(player);

        
        if (player.stats != null)
        {
            player.stats.OnLevelUp += CheckUnlock;
            CheckUnlock(player.stats.currentLevel); 
        }
    }

    void OnDestroy()
    {
        if (player != null && player.stats != null)
            player.stats.OnLevelUp -= CheckUnlock;
    }

    private void CheckUnlock(int level)
    {// ปลดล็อก Skill 1
        if (level >= 5 && !unlockSkill1)
        {
            unlockSkill1 = true;
            activeSlot1 = skill1_ThrowKnife;
            OnSkillUnlocked?.Invoke(1, skill1_ThrowKnife); // แจ้ง UI
            Debug.Log("Unlocked: Throw Knife!");
        }

        // ปลดล็อก Skill 2
        if (level >= 10 && !unlockSkill2)
        {
            unlockSkill2 = true;
            activeSlot2 = skill2_Invisible;
            OnSkillUnlocked?.Invoke(2, skill2_Invisible); // แจ้ง UI
            Debug.Log("Unlocked: Invisible!");
        }

        // ปลดล็อก Skill 3 
        if (level >= 15 && !unlockSkill3)
        {
            unlockSkill3 = true;
            activeSlot3 = skill3_Ultimate;
            OnSkillUnlocked?.Invoke(3, skill3_Ultimate); // แจ้ง UI
            Debug.Log("Unlocked: Ultimate!");
        }
    }



    public void TryUseSkill(int slotIndex)
    {
        // Prevent skill use if stunned
        if (player.stats != null && player.stats.IsHit) return;

        SkillBase targetSkill = null;
        switch (slotIndex)
        {
            case 1: targetSkill = activeSlot1; break;
            case 2: targetSkill = activeSlot2; break;
            case 3: targetSkill = activeSlot3; break;
        }

        if (targetSkill != null)
        {
            targetSkill.Use();
        }
    }
}