using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BasketController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float edgePadding = 0.2f;

    private Rigidbody2D rb;
    private float minX, maxX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        float halfW = Camera.main.orthographicSize * Camera.main.aspect;
        float halfBasket = GetComponent<Collider2D>().bounds.extents.x;
        minX = -halfW + halfBasket + edgePadding;
        maxX = halfW - halfBasket + -edgePadding;
    }

    private void Update()
    {
        float dir = 0f;

#if UNITY_EDITOR || UNITY_STANDALONE
        dir = Input.GetAxisRaw("Horizontal");
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
#endif

        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began || t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
            {
                float mid = Screen.width * 0.5f;
                dir = (t.position.x < mid) ? -1f : 1f;
            }
        }

        Vector3 p = transform.position;
        p.x += dir * moveSpeed * Time.deltaTime;
        p.x = Mathf.Clamp(p.x, minX, maxX);

        if (rb.bodyType == RigidbodyType2D.Kinematic) rb.MovePosition(p);
        else transform.position = p;
    }
}