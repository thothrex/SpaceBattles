using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;

namespace SpaceBattles
{
    // monobehaviour is okay here
    // https://docs.unity3d.com/ScriptReference/Network-isClient.html
    public class ButtonExitNetworkGame : MonoBehaviour
    {
        public delegate void exitNetworkGameButtonPressEventHandler();
        public event exitNetworkGameButtonPressEventHandler ExitNetGameButtonPress;

        void Start ()
        {
            Button b = gameObject.GetComponent<Button>();
            b.onClick.AddListener(exitNetworkGame);
        }

        public void exitNetworkGame ()
        {
            ExitNetGameButtonPress();
            if (Network.isServer)
            {
                Debug.Log("We're a server");
            }
			else if (Network.isClient) // we're a client
            {
                Debug.Log("We're a client");
                Network.CloseConnection(Network.connections[0], true);
            }
            else
            {
                Debug.Log("we're not a client or a server");
            }
        }
    }
}