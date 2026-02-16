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
    private float Distance;
    public float PointTimer;
    public bool Chase = false;

    public bool isWalking;
    public bool isAttacking;
    public bool isRunning;

    [SerializeField] float range;

    private enum State { Patrol, Chase, Attack}
    private State CurrentState;

    NavMeshAgent MeshAgent;

    [SerializeField] LayerMask GroundLayer, PlayerLayer;

    [SerializeField]
    private EnemySounds enemySounds; // Audio

    private void Awake()
    {
        //Player = GameObject.Find("Player");
        MeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        isWalking = false;
        isAttacking = false;
        isRunning = false;

        CurrentState = State.Patrol;
    }

    void Update()
    {
        Distance = Vector3.Distance(transform.position, Player.position);

        if (Distance <= AttackRange && CanSeePlayer())
        {
            CurrentState = State.Attack;
        }
        else if (Distance <= HearingValue && CanSeePlayer() || Chase == true)
        {
            CurrentState = State.Chase;
        }
        else
        {
            CurrentState = State.Patrol;
        }
        
        switch (CurrentState)
        {
            case State.Patrol:
                EnemyPatrol();
                break;
            case State.Chase:
                EnemyChase();
                break;
            case State.Attack:
                AttackPlayer();
                break;
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
        
    }

    public void EnemyChase()
    {
        isWalking = false;
        isRunning = true;
        isAttacking = false;

        Chase = true;
      
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isAttacking", isAttacking);
        MeshAgent.isStopped = false;
        MeshAgent.speed = EnemyChaseSpeed;
        MeshAgent.destination = Player.position;
        ChaseTimer += Time.deltaTime;

        Distance = Vector3.Distance(transform.position, Player.position);

        if (ChaseTimer > 5)
        {
            ChaseTimer = 0;
            Chase = false;
            CurrentState = State.Patrol;
        }
    }

    public void AttackPlayer()
    {
        isWalking = false;
        isRunning = false;
        isAttacking = true;

        //attack and player loses 1 heart. we havent decided how many hearts b4 player dies
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isAttacking", isAttacking);
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
    private void PlayFootstep()
    {
        enemySounds.PlayFootsteps();
    }

    void UpdateBreathing()
    {
        if (isRunning)
            enemySounds.SetBreathState(EnemySounds.MonsterBreathState.Running);
        else
           enemySounds.SetBreathState(EnemySounds.MonsterBreathState.Walking);
    }
}

public enum EnemyMovementType
{
    Walking = 0,
    Running = 1,
    Attacking = 2,
}