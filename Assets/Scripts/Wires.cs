using Unity.VisualScripting;
using UnityEngine;

public class Wires : MonoBehaviour
{
    float[] rotations = { 0, 90, 180, 270 };

    public float[] CorrectRotations;
    bool isPlaced = false;
    [SerializeField] GameObject FlowObject;
    private ElectricFlow flow;

    int NumberofCorrectRotations = 1;

    void Start()
    {
        flow = FlowObject.GetComponent<ElectricFlow>();

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

    private void OnMouseDown()
    {
        transform.Rotate(new Vector3(0, 0, 90));
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