using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BallController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float speed = 12f;
    public float minSpeed = 8f;
    public float maxSpeed = 20f;
    
    [Header("Configurações de Colisão")]
    public float paddleBounceForce = 1.2f;
    public float wallBounceRandomness = 0.1f;
    public float minAngle = 15f; // Ângulo mínimo em graus para evitar bounces horizontais
    
    [Header("Physics Material")]
    public PhysicsMaterial2D ballMaterial;
    
    private Rigidbody2D rb;
    private CircleCollider2D col;
    private bool launched = false;
    private Vector2 lastVelocity;
    
    // Propriedades para debugging
    public Vector2 CurrentVelocity => rb ? rb.linearVelocity : Vector2.zero;
    public float CurrentSpeed => rb ? rb.linearVelocity.magnitude : 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        
        ResetBall();
    }

    void Update()
    {
        if (!launched)
        {
            // Seguir paddle antes do lançamento
            GameObject paddle = GameObject.FindWithTag("Paddle");
            if (paddle)
                transform.position = paddle.transform.position + Vector3.up * 0.6f;

            if (Input.GetKeyDown(KeyCode.Space))
                Launch();
        }
    }

    void FixedUpdate()
    {
        if (launched)
        {
            // Armazenar última velocidade para detecção de colisão
            lastVelocity = rb.linearVelocity;
            
            // Manter velocidade constante
            MaintainSpeed();
            
            // Verificar e corrigir ângulos muito planos
            CorrectShallowAngles();
        }
    }
    
    void MaintainSpeed()
    {
        float currentSpeed = rb.linearVelocity.magnitude;
        
        if (currentSpeed < minSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * minSpeed;
        }
        else if (currentSpeed > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
        else if (currentSpeed < speed - 0.5f || currentSpeed > speed + 0.5f)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * speed;
        }
    }
    
    void CorrectShallowAngles()
    {
        Vector2 velocity = rb.linearVelocity;
        float angle = Mathf.Abs(Vector2.Angle(velocity, Vector2.right));
        
        // Se o ângulo for muito raso (muito horizontal)
        if (angle < minAngle || angle > (180f - minAngle))
        {
            float newAngle = velocity.y > 0 ? minAngle : -minAngle;
            float direction = velocity.x > 0 ? 1 : -1;
            
            Vector2 newVelocity = new Vector2(
                direction * Mathf.Cos(newAngle * Mathf.Deg2Rad),
                Mathf.Sin(newAngle * Mathf.Deg2Rad)
            ).normalized * speed;
            
            rb.linearVelocity = newVelocity;
        }
    }

    void Launch()
    {
        launched = true;
        
        // Lançamento com ângulo aleatório para cima
        float randomAngle = Random.Range(-45f, 45f);
        Vector2 direction = new Vector2(
            Mathf.Sin(randomAngle * Mathf.Deg2Rad),
            Mathf.Cos(randomAngle * Mathf.Deg2Rad)
        );
        
        rb.linearVelocity = direction * speed;
    }

    public void ResetBall()
    {
        launched = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
    
    public void SetSpeed(float newSpeed)
    {
        speed = Mathf.Clamp(newSpeed, minSpeed, maxSpeed);
        if (launched)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * speed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
    }
    
    void HandleCollision(Collision2D collision)
    {
        ContactPoint2D contact = collision.contacts[0];
        Vector2 normal = contact.normal;
        
        if (collision.gameObject.CompareTag("Paddle"))
        {
            HandlePaddleCollision(collision, contact);
        }
        else if (collision.gameObject.CompareTag("Wall"))
        {
            HandleWallCollision(normal);
        }
        else if (collision.gameObject.CompareTag("Brick"))
        {
            HandleBrickCollision(normal);
        }
        else
        {
            // Colisão genérica - reflexão simples
            Vector2 reflection = Vector2.Reflect(lastVelocity, normal);
            rb.linearVelocity = reflection.normalized * speed;
        }
        
        // Garantir que a velocidade seja mantida após qualquer colisão
        MaintainSpeed();
    }
    
    void HandlePaddleCollision(Collision2D collision, ContactPoint2D contact)
    {
        // Calcular posição relativa no paddle (-1 a 1)
        float paddleWidth = collision.collider.bounds.size.x;
        float paddleCenter = collision.transform.position.x;
        float hitPosition = contact.point.x;
        float relativePosition = (hitPosition - paddleCenter) / (paddleWidth * 0.5f);
        relativePosition = Mathf.Clamp(relativePosition, -1f, 1f);
        
        // Calcular novo ângulo baseado na posição do impacto
        float bounceAngle = relativePosition * 75f; // Máximo de 75 graus
        
        // Criar direção da bola
        Vector2 direction = new Vector2(
            Mathf.Sin(bounceAngle * Mathf.Deg2Rad),
            Mathf.Cos(Mathf.Abs(bounceAngle) * Mathf.Deg2Rad)
        ).normalized;
        
        // Aplicar velocidade com força extra
        rb.linearVelocity = direction * speed * paddleBounceForce;
        
        // Adicionar pequeno impulso para cima para garantir que saia do paddle
        rb.AddForce(Vector2.up * 2f, ForceMode2D.Impulse);
    }
    
    void HandleWallCollision(Vector2 normal)
    {
        // Reflexão com pequena variação aleatória
        Vector2 reflection = Vector2.Reflect(lastVelocity, normal);
        
        // Adicionar randomness pequena
        reflection.x += Random.Range(-wallBounceRandomness, wallBounceRandomness);
        reflection.y += Random.Range(-wallBounceRandomness * 0.5f, wallBounceRandomness * 0.5f);
        
        rb.linearVelocity = reflection.normalized * speed;
    }
    
    void HandleBrickCollision(Vector2 normal)
    {
        // Reflexão mais precisa para blocos
        Vector2 reflection = Vector2.Reflect(lastVelocity, normal);
        rb.linearVelocity = reflection.normalized * speed;
    }
    
    // Método para aplicar efeitos especiais (power-ups, etc.)
    public void ApplySpeedMultiplier(float multiplier, float duration = 0f)
    {
        float newSpeed = speed * multiplier;
        newSpeed = Mathf.Clamp(newSpeed, minSpeed, maxSpeed);
        
        if (duration > 0f)
        {
            StartCoroutine(TemporarySpeedChange(newSpeed, duration));
        }
        else
        {
            SetSpeed(newSpeed);
        }
    }
    
    System.Collections.IEnumerator TemporarySpeedChange(float tempSpeed, float duration)
    {
        float originalSpeed = speed;
        SetSpeed(tempSpeed);
        yield return new WaitForSeconds(duration);
        SetSpeed(originalSpeed);
    }
    
    // Debug visual
    void OnDrawGizmosSelected()
    {
        if (rb != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, rb.linearVelocity.normalized * 2f);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}