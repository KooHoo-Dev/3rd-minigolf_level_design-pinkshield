using UnityEngine;
using UnityEngine.AI;

namespace Study3D.MiniGolf.Obstacles
{
    [RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody))]
    public class CatController : MonoBehaviour
    {
        private enum CatState { Wander, Chase }

        [Header("연결")]
        [SerializeField] private Transform ball;
        [SerializeField] private Animator catAnimator;

        [Header("배회")]
        [SerializeField, Min(0.1f)] private float wanderRadius = 4f;
        [SerializeField, Min(0.1f)] private float wanderSpeed = 1f;
        [SerializeField, Min(0f)] private float waitTime = 2f;

        [Header("추적")]
        [SerializeField, Min(0.1f)] private float detectionRadius = 3f;
        [SerializeField, Min(0.1f)] private float loseRadius = 5f;
        [SerializeField, Min(0.1f)] private float chaseSpeed = 2f;
        [SerializeField, Min(0.1f)] private float stopDistance = 0.5f;
        [SerializeField, Min(0.1f)] private float maxChaseFromSpawn = 8f;
        [SerializeField, Min(0.1f)] private float maxHeightDifference = 1f;
        [SerializeField, Min(0.05f)] private float repathInterval = 0.25f;

        [Header("리셋 후 다시 추적할 조건")]
        [SerializeField, Min(0.05f)] private float ballMoveToRearm = 0.3f;

        private const int SampleAttempts = 12;
        private const float SampleDistance = 1f;
        private const float MovingThreshold = 0.02f;
        private const float StuckTimeout = 3f;

        private NavMeshAgent agent;
        private NavMeshPath path;
        private Vector3 spawnPosition;
        private Vector3 resetBallPosition;
        private CatState state;
        private bool waitForBallMovement;
        private float waitRemaining;
        private float repathRemaining;
        private float stuckTime;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            Rigidbody body = GetComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            path = new NavMeshPath();

            if (catAnimator == null)
            {
                catAnimator = GetComponentInChildren<Animator>();
            }

            if (catAnimator != null)
            {
                catAnimator.applyRootMotion = false;
            }
        }

        private void Start()
        {
            if (agent.isOnNavMesh == false)
            {
                Debug.LogError("고양이를 Bake된 NavMesh 위에 배치해주세요.", this);
                enabled = false;
                return;
            }

            spawnPosition = transform.position;
            EnterWander();
        }

        private void Update()
        {
            if (agent.isOnNavMesh == false)
            {
                return;
            }

            if (waitForBallMovement && ball != null)
            {
                if ((ball.position - resetBallPosition).sqrMagnitude >= ballMoveToRearm * ballMoveToRearm)
                {
                    waitForBallMovement = false;
                }
            }

            float distanceToBall = ball != null
                ? Vector3.Distance(transform.position, ball.position) : float.PositiveInfinity;
            bool canChase = ball != null && waitForBallMovement == false
                && Mathf.Abs(ball.position.y - transform.position.y) <= maxHeightDifference
                && Vector3.Distance(ball.position, spawnPosition) <= maxChaseFromSpawn;

            if (state == CatState.Wander && canChase && distanceToBall <= detectionRadius)
            {
                state = CatState.Chase;
                agent.ResetPath();
                agent.speed = chaseSpeed;
                agent.stoppingDistance = stopDistance;
                repathRemaining = 0f;
                stuckTime = 0f;
            }
            else if (state == CatState.Chase && (canChase == false || distanceToBall >= loseRadius))
            {
                EnterWander();
            }

            if (state == CatState.Chase)
            {
                UpdateChase();
            }
            else
            {
                UpdateWander();
            }

            // 현재 에셋이 걷기 클립만 가진 경우: 이동 중 재생, 정지 중 멈춤.
            if (catAnimator != null)
            {
                catAnimator.speed = agent.velocity.sqrMagnitude > MovingThreshold * MovingThreshold ? 1f : 0f;
            }
        }

        private void UpdateChase()
        {
            if (Vector3.Distance(transform.position, ball.position) <= stopDistance)
            {
                agent.ResetPath();
                stuckTime = 0f;
                return;
            }

            stuckTime = agent.velocity.sqrMagnitude <= MovingThreshold * MovingThreshold
                ? stuckTime + Time.deltaTime : 0f;

            if (stuckTime >= StuckTimeout)
            {
                EnterWander();
                waitForBallMovement = true;
                resetBallPosition = ball.position;
                return;
            }

            repathRemaining -= Time.deltaTime;

            if (repathRemaining > 0f)
            {
                return;
            }

            repathRemaining = repathInterval;

            if (TrySetDestination(ball.position) == false)
            {
                // 닫힌 문 뒤, 다른 층 등 도달할 수 없는 공은 추적하지 않음.
                EnterWander();
                waitForBallMovement = true;
                resetBallPosition = ball.position;
            }
        }

        private void UpdateWander()
        {
            if (agent.pathPending)
            {
                return;
            }

            if (agent.hasPath && agent.remainingDistance > agent.stoppingDistance + MovingThreshold)
            {
                stuckTime = agent.velocity.sqrMagnitude <= MovingThreshold * MovingThreshold
                    ? stuckTime + Time.deltaTime : 0f;

                if (stuckTime < StuckTimeout)
                {
                    return;
                }

                agent.ResetPath();
                stuckTime = 0f;
            }

            waitRemaining -= Time.deltaTime;

            if (waitRemaining > 0f)
            {
                return;
            }

            waitRemaining = waitTime;

            for (int i = 0; i < SampleAttempts; i++)
            {
                Vector2 randomPoint = Random.insideUnitCircle * wanderRadius;
                Vector3 candidate = spawnPosition + new Vector3(randomPoint.x, 0f, randomPoint.y);

                if (TrySetDestination(candidate))
                {
                    return;
                }
            }
        }

        private bool TrySetDestination(Vector3 destination)
        {
            if (NavMesh.SamplePosition(destination, out NavMeshHit hit, SampleDistance, agent.areaMask) == false)
            {
                return false;
            }

            if (Mathf.Abs(hit.position.y - spawnPosition.y) > maxHeightDifference)
            {
                return false;
            }

            if (state == CatState.Wander)
            {
                Vector3 fromSpawn = hit.position - spawnPosition;
                fromSpawn.y = 0f;

                if (fromSpawn.sqrMagnitude > wanderRadius * wanderRadius)
                {
                    return false;
                }
            }

            if (agent.CalculatePath(hit.position, path) == false || path.status != NavMeshPathStatus.PathComplete)
            {
                return false;
            }

            return agent.SetPath(path);
        }

        private void EnterWander()
        {
            state = CatState.Wander;
            agent.ResetPath();
            agent.speed = wanderSpeed;
            agent.stoppingDistance = 0.1f;
            waitRemaining = 0f;
            stuckTime = 0f;
        }

        // 공을 리셋/순간이동한 직후 외부 리셋 코드에서 호출하세요.
        public void NotifyBallReset()
        {
            if (ball != null)
            {
                resetBallPosition = ball.position;
                waitForBallMovement = true;
            }

            if (agent != null && agent.isOnNavMesh)
            {
                EnterWander();
            }
        }

        private void OnDisable()
        {
            if (agent != null && agent.isOnNavMesh)
            {
                agent.ResetPath();
            }

            if (catAnimator != null)
            {
                catAnimator.speed = 1f;
            }
        }
    }
}
