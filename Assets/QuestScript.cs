using TMPro;
using UnityEngine;

public class QuestScript : MonoBehaviour
{
    public StoryManager storyManager;
    [SerializeField] TMP_Text Questtext;
    
    void Update()
    {
        Questtext.text = storyManager.GetCurrentMissionIdOrName();
    }
}
