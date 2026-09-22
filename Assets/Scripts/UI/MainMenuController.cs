using Surexs.DanceOff.Core;
using Surexs.DanceOff.Gameplay;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Surexs.DanceOff.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        private GameObject mainPanel;
        private GameObject instructionsPanel;
        private Button playButton;
        private Button soloButton;
        private Button versusButton;
        private int instructionsShownFrame = -1;

        public void Configure(GameObject main, GameObject instructions, Button play, Button solo, Button versus)
        {
            mainPanel=main;
            instructionsPanel=instructions;
            playButton=play;
            soloButton=solo;
            versusButton=versus;
            ShowMain();
        }

        private void Update()
        {
            if (instructionsPanel != null && instructionsPanel.activeSelf)
            {
                if (Time.frameCount > instructionsShownFrame + 1 && AnyButtonPressedThisFrame())
                    BeginSelectedGame();
                return;
            }

            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
                Select(soloButton != null && soloButton.gameObject.activeInHierarchy ? soloButton : playButton);
        }

        public void ShowMain()
        {
            mainPanel.SetActive(true);
            instructionsPanel.SetActive(false);
            playButton.gameObject.SetActive(true);
            soloButton.gameObject.SetActive(false);
            versusButton.gameObject.SetActive(false);
            Select(playButton);
        }
        public void FocusModeSelection()
        {
            playButton.gameObject.SetActive(false);
            soloButton.gameObject.SetActive(true);
            versusButton.gameObject.SetActive(true);
            Select(soloButton);
        }
        public void StartSolo() { StartGame(GameMode.Solo); }
        public void StartVersus() { StartGame(GameMode.LocalVersus); }
        public void BeginSelectedGame()
        {
            if (instructionsPanel == null || !instructionsPanel.activeSelf ||
                Time.frameCount <= instructionsShownFrame + 1) return;
            SceneManager.LoadScene("Game");
        }
        private void StartGame(GameMode mode)
        {
            GameSession.SelectMode(mode);
            instructionsShownFrame=Time.frameCount;
            mainPanel.SetActive(false);
            instructionsPanel.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
        }
        private static void Select(Button button) { EventSystem.current.SetSelectedGameObject(button.gameObject); }

        private static bool AnyButtonPressedThisFrame()
        {
            foreach (var device in InputSystem.devices)
            {
                foreach (var control in device.allControls)
                {
                    if (control is ButtonControl button && button.wasPressedThisFrame) return true;
                }
            }

            return false;
        }
    }
}
