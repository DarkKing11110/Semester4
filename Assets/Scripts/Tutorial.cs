using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Tutorial : MonoBehaviour
{
    private Vector3 point1 = new Vector3(0, 0, -14.5f);
    private Vector3 point2 = new Vector3(0, 0, -5f);
    private Vector3 point3 = new Vector3 (25.5f, 0, -5);
    private Vector3 point4 = new Vector3(25.5f, 0, -14);

    public ElectricFlow flow;
    public GameObject TutorialElliot;

    private bool Entered = false;

    private bool cp1;
    private bool cp2;
    private bool cp3;

    public Animator Animator;
    public NavMeshAgent MeshAgent;

    public StoryManager StoryManager;

    public float timer;
    private float Firstspeed = 1;
    private float speed = 3;

    void Start()
    {
        cp1 = true;
        cp2 = false;
        cp3 = false;
    }

    void Update()
    {
        if (Entered == true)
        {
            MeshAgent = GetComponentInChildren<NavMeshAgent>();
            Animator = GetComponentInChildren<Animator>();
            TutorialElliot.SetActive(true);
            timer += Time.deltaTime;
            if (timer > 5)
            {
                Checkpoint1();
            }
            if (timer > 9.4)
            {
                Checkpoint2();
            }
            if (timer > 18.7)
            {
                Checkpoint3();
            }
        }       
    }

    private void OnTriggerEnter(Collider other)
    {
        if (flow.Completed == true)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Entered = true;
            }
        }
            
        
    }

    public void OpenDoor()
    {
        MeshAgent.isStopped = false;
        MeshAgent.speed = Firstspeed;
        MeshAgent.destination = point1;
    }

    public void Checkpoint1()
    {
        if (cp1 == true)
        {
            Debug.Log("Going to point 2");
            MeshAgent.speed = speed;
            MeshAgent.destination = point2;
            cp2 = true;
        }
    }
    public void Checkpoint2()
    {
        Debug.Log("Going to Point 3");
        if (cp2 == true)
        {
            MeshAgent.speed = speed;
            MeshAgent.destination = point3;
            cp1 = false;
            cp3 = true;
        }
    }
    public void Checkpoint3()
    {
        if (cp3 == true)
        {
            MeshAgent.speed = speed;
            MeshAgent.destination = point4;
            cp2 = false;
        }
    }

}