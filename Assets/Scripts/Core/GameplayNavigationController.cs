using System.Collections;
using Surexs.DanceOff.Gameplay;
using Surexs.DanceOff.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    public sealed class PauseMenuController : MonoBehaviour
    {
        private GameObject panel;
        private Button resumeButton;
        private Button restartButton;
        private Button exitButton;
        private GameFlowController flow;
        private RhythmPrototypeController gameplay;
        private GameplayNavigationController navigation;
        private GameObject sequenceOverlay;
        private CanvasGroup brandGroup;
        private Text countdownLabel;
        private CanvasGroup countdownGroup;
        private bool paused;
        private bool transitionRunning;

        public void Configure(GameObject pausePanel,Button resume,Button restart,Button exit,
            GameObject introOverlay,CanvasGroup introBrand,Text countdown,
            GameFlowController gameFlow,RhythmPrototypeController controller,GameplayNavigationController navigator)
        {
            panel=pausePanel;
            resumeButton=resume;
            restartButton=restart;
            exitButton=exit;
            sequenceOverlay=introOverlay;
            brandGroup=introBrand;
            countdownLabel=countdown;
            countdownGroup=countdownLabel.GetComponent<CanvasGroup>();
            if (countdownGroup==null) countdownGroup=countdownLabel.gameObject.AddComponent<CanvasGroup>();
            flow=gameFlow;
            gameplay=controller;
            navigation=navigator;
            resumeButton.onClick.AddListener(Resume);
            restartButton.onClick.AddListener(Restart);
            exitButton.onClick.AddListener(ExitToMenu);
            panel.SetActive(false);
            sequenceOverlay.SetActive(false);
        }

        private void Start() { StartCoroutine(PlayInitialSequence()); }

        private void Update()
        {
            if (transitionRunning) return;
            if (!WasPausePressedThisFrame()) return;
            if (paused) Resume();
            else Pause();
        }

        private void Pause()
        {
            if (flow==null || flow.State!=GameFlowState.Gameplay || !gameplay.PauseSession()) return;
            paused=true;
            panel.SetActive(true);
            if (EventSystem.current!=null) EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }

        private void Resume()
        {
            if (!paused) return;
            ClosePanel();
            StartCoroutine(ResumeAfterCountdown());
        }

        private void Restart()
        {
            if (!paused) return;
            ClosePanel();
            gameplay.StopSession();
            StartCoroutine(RestartAfterCountdown());
        }

        private void ExitToMenu()
        {
            if (!paused) return;
            ClosePanel();
            navigation.ReturnToMenu();
        }

        private void ClosePanel()
        {
            paused=false;
            panel.SetActive(false);
            if (EventSystem.current!=null) EventSystem.current.SetSelectedGameObject(null);
        }

        private IEnumerator PlayInitialSequence()
        {
            transitionRunning=true;
            ShowSequenceOverlay();
            brandGroup.alpha=0f;
            yield return FadeBrand(0f,1f,.55f);
            yield return WaitUnscaled(1.75f);
            yield return FadeBrand(1f,0f,.70f);
            yield return PlayCountdown();
            HideSequenceOverlay();
            transitionRunning=false;
            flow.StartSession();
        }

        private IEnumerator ResumeAfterCountdown()
        {
            transitionRunning=true;
            ShowSequenceOverlay();
            brandGroup.alpha=0f;
            yield return PlayCountdown();
            HideSequenceOverlay();
            transitionRunning=false;
            if (!gameplay.ResumeSession()) Debug.LogWarning("[Pause] No fue posible reanudar la partida.",this);
        }

        private IEnumerator RestartAfterCountdown()
        {
            transitionRunning=true;
            ShowSequenceOverlay();
            brandGroup.alpha=0f;
            yield return PlayCountdown();
            HideSequenceOverlay();
            transitionRunning=false;
            flow.StartSession();
        }

        private IEnumerator PlayCountdown()
        {
            for (var number=3;number>=1;number--)
            {
                countdownLabel.text=number.ToString();
                countdownGroup.alpha=1f;
                var elapsed=0f;
                const float stepDuration=.82f;
                while (elapsed<stepDuration)
                {
                    elapsed+=Time.unscaledDeltaTime;
                    var progress=Mathf.Clamp01(elapsed/stepDuration);
                    countdownLabel.rectTransform.localScale=Vector3.one*Mathf.Lerp(1.32f,.92f,progress);
                    countdownGroup.alpha=progress<.68f ? 1f : 1f-Mathf.InverseLerp(.68f,1f,progress);
                    yield return null;
                }
            }
            countdownLabel.text=string.Empty;
            countdownGroup.alpha=0f;
        }

        private IEnumerator FadeBrand(float from,float to,float duration)
        {
            var elapsed=0f;
            while (elapsed<duration)
            {
                elapsed+=Time.unscaledDeltaTime;
                brandGroup.alpha=Mathf.Lerp(from,to,Mathf.Clamp01(elapsed/duration));
                yield return null;
            }
            brandGroup.alpha=to;
        }

        private static IEnumerator WaitUnscaled(float duration)
        {
            var elapsed=0f;
            while (elapsed<duration)
            {
                elapsed+=Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private void ShowSequenceOverlay()
        {
            sequenceOverlay.SetActive(true);
            countdownLabel.text=string.Empty;
            countdownGroup.alpha=0f;
            if (EventSystem.current!=null) EventSystem.current.SetSelectedGameObject(null);
        }

        private void HideSequenceOverlay()
        {
            brandGroup.alpha=0f;
            countdownGroup.alpha=0f;
            countdownLabel.text=string.Empty;
            sequenceOverlay.SetActive(false);
        }

        private static bool WasPausePressedThisFrame()
        {
            if (Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame) return true;
            for (var index=0;index<Gamepad.all.Count;index++)
                if (Gamepad.all[index].startButton.wasPressedThisFrame) return true;
            for (var index=0;index<Joystick.all.Count;index++)
            {
                var joystick=Joystick.all[index];
                foreach (var control in joystick.allControls)
                {
                    if (!(control is ButtonControl button) || !button.wasPressedThisFrame) continue;
                    var controlName=button.name.ToLowerInvariant();
                    var displayName=button.displayName.ToLowerInvariant();
                    if (controlName.Contains("start") || controlName.Contains("menu") ||
                        displayName.Contains("start") || displayName.Contains("menu")) return true;
                }
            }
            return false;
        }

        private void OnDestroy()
        {
            if (resumeButton!=null) resumeButton.onClick.RemoveListener(Resume);
            if (restartButton!=null) restartButton.onClick.RemoveListener(Restart);
            if (exitButton!=null) exitButton.onClick.RemoveListener(ExitToMenu);
        }
    }
}
