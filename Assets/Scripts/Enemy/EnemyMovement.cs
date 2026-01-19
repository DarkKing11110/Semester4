using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Android;
using UnityEngine.UIElements;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] GameObject Enemy;

    private Transform Player;

    //Enemy Values
    public float EnemySpeed;
    public float EnemyChaseSpeed;
    public float HearingValue;
    public float ChaseTimer;
    public float AttackRange;

    public Animator animator;

    public Vector3 Point;
    public bool Pointset;
    private bool ChaseState = false;
    private float Distance;

    [SerializeField] float range;

    NavMeshAgent MeshAgent;

    [SerializeField] LayerMask GroundLayer, PlayerLayer;
    private void Awake()
    {
        Player = FindAnyObjectByType<PlayerBehaviour>().transform;
        MeshAgent = GetComponent<NavMeshAgent>();

        if (animator != null)
        {
            GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        Distance = Vector3.Distance(transform.position, Player.position);
        if (ChaseState == false)
        {
            EnemyPatrol();
        }
        else
        {
            EnemyChase();
        }
    }

    public void EnemyPatrol()
    {
        MeshAgent.isStopped = false;
        MeshAgent.speed = EnemySpeed;
        if (!Pointset)
        {
            NextPoint();
        }
        if (Pointset)
        {
            MeshAgent.destination = Point;
        }
        if (Vector3.Distance(transform.position, Point) < 10)
        { 
            Pointset = false;
        }
        if (Distance <= HearingValue && CanSeePlayer())
        {
            EnemyChase();
            ChaseState = true;
        }
    }

    public void EnemyChase()
    {
        MeshAgent.isStopped = false;
        MeshAgent.speed = EnemyChaseSpeed;
        MeshAgent.destination = Player.position;
        ChaseTimer += Time.deltaTime;
        
        Distance = Vector3.Distance(transform.position, Player.position);

        if (Distance < AttackRange)
        {
            //attack and player loses 1 heart. we havent decided how many hearts b4 player dies


        }

        if (ChaseTimer > 45f)
        {
            ChaseTimer = 0;
            ChaseState = false;
        } 
    }


    private bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (Player.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, HearingValue))
        {
            return hit.transform == Player;
        }
        return false;
    }

    public void NextPoint()
    {
        float x = Random.Range(-range, range);
        float z = Random.Range(-range, range);

        Point = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);

        if (Physics.Raycast(Point, Vector3.down, GroundLayer))
        {
            Pointset = true;
        }
    }
}
