// taken from http://forum.unity3d.com/threads/where-is-the-smooth-follow-script-in-unity3.62048/
// user jimmio92

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class InertialPlayerCameraController: MonoBehaviour
{
	public Transform FollowTransform;
	
	public Vector3 offset = new Vector3(0f, 2.5f, -5f);
	public float moveSpeed = 1;
	public float turnSpeed = 1;
	public Vector3 desiredEulerRotation = new Vector3(0, 0, 20);
	
	Vector3 goalPos;
	Quaternion goalRot;
	
	// Use this for initialization
	void Start()
	{
        if (!FollowTransform)
        {
            this.enabled = false;
        }
	}

    // TODO: remove me
    void OnConnectedToServer ()
    {
        Debug.Log("Player camera connected to server");
    }
	
	void FixedUpdate()
	{
        if (enabled && FollowTransform != null)
        {
            goalPos = FollowTransform.position + FollowTransform.TransformDirection(offset);
            goalRot = FollowTransform.rotation * Quaternion.Euler(desiredEulerRotation); // product combines quaternions
            transform.position = Vector3.Lerp(transform.position, goalPos, Time.deltaTime * moveSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, goalRot, Time.deltaTime * turnSpeed);
        }
	}
}
