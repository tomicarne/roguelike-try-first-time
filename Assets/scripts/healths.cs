using UnityEngine;
using TMPro;
using System.Collections;
<<<<<<< Updated upstream
=======
using System;

// Clase base abstracta para manejar la salud, daño, invencibilidad y UI de salud
>>>>>>> Stashed changes
public abstract class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;
    protected int currentHealth;
    [Header("Invincibility")]
    public float invincibilityDuration = 1.5f; // seconds
    protected bool isInvincible = false;
    private float invincibilityTimer = 0f;
    [Header("UI")]
    public TMP_Text healthText; // Assign in Inspector

    private SpriteRenderer spriteRenderer;
    private Coroutine blinkCoroutine;
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        updateHealthText();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                if (blinkCoroutine != null)
                {
                    StopCoroutine(blinkCoroutine);
                    blinkCoroutine = null; // Ensure sprite is visible
                }
                SetSpriteAlpha(1f);
                Debug.Log(gameObject.name + " is no longer invincible.");
            }
        }
    }
    public void restore_health()
    {
        currentHealth = maxHealth;
        updateHealthText();
        Debug.Log(gameObject.name + " health restored. HP: " + currentHealth);
    }

    public virtual void TakeDamage(int amount)
    {
        if (isInvincible) return;

        currentHealth -= amount;
        updateHealthText();
        Debug.Log(gameObject.name + " took " + amount + " damage. HP: " + currentHealth);

        isInvincible = true;
        invincibilityTimer = invincibilityDuration;
        if (spriteRenderer != null)
        {
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkSprite());
        }
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator BlinkSprite()
    {
        float blinkInterval = 0.15f;
        bool visible = true;
        while (isInvincible)
        {
            SetSpriteAlpha(visible ? 1f : 0.6f);
            visible = !visible;
            yield return new WaitForSeconds(blinkInterval);
        }
        SetSpriteAlpha(1f); // Ensure sprite is fully visible at end
    }

    private void SetSpriteAlpha(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color C = spriteRenderer.color;
            C.a = alpha;
            spriteRenderer.color = C;
        }
    }
    //shows the health in the text UI
    protected void updateHealthText()
    {
        if (healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;
    }
    public void SetTemporaryInvincibility(float duration)
    {
        isInvincible = true;
        invincibilityTimer = Mathf.Max(invincibilityTimer, duration);

        if (spriteRenderer != null)
        {
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkSprite());
        }
    }

<<<<<<< Updated upstream
    protected abstract void Die(); // each subclass decides what happens on death
}
=======
    // Método abstracto que define qué ocurre al morir (debe implementarse en las subclases)
    protected abstract void Die();

    internal void TakeDamage(float v)
    {
        throw new NotImplementedException();
    }
}
>>>>>>> Stashed changes
