using UnityEngine;
using System.Collections;

public class Brick : MonoBehaviour
{
    [Header("Configurações do Bloco")]
    public int points = 100;
    public int hits = 1; // Quantos hits para destruir
    public bool isUnbreakable = false;
    
    [Header("Efeitos Visuais")]
    public Color[] damageColors; // Cores conforme leva dano
    public GameObject destroyEffect; // Efeito de partículas
    public float flashDuration = 0.1f;
    
    [Header("Sons")]
    public AudioClip hitSound;
    public AudioClip destroySound;
    
    private int currentHits = 0;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private BoxCollider2D boxCollider;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        
        if (spriteRenderer)
            originalColor = spriteRenderer.color;
            
        currentHits = 0;
        
        // Configurar collider para melhor detecção
        if (boxCollider)
        {
            boxCollider.isTrigger = false;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            HandleBallCollision(collision);
        }
    }
    
    void HandleBallCollision(Collision2D collision)
    {
        if (isUnbreakable)
        {
            // Bloco indestrutível - apenas reflete a bola
            ReflectBall(collision);
            PlayHitEffect();
            return;
        }
        
        // Incrementar hits
        currentHits++;
        
        // Aplicar dano visual
        UpdateVisualDamage();
        
        // Refletir a bola
        ReflectBall(collision);
        
        // Reproduzir som
        PlayHitSound();
        
        // Verificar se deve ser destruído
        if (currentHits >= hits)
        {
            DestroyBrick();
        }
        else
        {
            // Efeito de flash se não foi destruído
            StartCoroutine(FlashEffect());
        }
    }
    
    void ReflectBall(Collision2D collision)
    {
        Rigidbody2D ballRb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (ballRb == null) return;
        
        // Obter informações da colisão
        ContactPoint2D contact = collision.contacts[0];
        Vector2 incomingVector = ballRb.linearVelocity;
        Vector2 reflectedVector;
        
        // Melhor cálculo de reflexão baseado na normal de contato
        Vector2 contactNormal = contact.normal;
        
        // Determinar se a colisão foi nas laterais ou em cima/baixo
        float horizontalComponent = Mathf.Abs(contactNormal.x);
        float verticalComponent = Mathf.Abs(contactNormal.y);
        
        if (horizontalComponent > verticalComponent)
        {
            // Colisão lateral - inverter X
            reflectedVector = new Vector2(-incomingVector.x, incomingVector.y);
        }
        else
        {
            // Colisão vertical - inverter Y
            reflectedVector = new Vector2(incomingVector.x, -incomingVector.y);
        }
        
        // Aplicar pequena variação para evitar loops infinitos
        float variation = 0.05f;
        reflectedVector.x += Random.Range(-variation, variation);
        reflectedVector.y += Random.Range(-variation, variation);
        
        // Aplicar a nova velocidade
        float originalSpeed = incomingVector.magnitude;
        ballRb.linearVelocity = reflectedVector.normalized * originalSpeed;
        
        // Pequeno impulso para garantir separação
        Vector2 separationForce = contactNormal * 0.1f;
        collision.gameObject.transform.position += (Vector3)separationForce;
    }
    
    void UpdateVisualDamage()
    {
        if (spriteRenderer == null || damageColors == null || damageColors.Length == 0)
            return;
            
        if (hits > 1)
        {
            int colorIndex = Mathf.Clamp(currentHits - 1, 0, damageColors.Length - 1);
            spriteRenderer.color = damageColors[colorIndex];
        }
    }
    
    void PlayHitSound()
    {
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
    }
    
    void PlayHitEffect()
    {
        StartCoroutine(FlashEffect());
    }
    
    IEnumerator FlashEffect()
    {
        if (spriteRenderer == null) yield break;
        
        Color flashColor = Color.white;
        spriteRenderer.color = flashColor;
        
        yield return new WaitForSeconds(flashDuration);
        
        if (currentHits < hits)
        {
            UpdateVisualDamage();
        }
    }
    
    void DestroyBrick()
    {
        // Adicionar pontos
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(points);
            GameManager.Instance.BrickDestroyed();
        }
        else
        {
            Debug.LogWarning("GameManager não encontrado!");
        }
        
        // Reproduzir som de destruição
        if (destroySound != null)
        {
            AudioSource.PlayClipAtPoint(destroySound, transform.position);
        }
        
        // Efeito de partículas
        if (destroyEffect != null)
        {
            GameObject effect = Instantiate(destroyEffect, transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        // Destruir o bloco
        Destroy(gameObject);
    }
    
    // Método para configurar blocos especiais
    public void SetBrickType(int newHits, Color[] colors = null, bool unbreakable = false)
    {
        hits = newHits;
        isUnbreakable = unbreakable;
        
        if (colors != null && colors.Length > 0)
        {
            damageColors = colors;
        }
        
        currentHits = 0;
        
        if (spriteRenderer)
        {
            spriteRenderer.color = originalColor;
        }
    }
    
    // Método para blocos que se regeneram
    public void RegenerateBrick(float delay = 2f)
    {
        StartCoroutine(RegenerationCoroutine(delay));
    }
    
    IEnumerator RegenerationCoroutine(float delay)
    {
        // Tornar invisível e desabilitar collider
        spriteRenderer.enabled = false;
        boxCollider.enabled = false;
        
        yield return new WaitForSeconds(delay);
        
        // Restaurar bloco
        currentHits = 0;
        spriteRenderer.enabled = true;
        boxCollider.enabled = true;
        spriteRenderer.color = originalColor;
        
        // Efeito de regeneração
        StartCoroutine(FlashEffect());
    }
}