#if UNITY_EDITOR
using Surexs.DanceOff.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Surexs.DanceOff.EditorTools
{
    public static class ChartRecorderLauncher
    {
        private const string GameScenePath = "Assets/Scenes/Game.unity";

        [MenuItem("Tools/Surexs Dance Off/Abrir Chart Recorder", priority = 100)]
        public static void OpenChartRecorder()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[ChartRecorder] Detén Play Mode antes de abrir el grabador.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            ChartRecorderAccess.RequestLaunch();
            EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("Window/Surexs Dance Off/Chart Recorder", priority = 2200)]
        private static void OpenChartRecorderFromWindow()
        {
            OpenChartRecorder();
        }
    }
}
#endif
