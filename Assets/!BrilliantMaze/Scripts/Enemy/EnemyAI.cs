using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    [Header("Маршрут")]
    [SerializeField] private Transform[] _waypoints;
    [SerializeField] private float _waitTime = 1f;

    [Header("Обнаружение")]
    [SerializeField] private float _sightRange = 10f;
    [SerializeField] private float _sightAngle = 90f;
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private float _closeRange = 3f;

    [Header("Погоня")]
    [SerializeField] private float _chaseSpeed = 5f;
    [SerializeField] private float _losePlayerDistance = 15f;
    [SerializeField] private float _killDistance = 1.5f;

    [Header("GameManager")]
    [SerializeField] private GameManager _gameManager;

    private NavMeshAgent _agent;
    private Animator _animator;
    private int _currentWaypoint;
    private float _waitTimer;
    private bool _isChasing;
    private Transform _player;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Start()
    {
        if (_waypoints.Length > 0)
            GoToWaypoint(0);
    }

    private void Update()
    {
        UpdateAnimations();

        if (_isChasing)
        {
            ChasePlayer();
        }
        else
        {
            CheckForPlayer();
            Patrol();
        }
    }

    private void UpdateAnimations()
    {
        float speed = _agent.velocity.magnitude;
        _animator.SetFloat(SpeedHash, speed);
    }

    private void Patrol()
    {
        if (_waypoints.Length == 0) return;

        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= _waitTime)
            {
                _waitTimer = 0f;
                _currentWaypoint = (_currentWaypoint + 1) % _waypoints.Length;
                GoToWaypoint(_currentWaypoint);
            }
        }
    }

    private void GoToWaypoint(int index)
    {
        _agent.SetDestination(_waypoints[index].position);
    }

    private void CheckForPlayer()
    {
        if (_player == null) return;

        Vector3 dirToPlayer = _player.position - transform.position;
        float dist = dirToPlayer.magnitude;

        if (dist <= _closeRange)
        {
            _isChasing = true;
            _agent.speed = _chaseSpeed;
            return;
        }

        if (dist > _sightRange) return;

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > _sightAngle * 0.5f) return;

        if (Physics.Raycast(transform.position + Vector3.up, dirToPlayer.normalized, dist, _obstacleMask))
            return;

        _isChasing = true;
        _agent.speed = _chaseSpeed;
    }

    private void ChasePlayer()
    {
        if (_player == null) { LosePlayer(); return; }

        float dist = Vector3.Distance(transform.position, _player.position);

        if (dist <= _killDistance)
        {
            _gameManager.ReportDeath();
            return;
        }

        _agent.isStopped = false;

        if (dist > _losePlayerDistance) { LosePlayer(); return; }

        _agent.SetDestination(_player.position);
    }

    private void LosePlayer()
    {
        _isChasing = false;
        _agent.isStopped = false;
        GoToWaypoint(_currentWaypoint);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _sightRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _closeRange);

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, _killDistance);

        Vector3 left = Quaternion.Euler(0, -_sightAngle * 0.5f, 0) * transform.forward * _sightRange;
        Vector3 right = Quaternion.Euler(0, _sightAngle * 0.5f, 0) * transform.forward * _sightRange;
        Gizmos.DrawRay(transform.position, left);
        Gizmos.DrawRay(transform.position, right);
    }
}
