using UnityEngine;

namespace Study3D.MiniGolf.Obstacles
{
    public class Trampoline : MonoBehaviour
    {
    
    
        [Header("트램펄린 설정")]
        [SerializeField] private float bounceForce = 15f; // 튀어 오르는 힘의 크기

        private void OnCollisionEnter(Collision collision)
        {
            // 부딪힌 오브젝트의 Rigidbody 컴포넌트 가져오기
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // 1. 공이 떨어지던 하강 속도(Y축)를 0으로 리셋
                // (낙하 속도가 중첩되는 것을 막아 항상 일정한 높이로 부드럽게 점프하게 합니다)
                Vector3 currentVel = rb.linearVelocity; // 유니티 최신 버전은 linearVelocity
                currentVel.y = 0f;
                rb.linearVelocity = currentVel;

                // 2. 위쪽 방향(Vector3.up)으로 순간적인 충격량(Impulse) 추가
                rb.AddForce(Vector3.up * bounceForce, ForceMode.Impulse);
            }
        }
    
    }
}