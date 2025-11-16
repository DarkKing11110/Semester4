using UnityEngine;
using TMPro;

public class MainKeypad : MonoBehaviour
{
    private int Digit1;
    private int Digit2;
    private int Digit3;
    private int Digit4;

    [SerializeField] int RealPin;
    private string TypedPin;

    [SerializeField] Button one;
    [SerializeField] Button two;
    [SerializeField] Button three;
    [SerializeField] Button four;
    [SerializeField] Button five;
    [SerializeField] Button six;
    [SerializeField] Button seven;
    [SerializeField] Button eight;
    [SerializeField] Button nine;
    [SerializeField] Button zero;
    [SerializeField] Button Clear;
    [SerializeField] Button Enter;

    //Visuals
    public TMP_Text ShowPin;
    public TMP_Text ErrorMessage;
    public TMP_Text AccessGrantedMessage;
   
    void Start()
    {
       Digit1 = Random.range(0, 10);
       Digit2 = Random.range(0, 10);
       Digit3 = Random.range(0, 10);
       Digit4 = Random.range(0, 10);

       
    }

    public void one()
    {
        TypedPin = TypedPin + 1;
        ShowPin.text;
    }

    public void two()
    {
        TypedPin = TypedPin + 2;
        ShowPin.text;
    }
    public void three()
    {
        TypedPin = TypedPin + 3;
        ShowPin.text;
    }
    public void four()
    {
        TypedPin = TypedPin + 4;
        ShowPin.text;
    }
    public void five()
    {
        TypedPin = TypedPin + 5;
        ShowPin.text;
    }
    public void six()
    {
        TypedPin = TypedPin + 6;
        ShowPin.text;
    }
    public void seven()
    {
        TypedPin = TypedPin + 7;
        ShowPin.text;
    }
    public void eight()
    {
        TypedPin = TypedPin + 8;
        ShowPin.text;
    }
    public void nine()
    {
        TypedPin = TypedPin + 9;
        ShowPin.text;
    }
    public void zero()
    {
        TypedPin = TypedPin + 0;
        ShowPin.text;
    }
    public void clear()
    {
        TypedPin = string.empty;
    }
    public void enter()
    {
        NumberChecker();
    }

    private void NumberChecker()
    {
        if (TypedPin == RealPin)
        {
            AccessGrantedMessage.text;
        }
        else
        {
            ErrorMessage.text;
            TypedPin = string.empty;
            ShowPin.text;
        }
    }
}