using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingRock : MonoBehaviour
{
     public Rigidbody2D playerRB;

    private GameObject player;

    public float Damage;
    public float horizontalKnockbackForce;
    public float verticalKnockbackForce;

    private GaleneMovement playerMovement;
    private HealthManager health;

    private DeepBreathing deepBr;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 10f);
        player = GameObject.FindWithTag("Player");
        playerRB = player.GetComponent<Rigidbody2D>();
        
        playerMovement = player.GetComponent<GaleneMovement>();
        health = player.GetComponent<HealthManager>();
        deepBr = player.GetComponent<DeepBreathing>();
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.forward * 180f * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player") &&!deepBr.isBreathing)
        {
        playerMovement.StartKnockback(0.4f);
        health.TakeDamage(Damage); 
        
        Vector2 currentVelocity = playerRB.velocity;
        playerRB.velocity = new Vector2(0f, 0f);
         Vector2 knockbackDirection = Vector2.left;

        
        Vector2 combinedKnockback = (knockbackDirection * horizontalKnockbackForce) + (Vector2.up * verticalKnockbackForce);

        playerRB.AddForce(combinedKnockback, ForceMode2D.Impulse);

        Collider2D rockCollider = GetComponent<Collider2D>();
        if (rockCollider != null)
        {
            rockCollider.enabled = false;
        }

        

    
    }
}
}
