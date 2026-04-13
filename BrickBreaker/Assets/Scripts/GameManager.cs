using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // Per caricare e scaricare scene
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; } // Singleton

    [Header("UI Panels")]
    [SerializeField]
    private GameObject pressAnyKeyPanel, gameOverPanel, levelCompletePanel, finalWinPanel;

    [Header("Scene")]
    [SerializeField]
    private string gameSceneName = "GameScene"; // Nome della scena di gioco

    // Riferimenti
    private Rigidbody2D ball;
    private GameObject bar;
    private BrickSpawner spawner;

    // Stati di gioco
    private bool gameStarted = false; // Indica se la palla è stata lanciata
    private bool gameOver = false;    // Indica se il round è terminato
    private bool isGameOverByDeath = false; // Distinguere morte da completamento livello
    private bool isFinalWin = false;        // True solo se hai completato tutti i livelli

    // Progressione livelli
    private int spawnerBricks = 0;   // Numero mattoni rimasti
    private int currentLevel = 1;    // Livello corrente
    private const int maxLevels = 5; // Numero massimo di livelli
    private int score = 0;           // Punteggio totale

    [Header("FX")]
    [SerializeField]
    private GameObject explosionEffect; // Effetto esplosione quando la barra muore

    [Header("UI Text (Level Complete)")]
    [SerializeField] private TMPro.TMP_Text nextLevelText;
    [SerializeField] private TMPro.TMP_Text scoreText;
    [SerializeField] private TMPro.TMP_Text finalScoreText;

    private PlayerControls controls;
    private bool submitPressed;

    private void Awake() {
        // Implementazione del pattern Singleton:
        // ci deve essere solo un GameManager in tutta l'applicazione.
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Mantiene il GameManager quando cambio scena
        }
        else {
            Destroy(gameObject); // Evita duplicati
        }

        controls = new PlayerControls();
        controls.UI.Submit.performed += _ =>
        {
            submitPressed = true;
        };
    }

    private void OnEnable() {
        if (controls != null)
            controls.Enable();
    }

    private void OnDisable() {
        if (controls != null)
            controls.Disable();
    }

    private void Start() {
        // Inizializza il gioco caricando la scena di gioco
        ResetGameScene();
    }

    private void Update() {
        if (!submitPressed)
            return;

        submitPressed = false; // reset (simula GetKeyDown)

        // 1. Restart dopo GameOver
        if (gameOver) {
            HideAllPanels();

            if (isFinalWin) {
                currentLevel = 1;
                score = 0;
            } else if (isGameOverByDeath) {
                score = 0;
            }

            isGameOverByDeath = false;
            isFinalWin = false;
            gameOver = false;

            ResetGameScene();
        }
        
        // 2. Avvio partita
        else if (!gameStarted && ball != null) {
            gameStarted = true;

            if (pressAnyKeyPanel != null)
                pressAnyKeyPanel.SetActive(false);

            ball.AddForce(Vector2.up);
        }
    }


    // Disattiva tutti i pannelli UI (utile per reset scena o cambi stato).
    private void HideAllPanels() {
        if (pressAnyKeyPanel) pressAnyKeyPanel.SetActive(false);
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (levelCompletePanel) levelCompletePanel.SetActive(false);
        if (finalWinPanel) finalWinPanel.SetActive(false);
    }


    // Resetta lo stato del gioco ricaricando la scena principale.
    void ResetGameScene() {
        StartCoroutine(LoadGameSceneAsync());
    }


    // Carica la scena di gioco in modo asincrono e inizializza i riferimenti.
    IEnumerator LoadGameSceneAsync() {
        HideAllPanels();

        // Se la scena è già caricata → scaricala prima
        if (SceneManager.GetSceneByName(gameSceneName).isLoaded)
            yield return SceneManager.UnloadSceneAsync(gameSceneName);

        // Carica scena in modalità Additive (non distrugge GameManager)
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(gameSceneName, LoadSceneMode.Additive);
        yield return new WaitUntil(() => asyncLoad.isDone);

        yield return null; // Attendo un frame per sicurezza

        // Recupera riferimenti agli oggetti della scena
        ball = GameObject.FindGameObjectWithTag("ball")?.GetComponent<Rigidbody2D>();
        bar = GameObject.FindGameObjectWithTag("bar");

        // Spawna i mattoni in base al livello corrente
        spawner = Object.FindFirstObjectByType<BrickSpawner>();
        if (spawner != null) {
            int spawned = spawner.SpawnBricks(currentLevel);
            SetSpawnerBricks(spawned);
        }

        // Stato iniziale
        gameStarted = false;
        gameOver = false;

        // Mostra il pannello "Premi un tasto per iniziare"
        if (pressAnyKeyPanel) pressAnyKeyPanel.SetActive(true);
    }


    // Chiamato quando il giocatore muore (la palla cade).
    public void GameOver() {
        if (bar == null) {
            bar = GameObject.FindGameObjectWithTag("bar");
        }

        if (bar != null) {
            // Effetto esplosione sulla barra
            Instantiate(explosionEffect, bar.transform.position, Quaternion.identity);

            // Nasconde la barra
            var sr = bar.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }

        gameOver = true;
        isGameOverByDeath = true; // Segna che è GameOver per morte

        if (gameOverPanel) gameOverPanel.SetActive(true);
    }

    // Chiamato quando tutti i mattoni di un livello vengono distrutti.
    // Gestisce sia il passaggio di livello sia la vittoria finale.
    private void GameWon() {
        if (ball != null)
            Destroy(ball.gameObject); // Rimuove la palla per evitare rimbalzi extra

        if (currentLevel < maxLevels) {
            // Livello completato → avanza
            gameOver = true;
            isFinalWin = false;

            if (levelCompletePanel) {
                levelCompletePanel.SetActive(true);

                if (nextLevelText != null)
                    nextLevelText.text = $"Prossimo livello: {currentLevel + 1}";

                if (scoreText != null)
                    scoreText.text = $"Punteggio: {score}";
            }

            // Dopo 4 secondi carica il livello successivo
            StartCoroutine(NextLevelAfterDelay(4f));
        }
        else {
            // Vittoria finale
            gameOver = true;
            isFinalWin = true;

            if (finalWinPanel)
                finalWinPanel.SetActive(true);

            if (finalScoreText != null)
                finalScoreText.text = $"Punteggio finale: {score}";

            // Non carichiamo subito, aspettiamo che il giocatore prema spazio
        }
    }

    // Dopo un certo delay, avanza al livello successivo.
    private IEnumerator NextLevelAfterDelay(float seconds) {
        yield return new WaitForSeconds(seconds);

        HideAllPanels();

        currentLevel++;
        ResetGameScene();
    }

    // Gestione mattoni e punteggio 

    public void SetSpawnerBricks(int value) => spawnerBricks = value;

    public void BrickDestroyed() {
        spawnerBricks--;

        if (spawnerBricks <= 0) {
            GameWon();
        }
    }

    public void ResetProgress() {
        currentLevel = 1;
        ResetGameScene();
    }

    public void AddScore(int value) {
        score += value;
        if (scoreText != null) scoreText.text = $"Score: {score}";
    }
}

