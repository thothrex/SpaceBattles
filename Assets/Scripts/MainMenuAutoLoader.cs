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
        
        public void Start ()
        {
            StartCoroutine(LoadMainMenu());
        }

        public IEnumerator LoadMainMenu()
        {
            Debug.Log("Main Menu Scene Loading");
            SceneIndex MainMenuIndex = SceneIndex.MainMenu;
            Scene SceneSwappingFrom = SceneManager.GetActiveScene();
            
            AsyncOperation SceneLoad
                = SceneManager.LoadSceneAsync((int)MainMenuIndex, LoadSceneMode.Additive);
            ConfirmSceneLoadNotNull(SceneIndex.MainMenu, SceneLoad);
            yield return new WaitUntil(() => SceneLoad.isDone);
            yield return new WaitUntil(() => ManagersHaveLoaded());

            Scene MainMenuScene
                = SceneManager.GetSceneByName(SceneIndex.MainMenu.SceneName());
            Debug.Log("Setting active scene to " + MainMenuScene.name);
            SceneManager.SetActiveScene(MainMenuScene);
            AsyncOperation Unloading = SceneManager.UnloadSceneAsync(SceneSwappingFrom);
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

        private void
        ConfirmSceneLoadNotNull
        (SceneIndex sceneIndex, AsyncOperation SceneLoad)
        {
            if (SceneLoad == null)
            {
                if (SceneManager.sceneCount < (int)sceneIndex)
                {
                    throw new ArgumentOutOfRangeException(
                        "sceneIndex",
                        sceneIndex,
                        CreateInsufficientScenesExceptionMessage(
                            SceneManager.sceneCount, (int)sceneIndex
                        )
                    );
                }
                else
                {
                    throw new InvalidOperationException(
                        "LoadSceneAsync returned null"
                    );
                }
            }
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
