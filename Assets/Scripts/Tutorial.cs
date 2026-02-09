using TMPro;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public Vector3 point1;
    public Vector3 point2;
    public Vector3 point3;

    private bool cp1;
    private bool cp2;
    private bool cp3;

    private float timer;
    private float speed = 3;

    void Start()
    {
       cp1 = true;
       cp2 = false;
       cp3 = false;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 0)
        {
            Checkpoint1();
        }
        if (timer > 4.4)
        {
            Checkpoint2();
        }
        if (timer > 13.7)
        {
            Checkpoint3();
        }
    }

    public void Checkpoint1()
    {
        if (cp1 == true)
        {
            transform.position = Vector3.MoveTowards(transform.position, point1, speed * Time.deltaTime);
            cp2 = true;
        }
    }
    public void Checkpoint2()
    {
        if (cp2 == true)
        {
            transform.position = Vector3.MoveTowards(transform.position, point2, speed * Time.deltaTime);
            cp1 = false;
            cp3 = true;
        }
    }
    public void Checkpoint3()
    {
        if (cp3 == true)
        {
            transform.position = Vector3.MoveTowards(transform.position, point3, speed * Time.deltaTime);
            cp2 = false;
        }
    }

}
