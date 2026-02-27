using System.Threading;
using UnityEngine;

public class CreditsSCript : MonoBehaviour
{
    public float ScrollSpeed = 40f;
    private RectTransform RectTransform;
    void Start()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        RectTransform.anchoredPosition += new Vector2(0, ScrollSpeed * Time.deltaTime);
    }
}
