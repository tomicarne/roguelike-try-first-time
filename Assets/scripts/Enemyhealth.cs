using UnityEngine;

public class EnemyHealth : Health
{
    protected override void Die()
    {
        Debug.Log(gameObject.name + " (Enemy) died!");
        // Example: play animation, drop loot, etc.
        Destroy(gameObject);
    }
}
