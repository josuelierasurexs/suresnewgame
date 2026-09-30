using Surexs.DanceOff.Core;
using Surexs.DanceOff.Gameplay;
using Surexs.DanceOff.Input;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Surexs.DanceOff.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        private GameObject mainPanel;
        private GameObject instructionsPanel;
        private GameObject leaderboardPanel;
        private Button playButton;
        private Button soloButton;
        private Button versusButton;
        private Button leaderboardButton;
        private Button leaderboardBackButton;
        private LeaderboardView leaderboardView;
        private Button instructionsBackButton;
        private PlayerReadyInputMonitor player1ReadyInput;
        private PlayerReadyInputMonitor player2ReadyInput;
        private PlayerReadyPromptView player1ReadyPrompt;
        private PlayerReadyPromptView player2ReadyPrompt;
        private Text continueLabel;
        private GameMode selectedMode;
        private float readyHoldDuration;
        private float player1HoldTime;
        private float player2HoldTime;
        private bool player1Ready;
        private bool player2Ready;
        private bool continueArmed;
        private bool warnedAboutDuplicateDevice;

        public void Configure(GameObject main, GameObject instructions, GameObject leaderboardPanelObject,
            Button play, Button solo, Button versus, Button leaderboard, Button leaderboardBack,
            LeaderboardView leaderboardPanelView, Button instructionsBack,
            PlayerReadyInputMonitor p1Input,PlayerReadyInputMonitor p2Input,
            PlayerReadyPromptView p1Prompt,PlayerReadyPromptView p2Prompt,Text readyContinueLabel,
            float holdDuration)
        {
            mainPanel=main;
            instructionsPanel=instructions;
            leaderboardPanel=leaderboardPanelObject;
            playButton=play;
            soloButton=solo;
            versusButton=versus;
            leaderboardButton=leaderboard;
            leaderboardBackButton=leaderboardBack;
            leaderboardView=leaderboardPanelView;
            instructionsBackButton=instructionsBack;
            player1ReadyInput=p1Input;
            player2ReadyInput=p2Input;
            player1ReadyPrompt=p1Prompt;
            player2ReadyPrompt=p2Prompt;
            continueLabel=readyContinueLabel;
            readyHoldDuration=Mathf.Max(.25f,holdDuration);
            ShowMain();
        }

        private void Update()
        {
            if (instructionsPanel != null && instructionsPanel.activeSelf)
            {
                UpdateReadyScreen();
                return;
            }

            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null)
                Select(soloButton != null && soloButton.gameObject.activeInHierarchy ? soloButton : playButton);
        }

        public void ShowMain()
        {
            mainPanel.SetActive(true);
            instructionsPanel.SetActive(false);
            leaderboardPanel.SetActive(false);
            playButton.gameObject.SetActive(true);
            leaderboardButton.gameObject.SetActive(true);
            soloButton.gameObject.SetActive(false);
            versusButton.gameObject.SetActive(false);
            Select(playButton);
        }
        public void FocusModeSelection()
        {
            mainPanel.SetActive(true);
            instructionsPanel.SetActive(false);
            leaderboardPanel.SetActive(false);
            playButton.gameObject.SetActive(false);
            leaderboardButton.gameObject.SetActive(false);
            soloButton.gameObject.SetActive(true);
            versusButton.gameObject.SetActive(true);
            Select(soloButton);
        }
        public void StartSolo() { StartGame(GameMode.Solo); }
        public void StartVersus() { StartGame(GameMode.LocalVersus); }
        public void ShowLeaderboard()
        {
            mainPanel.SetActive(false);
            instructionsPanel.SetActive(false);
            leaderboardView.Show();
            Select(leaderboardBackButton);
        }
        public void BeginSelectedGame()
        {
            if (instructionsPanel == null || !instructionsPanel.activeSelf || !AllPlayersReady() || !continueArmed)
                return;
            SceneManager.LoadScene("Game");
        }
        private void StartGame(GameMode mode)
        {
            GameSession.SelectMode(mode);
            selectedMode=mode;
            ResetReadyState();
            mainPanel.SetActive(false);
            instructionsPanel.SetActive(true);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        private void UpdateReadyScreen()
        {
            var versus=selectedMode==GameMode.LocalVersus;
            if (player1ReadyInput.WasBackPressedThisFrame ||
                (versus && player2ReadyInput.WasBackPressedThisFrame))
            {
                FocusModeSelection();
                return;
            }
            var duplicateDevice=versus && player1ReadyInput.IsAvailable && player2ReadyInput.IsAvailable &&
                                player1ReadyInput.DeviceId==player2ReadyInput.DeviceId &&
                                player1ReadyInput.SourceType!=RhythmInputSourceType.Keyboard;
            if (duplicateDevice && !warnedAboutDuplicateDevice)
            {
                warnedAboutDuplicateDevice=true;
                Debug.LogWarning("[Ready] Player 1 y Player 2 resolvieron el mismo control. " +
                                 "Configura dispositivos distintos antes de iniciar 1 VS 1.",this);
            }

            UpdatePlayerReady(player1ReadyInput,ref player1HoldTime,ref player1Ready,false);
            if (versus) UpdatePlayerReady(player2ReadyInput,ref player2HoldTime,ref player2Ready,duplicateDevice);

            player1ReadyPrompt.SetState(player1HoldTime/readyHoldDuration,player1Ready,
                player1ReadyInput.IsAvailable,false);
            if (versus)
                player2ReadyPrompt.SetState(player2HoldTime/readyHoldDuration,player2Ready,
                    player2ReadyInput.IsAvailable,duplicateDevice);

            if (!AllPlayersReady())
            {
                continueLabel.text=versus
                    ? "AMBOS JUGADORES DEBEN ESTAR READY"
                    : "MANTÉN UN BOTÓN PARA ESTAR READY";
                return;
            }

            var anyHeld=player1ReadyInput.IsHeld || (versus && player2ReadyInput.IsHeld);
            if (!continueArmed)
            {
                continueLabel.text="SUELTA EL BOTÓN";
                if (!anyHeld) continueArmed=true;
                return;
            }

            continueLabel.text="PRESIONA CUALQUIER BOTÓN PARA CONTINUAR";
            if (player1ReadyInput.WasPressedThisFrame || (versus && player2ReadyInput.WasPressedThisFrame))
                BeginSelectedGame();
        }

        private void UpdatePlayerReady(PlayerReadyInputMonitor input,ref float holdTime,ref bool ready,
            bool deviceConflict)
        {
            if (ready || deviceConflict || !input.IsAvailable)
            {
                if (!ready) holdTime=0f;
                return;
            }

            if (!input.IsHeld)
            {
                holdTime=0f;
                return;
            }

            holdTime=Mathf.Min(readyHoldDuration,holdTime+Time.unscaledDeltaTime);
            if (holdTime>=readyHoldDuration) ready=true;
        }

        private void ResetReadyState()
        {
            player1HoldTime=player2HoldTime=0f;
            player1Ready=player2Ready=false;
            continueArmed=false;
            warnedAboutDuplicateDevice=false;
            var versus=selectedMode==GameMode.LocalVersus;
            player1ReadyPrompt.gameObject.SetActive(true);
            player2ReadyPrompt.gameObject.SetActive(versus);
            player1ReadyPrompt.GetComponent<RectTransform>().anchoredPosition=versus
                ? new Vector2(-420,-390)
                : new Vector2(-250,-390);
            instructionsBackButton.GetComponent<RectTransform>().anchoredPosition=versus
                ? new Vector2(0,-390)
                : new Vector2(250,-390);
            player1ReadyPrompt.SetState(0f,false,player1ReadyInput.IsAvailable,false);
            if (versus) player2ReadyPrompt.SetState(0f,false,player2ReadyInput.IsAvailable,false);
            continueLabel.text=versus
                ? "AMBOS JUGADORES DEBEN ESTAR READY"
                : "MANTÉN UN BOTÓN PARA ESTAR READY";
        }

        private bool AllPlayersReady()
        {
            return player1Ready && (selectedMode!=GameMode.LocalVersus || player2Ready);
        }

        private static void Select(Button button)
        {
            if (EventSystem.current != null && button != null)
                EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }
}
