using System.Collections;
using System.Collections.Generic; // Contiene strutture generiche come List, Dictionary ecc.
using UnityEngine;
using UnityEngine.InputSystem; // New Input System

public class BarController : MonoBehaviour {
    [Header("Movement Settings")]

    [SerializeField]
    private float movementSpeed = 5f;
    // Velocità con cui la barra si muove

    [SerializeField]
    private float minX = -2.2f, maxX = 2.2f;
    // Limiti orizzontali del movimento

    // Riferimento all'Input Actions generato da Unity
    private PlayerControls controls;

    // Valore orizzontale dell'input (-1 sinistra, 0 fermo, 1 destra)
    private float moveInput;

    // Awake viene chiamato prima di Start
    private void Awake() {
        // Inizializza il sistema di controlli
        controls = new PlayerControls();

        // Quando l'input viene premuto o cambia valore
        controls.Player.Move.performed += ctx => {
            // Legge il Vector2 (X = sinistra/destra, Y = su/giù)
            moveInput = ctx.ReadValue<Vector2>().x;
        };

        // Quando l'input viene rilasciato
        controls.Player.Move.canceled += ctx => {
            moveInput = 0f;
        };
    }

    // Abilita i controlli quando l'oggetto è attivo
    private void OnEnable() {
        controls.Enable();
    }

    // Disabilita i controlli quando l'oggetto è disattivato
    private void OnDisable() {
        controls.Disable();
    }

    // Update viene chiamato ogni frame
    private void Update() {
        MoveBar();
    }

    // Gestisce lo spostamento della barra
    private void MoveBar() {
        // Calcola la nuova posizione sull'asse X
        Vector3 newPosition = transform.position +
                              Vector3.right * moveInput * movementSpeed * Time.deltaTime;

        // Impedisce alla barra di uscire dai limiti
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);

        // Applica la nuova posizione
        transform.position = newPosition;
    }
}
