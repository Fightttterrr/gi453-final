using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkillUIManager : MonoBehaviour
{
    [Header("Dependencies")]
    public SkillManager skillManager; // ลาก SkillManager ของ Player มาใส่

    [Header("Skill Slots")]
    public SkillSlotUI slot1;
    public SkillSlotUI slot2;
    public SkillSlotUI slot3;

    [Header("Assets & Animation")]
    public GameObject floatingIconPrefab; // Prefab รูปไอคอนที่จะให้บิน (Image)
    public Transform centerScreenPoint;   // จุดกลางจอ (สร้าง Empty Game Object ไว้กลาง Canvas)
    public float flyDuration = 1.0f;

    // ไอคอนของแต่ละสกิล (ลากมาใส่ใน Inspector)
    public Sprite iconSkill1;
    public Sprite iconSkill2;
    public Sprite iconSkill3;

    void Start()
    {
        if (skillManager != null)
        {
            skillManager.OnSkillUnlocked += PlayUnlockAnimation;
        }
    }

    void OnDestroy()
    {
        if (skillManager != null)
        {
            skillManager.OnSkillUnlocked -= PlayUnlockAnimation;
        }
    }

    private void PlayUnlockAnimation(int slotIndex, SkillBase skill)
    {
        SkillSlotUI targetSlot = null;
        Sprite targetIcon = null;

        switch (slotIndex)
        {
            case 1: targetSlot = slot1; targetIcon = iconSkill1; break;
            case 2: targetSlot = slot2; targetIcon = iconSkill2; break;
            case 3: targetSlot = slot3; targetIcon = iconSkill3; break;
        }

        if (targetSlot != null)
        {
            StartCoroutine(AnimateIconRoutine(targetSlot, skill, targetIcon));
        }
    }

    IEnumerator AnimateIconRoutine(SkillSlotUI targetSlot, SkillBase skill, Sprite icon)
    {
        // 1. สร้างไอคอนชั่วคราวกลางจอ
        GameObject tempObj = Instantiate(floatingIconPrefab, centerScreenPoint.position, Quaternion.identity, transform);
        Image tempImg = tempObj.GetComponent<Image>();
        tempImg.sprite = icon;

        // 2. เอฟเฟกต์เด้งดึ๋ง (Scale Up)
        float timer = 0f;
        while (timer < 0.5f)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(0f, 2f, timer / 0.5f); // ขยายใหญ่ 2 เท่า
            tempObj.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f); // ค้างโชว์แปปนึง

        // 3. บินไปหาช่อง (Lerp Position & Scale Down)
        Vector3 startPos = tempObj.transform.position;
        Vector3 endPos = targetSlot.transform.position;
        timer = 0f;

        while (timer < flyDuration)
        {
            timer += Time.deltaTime;
            float t = timer / flyDuration;
            // ใช้ SmoothStep ให้ดูนุ่มนวล
            t = t * t * (3f - 2f * t);

            tempObj.transform.position = Vector3.Lerp(startPos, endPos, t);
            tempObj.transform.localScale = Vector3.Lerp(Vector3.one * 2f, Vector3.one, t); // ย่อลงเท่าขนาดจริง
            yield return null;
        }

        // 4. ถึงที่หมาย -> ลบตัวบิน -> เปิดใช้งานช่องจริง
        Destroy(tempObj);
        targetSlot.UnlockAndSetSkill(skill, icon);
    }
}