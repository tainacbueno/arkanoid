using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DefeatMenu : MonoBehaviour
{
    public Text scoreText;

    void Start()
    {
        if (scoreText) scoreText.text = "Final Score: " + GameManager.Instance.score;
    }

    public void RestartGame() { GameManager.Instance.ResetGame(); SceneManager.LoadScene("Level_1"); }
    public void BackToMenu() { SceneManager.LoadScene("MainMenu"); }
}
