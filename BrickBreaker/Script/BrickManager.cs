using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickManager : MonoBehaviour {
    [Header("Colori per vite")]
    [SerializeField]
    private Color oneLifeColor, twoLifeColor, threeLifeColor;
    // Colori del brick in base al numero di vite rimaste

    [Header("Vita e Punteggio")]
    [SerializeField]
    private int hitPoints = 3;       // Numero di colpi che il brick può subire
    [SerializeField]
    private int scoreValue = 100;    // Punti assegnati quando il brick viene distrutto

    private SpriteRenderer sprite;   // Per cambiare il colore del brick

    // Per evitare colpi multipli nello stesso frame
    private int lastFrameHit = -1;

    [Header("FX")]
    [SerializeField]
    private AudioSource brickHitSound;     // Suono quando il brick viene colpito
    [SerializeField]
    private ParticleSystem brickHitParticles; // Particelle quando la palla colpisce

    // Flag per evitare che la stessa collisione venga contata più volte
    private bool alreadyHit = false;

    private void Start() {
        sprite = GetComponent<SpriteRenderer>();
        ChangeColorOnLife(); // Imposta colore iniziale in base alle vite
    }


    // Gestisce l’impatto della palla con il brick:
    // - Riduce le vite
    // - Cambia colore
    // - Riproduce effetti sonori/visivi
    // - Se le vite finiscono, distrugge il brick e notifica il GameManager

    private void OnCollisionEnter2D(Collision2D collision) {
        if (!collision.gameObject.CompareTag("ball")) return;

        if (alreadyHit) return;   // evita doppio conteggio nella stessa collisione
        alreadyHit = true;

        // Se è già stato colpito in questo frame → ignora
        if (Time.frameCount == lastFrameHit) return;
        lastFrameHit = Time.frameCount;

        // ↓↓↓ Logica del danno ↓↓↓
        hitPoints = Mathf.Max(0, hitPoints - 1);
        ChangeColorOnLife();

        // Suono di impatto
        if (brickHitSound != null) brickHitSound.Play();

        // Particelle di impatto (orientate nella direzione della palla)
        if (brickHitParticles != null && collision.contacts.Length > 0) {
            Vector3 collisionPoint = collision.contacts[0].point;
            Vector3 ballDir = collision.gameObject.transform.position - collisionPoint;

            brickHitParticles.transform.position = collisionPoint;
            brickHitParticles.transform.rotation = Quaternion.FromToRotation(Vector3.up, ballDir.normalized);
            brickHitParticles.Play();
        }

        // Se il brick non ha più vite → distruggi
        if (hitPoints <= 0) {
            GameManager.Instance.BrickDestroyed();
            Destroy(gameObject);
        }

        // Aggiungi punteggio al GameManager
        GameManager.Instance.AddScore(scoreValue);
    }


    // Reset del flag "alreadyHit" quando la palla si stacca dal brick.
    private void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("ball"))
            alreadyHit = false;
    }


    // Aggiorna il colore del brick e delle particelle in base alle vite rimaste.
    private void ChangeColorOnLife() {
        if (hitPoints >= 3) sprite.color = threeLifeColor;
        else if (hitPoints == 2) sprite.color = twoLifeColor;
        else if (hitPoints == 1) sprite.color = oneLifeColor;

        if (brickHitParticles != null) {
            var main = brickHitParticles.main;
            main.startColor = new ParticleSystem.MinMaxGradient(sprite.color);
        }
    }
}



