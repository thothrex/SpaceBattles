using UnityEngine;
using System.Collections;

namespace SpaceBattles
{
    public class RaycastDebugger : MonoBehaviour
    {
        private static readonly string OrreryOrbitStartRotationButtonName
            = "StartOrreryRotation";
        private float TimeElapsed = 0;

        void Update()
        {
            //TimeElapsed += Time.deltaTime;
            //if (TimeElapsed > 2.0)
            //{
            //    TimeElapsed = 0;
            //    Debug.Log("RaycastDebugger: Running");
            //}
            
            if (Input.GetMouseButtonDown(0)
                //CnControls
                //.CnInputManager
                //.GetButton(OrreryOrbitStartRotationButtonName)
                )
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    Debug.Log("Name = " + hit.collider.name);
                    Debug.Log("Tag = " + hit.collider.tag);
                    Debug.Log("Hit Point = " + hit.point);
                    Debug.Log("Object position = " + hit.collider.gameObject.transform.position);
                    Debug.Log("--------------");
                }
            }
        }
    }
}
