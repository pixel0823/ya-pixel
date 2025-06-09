// using UnityEditor.Build.Content;
// using UnityEngine;

// public class EnemyGenerator : MonoBehaviour
// {
//     public GameObject enemyPrefab;
//     public Transform[] spawnPoints;

//     private int spawnedEnemies = 0;
//     private int defeatedEnemies = 0;
//     private int enemiesPerStage = 10;

//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {

//     }

//     void SpawnEnemies()
//     {
//         for (int i = 0; i < enemiesPerStage; i++)
//         {
//             Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
//             GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
//             enemy.GetComponent<EnemyMove>().generator = this;
//             spawnedEnemies++;
//         }
//     }

//     public void OnEnemyDefeated()
//     {
//         defeatedEnemies++;
//         if (defeatedEnemies >= enemiesPerStage)
//         {
//             // LoadBossScene();
//         }
//     }
//     // Update is called once per frame
//     void Update()
//     {

//     }
// }
