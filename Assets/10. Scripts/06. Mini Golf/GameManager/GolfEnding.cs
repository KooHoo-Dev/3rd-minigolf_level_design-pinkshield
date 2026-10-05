using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Study3D.MiniGolf
{
    public class GolfEnding : MonoBehaviour
    {
        [Header("엔딩 존 - 이 오브젝트의 Collider는 Is Trigger 켜기")]
        [SerializeField] private Rigidbody ballBody;

        [Header("엔딩 UI - 별도 Overlay Canvas 아래 배치")]
        [SerializeField] private GameObject endingPanel;
        [SerializeField] private TMP_Text moveCountText;
        [SerializeField] private TMP_Text fallCountText;
        [FormerlySerializedAs("scoreText")]
        [SerializeField] private TMP_Text elapsedTimeText;
        [SerializeField] private Button closeButton;

        [Header("엔딩 중 입력을 막을 컴포넌트")]
        [SerializeField] private Behaviour[] disableOnEnding;

        public int MoveCount { get; private set; }
        public int FallCount { get; private set; }
        public float ElapsedSeconds => IsFinished ? finalElapsedSeconds : Time.timeSinceLevelLoad;
        public bool IsFinished { get; private set; }

        private bool isRestarting;
        private float finalElapsedSeconds;
        private bool ownsPause;
        private float previousTimeScale;
        private CursorLockMode previousCursorLock;
        private bool previousCursorVisible;

        private void Awake()
        {
            if (ballBody == null || endingPanel == null || moveCountText == null
                || fallCountText == null || elapsedTimeText == null || closeButton == null)
            {
                Debug.LogError("GolfEnding의 공, 패널, 텍스트 3개, 닫기 버튼을 모두 연결해주세요.", this);
                enabled = false;
                return;
            }

            endingPanel.SetActive(false);
            // Inspector OnClick에는 같은 함수를 중복 연결하지 않습니다.
            closeButton.onClick.AddListener(CloseAndRestart);
        }

        // 유효한 타격이 확정된 지점에서 한 번 호출합니다.
        public void RecordMove()
        {
            if (IsFinished == false)
            {
                MoveCount++;
            }
        }

        // 실제 낙하 복귀 처리에서 한 번 호출합니다. 디버그 복귀에는 호출하지 않습니다.
        public void RecordFall()
        {
            if (IsFinished == false)
            {
                FallCount++;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsFinished || other.attachedRigidbody != ballBody)
            {
                return;
            }

            finalElapsedSeconds = Time.timeSinceLevelLoad;
            IsFinished = true;
            moveCountText.text = $"자유를 향한 시도 : {MoveCount}회 (타격 횟수) ";
            fallCountText.text = $"떨어진 명예 : {FallCount}회 (낙하 횟수)  ";
            int totalSeconds = Mathf.FloorToInt(finalElapsedSeconds);
            int hours = totalSeconds / 3600;
            int minutes = totalSeconds % 3600 / 60;
            int seconds = totalSeconds % 60;
            string formattedTime = hours > 0
                ? $"{hours}시간 {minutes:00}분 {seconds:00}초"
                : $"{minutes:00}분 {seconds:00}초";
            elapsedTimeText.text = $"자유를 되찾기까지 걸린 시간 :  {formattedTime}";

            // timeScale=0이어도 Update의 입력 처리는 실행될 수 있으므로 별도로 비활성화.
            if (disableOnEnding != null)
            {
                for (int i = 0; i < disableOnEnding.Length; i++)
                {
                    Behaviour component = disableOnEnding[i];
                    if (component != null && component != this)
                    {
                        component.enabled = false;
                    }
                }
            }

            previousTimeScale = Time.timeScale;
            previousCursorLock = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            ownsPause = true;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            endingPanel.SetActive(true);
            closeButton.Select();
        }

        public void CloseAndRestart()
        {
            if (IsFinished == false || isRestarting)
            {
                return;
            }

            Scene currentScene = SceneManager.GetActiveScene();
            if (currentScene.buildIndex < 0)
            {
                Debug.LogError("현재 게임 씬을 Build Profiles의 Scene List에 추가해주세요.", this);
                return;
            }

            isRestarting = true;
            RestorePause();
            SceneManager.LoadScene(currentScene.buildIndex);
        }

        private void RestorePause()
        {
            if (ownsPause == false)
            {
                return;
            }

            Time.timeScale = previousTimeScale;
            Cursor.lockState = previousCursorLock;
            Cursor.visible = previousCursorVisible;
            ownsPause = false;
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CloseAndRestart);
            }

            RestorePause();
        }
    }
}
