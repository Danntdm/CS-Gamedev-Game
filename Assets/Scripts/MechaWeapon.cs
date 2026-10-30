
using UnityEngine;
using UnityEngine.InputSystem;

public class MechaWeapon : MonoBehaviour
{
    [Header("Weapon")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.15f;

    [Header("Damage")]
    public float bulletDamage = 10f;
    public float damageUpgradeMultiplier = 1.2f;

    [Header("Rapid Fire")]
    public float fireSpeedUpgradeMultiplier = 1.15f;
    public float minimumFireInterval = 0.03f;

    [Header("Aiming")]
    public LayerMask groundLayer = ~0;
    public float maxAimDistance = 500f;

    private float nextFireTime;
    private MechaAnimator mechaAnimator;
    private Camera mainCamera;

    void Awake()
    {
        mechaAnimator = GetComponent<MechaAnimator>();
        mainCamera = Camera.main;
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

    public void UpgradeDamage()
    {
        bulletDamage *= damageUpgradeMultiplier;

        Debug.Log("MECHA DAMAGE UPGRADED! New damage: " + bulletDamage);
    }

    public void UpgradeFireRate()
    {
        fireRate = Mathf.Max(
            minimumFireInterval,
            fireRate / fireSpeedUpgradeMultiplier
        );

        Debug.Log("RAPID FIRE UPGRADED! New fire interval: " + fireRate);
    }

    void Fire()
    {
        if (bulletPrefab == null || firePoint == null)
            return;

        Vector3 targetPoint = GetMouseTarget();

        Vector3 direction = targetPoint - firePoint.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = transform.forward;
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        Quaternion bulletRotation =
            Quaternion.LookRotation(direction, Vector3.up);

        GameObject bullet = Instantiate(
            bulletPrefab,
            firePoint.position,
            bulletRotation
        );

        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
            bulletScript.SetDamage(bulletDamage);

        IgnoreMechaCollision(bullet);

        if (mechaAnimator != null)
            mechaAnimator.PlayShootAnimation();

        nextFireTime = Time.time + fireRate;
    }

    Vector3 GetMouseTarget()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null || Mouse.current == null)
            return firePoint.position + transform.forward * 30f;

        Ray ray = mainCamera.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            maxAimDistance,
            ~0,
            QueryTriggerInteraction.Collide
        );

        System.Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance)
        );

        foreach (RaycastHit hit in hits)
        {
            EnemyController enemy =
                hit.collider.GetComponentInParent<EnemyController>();

            if (enemy != null)
                return hit.collider.bounds.center;
        }

        if (Physics.Raycast(
            ray,
            out RaycastHit groundHit,
            maxAimDistance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        ))
        {
            return groundHit.point;
        }

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float distance))
            return ray.GetPoint(distance);

        return firePoint.position + transform.forward * 30f;
    }

    void IgnoreMechaCollision(GameObject bullet)
    {
        Collider[] bulletColliders =
            bullet.GetComponentsInChildren<Collider>();

        Collider[] mechaColliders =
            GetComponentsInChildren<Collider>();

        foreach (Collider bulletCollider in bulletColliders)
        {
            foreach (Collider mechaCollider in mechaColliders)
            {
                Physics.IgnoreCollision(
                    bulletCollider,
                    mechaCollider
                );
            }
        }
    }
}
