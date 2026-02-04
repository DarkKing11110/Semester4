using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Android;
using UnityEngine.UIElements;

public class EnemyMovement : MonoBehaviour
{
    public Transform Player;

    [Header("Enemy Values")]
    public float EnemySpeed;
    public float EnemyChaseSpeed;
    public float HearingValue;
    public float ChaseTimer;
    public float AttackRange;

    public Animator animator;

    public Vector3 Point;
    public bool Pointset;
    public bool ChaseState = false;
    private float Distance;
    public float PointTimer;

    public bool isWalking;
    public bool isAttacking;
    public bool isRunning;

    [SerializeField] float range;

    NavMeshAgent MeshAgent;

    [SerializeField] LayerMask GroundLayer, PlayerLayer;

    private void Awake()
    {
        //Player = GameObject.Find("Player");
        MeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        isWalking = false;
        isAttacking = false;
        isRunning = false;

    }

    void Update()
    {
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

        isWalking = true;
        isRunning = false;
        isAttacking = false;

        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isAttacking", isAttacking);

        if (!Pointset)
        {
            NextPoint();
        }
        if (Pointset)
        {
            MeshAgent.SetDestination(Point);
            PointTimer += Time.deltaTime;
            if (PointTimer > 20)
            {
                PointTimer = 0;
                Pointset = false;
            }
        }
        if (Vector3.Distance(transform.position, Point) < 10)
        {
            Pointset = false;
        }
        if (Distance <= HearingValue && CanSeePlayer())
        {
            ChaseState = true;
            EnemyChase();
        }
    }

    public void EnemyChase()
    {
        isWalking = false;
        isRunning = true;
        isAttacking = false;

        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isAttacking", isAttacking);
        MeshAgent.isStopped = false;
        MeshAgent.speed = EnemyChaseSpeed;
        MeshAgent.destination = Player.position;
        ChaseTimer += Time.deltaTime;

        Distance = Vector3.Distance(transform.position, Player.position);

        if (Distance < AttackRange)
        {
            isWalking = false;
            isRunning = false;
            isAttacking = true;

            //attack and player loses 1 heart. we havent decided how many hearts b4 player dies
            animator.SetBool("isWalking", isWalking);
            animator.SetBool("isRunning", isRunning);
            animator.SetBool("isAttacking", isAttacking);

        }

        if (ChaseTimer > 45)
        {
            ChaseTimer = 0;
            ChaseState = false;
            EnemyPatrol();
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
            print("Point set");
        }
    }
}