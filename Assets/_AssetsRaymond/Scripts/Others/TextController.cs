using System.Collections;
using UnityEngine;

public class TextController : MonoBehaviour
{
    public Dialogue3DText mitaDialogue;
    // public Dialogue3DText playerDialogue;
    public float interval;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(DialogueTest2());
        }
    }

    private IEnumerator DialogueTest2()
    {
        mitaDialogue.SetDialogueInfo("Hello I am Rendify");
        yield return new WaitForSeconds(interval);

        mitaDialogue.SetDialogueInfo("Hello I am Rendify");
        yield return new WaitForSeconds(interval);

        mitaDialogue.SetDialogueInfo("Hello I am Rendify");
        yield return new WaitForSeconds(interval);

        mitaDialogue.SetDialogueInfo("Hello I am Rendify");
        yield return new WaitForSeconds(interval);
        
    }
}
