using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpaceBattles
{
    // Most of this class needs to be pulled out with
    // common functionality from the PIM
    // into a "Scene loader" module
    public class MainMenuAutoLoader : MonoBehaviour
    {
        public PassthroughNetworkManager PassthroughNetworkManager;
        public ProgramInstanceManager ProgramInstanceManager;
        public GameObject LoadScreenCanvas;
        public GameObject LoadScreenCamera;
        
        public void Start ()
        {
            StartCoroutine(LoadMainMenu());
        }

        public IEnumerator LoadMainMenu()
        {
            MyContract.RequireFieldNotNull(LoadScreenCamera, "LoadScreenCamera");
            MyContract.RequireFieldNotNull(LoadScreenCanvas, "LoadScreenCanvas");
            Debug.Log("Main Menu Scene Loading");
            yield return new WaitUntil(() => ManagersHaveLoaded());
            LoadScreenCanvas.SetActive(false);
            LoadScreenCamera.SetActive(false);
            gameObject.SetActive(false);
        }

        private bool ManagersHaveLoaded ()
        {
            MyContract.RequireFieldNotNull(
                PassthroughNetworkManager,
                "Passthrough Network Manager"
            );
            MyContract.RequireFieldNotNull(
                ProgramInstanceManager,
                "Program Instance Manager"
            );
            return PassthroughNetworkManager.FinishedLoading
                && ProgramInstanceManager.FinishedLoading;
        }

        private string
        CreateInsufficientScenesExceptionMessage
            (int buildScenes, int sceneIndex)
        {
            return "There are fewer scenes specified in the project's "
                 + "build settings than the index of the scene you are "
                 + "trying to load.\n"
                 + "Total Scenes: "
                 + buildScenes.ToString()
                 + "\tAttempted Loading Scene Index: "
                 + sceneIndex.ToString();
        }
    }

}
