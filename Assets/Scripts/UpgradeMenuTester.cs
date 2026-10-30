
using UnityEngine;

public class UpgradeMenuTester : MonoBehaviour
{
    public UpgradeManager upgradeManager;

    [Header("Testing")]
    public float delay = 5f;

    private float timer;
    private bool triggered;

    void Update()
    {
        if (triggered || upgradeManager == null)
            return;

        timer += Time.deltaTime;

        if (timer >= delay)
        {
            triggered = true;
            upgradeManager.OpenUpgradeMenu();
        }
    }
}
