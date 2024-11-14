using System.Collections.Generic;
using UnityEngine;

public class ProjectileTurret : MonoBehaviour
{
    [SerializeField] float projectileSpeed = 1f;
    [SerializeField] Vector3 gravity = new Vector3(0, -9.8f, 0);
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3f;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;
    [SerializeField] bool useLowAngle = true;
    [SerializeField] int trajectoryResolution = 30; // Number of points for trajectory preview

    private List<Vector3> points = new List<Vector3>();

    void Update()
    {
        TrackMouse();
        TurnBase();
        RotateGun();
        PreviewTrajectory();

        if (Input.GetButtonDown("Fire1"))
        {
            Fire();
        }
    }

    void Fire()
    {
        // Instantiate and fire the projectile
        GameObject projectile = Instantiate(projectilePrefab, barrelEnd.position, gun.transform.rotation);
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = projectileSpeed * barrelEnd.transform.forward;
        }
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(cameraRay, out RaycastHit hit, 1000f, targetLayer))
        {
            // Position the crosshair at the mouse target
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, 0, directionToTarget.z));
        turretBase.rotation = Quaternion.Slerp(turretBase.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }

    void RotateGun()
    {
        float? angle = CalculateTrajectory(crosshair.transform.position, useLowAngle);
        if (angle.HasValue)
        {
            // Set gun's local rotation to match the calculated angle
            gun.transform.localEulerAngles = new Vector3(-(float)angle.Value, 0, 0);
        }
    }

    float? CalculateTrajectory(Vector3 target, bool useLow)
    {
        // Direction from the barrel to the target
        Vector3 targetDir = target - barrelEnd.position;

        float y = targetDir.y;  // Vertical difference
        targetDir.y = 0;        // Ignore vertical for horizontal distance
        float x = targetDir.magnitude; // Horizontal distance

        float v = projectileSpeed; // Initial speed
        float g = -gravity.y;      // Gravitational acceleration (positive value)

        float v2 = Mathf.Pow(v, 2);
        float underRoot = Mathf.Pow(v, 4) - g * (g * Mathf.Pow(x, 2) + 2 * y * v2);

        if (underRoot >= 0)
        {
            float root = Mathf.Sqrt(underRoot);
            float angleHigh = Mathf.Atan((v2 + root) / (g * x)) * Mathf.Rad2Deg;
            float angleLow = Mathf.Atan((v2 - root) / (g * x)) * Mathf.Rad2Deg;

            return useLow ? angleLow : angleHigh;
        }
        else
        {
            return null; // No valid trajectory
        }
    }

    void PreviewTrajectory()
    {
        points.Clear();

        // Get the initial velocity based on the barrel's direction
        Vector3 initialVelocity = barrelEnd.forward * projectileSpeed;
        Vector3 currentPosition = barrelEnd.position;
        Vector3 currentVelocity = initialVelocity;

        for (int i = 0; i < trajectoryResolution; i++)
        {
            points.Add(currentPosition);

            // Predict the next position and velocity
            Vector3 nextVelocity = currentVelocity + gravity * Time.fixedDeltaTime;
            Vector3 nextPosition = currentPosition + currentVelocity * Time.fixedDeltaTime;

            // Check for collisions
            if (Physics.Raycast(currentPosition, nextPosition - currentPosition, out RaycastHit hit,
                                (nextPosition - currentPosition).magnitude, targetLayer))
            {
                points.Add(hit.point); // Stop trajectory at collision point
                break;
            }

            // Update position and velocity for the next iteration
            currentVelocity = nextVelocity;
            currentPosition = nextPosition;
        }

        // Update the LineRenderer
        line.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++)
        {
            line.SetPosition(i, points[i]);
        }
    }
}
