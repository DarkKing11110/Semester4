using Unity.VisualScripting;
using UnityEngine.EventSystems;
using UnityEngine;

public class Wires : MonoBehaviour, IPointerDownHandler
{
    float[] rotations = { 0, 90, 180, 270 };

    public float[] CorrectRotations;
    public bool isPlaced = false;
    [SerializeField] GameObject FlowObject;
    public ElectricFlow flow;

    int NumberofCorrectRotations = 1;

    void Awake()
    {
        flow = FlowObject.GetComponent<ElectricFlow>();
    }
    void Start()
    {
        NumberofCorrectRotations = CorrectRotations.Length;
        int rand = Random.Range(0, rotations.Length);
        transform.eulerAngles = new Vector3(0, 0, rotations[rand]);

        if (NumberofCorrectRotations > 1)
        {
            if (transform.eulerAngles.z == CorrectRotations[0] || transform.eulerAngles.z == CorrectRotations[1])
            {
                isPlaced = true;
                flow.Correct();
            }
        }
        else
        {
            if (transform.eulerAngles.z == CorrectRotations[0])
            {
                isPlaced = true;
                flow.Correct();
            }
        }
    }

   public void OnPointerDown(PointerEventData eventData)
    {
        transform.Rotate(0, 0, 90);
        transform.eulerAngles = new Vector3(0, 0, Mathf.Round(transform.eulerAngles.z));
        if (NumberofCorrectRotations > 1)
        {
            if (transform.eulerAngles.z == CorrectRotations[0] || transform.eulerAngles.z == CorrectRotations[1] && isPlaced == false)
            {
                isPlaced = true;
                flow.Correct();
            }
            else if (isPlaced == true)
            {
                isPlaced = false;
                flow.Wrong();
            }
        }
        else
        {
            if (transform.eulerAngles.z == CorrectRotations[0] && isPlaced == false)
            {
                isPlaced = true;
                flow.Correct();
            }
            else if (isPlaced == true)
            {
                isPlaced = false;
                flow.Wrong();
            }
        }
    }
}