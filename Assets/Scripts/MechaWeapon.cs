
using UnityEngine;
using UnityEngine.InputSystem;

public class MechaWeapon : MonoBehaviour
{
    [Header("Weapon")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.15f;

    private float nextFireTime;
    private MechaAnimator mechaAnimator;

    void Awake()
    {
        mechaAnimator = GetComponent<MechaAnimator>();
    }

    void Update()
    {
        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.isPressed &&
            Time.time >= nextFireTime)
        {
            Fire();
        }
    }

    void Fire()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            firePoint.rotation
        );

        Collider bulletCollider = bullet.GetComponent<Collider>();

        if (bulletCollider != null)
        {
            Collider[] mechaColliders =
                GetComponentsInChildren<Collider>();

            foreach (Collider mechaCollider in mechaColliders)
            {
                Physics.IgnoreCollision(
                    bulletCollider,
                    mechaCollider
                );
            }
        }

        if (mechaAnimator != null)
            mechaAnimator.PlayShootAnimation();

        nextFireTime = Time.time + fireRate;
    }
}
