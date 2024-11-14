using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTurret : MonoBehaviour
{
    [SerializeField] LayerMask targetLayer;
    [SerializeField] GameObject crosshair;
    [SerializeField] float baseTurnSpeed = 3;
    [SerializeField] GameObject gun;
    [SerializeField] Transform turretBase;
    [SerializeField] Transform barrelEnd;
    [SerializeField] LineRenderer line;
    [SerializeField] int maxBounces = 5; // Maximum number of bounces

    List<Vector3> laserPoints = new List<Vector3>();

    void Update()
    {
        TrackMouse();
        TurnBase();
        DrawLaserWithBounces();
    }

    void TrackMouse()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(cameraRay, out RaycastHit hit, 1000, targetLayer))
        {
            crosshair.transform.forward = hit.normal;
            crosshair.transform.position = hit.point + hit.normal * 0.1f;
        }
    }

    void TurnBase()
    {
        Vector3 directionToTarget = (crosshair.transform.position - turretBase.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(directionToTarget.x, directionToTarget.y, directionToTarget.z));
        turretBase.transform.rotation = Quaternion.Slerp(turretBase.transform.rotation, lookRotation, Time.deltaTime * baseTurnSpeed);
    }

    void DrawLaserWithBounces()
    {
        laserPoints.Clear();
        laserPoints.Add(barrelEnd.position);

        Vector3 currentPosition = barrelEnd.position;
        Vector3 currentDirection = barrelEnd.forward;

        for (int i = 0; i < maxBounces; i++)
        {
            if (Physics.Raycast(currentPosition, currentDirection, out RaycastHit hit, 1000.0f, targetLayer))
            {
                laserPoints.Add(hit.point);

                // Calculate the reflected direction manually
                Vector3 normal = hit.normal;
                currentDirection = currentDirection - 2 * Vector3.Dot(currentDirection, normal) * normal;

                // Update current position for the next bounce
                currentPosition = hit.point;

                // Stop bouncing if no further objects are hit
                if (hit.collider.CompareTag("NoBounce"))
                {
                    break;
                }
            }
            else
            {
                // Extend laser to max range if no further hits
                laserPoints.Add(currentPosition + currentDirection * 1000.0f);
                break;
            }
        }

        // Set line renderer positions
        line.positionCount = laserPoints.Count;
        for (int i = 0; i < line.positionCount; i++)
        {
            line.SetPosition(i, laserPoints[i]);
        }
    }
}
