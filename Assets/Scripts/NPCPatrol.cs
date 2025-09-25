using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class NPCPatrol : MonoBehaviour
{
    [Header("Patrulla")]
    public Transform[] waypoints;
    private int currentWaypoint = 0;

    [Header("Detección")]
    public float visionRange = 10f;
    public float visionAngle = 120f;
    public float loseTime = 2f;
    private float loseTimer = 0f;

    [Header("Idle")]
    public float idleTime = 1.5f;
    private float idleTimer = 0f;

    [Header("Configuración")]
    public LayerMask playerMask;
    private NavMeshAgent agent;
    private Transform player;

    private bool chasingPlayer = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToNextWaypoint();
    }

    void Update()
    {
        if (PlayerInSight())
        {
            chasingPlayer = true;
            loseTimer = 0f;
            agent.SetDestination(player.position);
        }
        else if (chasingPlayer)
        {
            loseTimer += Time.deltaTime;
            if (loseTimer >= loseTime)
            {
                chasingPlayer = false;
                GoToNextWaypoint();
            }
            else
            {
                if (player != null)
                    agent.SetDestination(player.position);
            }
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer >= idleTime)
            {
                GoToNextWaypoint();
                idleTimer = 0f;
            }
        }
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;
        agent.destination = waypoints[currentWaypoint].position;
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
    }

    bool PlayerInSight()
    {
        Collider[] targets = Physics.OverlapSphere(transform.position, visionRange, playerMask);

        foreach (Collider target in targets)
        {
            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToTarget);

            if (angle < visionAngle / 2f)
            {
                float dist = Vector3.Distance(transform.position, target.transform.position);

                if (!Physics.Raycast(transform.position + Vector3.up * 1.5f, dirToTarget, dist, ~playerMask))
                {
                    player = target.transform;
                    return true;
                }
            }
        }
        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Cargar pantalla GameOver
            SceneManager.LoadScene("GameOverScene");
        }
    }
}