using UnityEngine;

namespace Study3D.MiniGolf.Obstacles
{
    public class AccelerationPad : MonoBehaviour
    {
        [Header("가속 발판 설정")]
        [SerializeField, Min(0f)] private float acceleration = 10f;
        [SerializeField, Min(0f)] private float minimumSpeed = 0.01f;
        [SerializeField] private LayerMask affectedLayers = ~0;

        private void OnTriggerStay(Collider other)
        {
            Rigidbody ballRigidbody = other.attachedRigidbody;

            if (ballRigidbody == null || ballRigidbody.isKinematic)
            {
                return;
            }

            int ballLayerMask = 1 << ballRigidbody.gameObject.layer;

            if ((affectedLayers.value & ballLayerMask) == 0)
            {
                return;
            }

            Vector3 currentVelocity = ballRigidbody.linearVelocity;

            // 정지한 공에는 운동 방향이 없으므로 가속하지 않습니다.
            if (currentVelocity.sqrMagnitude <= minimumSpeed * minimumSpeed)
            {
                return;
            }

            Vector3 moveDirection = currentVelocity.normalized;

            // Acceleration 모드는 질량과 무관하게 가속도를 적용합니다.
            // 시간 간격은 물리 엔진이 반영하므로 deltaTime을 곱하지 않습니다.
            ballRigidbody.AddForce(moveDirection * acceleration, ForceMode.Acceleration);
        }
    }
}
