using UnityEngine;

public class Sword : Weapon
{
    [Header("Sword Settings")]
    public Transform attackPoint;
    public Vector2 attackArea = new Vector2(1f, 0.5f);
    public LayerMask enemyLayers;
    

    public GameObject swordPrefab;




    public override void Turn(bool isRight)
    {
        if (attackPoint == null) return;

        // Flip Position (X)
        Vector3 pos = attackPoint.localPosition;
        pos.x = isRight ? Mathf.Abs(pos.x) : -Mathf.Abs(pos.x);
        attackPoint.localPosition = pos;

        // Flip Rotation (Y)
        Vector3 rot = attackPoint.localEulerAngles;
        rot.y = isRight ? 0f : 180f;
        attackPoint.localEulerAngles = rot;
    }

    protected override void PerformAttack()
    {
        if (attackPoint == null) return;

        if (swordPrefab != null)
        {
            // Instantiate with attackPoint's rotation, so the effect faces the correct way automatically
            Instantiate(swordPrefab, attackPoint.position, attackPoint.rotation);
        }

        // 2. Detect enemies
        // Use attackPoint.eulerAngles.z for box rotation if needed, or 0 if the box should handle rotation via matrix
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, attackArea, attackPoint.eulerAngles.z, enemyLayers);

            //Debug.Log("Attack");
            // 3. วนลูปสั่งลดเลือดศัตรูทุกตัวที่โดน
            foreach (Collider2D enemy in hitEnemies)
            {
                Debug.Log("ฟันโดน " + enemy.name);

                
                enemy.GetComponent<Enemy>()?.TakeDamage(damage);
            }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        
        Gizmos.matrix = Matrix4x4.TRS(attackPoint.position, attackPoint.rotation, Vector3.one);

        
        Gizmos.DrawWireCube(Vector3.zero, attackArea);
    }
}
