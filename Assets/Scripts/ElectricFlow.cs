using UnityEngine;

public class ElectricFlow : MonoBehaviour
{
    public GameObject WiresHolder;
    public GameObject[] Wires;

    private int CorrectWires;

    [SerializeField] int TotalWires = 0;
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
            //Ending 
        }
    }

    public void Wrong()
    {
        CorrectWires -= 1;
    }
}