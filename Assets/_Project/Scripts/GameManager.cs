using UnityEngine;

public enum GameState { Title, Playing, GameOver }

public class GameManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private UIHud ui;          // Canvas �ɕt���� UIHud ���h���b�O
    [SerializeField] private FruitSpawner spawner;

    [Header("Game Rules")]
    [SerializeField] private int startLife = 3;
    [SerializeField] private float maxTime = 60f;          // ���
    [SerializeField] private float recoverPerCatch = 0.5f; // �L���b�`�񕜁i0�Ŗ����j

    [Header("Combo Bonus")]
    [SerializeField] private int chainBonusThreshold = 3;  // ����3�A����+1
    [SerializeField] private int chainBonusScore = 1;

    public GameState State { get; private set; } = GameState.Playing;
    public float Elapsed => elapsed;

    // ���
    int score, best, life;
    float timeLeft, elapsed;

    // �R���{�Ǘ�
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

        // ���Ԃ����炷
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            GameOver();
            return;
        }

        // �\���� 0.1 �b���݂Ɋۂ߂āg�h��h�����炷
        float display = Mathf.Max(0f, Mathf.Floor(timeLeft * 10f) / 10f);
        ui?.UpdateHud(score, life, display);
    }

    // === API�iFruit / UI ����Ă΂��j ===
    public void AddScore(int fruitType)
    {
        if (State != GameState.Playing) return;

        // ��{�X�R�A
        int add = 1;

        // ����R���{
        if (fruitType == lastType) chainCount++;
        else { lastType = fruitType; chainCount = 1; }

        if (chainCount >= chainBonusThreshold) add += chainBonusScore;

        score += add;

        // ���ԉ񕜁i����ŃN�����v�j
        if (recoverPerCatch > 0f)
        {
            timeLeft = Mathf.Min(maxTime, timeLeft + recoverPerCatch);
        }

        // HUD�X�V
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

    // === ���� ===
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