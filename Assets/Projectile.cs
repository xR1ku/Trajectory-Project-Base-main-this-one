using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer;

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            // Destroy projectile upon hitting a valid target
            Destroy(gameObject);
        }
    }
}
