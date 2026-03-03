using UnityEngine;

public class BottomTrigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D c)
    {
        if (!c.CompareTag("Ball")) return;

        BallController ball = c.GetComponent<BallController>();
        if (ball != null)
        {
            ball.ResetBall();
        }
        else
        {
            Debug.LogWarning("Objeto com tag Ball sem BallController.");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();
        }
        else
        {
            Debug.LogError("GameManager.Instance estah nulo. Verifique o bootstrap do GameManager.");
        }
    }
}
