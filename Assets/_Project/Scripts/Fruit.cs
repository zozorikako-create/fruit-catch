using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Fruit : MonoBehaviour
{
    private int typeId = 0;
    private bool consumed = false;
    private GameManager game;

    private void Start()
    {
        game = FindObjectOfType<GameManager>();
    }

    public void SetType(int id) => typeId = id;

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (consumed) return;
        if (col.collider.CompareTag("Basket"))
        {
            consumed = true;
            game?.AddScore(typeId);
            Destroy(gameObject);
        }
    }

    public void Miss()
    {
        if (consumed) return;
        consumed = true;
        game?.Miss();
        Destroy(gameObject);
    }
}