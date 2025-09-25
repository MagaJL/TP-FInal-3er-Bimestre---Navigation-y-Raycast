using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class NPCPatrol : MonoBehaviour
{
    public Transform[] waypoints;
    public float visionRange = 10f;
    public float visionAngle = 120f;
    public float loseTime = 2f;
    public LayerMask playerMask;

    private NavMeshAgent agent;
    private Animator anim;
    private Transform player;
    private int wpIndex = 0;
    private float loseTimer = 0;
    private bool chasing = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        GoToNextPoint();
    }

    void Update()
    {
        if (CanSeePlayer())
        {
            chasing = true;
            loseTimer = 0;
            agent.SetDestination(player.position);
        }
        else if (chasing)
        {
            loseTimer += Time.deltaTime;
            if (loseTimer >= loseTime)
            {
                chasing = false;
                GoToNextPoint();
            }
            else
                agent.SetDestination(player.position);
        }
        else if (!agent.pathPending && agent.remainingDistance < 0.2f)
        {
            GoToNextPoint();
        }

        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f);
    }

    bool CanSeePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);

        if (Vector3.Distance(transform.position, player.position) <= visionRange && angle < visionAngle * 0.5f)
        {
            if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, visionRange, ~0))
                return hit.collider.CompareTag("Player");
        }
        return false;
    }

    void GoToNextPoint()
    {
        if (waypoints.Length == 0) return;
        agent.destination = waypoints[wpIndex].position;
        wpIndex = (wpIndex + 1) % waypoints.Length;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.collider.CompareTag("Player"))
            SceneManager.LoadScene("GameOverScene");
    }
}