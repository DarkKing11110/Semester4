using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingScene : MonoBehaviour
{
    [SerializeField] GameObject Elliot;
    [SerializeField] GameObject Kai;

    [SerializeField] Camera MainCamera;
    [SerializeField] Camera EndingCamera;

    public GameObject Door;

    public doorFunction doorfunction;
    public StoryManager storyManager;

    public bool KaiAnimations = false;
    public bool ElliotAnimations = false;
    public Animator Kaianimator;
    public Animator Elliotanimator;
    public float Timer;
    public int index;
    private bool timerstart = false;

    private void Start()
    {
        Kaianimator = GetComponentInChildren<Animator>();
        Elliotanimator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        index = storyManager.CurrentStepIndex;
        if (timerstart == true)
        {
            Timer += Time.deltaTime;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (storyManager.CurrentStepIndex == 16)
        {
            MainCamera.enabled = false;
            timerstart = true;
            if (Timer > 2.5)
            {
                Kai.SetActive(true);
                Elliot.SetActive(true);
                if (Timer < 3)
                {
                    EndingCamera.enabled = true;
                }
                if (Timer > 5)
                {
                    SceneManager.LoadScene(2);
                }
            }
        }
    }
}
