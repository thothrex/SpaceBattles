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
        MainMenu = 0,
        MultiplayerGame = 1,
        Orrery = 2
    }

    public static class SceneIndexExtensions
    {
        public static string SceneName (this SceneIndex sceneIndex)
        {
            switch (sceneIndex)
            {
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