using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Android;
using UnityEngine.UIElements;

public class EnemyMovement : MonoBehaviour
{
    public Transform Player;

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
        //Player = GameObject.Find("Player");
        MeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();         
    }

    void Update()
    {
        EnemyPatrol();
    }

    public void EnemyPatrol()
    {
        MeshAgent.isStopped = false;
        MeshAgent.speed = EnemySpeed;

        animator.SetBool("iswalking", true);
        animator.SetBool("isrunning", false);
        animator.SetBool("isattacking", false);

        if (!Pointset)
        {
            NextPoint();
        }
        if (Pointset)
        {
            print("Going to the point");
            MeshAgent.SetDestination(Point);
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
        animator.SetBool("iswalking", false);
        animator.SetBool("isrunning", true);
        animator.SetBool("isattacking", false);
        MeshAgent.isStopped = false;
        MeshAgent.speed = EnemyChaseSpeed;
        MeshAgent.destination = Player.position;
        ChaseTimer += Time.deltaTime;

        Distance = Vector3.Distance(transform.position, Player.position);

        if (Distance < AttackRange)
        {
            //attack and player loses 1 heart. we havent decided how many hearts b4 player dies
            animator.SetBool("iswalking", false);
            animator.SetBool("isrunning", false);
            animator.SetBool("isattacking", true);

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
            print("Point set");
        }
    }
}