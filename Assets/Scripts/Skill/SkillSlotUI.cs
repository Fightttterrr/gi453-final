using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image skillIcon;       // รูปไอคอนสกิล
    public Image cooldownOverlay; // รูปสีดำจางๆ (ต้องตั้ง Image Type = Filled)
    public TMP_Text cooldownText; // Text แสดงตัวเลข Cooldown
    public GameObject lockIcon;   // รูปกุญแจล็อค (ถ้ามี)

    private SkillBase linkedSkill;

    void Awake()
    {
        // เริ่มต้นให้ปิดไอคอนสกิลและเปิดตัวล็อคไว้
        if (skillIcon) skillIcon.enabled = false;
        if (cooldownOverlay) cooldownOverlay.fillAmount = 0;
        if (cooldownText) cooldownText.gameObject.SetActive(false);
        if (lockIcon) lockIcon.SetActive(true);
    }

    void Update()
    {
        if (linkedSkill == null)
        {
             if (cooldownText) cooldownText.gameObject.SetActive(false);
             return;
        }

        // อัปเดตวงกลม Cooldown
        if (cooldownOverlay != null)
        {
            if (linkedSkill.IsOnCooldown())
            {
                cooldownOverlay.fillAmount = linkedSkill.GetCooldownRatio();
                
                // Show text if < 1 second
                float remaining = linkedSkill.GetRemainingTime();
                if (remaining <= 1.0f && remaining > 0f)
                {
                   if(cooldownText) 
                   {
                       cooldownText.gameObject.SetActive(true);
                       cooldownText.text = remaining.ToString("F1");
                   }
                }
                else
                {
                    if(cooldownText) cooldownText.gameObject.SetActive(false);
                }
            }
            else
            {
                cooldownOverlay.fillAmount = 0f;
                if(cooldownText) cooldownText.gameObject.SetActive(false);
            }
        }
    }

    // ฟังก์ชันนี้จะถูกเรียกตอนอนิเมชั่นบินมาถึง
    public void UnlockAndSetSkill(SkillBase skill, Sprite iconSprite)
    {
        linkedSkill = skill;

        if (lockIcon) lockIcon.SetActive(false); // ปิดกุญแจ

        if (skillIcon)
        {
            skillIcon.enabled = true;
            skillIcon.sprite = iconSprite; // ใส่รูป
            skillIcon.color = Color.white;
        }
    }
}