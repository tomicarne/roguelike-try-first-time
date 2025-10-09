using UnityEngine;
using TMPro;
using System.Collections;
<<<<<<< Updated upstream
=======
using System;
>>>>>>> Stashed changes

// Clase base abstracta para manejar la salud, daño, invencibilidad y UI de salud
public abstract class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 3;                  // Salud máxima
    protected int currentHealth;               // Salud actual

    [Header("Invincibility")]
    public float invincibilityDuration = 1.5f; // Duración de invencibilidad tras recibir daño
    protected bool isInvincible = false;       // Si está invencible actualmente
    private float invincibilityTimer = 0f;     // Temporizador de invencibilidad

    [Header("UI")]
    public TMP_Text healthText;                // Referencia al texto de salud (asignar en Inspector)

    private SpriteRenderer spriteRenderer;     // Referencia al SpriteRenderer para efectos visuales
    private Coroutine blinkCoroutine;          // Corrutina para el parpadeo durante invencibilidad

    // Inicializa la salud y referencias al iniciar el objeto
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        updateHealthText();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    //maneja el temporizador de invencibilidad y el parpadeo
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
                    blinkCoroutine = null; // Asegura que el sprite sea visible
                }
                SetSpriteAlpha(1f); // Restaura la opacidad total
                Debug.Log(gameObject.name + " is no longer invincible.");
            }
        }
    }

    // Restaura la salud al máximo y actualiza la UI
    public void restore_health()
    {
        currentHealth = maxHealth;
        updateHealthText();
        Debug.Log(gameObject.name + " health restored. HP: " + currentHealth);
    }

    // Aplica daño si no está invencible, inicia invencibilidad y parpadeo
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

    // Corrutina que hace parpadear el sprite entre visible y 60% opacidad durante la invencibilidad
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
        SetSpriteAlpha(1f); // Asegura opacidad total al terminar
    }

    // Cambia la opacidad del sprite
    private void SetSpriteAlpha(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color C = spriteRenderer.color;
            C.a = alpha;
            spriteRenderer.color = C;
        }
    }

    // Actualiza el texto de salud en la UI
    protected void updateHealthText()
    {
        if (healthText != null)
            healthText.text = currentHealth + " / " + maxHealth;
    }

    // Permite establecer invencibilidad temporal desde otros scripts (por ejemplo, durante un dash)
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

    // Método abstracto que define qué ocurre al morir (debe implementarse en las subclases)
    protected abstract void Die();
<<<<<<< Updated upstream
}
=======

    internal void TakeDamage(float v)
    {
        throw new NotImplementedException();
    }
}
>>>>>>> Stashed changes
