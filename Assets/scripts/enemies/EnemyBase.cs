using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    private Transform target;
    // activates the enemy AI
    public void Activate(Transform player)
    {
        //target the player
        target = player;

        //activate other AI scripts on this enemy
        EnemyMeleeAI meleeAI = GetComponent<EnemyMeleeAI>();
        if (meleeAI != null) meleeAI.Activate(player);

        EnemyShooterAI shooterAI = GetComponent<EnemyShooterAI>();
        if (shooterAI != null) shooterAI.Activate(player);
    }
}
