using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Study3D.MiniGolf
{
    public class GolfPauseMenu : MonoBehaviour
    {
        [Header("별도 Canvas 아래 메뉴 패널")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button quitButton;

        [Header("메뉴가 열린 동안 멈출 입력 스크립트")]
        [SerializeField] private Behaviour[] pauseInput;

        public bool IsOpen { get; private set; }

        private bool[] previousEnabled;
        private float previousTimeScale;
        private CursorLockMode previousCursorLock;
        private bool previousCursorVisible;

        private void Awake()
        {
            if (pausePanel == null || resumeButton == null || quitButton == null)
            {
                Debug.LogError("GolfPauseMenu에 패널과 버튼 2개를 연결해주세요.", this);
                enabled = false;
                return;
            }

            pausePanel.SetActive(false);
            resumeButton.onClick.AddListener(Resume);
            quitButton.onClick.AddListener(QuitGame);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || keyboard.escapeKey.wasPressedThisFrame == false)
            {
                return;
            }

            if (IsOpen)
            {
                Resume();
            }
            else if (Time.timeScale > 0f)
            {
                Open();
            }
        }

        public void Open()
        {
            if (IsOpen || Time.timeScale <= 0f)
            {
                return;
            }

            IsOpen = true;
            previousTimeScale = Time.timeScale;
            previousCursorLock = Cursor.lockState;
            previousCursorVisible = Cursor.visible;

            if (pauseInput != null)
            {
                previousEnabled = new bool[pauseInput.Length];
                for (int i = 0; i < pauseInput.Length; i++)
                {
                    Behaviour component = pauseInput[i];
                    if (component != null && component != this)
                    {
                        previousEnabled[i] = component.enabled;
                        component.enabled = false;
                    }
                }
            }

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            pausePanel.SetActive(true);
            resumeButton.Select();
        }

        public void Resume()
        {
            if (IsOpen == false)
            {
                return;
            }

            IsOpen = false;
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            if (previousEnabled != null && pauseInput != null)
            {
                int count = Mathf.Min(previousEnabled.Length, pauseInput.Length);
                for (int i = 0; i < count; i++)
                {
                    Behaviour component = pauseInput[i];
                    if (component != null && component != this)
                    {
                        component.enabled = previousEnabled[i];
                    }
                }
            }

            Time.timeScale = previousTimeScale;
            Cursor.lockState = previousCursorLock;
            Cursor.visible = previousCursorVisible;
        }

        public void QuitGame()
        {
            Resume();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnDisable()
        {
            Resume();
        }

        private void OnDestroy()
        {
            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveListener(Resume);
            }
            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(QuitGame);
            }
        }
    }
}
