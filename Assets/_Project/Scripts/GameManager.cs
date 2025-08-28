using UnityEngine;

public enum GameState { Title, Playing, GameOver }

public class GameManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private UIHud ui;          // Canvas に付けた UIHud をドラッグ
    [SerializeField] private FruitSpawner spawner;

    [Header("Game Rules")]
    [SerializeField] private int startLife = 3;
    [SerializeField] private float maxTime = 60f;          // 上限
    [SerializeField] private float recoverPerCatch = 0.5f; // キャッチ回復（0で無効）

    [Header("Combo Bonus")]
    [SerializeField] private int chainBonusThreshold = 3;  // 同種3連続で+1
    [SerializeField] private int chainBonusScore = 1;

    public GameState State { get; private set; } = GameState.Playing;
    public float Elapsed => elapsed;

    // 状態
    int score, best, life;
    float timeLeft, elapsed;

    // コンボ管理
    int lastType = -1;
    int chainCount = 0;

    void Start()
    {
        best = PlayerPrefs.GetInt("BEST", 0);
        StartGame();
    }

    void Update()
    {
        if (State != GameState.Playing) return;

        elapsed += Time.deltaTime;

        // 時間を減らす
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            GameOver();
            return;
        }

        // 表示は 0.1 秒刻みに丸めて“揺れ”を減らす
        float display = Mathf.Max(0f, Mathf.Floor(timeLeft * 10f) / 10f);
        ui?.UpdateHud(score, life, display);
    }

    // === API（Fruit / UI から呼ばれる） ===
    public void AddScore(int fruitType)
    {
        if (State != GameState.Playing) return;

        // 基本スコア
        int add = 1;

        // 同種コンボ
        if (fruitType == lastType) chainCount++;
        else { lastType = fruitType; chainCount = 1; }

        if (chainCount >= chainBonusThreshold) add += chainBonusScore;

        score += add;

        // 時間回復（上限でクランプ）
        if (recoverPerCatch > 0f)
        {
            timeLeft = Mathf.Min(maxTime, timeLeft + recoverPerCatch);
        }

        // HUD更新
        float display = Mathf.Max(0f, Mathf.Floor(timeLeft * 10f) / 10f);
        ui?.UpdateHud(score, life, display);
        ui?.PlayCatchEffect();

        if (score > best)
        {
            best = score;
            ui?.SetBest(best);
            PlayerPrefs.SetInt("BEST", best);
        }
    }

    public void Miss()
    {
        if (State != GameState.Playing) return;

        life--;
        ui?.PlayMissEffect();

        if (life <= 0)
        {
            life = 0;
            GameOver();
        }
        else
        {
            float display = Mathf.Max(0f, Mathf.Floor(timeLeft * 10f) / 10f);
            ui?.UpdateHud(score, life, display);
        }
    }

    public void Retry()
    {
        StartGame();
    }

    // === 内部 ===
    void StartGame()
    {
        State = GameState.Playing;
        score = 0;
        life = startLife;
        timeLeft = maxTime;
        elapsed = 0f;
        lastType = -1;
        chainCount = 0;

        ui?.SetBest(best);
        ui?.UpdateHud(score, life, maxTime);
    }

    void GameOver()
    {
        State = GameState.GameOver;
        ui?.ShowGameOver(score, best);
    }
}