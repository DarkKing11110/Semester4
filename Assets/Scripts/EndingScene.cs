using UnityEditor;
using UnityEngine;

public class EndingScene : MonoBehaviour
{
    [SerializeField] GameObject Elliot;
    [SerializeField] GameObject Kai;

    public GameObject Door;

    public doorFunction doorfunction;
    public StoryManager storyManager;

    public bool KaiAnimations = false;
    public bool ElliotAnimations = false;
    public Animator Kaianimator;
    public Animator Elliotanimator;
    public float Timer;

    private void Start()
    {
        Kaianimator = GetComponentInChildren<Animator>();
        Elliotanimator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            {
                Kai.SetActive(true);
                Elliot.SetActive(true);

                Timer += Time.deltaTime;

                if (Timer > 3f)
                {
                    doorfunction._isOpen = true;
                    if (Timer > 5)
                    {

                    }
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        //if (storyManager.currentStepIndex == 17)
        if (Input.GetKey(KeyCode.E))
        {
            {
                Kai.SetActive(true);
                Elliot.SetActive(true);


                Timer += Time.deltaTime;

                if (Timer > 3f)
                {
                    doorfunction.editorForceOpen = true;
                    if (Timer > 5)
                    {

                    }
                }
            }
        }
    }
}
