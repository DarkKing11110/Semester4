using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    private Vector3 point1 = new Vector3(0, 0, -14.5f);
    private Vector3 point2 = new Vector3(0, 0, -5f);
    private Vector3 point3 = new Vector3 (25.5f, 0, -5);
    private Vector3 point4 = new Vector3(25.5f, 0, -14);

    private bool cp1;
    private bool cp2;
    private bool cp3;

    public Animator animator;

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
        transform.position = Vector3.MoveTowards(transform.position, point3, speed * Time.deltaTime);

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

    public void Checkpoint1()
    {
        if (cp1 == true)
        {
            Debug.Log("Going to point 2");
            transform.position = Vector3.MoveTowards(transform.position, point2, speed * Time.deltaTime);
            
            cp2 = true;
        }
    }
    public void Checkpoint2()
    {
        Debug.Log("Going to Point 3");
        if (cp2 == true)
        {
            transform.position = Vector3.MoveTowards(transform.position, point3, speed * Time.deltaTime);
            //transform.rotation = quaternion.Euler(0, 90, 0);
            cp1 = false;
            cp3 = true;
        }
    }
    public void Checkpoint3()
    {
        if (cp3 == true)
        {           
            transform.position = Vector3.MoveTowards(transform.position, point4, speed * Time.deltaTime);
            //transform.rotation = quaternion.Euler(0, 180, 0);
            cp2 = false;
        }
    }

}