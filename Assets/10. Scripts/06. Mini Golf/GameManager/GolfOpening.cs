using System.Collections;
using UnityEngine;

namespace Study3D.MiniGolf
{
    public class GolfOpening : MonoBehaviour
    {
        [Header("카메라: 첫 번째 골 → 두 번째 골 → 플레이")]
        [SerializeField] private Camera firstGoalCamera;
        [SerializeField] private Camera secondGoalCamera;
        [SerializeField] private Camera gameplayCamera;

        [Header("바라볼 지점 - 비워두면 씬에 배치한 회전 유지")]
        [SerializeField] private Transform firstGoal;
        [SerializeField] private Transform secondGoal;
        [SerializeField, Min(0.1f)] private float secondsPerGoal = 3f;

        [Header("연출 동안 멈출 입력·카메라 스크립트")]
        [SerializeField] private Behaviour[] pauseDuringOpening;
        [SerializeField] private GameObject gameplayUi;
        [SerializeField] private GolfHowToPlay howToPlay;

        private bool[] previousEnabled;
        private bool previousUiActive;
        private float previousTimeScale;
        private bool isOpening;

        private void Awake()
        {
            if (firstGoalCamera == null || secondGoalCamera == null || gameplayCamera == null
                || firstGoalCamera == secondGoalCamera || firstGoalCamera == gameplayCamera
                || secondGoalCamera == gameplayCamera)
            {
                Debug.LogError("GolfOpening에 서로 다른 카메라 3개를 연결해주세요.", this);
                enabled = false;
                return;
            }

            previousTimeScale = Time.timeScale;
            isOpening = true;
            Time.timeScale = 0f;

            if (pauseDuringOpening != null)
            {
                previousEnabled = new bool[pauseDuringOpening.Length];
                for (int i = 0; i < pauseDuringOpening.Length; i++)
                {
                    Behaviour component = pauseDuringOpening[i];
                    if (component != null && component != this)
                    {
                        previousEnabled[i] = component.enabled;
                        component.enabled = false;
                    }
                }
            }

            if (gameplayUi != null)
            {
                previousUiActive = gameplayUi.activeSelf;
                gameplayUi.SetActive(false);
            }

            // AudioListener는 메인 카메라 하나만 사용합니다.
            DisableExtraListener(firstGoalCamera);
            DisableExtraListener(secondGoalCamera);
            ShowCamera(firstGoalCamera, firstGoal);
        }

        private IEnumerator Start()
        {
            // timeScale=0인 동안에도 실제 시간 3초씩 대기합니다.
            yield return new WaitForSecondsRealtime(secondsPerGoal);
            ShowCamera(secondGoalCamera, secondGoal);
            yield return new WaitForSecondsRealtime(secondsPerGoal);
            FinishOpening();
            if (howToPlay != null)
            {
                howToPlay.ShowAfterOpening();
            }
        }

        private void ShowCamera(Camera selectedCamera, Transform goal)
        {
            firstGoalCamera.enabled = selectedCamera == firstGoalCamera;
            secondGoalCamera.enabled = selectedCamera == secondGoalCamera;
            gameplayCamera.enabled = selectedCamera == gameplayCamera;

            if (goal != null && (goal.position - selectedCamera.transform.position).sqrMagnitude > Mathf.Epsilon)
            {
                selectedCamera.transform.LookAt(goal.position);
            }
        }

        private void DisableExtraListener(Camera goalCamera)
        {
            AudioListener listener = goalCamera.GetComponent<AudioListener>();
            if (listener != null)
            {
                listener.enabled = false;
            }
        }

        private void FinishOpening()
        {
            if (isOpening == false)
            {
                return;
            }

            isOpening = false;
            firstGoalCamera.enabled = false;
            secondGoalCamera.enabled = false;
            gameplayCamera.enabled = true;

            if (previousEnabled != null)
            {
                for (int i = 0; i < previousEnabled.Length; i++)
                {
                    Behaviour component = pauseDuringOpening[i];
                    if (component != null && component != this)
                    {
                        component.enabled = previousEnabled[i];
                    }
                }
            }

            if (gameplayUi != null)
            {
                gameplayUi.SetActive(previousUiActive);
            }

            Time.timeScale = previousTimeScale;
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            if (isOpening)
            {
                FinishOpening();
            }
        }
    }
}
