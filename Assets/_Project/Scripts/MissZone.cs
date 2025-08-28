using UnityEngine;

public class MissZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        var fruit = other.GetComponent<Fruit>();
        if (fruit != null)
        {
            fruit.Miss();
        }
    }
}