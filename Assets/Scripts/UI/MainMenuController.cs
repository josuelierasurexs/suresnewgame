using Surexs.DanceOff.Core;
using Surexs.DanceOff.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Surexs.DanceOff.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        private GameObject mainPanel;
        private GameObject modePanel;
        private GameObject optionsPanel;
        private Button playButton;
        private Button soloButton;
        private Button optionsBackButton;

        public void Configure(GameObject main, GameObject modes, GameObject options, Button play,
            Button solo, Button optionsBack)
        { mainPanel=main; modePanel=modes; optionsPanel=options; playButton=play; soloButton=solo; optionsBackButton=optionsBack; ShowMain(); }

        private void Update()
        {
            var keyboard=Keyboard.current;
            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame) return;
            if (modePanel.activeSelf || optionsPanel.activeSelf) ShowMain();
        }

        public void ShowMain() { ShowOnly(mainPanel); Select(playButton); }
        public void ShowModes() { ShowOnly(modePanel); Select(soloButton); }
        public void ShowOptions() { ShowOnly(optionsPanel); Select(optionsBackButton); }
        public void StartSolo() { StartGame(GameMode.Solo); }
        public void StartVersus() { StartGame(GameMode.LocalVersus); }
        public void Quit()
        {
#if UNITY_EDITOR
            Debug.Log("[MainMenu] SALIR ejecutará Application.Quit en un build.", this);
#else
            Application.Quit();
#endif
        }

        private void StartGame(GameMode mode) { GameSession.SelectMode(mode); SceneManager.LoadScene("Game"); }
        private void ShowOnly(GameObject selected)
        { mainPanel.SetActive(selected==mainPanel); modePanel.SetActive(selected==modePanel); optionsPanel.SetActive(selected==optionsPanel); }
        private static void Select(Button button) { EventSystem.current.SetSelectedGameObject(button.gameObject); }
    }
}
