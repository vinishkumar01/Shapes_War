using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogues _dialouge;
    public Dialogues _gameOverDialogue;
    public Dialogues _chaserDialogue;
    public Dialogues _smasherDialogue;
    public Dialogues _tracerDialogue;

    
    public void TriggerDialouge()
    {
        DialogueManager.instance.StartDialouge(_dialouge);
    }

    public void TriggerGameOverDialogue()
    {
        DialogueManager.instance.StartGameOverDialogue(_gameOverDialogue);
    }

    //Tutorial
    public void TriggerChaserDialouge()
    {
        DialogueManager.instance.StartTutorialDialouges(_chaserDialogue);
    }

    public void TriggerSmasherDialouge()
    {
        DialogueManager.instance.StartTutorialDialouges(_smasherDialogue);
    }

    public void TriggerTracerDialouge()
    {
        DialogueManager.instance.StartTutorialDialouges(_tracerDialogue);
    }

}
