using TMPro;
using UnityEngine;

public class QuestScript : MonoBehaviour
{
    public StoryManager storyManager;
    [SerializeField] TMP_Text Questtext;
    private string quest;
    private string color = "White";
    void Update()
    {
        quest = storyManager.GetCurrentMissionIdOrName();
        Questtext.text = ("<color=" + color + ">" + quest + "</color>");
    }
}
