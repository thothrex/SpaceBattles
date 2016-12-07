using System;
using UnityEngine;
using UnityEngine.Networking;

namespace SpaceBattles
{
    public class Projectile : NetworkBehaviour
    {
        public PlayerIdentifier shooter;

        /// <summary>
        /// Projectile hits will be server-only in order to simplify the logical flow
        /// </summary>
        /// <param name="collision"></param>
        void OnCollisionEnter(Collision collision)
        {
            if (hasAuthority)
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
                        hitPlayer.OnProjectileHit(shooter);
                    }
                    Destroy(this.gameObject);
                }
            }      
        }
    }
}
