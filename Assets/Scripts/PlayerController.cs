using UnityEngine;
using System.Collections;

public class PlayerController: MonoBehaviour {

	public float speedCoefficient;
    public float maxSpeed = 10;

	// updates for phsyics
	void FixedUpdate () {

		// accel for accelerometer, input for keyboard
		float rotate_roll  = -Input.acceleration.x * 0.5f + (-Input.GetAxis("Horizontal"));
		float rotate_pitch = -Input.acceleration.z * 0.5f + Input.GetAxis("Vertical");

		Vector3 torque = new Vector3(rotate_pitch, 0.0f, rotate_roll);

        Rigidbody body = GetComponent<Rigidbody>();
        body.AddRelativeTorque(torque * speedCoefficient * Time.deltaTime);
        if (body.velocity.magnitude > maxSpeed)
        {
            body.velocity = body.velocity.normalized * maxSpeed;
        }

    }

    /// <summary>
    /// For now, just returns player to origin of playable space
    /// </summary>
    public void warp ()
    {
        transform.position = new Vector3(0, 0, 0);
    }
}
