using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public float moveSpeed = 3f;
    public bool isActive = false;

    private Transform target;

    public void Activate(Transform player)
    {
        target = player;
        Debug.Log("checking enemie base");
        Debug.Log(isActive);

        // ✅ activate other AI scripts on this enemy
        EnemyMeleeAI meleeAI = GetComponent<EnemyMeleeAI>();
        if (meleeAI != null) meleeAI.Activate(player);

        EnemyShooterAI shooterAI = GetComponent<EnemyShooterAI>();
        if (shooterAI != null) shooterAI.Activate(player);
    }
}
