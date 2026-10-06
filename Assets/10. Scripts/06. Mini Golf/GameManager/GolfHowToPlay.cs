using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Study3D.MiniGolf
{
    public class GolfHowToPlay : MonoBehaviour
    {
        [SerializeField] private GameObject helpPanel;
        [SerializeField] private TMP_Text instructionsText;
        [SerializeField] private TMP_Text closeButtonText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Behaviour[] pauseInput;

        public bool IsVisible { get; private set; }
        private bool ownsPause;
        private bool[] previousEnabled;
        private float previousTimeScale;
        private CursorLockMode previousLock;
        private bool previousVisible;
        private Action onClosed;

        private void Awake()
        {
            if (helpPanel == null || instructionsText == null || closeButtonText == null || closeButton == null)
            {
                Debug.LogError("GolfHowToPlay의 패널, 본문, 버튼 글자, 버튼을 연결해주세요.", this);
                enabled = false;
                return;
            }

            instructionsText.text = "<b>장식장에 갇힌 홀인원 공, 집 밖에 자유를 향해!</b>\n\n"
                + "<b>[게임 방법]</b>\n"
                + "홀인원 공이 바닥에 떨어지면 수치심을 느껴 마지막 타격 전의 위치로 되돌아갑니다.\n\n"
                + "골 지점에 도달하면 문이 열립니다. 두 골 지점에 도착하여 집에서 탈출하세요!\n\n"
                + "<b>[키 안내]</b>\n"
                + "시점 변경 : 마우스 우클릭을 누른 채 드래그\n"
                + "줌 인 / 아웃 : 마우스 휠\n"
                + "공 타격 : 스페이스를 길게 눌러 세기 조절, 떼면 발사\n"
                + "공 리셋 : R — 공을 시작 위치로 복귀\n\n"
                + "<size=85%>R은 공 위치만 되돌립니다. 기록과 열린 문은 유지됩니다.</size>";
            helpPanel.SetActive(false);
            closeButton.onClick.AddListener(Close);
        }

        public void ShowAfterOpening()
        {
            Show(true, null);
        }

        public void ShowFromPause(Action returnToMenu)
        {
            Show(false, returnToMenu);
        }

        private void Show(bool pauseGameplay, Action returnAction)
        {
            if (enabled == false || IsVisible)
            {
                return;
            }

            IsVisible = true;
            onClosed = returnAction;
            ownsPause = pauseGameplay;

            if (ownsPause)
            {
                previousTimeScale = Time.timeScale;
                previousLock = Cursor.lockState;
                previousVisible = Cursor.visible;
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
            }

            closeButtonText.text = pauseGameplay ? "탈출 시작!" : "메뉴로 돌아가기";
            helpPanel.SetActive(true);
            closeButton.Select();
        }

        public void Close()
        {
            if (IsVisible == false)
            {
                return;
            }
            IsVisible = false;
            if (helpPanel != null) helpPanel.SetActive(false);

            if (ownsPause)
            {
                ownsPause = false;
                if (previousEnabled != null && pauseInput != null)
                {
                    int count = Mathf.Min(previousEnabled.Length, pauseInput.Length);
                    for (int i = 0; i < count; i++)
                    {
                        Behaviour component = pauseInput[i];
                        if (component != null && component != this)
                            component.enabled = previousEnabled[i];
                    }
                }
                Time.timeScale = previousTimeScale;
                Cursor.lockState = previousLock;
                Cursor.visible = previousVisible;
            }

            Action callback = onClosed;
            onClosed = null;
            callback?.Invoke();
        }

        private void OnDisable() { Close(); }

        private void OnDestroy()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
        }
    }
}
