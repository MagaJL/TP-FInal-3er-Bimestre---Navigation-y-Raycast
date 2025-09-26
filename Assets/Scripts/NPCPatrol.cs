using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class NPCPatrol : MonoBehaviour
{
    [Header("Patrulla")]
    public Transform[] waypoints;
    int destinoActual = 0;

    [Header("Jugador")]
    public Transform jugador;
    public Transform vistaNPC;
    public float visionDistancia = 10f;
    public float visionAngulo = 60f;
    public float distanciaPerseguir = 1.5f;

    [Header("Persecución")]
    public float tiempoPerdida = 2f;
    float tiempoSinVer = 0f;
    bool persiguiendo = false;

    NavMeshAgent agente;
    Animator anim;

    void Awake()
    {
        agente = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (waypoints.Length > 0)
            agente.SetDestination(waypoints[0].position);
    }

    void Update()
    {
        anim.SetFloat("Speed", agente.velocity.magnitude);

        if (persiguiendo) PerseguirJugador();
        else { Patrullar(); DetectarJugador(); }
    }

    void Patrullar()
    {
        if (waypoints.Length == 0) return;

        if (!agente.pathPending && agente.remainingDistance < 0.5f)
        {
            destinoActual = (destinoActual + 1) % waypoints.Length;
            agente.SetDestination(waypoints[destinoActual].position);
        }
    }

    void DetectarJugador()
    {
        Vector3 dir = (jugador.position - vistaNPC.position).normalized;
        float dist = Vector3.Distance(jugador.position, vistaNPC.position);

        if (dist <= visionDistancia && Vector3.Angle(vistaNPC.forward, dir) < visionAngulo)
        {
            if (Physics.Raycast(vistaNPC.position, dir, out RaycastHit hit, visionDistancia) &&
                hit.collider.CompareTag("Player"))
            {
                persiguiendo = true;
                tiempoSinVer = 0f;
            }
        }
    }

    void PerseguirJugador()
    {
        agente.SetDestination(jugador.position);

        if (Vector3.Distance(transform.position, jugador.position) <= distanciaPerseguir)
            SceneManager.LoadScene("GameOverScene");

        Vector3 dir = (jugador.position - vistaNPC.position).normalized;
        if (Physics.Raycast(vistaNPC.position, dir, out RaycastHit hit, visionDistancia) && hit.collider.CompareTag("Player"))
            tiempoSinVer = 0f;
        else
        {
            tiempoSinVer += Time.deltaTime;
            if (tiempoSinVer >= tiempoPerdida)
            {
                persiguiendo = false;
                agente.SetDestination(waypoints[destinoActual].position);
            }
        }
    }
}