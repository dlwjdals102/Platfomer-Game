using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockWave : MonoBehaviour
{
    public float speed = 5f; // Speed of the shockwave
    public float lifeTime = 2f; // Lifetime of the shockwave
    public int damage = 2; // Damage caused by the shockwave

    public Vector2 direction; // Movement direction of the shockwave

    // Start is called before the first frame update
    void Start()
    {
        // Automatically destroy the shockwave after its lifetime
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        // Move the shockwave in the specified direction
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the shockwave hits the player
        if (collision.CompareTag("Player"))
        {
            // If the player is hit, deal damage
            PlayerHurt playerHealth = collision.GetComponent<PlayerHurt>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // Optionally destroy the shockwave upon impact
            Destroy(gameObject);
        }
    }
}
