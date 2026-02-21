using UnityEngine;
using TMPro;
using UnityEngine.UI;
using FMODUnity;

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
        RuntimeManager.PlayOneShot("event:/Environment/Minigames/Keypad click");
        TypedPin = TypedPin + 1;
        ShowPin.text = TypedPin;
    }

    public void Buttontwo()
    {
        RuntimeManager.PlayOneShot("event:/Environment/Minigames/Keypad click");
        TypedPin = TypedPin + 2;
        ShowPin.text = TypedPin;
    }
    public void Buttonthree()
    {
        RuntimeManager.PlayOneShot("event:/Environment/Minigames/Keypad click");
        TypedPin = TypedPin + 3;
        ShowPin.text = TypedPin;
    }
    public void Buttonfour()
    {
        RuntimeManager.PlayOneShot("event:/Environment/Minigames/Keypad click");
        TypedPin = TypedPin + 4;
        ShowPin.text = TypedPin;
    }
    public void Buttonfive()
    {
        RuntimeManager.PlayOneShot("event:/Environment/Minigames/Keypad click");
        TypedPin = TypedPin + 5;
        ShowPin.text = TypedPin;
    }
    public void Buttonsix()
    {
        RuntimeManager.PlayOneShot("event:/Environment/Minigames/Keypad click");
        TypedPin = TypedPin + 6;
        ShowPin.text = TypedPin;
    }
    public void Buttonseven()
    {
        RuntimeManager.PlayOneShot("event:/Environment/Minigames/Keypad click");
        TypedPin = TypedPin + 7;
        ShowPin.text = TypedPin;
    }
    public void Buttoneight()
    {
        RuntimeManager.PlayOneShot("event:/Environment/Minigames/Keypad click");
        TypedPin = TypedPin + 8;
        ShowPin.text = TypedPin;
    }
    public void Buttonnine()
    {
        RuntimeManager.PlayOneShot("event:/Environment/Minigames/Keypad click");
        TypedPin = TypedPin + 9;
        ShowPin.text = TypedPin;
    }
    public void Buttonzero()
    {
        RuntimeManager.PlayOneShot("event:/Environment/Minigames/Keypad click");
        TypedPin = TypedPin + 0;
        ShowPin.text = TypedPin;
    }
    public void Buttonclear()
    {
        RuntimeManager.PlayOneShot("event:/Environment/Minigames/EnterButton");
        TypedPin = string.Empty;
        ShowPin.text = TypedPin;
    }
    public void Buttonenter()
    {
         RuntimeManager.PlayOneShot("event:/Environment/Minigames/EnterButton");
        NumberChecker();
    }

    private void NumberChecker()
    {
        if (TypedPin == RealPin)
        {
            RuntimeManager.PlayOneShot("event:/Environment/Minigames/Granted");
            ShowPin.text = "     ";
            AccessGrantedMessage.text = "Access Granted";
        }
        else
        {
            RuntimeManager.PlayOneShot("event:/Environment/Minigames/Denied");
            ErrorMessage.text = "Incorrect Pin";
            TypedPin = string.Empty;
            ShowPin.text = TypedPin;
        }
    }
}