using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;

        GameManager existing = FindObjectOfType<GameManager>();
        if (existing != null)
        {
            Instance = existing;
            return;
        }

        GameObject go = new GameObject("GameManager");
        go.AddComponent<GameManager>();
    }

    [Header("Game Settings")]
    public int lives = 3;
    public int score = 0;
    public int lastLevelBuildIndex = 2; // até Level_2
    
    [Header("Scene Names")]
    public string victoryScene = "Victory";
    public string defeatScene = "Defeat";
    public string level1Scene = "Level_1";
    public string level2Scene = "Level_2";
    
    [Header("Level Transition")]
    public float levelTransitionDelay = 0.5f;
    public bool showLevelCompleteMessage = true;
    
    // UI References
    private Text scoreText;
    private Text livesText;
    private Text messageText; // Para mensagens temporárias
    
    // Internal tracking
    private int bricksRemaining = 0;
    private bool levelCompleted = false;
    private bool gameEnded = false;

    private void Awake()
    {
        if (Instance == null) 
        { 
            Instance = this; 
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else 
        { 
            Destroy(gameObject); 
            return; 
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene carregada: {scene.name}");
        
        // Reset level completion flag
        levelCompleted = false;
        gameEnded = false;
        
        // Find UI elements
        FindUIElements();
        
        // Count bricks in the new scene
        CountBricks();
        
        // Update UI
        UpdateUI();
        
        Debug.Log($"Blocos encontrados na cena: {bricksRemaining}");
    }
    
    private void FindUIElements()
    {
        var scoreObj = GameObject.Find("ScoreText");
        if (scoreObj) 
            scoreText = scoreObj.GetComponent<Text>();
        else
            Debug.LogWarning("ScoreText não encontrado na cena!");
            
        var livesObj = GameObject.Find("LivesText");
        if (livesObj) 
            livesText = livesObj.GetComponent<Text>();
        else
            Debug.LogWarning("LivesText não encontrado na cena!");
            
        // Procurar por texto de mensagem (opcional)
        var messageObj = GameObject.Find("MessageText");
        if (messageObj)
            messageText = messageObj.GetComponent<Text>();
    }
    
    private void CountBricks()
    {
        // Aguardar um frame para garantir que todos os objetos foram criados
        StartCoroutine(CountBricksCoroutine());
    }
    
    private IEnumerator CountBricksCoroutine()
    {
        yield return null; // Aguarda um frame
        
        GameObject[] bricks = GameObject.FindGameObjectsWithTag("Brick");
        bricksRemaining = bricks.Length;
        
        Debug.Log($"Contagem atualizada de blocos: {bricksRemaining}");
        
        // Se não há blocos, algo está errado
        if (bricksRemaining == 0 && IsGameLevel())
        {
            Debug.LogError("Nenhum bloco encontrado na cena de jogo! Verifique as tags.");
        }
    }
    
    private bool IsGameLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return sceneName.StartsWith("Level_");
    }

    public void AddScore(int value) 
    { 
        score += value; 
        UpdateUI(); 
        Debug.Log($"Score atualizado: {score}");
    }
    
    public void LoseLife()
    {
        if (gameEnded) return;
        
        lives--; 
        UpdateUI();
        Debug.Log($"Vida perdida. Vidas restantes: {lives}");
        
        if (lives <= 0) 
        {
            gameEnded = true;
            Debug.Log("Game Over! Carregando cena de derrota...");
            StartCoroutine(LoadSceneWithDelay(defeatScene, 0.15f));
        }
    }

    public void BrickDestroyed()
    {
        if (levelCompleted || gameEnded) return;
        
        bricksRemaining--;
        Debug.Log($"Bloco destruído. Blocos restantes: {bricksRemaining}");
        
        // Verificação dupla para garantir contagem correta
        StartCoroutine(VerifyBrickCount());
    }
    
    private IEnumerator VerifyBrickCount()
    {
        yield return null; // Aguarda um frame para o Destroy() fazer efeito
        
        GameObject[] remainingBricks = GameObject.FindGameObjectsWithTag("Brick");
        int actualCount = remainingBricks.Length;
        
        Debug.Log($"Verificação: Contagem interna = {bricksRemaining}, Contagem real = {actualCount}");
        
        // Usar a contagem real como referência
        bricksRemaining = actualCount;
        
        if (bricksRemaining <= 0 && !levelCompleted)
        {
            levelCompleted = true;
            Debug.Log("Nível completado!");
            OnLevelCompleted();
        }
    }
    
    private void OnLevelCompleted()
    {
        if (gameEnded) return;
        
        Debug.Log("OnLevelCompleted chamado");
        
        // Mostrar mensagem de nível completo (se habilitado)
        if (showLevelCompleteMessage && messageText)
        {
            messageText.text = "LEVEL COMPLETE!";
            messageText.gameObject.SetActive(true);
        }
        
        // Determinar próximo nível
        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log($"Build Index atual: {currentBuildIndex}");
        Debug.Log($"Último nível (Build Index): {lastLevelBuildIndex}");
        
        if (currentBuildIndex < lastLevelBuildIndex)
        {
            int nextLevel = currentBuildIndex + 1;
            Debug.Log($"Carregando próximo nível (Build Index {nextLevel})...");
            StartCoroutine(LoadSceneWithDelay(nextLevel, levelTransitionDelay));
        }
        else
        {
            Debug.Log("Todos os níveis completados! Carregando tela de vitória...");
            gameEnded = true;
            StartCoroutine(LoadSceneWithDelay(victoryScene, levelTransitionDelay));
        }
    }
    
    private IEnumerator LoadSceneWithDelay(string sceneName, float delay)
    {
        Debug.Log($"Aguardando {delay} segundos antes de carregar: {sceneName}");
        yield return new WaitForSeconds(delay);
        
        Debug.Log($"Carregando cena: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
    
    private IEnumerator LoadSceneWithDelay(int buildIndex, float delay)
    {
        Debug.Log($"Aguardando {delay} segundos antes de carregar Build Index: {buildIndex}");
        yield return new WaitForSeconds(delay);
        
        Debug.Log($"Carregando cena por Build Index: {buildIndex}");
        SceneManager.LoadScene(buildIndex);
    }

    void UpdateUI()
    {
        if (scoreText) 
            scoreText.text = "Score: " + score;
        if (livesText) 
            livesText.text = "Lives: " + lives;
    }

    public void ResetGame()
    {
        Debug.Log("Resetando jogo...");
        score = 0; 
        lives = 3; 
        levelCompleted = false;
        gameEnded = false;
        UpdateUI();
    }
    
    // Método para debug - chamado manualmente se necessário
    public void ForceCountBricks()
    {
        GameObject[] bricks = GameObject.FindGameObjectsWithTag("Brick");
        bricksRemaining = bricks.Length;
        Debug.Log($"Contagem forçada de blocos: {bricksRemaining}");
        
        if (bricksRemaining <= 0 && IsGameLevel() && !levelCompleted)
        {
            Debug.Log("Forçando transição de nível...");
            OnLevelCompleted();
        }
    }
    
    // Método para debug no inspector
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugInfo()
    {
        Debug.Log("=== DEBUG INFO ===");
        Debug.Log($"Score: {score}");
        Debug.Log($"Lives: {lives}");
        Debug.Log($"Bricks Remaining: {bricksRemaining}");
        Debug.Log($"Level Completed: {levelCompleted}");
        Debug.Log($"Game Ended: {gameEnded}");
        Debug.Log($"Current Scene: {SceneManager.GetActiveScene().name}");
        Debug.Log($"Current Build Index: {SceneManager.GetActiveScene().buildIndex}");
        Debug.Log($"Last Level Build Index: {lastLevelBuildIndex}");
        
        GameObject[] actualBricks = GameObject.FindGameObjectsWithTag("Brick");
        Debug.Log($"Blocos reais na cena: {actualBricks.Length}");
        
        if (actualBricks.Length > 0)
        {
            Debug.Log("Blocos encontrados:");
            foreach (var brick in actualBricks)
            {
                Debug.Log($"- {brick.name} at {brick.transform.position}");
            }
        }
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
