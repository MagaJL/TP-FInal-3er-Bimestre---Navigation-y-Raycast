using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class NPCPatrol : MonoBehaviour
{
    public Transform[] waypoints;
    public Transform player;
    public float visionRange = 10f;
    public float visionAngle = 60f;
    public float loseTime = 2f;
    public float idleTime = 1.5f;

    private NavMeshAgent agent;
    private Animator anim;

    private int currentIndex;
    private bool waiting;
    private float loseTimer;
    private enum State { Patrol, Chase }
    private State state = State.Patrol;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        GoToNextPoint();
    }

    void Update()
    {
        switch (state)
        {
            case State.Patrol:
                PatrolBehaviour();
                DetectPlayer();
                break;

            case State.Chase:
                ChaseBehaviour();
                break;
        }

        // Animación caminar
        anim.SetBool("isWalking", agent.velocity.magnitude > 0.1f);
    }

    // --- PATRULLA ---
    void PatrolBehaviour()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.3f && !waiting)
            StartCoroutine(WaitAndGo());
    }

    IEnumerator WaitAndGo()
    {
        waiting = true;
        anim.SetBool("isWalking", false);
        yield return new WaitForSeconds(idleTime);
        GoToNextPoint();
        waiting = false;
    }

    void GoToNextPoint()
    {
        if (waypoints.Length == 0) return;
        agent.destination = waypoints[currentIndex].position;
        currentIndex = (currentIndex + 1) % waypoints.Length;
    }

    // --- DETECCIÓN ---
    void DetectPlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (Vector3.Distance(transform.position, player.position) < visionRange && angle < visionAngle)
        {
            if (Physics.Raycast(transform.position + Vector3.up, dirToPlayer, out RaycastHit hit, visionRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    state = State.Chase;
                    loseTimer = 0;
                }
            }
        }
    }

    // --- PERSECUCIÓN ---
    void ChaseBehaviour()
    {
        if (player == null) return;
        agent.destination = player.position;

        float distance = Vector3.Distance(transform.position, player.position);

        // Si alcanza al jugador
        if (distance < 1.5f)
        {
            SceneManager.LoadScene("GameOverScene"); // crea escena GameOver con UI
        }

        // Si no lo ve
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (Vector3.Distance(transform.position, player.position) > visionRange || angle > visionAngle)
        {
            loseTimer += Time.deltaTime;
            if (loseTimer > loseTime)
            {
                state = State.Patrol;
                GoToNextPoint();
            }
        }
        else
        {
            loseTimer = 0;
        }
    }
}