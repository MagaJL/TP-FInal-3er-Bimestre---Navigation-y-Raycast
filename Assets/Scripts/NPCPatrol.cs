using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class NPCPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] waypoints;
    public float detectionRange = 10f;
    public float viewAngle = 45f;
    public float losePlayerTime = 2f;

    [Header("Chase Settings")]
    public float attackDistance = 1.5f;

    private int currentWaypoint = 0;
    private NavMeshAgent agent;
    private Animator anim;
    private Transform player;
    private float lastTimeSeenPlayer;
    private bool chasingPlayer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (waypoints.Length > 0)
            GoToNextWaypoint();
    }

    void Update()
    {
        if (chasingPlayer)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }

        DetectPlayer();
        UpdateAnimations();
    }

    #region Patrol
    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextWaypoint();
        }
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        agent.destination = waypoints[currentWaypoint].position;
        currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
    }

    void ResumePatrol()
    {
        chasingPlayer = false;

        // Encuentra el waypoint más cercano para reiniciar patrullaje
        float minDist = Mathf.Infinity;
        int nearest = 0;
        for (int i = 0; i < waypoints.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, waypoints[i].position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = i;
            }
        }

        currentWaypoint = nearest;
        GoToNextWaypoint();
    }
    #endregion

    #region Detection
    void DetectPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, direction);

        Ray ray = new Ray(transform.position + Vector3.up, direction);
        RaycastHit hit;

        if (angle < viewAngle && Physics.Raycast(ray, out hit, detectionRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                chasingPlayer = true;
                lastTimeSeenPlayer = Time.time;
            }
        }

        // Si pierde al jugador por más de losePlayerTime segundos
        if (chasingPlayer && Time.time - lastTimeSeenPlayer > losePlayerTime)
        {
            ResumePatrol();
        }
    }
    #endregion

    #region Chase
    void ChasePlayer()
    {
        agent.destination = player.position;

        if (Vector3.Distance(transform.position, player.position) < attackDistance)
        {
            // Trigger de ataque antes de cambiar escena
            anim.SetTrigger("attack");
            Invoke("GameOver", 0.5f);
        }
    }

    void GameOver()
    {
        SceneManager.LoadScene("GameOverScene");
    }
    #endregion

    #region Animations
    void UpdateAnimations()
    {
        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f && !chasingPlayer);
        anim.SetBool("isChasing", chasingPlayer);
    }
    #endregion
}