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

        [Header("마우스 위치 이동 디버깅")]
        [SerializeField] private bool enableMouseTeleport = true;
        [SerializeField] private Camera testCamera;
        [SerializeField] private LayerMask teleportLayers = ~0;
        [SerializeField, Min(0f)] private float surfaceOffset = 0.15f;
        [SerializeField, Min(0.01f)] private float rayDistance = 1000f;

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
            if (SimpleInput.GetKeyDown(Key.Digit1))
            {
                ball.ReturnToLasShotPosition();
            }

            if (SimpleInput.GetKeyDown(Key.Digit2))
            {
                ball.PlaceAt(startPosition);
            }

            if (enableMouseTeleport == false || testCamera == null)
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
