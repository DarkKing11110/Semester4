using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class ElectricFlow : MonoBehaviour
{
    public GameObject WiresHolder;
    public GameObject[] Wires;

    public int CorrectWires = 0;

    [SerializeField] int TotalWires = 0;

    [SerializeField] private doorIntro introDoor;
    void Start()
    {
        TotalWires = WiresHolder.transform.childCount;
        Wires = new GameObject[TotalWires];

        for (int i = 0; i < Wires.Length; i++)
        {
            Wires[i] = WiresHolder.transform.GetChild(i).gameObject;
        }

    }

    public void Correct()
    {      
        CorrectWires += 1;
        if (CorrectWires == TotalWires)
        {
            Debug.Log("All Correct");
            GameEvents.OnMinigameCompleted?.Invoke("FoundCircuit");
            introDoor.SetMinigameSolved(true);

        }
    }

    public void Wrong()
    {
        CorrectWires -= 1;
    }
}