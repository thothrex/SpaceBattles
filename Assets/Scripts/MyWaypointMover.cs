using System;
using UnityEngine;

namespace SpaceBattles
{
    public class MyWaypointMover : MonoBehaviour
    {
        // CamelCase for the Unity editor
        public float TransitionTime;
        public float EasingCoefficient;
        public Transform StartPosition;
        public Transform EndPosition;
        public bool Debugging;

        private bool moving = false;
        private bool moving_towards_end_position = true;
        private float current_move_time = 0f;
        private Transform goal_position;
        private Transform origin_position;
        
        public void Update()
        {
            if (moving)
            {
                //increment timer once per frame
                current_move_time += Time.deltaTime;
                if (current_move_time >= TransitionTime)
                {
                    moving = false;
                    current_move_time = TransitionTime;
                    if (Debugging)
                    {
                        Debug.Log("Finished movement");
                    }
                }
                //lerp!
                float movement_progress = current_move_time / TransitionTime;

                transform.position
                    = Sinerp(origin_position.position,
                             goal_position.position,
                             movement_progress);
            }
        }

        public void toggleMoveState ()
        {
            setMoveState(!moving_towards_end_position);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">
        /// True is equal to the end positon,
        /// false is the start (0 -> 1)
        /// </param>
        public void setMoveState (bool state)
        {
            if (!state)
            {
                moveToStart();
            }
            else
            {
                moveToEnd();
            }
        }

        public void moveToStart ()
        {
            if (Debugging)
            {
                Debug.Log("Moving to start position: "
                        + StartPosition.ToString());
            }
            moving_towards_end_position = false;
            setMoveTarget(StartPosition);
        }

        public void moveToEnd()
        {
            if (Debugging)
            {
                Debug.Log("Moving to end position: "
                        + EndPosition.ToString());
            }
            moving_towards_end_position = true;
            setMoveTarget(EndPosition);
        }

        private void setMoveTarget (Transform move_target)
        {
            moving = true;
            origin_position = gameObject.transform;
            goal_position = move_target;
            current_move_time = 0f;
        }

        //Ease out
        private float Sinerp(float start, float end, float value)
        {
            return Mathf.Lerp(start, end, Mathf.Sin(value * Mathf.PI * EasingCoefficient));
        }

        private Vector2 Sinerp(Vector2 start, Vector2 end, float value)
        {
            return new Vector2(Sinerp(start.x, end.x, value),
                               Sinerp(start.y, end.y, value));
        }

        private Vector3 Sinerp(Vector3 start, Vector3 end, float value)
        {
            return new Vector3(Sinerp(start.x, end.x, value),
                               Sinerp(start.y, end.y, value),
                               Sinerp(start.z, end.z, value));
        }
    }

}
