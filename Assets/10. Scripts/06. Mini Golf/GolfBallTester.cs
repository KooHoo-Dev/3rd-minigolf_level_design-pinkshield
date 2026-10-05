using Study.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Study3D.MiniGolf
{
    public class GolfBallTester : MonoBehaviour
    {
        public GolfBall ball;
        public Vector3 startPosition;

        public bool IsTestOn { get; private set; }
        
        [Header("마우스 위치 이동 디버깅")]
        [SerializeField] private bool enableMouseTeleport = true;
        [SerializeField] private Camera testCamera;
        [SerializeField] private LayerMask teleportLayers = ~0;
        [SerializeField, Min(0f)] private float surfaceOffset = 0.15f;
        [SerializeField, Min(0.01f)] private float rayDistance = 1000f;

        private void Awake()
        {
            IsTestOn = false;
        }

        private void OnDisable()
        {
            IsTestOn = false;
        }

        private void Start()
        {
            if (ball == null)
            {
                Debug.LogError("GolfBallTester에 Ball을 연결해주세요.", this);
                enabled = false;
                return;
            }

            startPosition = ball.transform.position;

            if (testCamera == null)
            {
                testCamera = Camera.main;
            }
        }

        private void Update()
        {
            // 시작 연출과 엔딩에서는 테스트 입력도 처리하지 않습니다.
            if (Time.timeScale <= 0f || ball == null)
            {
                return;
            }
            
            
            // 테스트 모드와 관계없이 시작위치로 복귀
            if (SimpleInput.GetKeyDown(Key.R))
            {
                ball.PlaceAt(startPosition);
                return;
            }
            
            if (SimpleInput.GetKeyDown(Key.F1))
            {
                IsTestOn = IsTestOn == false;
                Debug.Log(IsTestOn ? "테스트 모드 ON" : "테스트 모드 OFF", this);
            }

            if (IsTestOn == false)
            {
                return;
            }

            // 시작 연출에서 Camera.main을 못 찾았으면 다시 시도합니다.
            if (testCamera == null)
            {
                testCamera = Camera.main;
            }

            if (SimpleInput.GetKeyDown(Key.Digit1))
            {
                ball.ReturnToLasShotPosition();
            }
            

            if (enableMouseTeleport == false || testCamera == null || testCamera.isActiveAndEnabled == false)
            {
                return;
            }

            Mouse mouse = Mouse.current;

            // 좌클릭 타격, 우클릭 카메라 회전과 겹치지 않도록 휠 클릭 사용.
            if (mouse == null || mouse.middleButton.wasPressedThisFrame == false)
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = testCamera.ScreenPointToRay(mouse.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance,
                teleportLayers, QueryTriggerInteraction.Ignore))
            {
                // 표면에 공이 파묻히지 않도록 법선 방향으로 띄웁니다.
                Vector3 destination = hit.point + hit.normal * surfaceOffset;
                ball.PlaceAt(destination);

                Rigidbody ballRigidbody = ball.GetComponent<Rigidbody>();

                if (ballRigidbody != null && ballRigidbody.isKinematic == false)
                {
                    ballRigidbody.linearVelocity = Vector3.zero;
                    ballRigidbody.angularVelocity = Vector3.zero;
                }
            }
        }
    }
}
