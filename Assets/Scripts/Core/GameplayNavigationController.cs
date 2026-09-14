using Surexs.DanceOff.Gameplay;
using Surexs.DanceOff.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Surexs.DanceOff.Core
{
    public sealed class GameplayNavigationController : MonoBehaviour
    {
        private GameFlowController flow;
        private RhythmPrototypeController gameplay;
        private ResultsView results;

        public void Configure(GameFlowController gameFlow, RhythmPrototypeController controller, ResultsView view)
        {
            flow=gameFlow; gameplay=controller; results=view;
            results.MenuRequested += ReturnToMenu;
        }

        private void Update()
        {
            var keyboard=Keyboard.current;
            if (flow.State == GameFlowState.Results && keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                ReturnToMenu();
        }

        public void ReturnToMenu()
        {
            gameplay.StopSession();
            SceneManager.LoadScene("MainMenu");
        }

        private void OnDestroy() { if (results != null) results.MenuRequested -= ReturnToMenu; }
    }
}
