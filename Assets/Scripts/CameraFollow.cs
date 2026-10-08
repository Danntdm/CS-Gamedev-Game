
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Camera Offsets")]
    public Vector3 mechaOffset = new Vector3(0f, 18f, -10f);
    public Vector3 pilotOffset = new Vector3(0f, 9f, -5f);

    [Header("Camera Settings")]
    public float followSpeed = 8f;
    public float zoomSpeed = 5f;

    [Header("Pilot Detection")]
    public string pilotObjectName = "Pilot";

    private Vector3 currentOffset;

    void Start()
    {
        currentOffset = mechaOffset;

        if (target != null)
            transform.position = target.position + currentOffset;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        bool isPilot =
            target.name == pilotObjectName ||
            target.CompareTag("Pilot");

        Vector3 desiredOffset =
            isPilot ? pilotOffset : mechaOffset;

        currentOffset = Vector3.Lerp(
            currentOffset,
            desiredOffset,
            zoomSpeed * Time.deltaTime
        );

        Vector3 desiredPosition =
            target.position + currentOffset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );
    }
}
