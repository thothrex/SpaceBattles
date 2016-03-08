using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace SpaceBattles
{
    public class ButtonAccelerate : MonoBehaviour
    {

        public Rigidbody target;
        public Vector3 button_direction;
        public float speedCoefficient;
        private bool applying_force = false;
        private Vector3 acceleration;

        void Start()
        {
            Button b = gameObject.GetComponent<Button>();
            b.onClick.AddListener(delegate () { toggle_force(); });
        }

        public void toggle_force()
        {
            applying_force = !applying_force;
        }

        void FixedUpdate()
        {
            if (applying_force)
            {
                acceleration = target.transform.TransformDirection(button_direction); // local forward
                target.AddForce(acceleration * speedCoefficient);
            }
        }
    }
}
