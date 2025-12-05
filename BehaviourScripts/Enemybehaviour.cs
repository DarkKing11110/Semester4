using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemyBehaviour : MonoBehaviour
{
    public List<PatrolPath> patrolRoutes; // Liste von Patrouillenrouten
    public Transform player; // Referenz zum Spieler
    public float chaseRange = 10f; // Sichtweite, um den Spieler zu verfolgen
    public float hearingRange = 15f; // Hörreichweite
    public float attackRange = 3f; // Reichweite, um den Spieler anzugreifen
    public float fleeHealthThreshold = 20f; // Gesundheitsschwelle zum Wegrennen
    public float fleeDistance = 10f; // Distanz zum Weglaufen
    public int maxHealth = 100; // Maximale Gesundheit
    public float routeSwitchInterval = 30f; // Intervall für Routenwechsel
    public int currentHealth; // Aktuelle Gesundheit des Gegners

    [Header("Geschwindigkeiten")]
    public float normalSpeed = 3.5f;
    public float chaseSpeed = 6f; // Im Editor anpassbar

    [Header("Versteck-Logik")]
    public LayerMask hidingLayerMask; // Layer für Verstecke
    public float lostInterestTime = 5f; // Zeit, bis das Interesse verloren geht

    [Header("Angriff")]
    public int attackDamage = 20;
    public float attackCooldown = 1.2f; // Sekunden zwischen Angriffen

    private float lastAttackTime = -999f;


    private int currentPatrolIndex;
    private NavMeshAgent agent;
    private Animator animator;
    private bool isFleeing;
    private Transform[] patrolPoints;
    private PatrolPath currentPatrolRoute;
    private float routeSwitchTimer;

    private float hidingTimer = 0f;
    private bool playerIsHidden = false;

    private enum State { Patrolling, Chasing, Attacking, Fleeing }
    private State currentState;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        agent.speed = normalSpeed;

        if (patrolRoutes != null && patrolRoutes.Count > 0)
        {
            SwitchToRandomPatrolRoute();
        }

        currentState = State.Patrolling;
        GoToNextPatrolPoint();
    }


    void Update()
    {
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Versteck-Logik: Prüfen, ob Spieler versteckt ist
        if ((currentState == State.Chasing || currentState == State.Attacking) && IsPlayerHidden())
        {
            hidingTimer += Time.deltaTime;
            if (hidingTimer >= lostInterestTime)
            {
                currentState = State.Patrolling;
                GoToNextPatrolPoint();
                playerIsHidden = false;
                hidingTimer = 0f;
                return;
            }
        }
        else
        {
            hidingTimer = 0f;
        }

        // State-Wechsel nur hier!
        if (currentHealth <= fleeHealthThreshold && distanceToPlayer <= chaseRange)
        {
            currentState = State.Fleeing;
        }
        else if (distanceToPlayer <= attackRange && CanSeePlayer())
        {
            currentState = State.Attacking;
        }
        else if (distanceToPlayer <= chaseRange && CanSeePlayer())
        {
            currentState = State.Chasing;
        }
        else
        {
            currentState = State.Patrolling;
        }

        switch (currentState)
        {
            case State.Patrolling:
                HandlePatrollingState(distanceToPlayer);
                break;
            case State.Chasing:
                HandleChasingState(distanceToPlayer);
                break;
            case State.Attacking:
                HandleAttackingState(distanceToPlayer);
                break;
            case State.Fleeing:
                HandleFleeingState();
                break;
        }

        // Routenwechsel-Logik
        routeSwitchTimer += Time.deltaTime;
        if (routeSwitchTimer >= routeSwitchInterval)
        {
            SwitchToRandomPatrolRoute();
            routeSwitchTimer = 0f;
        }
    }



    private void HandlePatrollingState(float distanceToPlayer)
    {
        agent.isStopped = false;
        agent.speed = normalSpeed;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPatrolPoint();
        }
        animator.SetBool("isWalking", true);
        animator.SetBool("isRunning", false);
        animator.SetBool("isAttacking", false);
    }

    private void HandleChasingState(float distanceToPlayer)
    {
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.destination = player.position;
        animator.SetBool("isRunning", true);
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);
    }

    private void HandleAttackingState(float distanceToPlayer)
    {
        agent.isStopped = true;
        animator.SetBool("isAttacking", true);
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
    }



    private void HandleFleeingState()
    {
        agent.isStopped = false;
        FleeFromPlayer();
    }

    void GoToNextPatrolPoint()
    {
        agent.isStopped = false;
        agent.speed = normalSpeed;
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Debug.Log("Gehe zu PatrolPoint: " + patrolPoints[currentPatrolIndex].name);
        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        animator.SetBool("isWalking", true);
        animator.SetBool("isRunning", false);
        animator.SetBool("isAttacking", false);
    }

    void ChasePlayer()
    {
        agent.isStopped = false;
        agent.speed = chaseSpeed;
        agent.destination = player.position;
        animator.SetBool("isRunning", true);
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);
    }

    void AttackPlayer()
    {
        // Animation-Flags werden in HandleAttackingState gesetzt
        // Schaden wird jetzt durch Animation Event ausgeführt!
    }

    public void DealDamageToPlayer()
    {
        Debug.Log("Schaden Event!");

        PlayerBehaviour playerBehaviour = null;

        if (player != null)
        {
            playerBehaviour = player.GetComponent<PlayerBehaviour>();
            if (playerBehaviour == null)
            {
                playerBehaviour = player.GetComponentInChildren<PlayerBehaviour>();
                if (playerBehaviour == null)
                    playerBehaviour = player.GetComponentInParent<PlayerBehaviour>();
            }
        }

        if (playerBehaviour != null && Time.time - lastAttackTime >= attackCooldown)
        {
            playerBehaviour.TakeDamage(attackDamage);
            Debug.Log("Spieler erhält Schaden: " + attackDamage);
            lastAttackTime = Time.time;
        }
        else if (playerBehaviour == null)
        {
            Debug.LogWarning("PlayerBehaviour-Script nicht gefunden!");
        }
    }







    void FleeFromPlayer()
    {
        agent.isStopped = false;
        agent.speed = normalSpeed;
        isFleeing = true;
        Vector3 fleeDirection = (transform.position - player.position).normalized;
        Vector3 fleePosition = transform.position + fleeDirection * fleeDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(fleePosition, out hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.destination = hit.position;
        }
        else
        {
            fleeDirection = Quaternion.Euler(0, 90, 0) * fleeDirection;
            fleePosition = transform.position + fleeDirection * fleeDistance;
            if (NavMesh.SamplePosition(fleePosition, out hit, fleeDistance, NavMesh.AllAreas))
            {
                agent.destination = hit.position;
            }
        }

        animator.SetBool("isWalking", true);
        animator.SetBool("isAttacking", false);

        Invoke("StopFleeing", 5f);
    }

    void StopFleeing()
    {
        isFleeing = false;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        animator.SetTrigger("takeDamage");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        animator.SetTrigger("isDead");
        agent.isStopped = true;
        Destroy(gameObject, 2f);
    }

    private bool IsPlayerRunning()
    {
        //Überprüfen, ob der Spieler rennt
        PlayerBehaviour playerMovement = player.GetComponent<PlayerBehaviour>();
        return playerMovement != null && playerMovement.isRunning;
    }

    private bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, chaseRange))
        {
            return hit.transform == player;
        }
        return false;
    }

    // Prüft, ob der Spieler unter einem Versteck-Objekt ist
    private bool IsPlayerHidden()
    {
        // Raycast von oben nach unten auf den Spieler, prüft auf hidingLayerMask
        Ray ray = new Ray(player.position + Vector3.up * 0.5f, Vector3.up);
        float maxHideCheck = 5f;
        return Physics.Raycast(ray, maxHideCheck, hidingLayerMask);
    }

    private void SwitchToRandomPatrolRoute()
    {
        if (patrolRoutes == null || patrolRoutes.Count == 0)
            return;

        // Wenn es nur eine Route gibt, einfach diese nehmen
        if (patrolRoutes.Count == 1)
        {
            currentPatrolRoute = patrolRoutes[0];
        }
        else
        {
            // Wähle eine andere Route als die aktuelle
            int currentIndex = patrolRoutes.IndexOf(currentPatrolRoute);
            int newIndex = currentIndex;
            int safety = 0; // Schutz vor Endlosschleife

            while (newIndex == currentIndex && safety < 10)
            {
                newIndex = Random.Range(0, patrolRoutes.Count);
                safety++;
            }
            currentPatrolRoute = patrolRoutes[newIndex];
        }

        patrolPoints = currentPatrolRoute.patrolPoints.ToArray();
        currentPatrolIndex = 0;
        GoToNextPatrolPoint();
    }

}
