using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Study3D.MiniGolf
{
    public class GolfPlayer : MonoBehaviour
    {
        [Header("Settings")]
        public float minPower;
        public float maxPower;
        [Min(0.01f)] public float cycleTime = 1f;

        [Header("Ref")]
        public GolfBall ball;
        public GolfAimIndicatorV2 aimIndicator;
        [SerializeField] private Camera mainCam;
        
        [SerializeField] private GolfEnding golfEnding;
        
        
        private const float MinimumCycleTime = 0.01f;
        private float chargeTime;
        private float power01;
        private bool isCharging;

        private void Awake()
        {
            TryFindCamera();
        }

        private void OnEnable()
        {
            TryFindCamera();
            ResetCharge();
        }

        private void TryFindCamera()
        {
            // Inspector에 연결한 카메라는 Camera.main으로 덮어쓰지 않습니다.
            if (mainCam == null)
            {
                mainCam = Camera.main;
            }
        }

        private void Update()
        {
            // 연출·엔딩 중에는 충전과 발사를 처리하지 않습니다.
            if (Time.timeScale <= 0f)
            {
                CancelAim();
                return;
            }

            TryFindCamera();

            if (mainCam == null || mainCam.isActiveAndEnabled == false || ball == null)
            {
                CancelAim();
                return;
            }

            Vector3 aimDirection = mainCam.transform.forward;
            aimDirection.y = 0f;

            // Vector3는 값 형식이므로 null 검사 대신 길이를 확인합니다.
            if (aimDirection.sqrMagnitude <= Mathf.Epsilon)
            {
                CancelAim();
                return;
            }

            aimDirection.Normalize();
            bool canShoot = ball.IsStopped;

            if (canShoot == false)
            {
                CancelAim();
                return;
            }

            if (SimpleInput.GetKey(Key.Space))
            {
                isCharging = true;
                chargeTime += Time.deltaTime;
                float safeCycleTime = Mathf.Max(cycleTime, MinimumCycleTime);
                power01 = Mathf.PingPong(chargeTime / safeCycleTime, 1f);
            }
            else if (SimpleInput.GetKeyUp(Key.Space) && isCharging)
            {
                float power = Mathf.Lerp(minPower, maxPower, power01);
                ball.Shoot(aimDirection, power);
                CancelAim();
                if (golfEnding != null)
                {
                    golfEnding.RecordMove();
                }
                return;
            }

            if (aimIndicator != null)
            {
                aimIndicator.Show(ball.transform.position, aimDirection, power01);
            }
        }

        private void ResetCharge()
        {
            chargeTime = 0f;
            power01 = 0f;
            isCharging = false;
        }

        private void CancelAim()
        {
            ResetCharge();
            if (aimIndicator != null)
            {
                aimIndicator.Hide();
            }
        }

        private void OnDisable()
        {
            CancelAim();
        }
    }
}
