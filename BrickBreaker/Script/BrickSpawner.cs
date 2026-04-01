using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrickSpawner : MonoBehaviour {
    [SerializeField] private GameObject brickPrefab; // Prefab del brick da spawnare

    [SerializeField] private Transform leftWall;  // Riferimento al muro sinistro
    [SerializeField] private Transform rightWall; // Riferimento al muro destro

    [SerializeField] private float borderMargin = 0.5f;
    // Margine extra tra i brick e i muri laterali

    // Genera i brick in base al livello.
    // Più il livello è alto → più righe/colonne vengono generate.
    // Ritorna il numero totale di brick spawnati.
    public int SpawnBricks(int level) {
        if (brickPrefab == null) {
            Debug.LogError("[BrickSpawner] Brick prefab non assegnato!");
            return 0;
        }

        int rows, cols;

        // Definizione della griglia in base al livello
        switch (level) {
            case 1:
                rows = 3; cols = 5;
                break;
            case 2:
                rows = 4; cols = 5;
                break;
            case 3:
                rows = 4; cols = 5;
                break;
            case 4:
                rows = 5; cols = 5;
                break;
            case 5:
                rows = 5; cols = 6;
                break;
            default:
                rows = 3; cols = 5;
                break;
        }

        // Dimensioni standard di un brick
        float brickWidth = 0.6f;
        float brickHeight = 0.25f;

        // Gap (spazio) desiderato tra i brick
        float xGap = 0.1f;
        float yGap = 0.1f;

        // Calcolo dei limiti orizzontali usando i muri + margine extra
        float leftLimit = leftWall.position.x + leftWall.GetComponent<BoxCollider2D>().bounds.size.x / 2f + borderMargin;
        float rightLimit = rightWall.position.x - rightWall.GetComponent<BoxCollider2D>().bounds.size.x / 2f - borderMargin;

        float totalAvailableWidth = rightLimit - leftLimit;

        // Larghezza totale della griglia (brick + gap)
        float totalGridWidth = cols * brickWidth + (cols - 1) * xGap;

        // Se la griglia è troppo larga → ridimensiona il gap per farcela stare
        if (totalGridWidth > totalAvailableWidth) {
            xGap = (totalAvailableWidth - cols * brickWidth) / (cols - 1);
            xGap = Mathf.Max(0f, xGap); // il gap non può essere negativo
            totalGridWidth = cols * brickWidth + (cols - 1) * xGap;
        }

        // Punto di partenza X centrato
        float startX = leftLimit + (totalAvailableWidth - totalGridWidth) / 2f + brickWidth / 2f;
        // Punto di partenza Y = posizione dello spawner
        float startY = transform.position.y;

        int count = 0;

        // Ciclo di spawn dei brick riga per riga
        for (int i = 0; i < rows; i++) {
            for (int j = 0; j < cols; j++) {
                float xPos = startX + j * (brickWidth + xGap);
                float yPos = startY + i * (brickHeight + yGap);
                Vector3 pos = new Vector3(xPos, yPos, 0f);

                // Instanzia il brick come figlio dello spawner
                GameObject brick = Instantiate(brickPrefab, pos, Quaternion.identity, transform);

                // Imposta scala uniforme dei brick
                brick.transform.localScale = new Vector3(brickWidth, brickHeight, 1f);

                count++;
            }
        }

        return count;
    }
}

//da tutorial
//[SerializeField] private int rows = 4, cols = 5;
//[SerializeField] private float xDistanceBetweenBricks = 1, yDistanceBetweenBricks = -0.3f;
//private void Start()
//{
//int spawnedBricks = SpawnBricks();
//GameManager.Instance.SetSpawnerBricks(spawnedBricks);
//}

//private int SpawnBricks()
//{
//if (brickPrefab == null)
//{
//Debug.LogError("Brick prefab non assegnato!");
//return 0;
//}

//int count = 0;

//for (int i = 0; i < rows; i++)
//{
//for (int j = 0; j < cols; j++)
//{
//Vector3 newBrickPosition = new Vector3(
//transform.position.x + (j * xDistanceBetweenBricks),
//transform.position.y - (i * yDistanceBetweenBricks),
//transform.position.z
//);

//Instantiate(brickPrefab, newBrickPosition, Quaternion.identity, transform);
//count++;
//}
//}

//return count;
//}
