using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class SceneAutoCreator : EditorWindow
{
    [MenuItem("Tools/Create Arkanoid Scenes")]
    public static void ShowWindow() => GetWindow<SceneAutoCreator>("Arkanoid Scene Creator");

    private void OnGUI()
    {
        GUILayout.Label("Gerar cenas Arkanoid", EditorStyles.boldLabel);

        if (GUILayout.Button("Criar MainMenu")) CreateMainMenu();
        if (GUILayout.Button("Criar Victory")) CreateVictory();
        if (GUILayout.Button("Criar Defeat")) CreateDefeat();
        if (GUILayout.Button("Criar Level")) CreateLevel();
    }

    // Fonte LegacyRuntime
    static Font legacyFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

    // ===== Helpers =====
    static GameObject CreateCanvas()
    {
        var go = new GameObject("Canvas");
        var c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        go.AddComponent<CanvasScaler>();
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    static GameObject CreateText(Transform parent, string txt, int size, Vector2 pos)
    {
        var go = new GameObject("Text");
        go.transform.SetParent(parent);
        var t = go.AddComponent<Text>();
        t.text = txt;
        t.font = legacyFont;
        t.fontSize = size;
        t.alignment = TextAnchor.MiddleCenter;
        t.color = Color.white;

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(500, 100);
        rt.anchoredPosition = pos;
        return go;
    }

    static GameObject CreateButton(Transform parent, string label, Vector2 pos)
    {
        var btn = new GameObject(label);
        btn.transform.SetParent(parent);
        btn.AddComponent<Image>().color = Color.gray;
        var b = btn.AddComponent<Button>();

        var rt = btn.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 60);
        rt.anchoredPosition = pos;

        var txt = CreateText(btn.transform, label, 24, Vector2.zero);
        txt.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

        return btn;
    }

    static void SaveScene(string name, UnityEngine.SceneManagement.Scene s)
    {
        string path = EditorUtility.SaveFilePanelInProject("Salvar cena", name, "unity", "Escolha onde salvar");
        if (!string.IsNullOrEmpty(path)) EditorSceneManager.SaveScene(s, path);
    }

    static void SafeSetTag(GameObject obj, string tagName)
    {
        try { obj.tag = tagName; }
        catch { Debug.LogWarning($"Tag \"{tagName}\" não existe! Crie-a em Edit > Project Settings > Tags and Layers."); }
    }

    // ===== MainMenu =====
    static void CreateMainMenu()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var canvas = CreateCanvas();

        CreateText(canvas.transform, "ARKANOID", 48, new Vector2(0, 150));
        var start = CreateButton(canvas.transform, "Start", new Vector2(0, 40));
        var quit = CreateButton(canvas.transform, "Quit", new Vector2(0, -40));

        var ctrl = new GameObject("MainMenuController").AddComponent<MainMenuController>();
        start.GetComponent<Button>().onClick.AddListener(ctrl.StartGame);
        quit.GetComponent<Button>().onClick.AddListener(ctrl.QuitGame);

        new GameObject("GameManager").AddComponent<GameManager>();
        SaveScene("MainMenu", scene);
    }

    // ===== Victory =====
    static void CreateVictory()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var canvas = CreateCanvas();

        CreateText(canvas.transform, "Você venceu!", 48, new Vector2(0, 150));
        var score = CreateText(canvas.transform, "Final Score: 0", 32, new Vector2(0, 70));

        var restart = CreateButton(canvas.transform, "Restart", new Vector2(0, -20));
        var menu = CreateButton(canvas.transform, "Menu", new Vector2(0, -100));

        var ctrl = new GameObject("VictoryMenu").AddComponent<VictoryMenu>();
        ctrl.scoreText = score.GetComponent<Text>();
        restart.GetComponent<Button>().onClick.AddListener(ctrl.RestartGame);
        menu.GetComponent<Button>().onClick.AddListener(ctrl.BackToMenu);

        SaveScene("Victory", scene);
    }

    // ===== Defeat =====
    static void CreateDefeat()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var canvas = CreateCanvas();

        CreateText(canvas.transform, "Game Over", 48, new Vector2(0, 150));
        var score = CreateText(canvas.transform, "Final Score: 0", 32, new Vector2(0, 70));

        var restart = CreateButton(canvas.transform, "Restart", new Vector2(0, -20));
        var menu = CreateButton(canvas.transform, "Menu", new Vector2(0, -100));

        var ctrl = new GameObject("DefeatMenu").AddComponent<DefeatMenu>();
        ctrl.scoreText = score.GetComponent<Text>();
        restart.GetComponent<Button>().onClick.AddListener(ctrl.RestartGame);
        menu.GetComponent<Button>().onClick.AddListener(ctrl.BackToMenu);

        SaveScene("Defeat", scene);
    }

    // ===== Level =====
    static void CreateLevel()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var cam = new GameObject("Main Camera");
        var camera = cam.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        cam.tag = "MainCamera";
        cam.transform.position = new Vector3(0, 0, -10);

        // Paddle
        var paddle = new GameObject("Paddle");
        var srP = paddle.AddComponent<SpriteRenderer>();
        srP.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/paddleRed.png");
        paddle.AddComponent<BoxCollider2D>();
        var rbP = paddle.AddComponent<Rigidbody2D>();
        rbP.bodyType = RigidbodyType2D.Kinematic;
        paddle.AddComponent<PaddleController>();
        SafeSetTag(paddle, "Paddle");
        paddle.transform.position = new Vector3(0, -4f, 0);

        // Ball
        var ball = new GameObject("Ball");
        var srB = ball.AddComponent<SpriteRenderer>();
        srB.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/ballGrey.png");
        ball.AddComponent<CircleCollider2D>();
        var rbB = ball.AddComponent<Rigidbody2D>();
        rbB.gravityScale = 0;
        rbB.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        ball.AddComponent<BallController>();
        SafeSetTag(ball, "Ball");
        ball.transform.position = new Vector3(0, -3.5f, 0);

        // Bottom
        var bottom = new GameObject("Bottom");
        var bc = bottom.AddComponent<BoxCollider2D>();
        bc.isTrigger = true;
        bottom.AddComponent<BottomTrigger>();
        SafeSetTag(bottom, "Bottom");
        bottom.transform.position = new Vector3(0, -5.5f, 0);
        bottom.transform.localScale = new Vector3(20, 1, 1);

        // Walls
        CreateWalls(camera);

        // Canvas UI
        var canvas = CreateCanvas();
        var score = CreateText(canvas.transform, "Score: 0", 24, new Vector2(-200, -30));
        score.name = "ScoreText";
        var lives = CreateText(canvas.transform, "Lives: 3", 24, new Vector2(200, -30));
        lives.name = "LivesText";
        score.GetComponent<Text>().alignment = TextAnchor.UpperLeft;
        lives.GetComponent<Text>().alignment = TextAnchor.UpperRight;

        // Blocos (grid simples)
        string[] sprites = {
            "Assets/Sprites/element_blue_rectangle.png",
            "Assets/Sprites/element_green_rectangle.png",
            "Assets/Sprites/element_yellow_rectangle.png",
            "Assets/Sprites/element_red_rectangle.png",
            "Assets/Sprites/element_purple_rectangle.png",
            "Assets/Sprites/element_grey_rectangle.png"
        };

        int rows = 5, cols = 10;
        float xOff = 1.1f, yOff = 0.6f, startY = 3.5f;
        float startX = -(cols - 1) * xOff / 2f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                var brick = new GameObject("Brick_" + r + "_" + c);
                var sr = brick.AddComponent<SpriteRenderer>();
                sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(sprites[r % sprites.Length]);
                brick.AddComponent<BoxCollider2D>();
                var comp = brick.AddComponent<Brick>();
                comp.points = 100;
                SafeSetTag(brick, "Brick");
                brick.transform.position = new Vector3(startX + c * xOff, startY - r * yOff, 0);
            }
        }

        SaveScene("Level", scene);
    }

    // ===== Walls =====
    static void CreateWalls(Camera cam)
    {
        float camSize = cam.orthographicSize;
        float aspect = 16f / 9f; // fixo 16:9 para manter proporção
        float width = camSize * aspect;

        // Left
        var left = new GameObject("LeftWall");
        left.AddComponent<BoxCollider2D>();
        left.transform.position = new Vector3(-width - 0.5f, 0, 0);
        left.transform.localScale = new Vector3(1, camSize * 2, 1);

        // Right
        var right = new GameObject("RightWall");
        right.AddComponent<BoxCollider2D>();
        right.transform.position = new Vector3(width + 0.5f, 0, 0);
        right.transform.localScale = new Vector3(1, camSize * 2, 1);

        // Top
        var top = new GameObject("TopWall");
        top.AddComponent<BoxCollider2D>();
        top.transform.position = new Vector3(0, camSize + 0.5f, 0);
        top.transform.localScale = new Vector3(width * 2, 1, 1);
    }
}
