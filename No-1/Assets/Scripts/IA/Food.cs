using UnityEngine;

public class Food : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Boid"))
        {
            GameManager.Instance.foods.Remove(transform);
            Destroy(gameObject);
        }
    }
}
