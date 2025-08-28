using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager game;           // GameRoot(=GameManager) をドラッグ
    [SerializeField] private GameObject[] fruitPrefabs;  // Fruit_Apple/Banana/Grape の Prefab を入れる

    [Header("Spawn Timing")]
    [Tooltip("最初はゆっくり（秒）")]
    [SerializeField] private float startInterval = 1.0f;
    [Tooltip("最短（秒）")]
    [SerializeField] private float minInterval = 0.6f;

    [Header("Difficulty (Gravity)")]
    [Tooltip("基礎の重力（Rigidbody2D.gravityScale に設定）")]
    [SerializeField] private float baseGravity = 1.0f;

    [Header("Spawn Position")]
    [Tooltip("画面上端からのYオフセット")]
    [SerializeField] private float spawnYOffset = 1.0f;
    [Tooltip("左右端からの安全マージン")]
    [SerializeField] private float edgePadding = 0.5f;

    [Header("Debug")]
    [Tooltip("デバッグ用：一定間隔（Debug Interval）で固定スポーン")]
    [SerializeField] private bool useFixedInterval = false;
    [Tooltip("Use Fixed Interval が ON のときの間隔")]
    [SerializeField] private float debugInterval = 0.2f;
    [Tooltip("詳細ログを出す")]
    [SerializeField] private bool logVerbose = true;

    private float timer;
    private Camera cachedCam;

    private void Awake()
    {
        // カメラをキャッシュ（Tag=MainCamera を推奨）
        cachedCam = Camera.main;
        if (cachedCam == null)
        {
            cachedCam = FindObjectOfType<Camera>();
            if (logVerbose) Debug.LogWarning("[Spawner] Camera.main not found. Use first Camera: " +
                                             (cachedCam ? cachedCam.name : "null"));
        }
        if (logVerbose) Debug.Log("[Spawner] Awake. cam=" + (cachedCam ? cachedCam.name : "null"));
    }

    private void OnEnable()
    {
        timer = 0f;
        if (logVerbose) Debug.Log("[Spawner] OnEnable");
    }

    private void Update()
    {
        // ゲーム状態：Playing以外は停止
        if (game != null && game.State != GameState.Playing) return;

        // Prefabが無ければ何もしない
        if (fruitPrefabs == null || fruitPrefabs.Length == 0) return;

        if (logVerbose && (Time.frameCount % 60 == 0))
            Debug.Log("[Spawner] update t=" + Time.time.ToString("F1"));

        timer += Time.deltaTime;

        // 仕様どおり：時間経過で 1.0s → 0.6s に短縮（60秒を想定）
        float interval;
        if (useFixedInterval)
        {
            interval = Mathf.Max(0.01f, debugInterval);
        }
        else
        {
            float elapsed = (game != null) ? game.Elapsed : 0f; // 経過秒
            float t = Mathf.Clamp01(elapsed / 60f);
            interval = Mathf.Lerp(startInterval, minInterval, t);
        }

        if (timer >= interval)
        {
            timer = 0f;
            SpawnOne();
        }
    }

    private void SpawnOne()
    {
        if (logVerbose) Debug.Log("[Spawner] spawn!");

        Vector3 pos = GetSpawnPosition();

        int idx = Random.Range(0, fruitPrefabs.Length);
        GameObject prefab = fruitPrefabs[idx];
        if (prefab == null)
        {
            if (logVerbose) Debug.LogWarning("[Spawner] fruitPrefabs[" + idx + "] is null.");
            return;
        }

        GameObject go = Instantiate(prefab, pos, Quaternion.identity);

        // 種類IDを Fruit に通知
        var fruit = go.GetComponent<Fruit>();
        if (fruit != null) fruit.SetType(idx);

        // 重力カーブ設定（20秒ごとに +0.2、最大1.4）
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float add = 0f;
            if (game != null) add = 0.2f * Mathf.Floor(game.Elapsed / 20f);
            rb.gravityScale = Mathf.Clamp(baseGravity + add, baseGravity, 1.4f);
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private Vector3 GetSpawnPosition()
    {
        float ortho = 5.5f;
        float halfW = ortho * 0.5625f; // 1080x1920 の想定比（フォールバック）
        float y = ortho + spawnYOffset;

        if (cachedCam != null)
        {
            ortho = cachedCam.orthographicSize;
            halfW = ortho * cachedCam.aspect;
            y = ortho + spawnYOffset;
        }

        float x = Random.Range(-halfW + edgePadding, halfW - edgePadding);
        return new Vector3(x, y, 0f);
    }

#if UNITY_EDITOR
    // Sceneビューで選択時、スポーンラインを可視化
    private void OnDrawGizmosSelected()
    {
        var cam = Camera.main;
        if (cam == null) return;
        float ortho = cam.orthographicSize;
        float halfW = ortho * cam.aspect;
        Vector3 a = new Vector3(-halfW + edgePadding, ortho + spawnYOffset, 0f);
        Vector3 b = new Vector3(halfW - edgePadding, ortho + spawnYOffset, 0f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawSphere(a, 0.05f);
        Gizmos.DrawSphere(b, 0.05f);
    }
#endif
}