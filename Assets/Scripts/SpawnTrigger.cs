
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnTrigger : MonoBehaviour
{
    [Header("Enemy Spawner")]
    public MonoBehaviour enemySpawner;

    [Header("Settings")]
    public Key startKey = Key.P;

    private bool hasStarted = false;

    void Start()
    {
        if (enemySpawner != null)
        {
            enemySpawner.enabled = false;
        }
    }

    void Update()
    {
        if (hasStarted || enemySpawner == null)
            return;

        if (Keyboard.current != null &&
            Keyboard.current[startKey].wasPressedThisFrame)
        {
            hasStarted = true;

            enemySpawner.enabled = true;

            Debug.Log("Enemy spawning started!");
        }
    }
}
