using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour {
    private Rigidbody2D body;

    [SerializeField]
    private float ballSpeed = 10f; // Velocità costante della palla

    [SerializeField]
    private AudioSource ballSound, deathSound; // Suoni (rimbalzo e morte)

    // Start viene chiamato una volta sola all’avvio dello script
    void Start() {
        // Mi salvo il riferimento al Rigidbody2D della palla
        body = GetComponent<Rigidbody2D>();

        // Se volessi prendere un singolo AudioSource presente nello stesso GameObject:
        // ballSound = GetComponent<AudioSource>();
    }

    // FixedUpdate viene chiamato ad intervalli regolari legati alla fisica
    void FixedUpdate() {
        keepCostantVelocity();
    }

    // Mantiene la velocità della palla costante:
    // - Se la palla è in movimento, normalizzo la direzione
    // - Riapplico la velocità fissa (ballSpeed)
    // Questo evita che la palla rallenti o acceleri in modo anomalo dopo i rimbalzi.
    private void keepCostantVelocity() {
        if (body.linearVelocity != Vector2.zero) {
            body.linearVelocity = ballSpeed * body.linearVelocity.normalized;
        }
    }


    // Gestione collisioni con oggetti solidi.
    // Se la palla tocca qualcosa che NON è un "brick",
    // viene riprodotto il suono del rimbalzo (ballSound).
    private void OnCollisionEnter2D(Collision2D collision) {
        if (!collision.gameObject.CompareTag("brick") && ballSound != null) {
            ballSound.Play();
        }
    }

    // Gestione dei trigger (zone speciali).
    // Se la palla entra in un oggetto con tag "death":
    // - viene chiamato il GameOver nel GameManager
    // - viene riprodotto il suono di morte (deathSound).
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("death")) {
            GameManager.Instance.GameOver();
            if (deathSound != null) deathSound.Play();
        }
    }
}
