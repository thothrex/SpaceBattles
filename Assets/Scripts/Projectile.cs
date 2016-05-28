using System;
using UnityEngine;

namespace SpaceBattles
{
    public class Projectile : MonoBehaviour
    {
        void OnCollisionEnter(Collision collision)
        {
            var hit = collision.gameObject;
            // projectiles pass through each other
            var hit_projectile = hit.GetComponent<Projectile>();
            if (hit_projectile != null)
            {
                Debug.Log("Projectile x projectile collision registered");
            }
            else
            {
                Debug.Log("Projectile hit registered");
                var hitPlayer = hit.GetComponent<PlayerShipController>();
                if (hitPlayer != null)
                {
                    Debug.Log("Player hit registered");
                    hitPlayer.onProjectileHit();
                }
                Destroy(this.gameObject);
            }            
        }
    }
}
