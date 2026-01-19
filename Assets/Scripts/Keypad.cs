using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Keypad : MonoBehaviour
{
    private int Digit1;
    private int Digit2;
    private int Digit3;
    private int Digit4;

    [SerializeField] string RealPin;
    public string TypedPin;

    [SerializeField] Button ButtonOne;
    [SerializeField] Button ButtonTwo;
    [SerializeField] Button ButtonThree;
    [SerializeField] Button ButtonFour;
    [SerializeField] Button ButtonFive;
    [SerializeField] Button ButtonSix;
    [SerializeField] Button ButtonSeven;
    [SerializeField] Button ButtonEight;
    [SerializeField] Button ButtonNine;
    [SerializeField] Button ButtonZero;
    [SerializeField] Button ButtonClear;
    [SerializeField] Button ButtonEnter;

    //Visuals
    [SerializeField] TMP_Text ShowPin;
    [SerializeField] TMP_Text ErrorMessage;
    [SerializeField] TMP_Text AccessGrantedMessage;
   
    void Start()
    {
        Digit1 = Random.Range(0, 10);
        Digit2 = Random.Range(0, 10);
        Digit3 = Random.Range(0, 10);
        Digit4 = Random.Range(0, 10);

        RealPin += Digit1;
        RealPin += Digit2;
        RealPin += Digit3;
        RealPin += Digit4;
    }

    public void Buttonone()
    {
        TypedPin = TypedPin + 1;
        ShowPin.text = TypedPin;
    }

    public void Buttontwo()
    {
        TypedPin = TypedPin + 2;
        ShowPin.text = TypedPin;
    }
    public void Buttonthree()
    {
        TypedPin = TypedPin + 3;
        ShowPin.text = TypedPin;
    }
    public void Buttonfour()
    {
        TypedPin = TypedPin + 4;
        ShowPin.text = TypedPin;
    }
    public void Buttonfive()
    {
        TypedPin = TypedPin + 5;
        ShowPin.text = TypedPin;
    }
    public void Buttonsix()
    {
        TypedPin = TypedPin + 6;
        ShowPin.text = TypedPin;
    }
    public void Buttonseven()
    {
        TypedPin = TypedPin + 7;
        ShowPin.text = TypedPin;
    }
    public void Buttoneight()
    {
        TypedPin = TypedPin + 8;
        ShowPin.text = TypedPin;
    }
    public void Buttonnine()
    {
        TypedPin = TypedPin + 9;
        ShowPin.text = TypedPin;
    }
    public void Buttonzero()
    {
        TypedPin = TypedPin + 0;
        ShowPin.text = TypedPin;
    }
    public void Buttonclear()
    {
        TypedPin = string.Empty;
        ShowPin.text = TypedPin;
    }
    public void Buttonenter()
    {
        NumberChecker();
    }

    private void NumberChecker()
    {
        if (TypedPin == RealPin)
        {
            ShowPin.text = "     ";
            AccessGrantedMessage.text = "Access Granted";
        }
        else
        {
            ErrorMessage.text = "Incorrect Pin";
            TypedPin = string.Empty;
            ShowPin.text = TypedPin;            
        }
    }
}