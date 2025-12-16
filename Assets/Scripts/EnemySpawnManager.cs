using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Enemy")]
    public GameObject enemyPrefab;
    public float respawnDelay = 2f;

    [Header("Puntos de Spawn (arrástralos aquí)")]
    public Transform[] spawnPoints;

    private GameObject currentEnemy;

    void Start()
    {
        SpawnEnemy();
    }

    public void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawnManager: Falta asignar enemyPrefab.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("EnemySpawnManager: No hay spawnPoints asignados.");
            return;
        }

        int index = Random.Range(0, spawnPoints.Length);
        Vector3 pos = spawnPoints[index].position;

        currentEnemy = Instantiate(enemyPrefab, pos, Quaternion.identity);

        EnemyController ec = currentEnemy.GetComponent<EnemyController>();
        if (ec != null)
        {
            ec.spawnManager = this;
        }
        else
        {
            Debug.LogWarning("EnemySpawnManager: El prefab no tiene EnemyController.");
        }
    }

    public void EnemyDied()
    {
        Invoke(nameof(SpawnEnemy), respawnDelay);
    }
}
