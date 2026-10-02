using UnityEngine;

namespace Study3D.MiniGolf.Obstacles
{
    public class DoorTrigger : MonoBehaviour
    {
        [Header("열 문")]
        [SerializeField] private Animator doorAnimator;

        [Header("감지 설정")]
        [SerializeField] private string ballTag = "GolfBall";

        private static readonly int OpenParameter = Animator.StringToHash("Open");
        private bool isOpened;

        private void OnTriggerEnter(Collider other)
        {
            if (isOpened || doorAnimator == null)
            {
                return;
            }

            // Collider가 자식에 있어도 부모의 Rigidbody를 찾습니다.
            Rigidbody ballRigidbody = other.attachedRigidbody;

            if (ballRigidbody == null)
            {
                return;
            }

            if (ballRigidbody.gameObject.CompareTag(ballTag) == false)
            {
                return;
            }

            doorAnimator.SetTrigger(OpenParameter);
            isOpened = true;
        }
    }
}
