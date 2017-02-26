using System;
using UnityEngine;

namespace SpaceBattles
{
    /// <summary>
    /// This needs to be kept updated with the values
    /// specified in build settings.
    /// </summary>
    public enum SceneIndex
    {
        Loading = 0,
        MainMenu = 1,
        MultiplayerGame = 2,
        Orrery = 3
    }

    public static class SceneIndexExtensions
    {
        public static string SceneName (this SceneIndex sceneIndex)
        {
            switch (sceneIndex)
            {
                case SceneIndex.Loading:
                    return "LoadGameScene";
                case SceneIndex.MainMenu:
                    return "MainMenu";
                case SceneIndex.MultiplayerGame:
                    return "EarthOrbit";
                case SceneIndex.Orrery:
                    return "Orrery";
                default:
                    throw new UnexpectedEnumValueException<SceneIndex>(sceneIndex);
            }
        }
    }
}