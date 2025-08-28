using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameManager game;           // GameRoot(=GameManager) ���h���b�O
    [SerializeField] private GameObject[] fruitPrefabs;  // Fruit_Apple/Banana/Grape �� Prefab ������

    [Header("Spawn Timing")]
    [Tooltip("�ŏ��͂������i�b�j")]
    [SerializeField] private float startInterval = 1.0f;
    [Tooltip("�ŒZ�i�b�j")]
    [SerializeField] private float minInterval = 0.6f;

    [Header("Difficulty (Gravity)")]
    [Tooltip("��b�̏d�́iRigidbody2D.gravityScale �ɐݒ�j")]
    [SerializeField] private float baseGravity = 1.0f;

    [Header("Spawn Position")]
    [Tooltip("��ʏ�[�����Y�I�t�Z�b�g")]
    [SerializeField] private float spawnYOffset = 1.0f;
    [Tooltip("���E�[����̈��S�}�[�W��")]
    [SerializeField] private float edgePadding = 0.5f;

    [Header("Debug")]
    [Tooltip("�f�o�b�O�p�F���Ԋu�iDebug Interval�j�ŌŒ�X�|�[��")]
    [SerializeField] private bool useFixedInterval = false;
    [Tooltip("Use Fixed Interval �� ON �̂Ƃ��̊Ԋu")]
    [SerializeField] private float debugInterval = 0.2f;
    [Tooltip("�ڍ׃��O���o��")]
    [SerializeField] private bool logVerbose = true;

    private float timer;
    private Camera cachedCam;

    private void Awake()
    {
        // �J�������L���b�V���iTag=MainCamera �𐄏��j
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
        // �Q�[����ԁFPlaying�ȊO�͒�~
        if (game != null && game.State != GameState.Playing) return;

        // Prefab��������Ή������Ȃ�
        if (fruitPrefabs == null || fruitPrefabs.Length == 0) return;

        if (logVerbose && (Time.frameCount % 60 == 0))
            Debug.Log("[Spawner] update t=" + Time.time.ToString("F1"));

        timer += Time.deltaTime;

        // �d�l�ǂ���F���Ԍo�߂� 1.0s �� 0.6s �ɒZ�k�i60�b��z��j
        float interval;
        if (useFixedInterval)
        {
            interval = Mathf.Max(0.01f, debugInterval);
        }
        else
        {
            float elapsed = (game != null) ? game.Elapsed : 0f; // �o�ߕb
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

        // ���ID�� Fruit �ɒʒm
        var fruit = go.GetComponent<Fruit>();
        if (fruit != null) fruit.SetType(idx);

        // �d�̓J�[�u�ݒ�i20�b���Ƃ� +0.2�A�ő�1.4�j
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
        float halfW = ortho * 0.5625f; // 1080x1920 �̑z���i�t�H�[���o�b�N�j
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
    // Scene�r���[�őI�����A�X�|�[�����C��������
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