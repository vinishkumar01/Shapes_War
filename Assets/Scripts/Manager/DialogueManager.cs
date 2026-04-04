using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI _nameText;
    public TextMeshProUGUI _dialogueText;

    [SerializeField] private Animator _animator;

    private Queue<string> _sentences = new Queue<string>();

    public static DialogueManager instance;

    public static event Action OnDialogueEnded;
    public static event Action OnTutorialDialogueEnded;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

       instance = this;
    }

    #region Main----

    public void StartDialouge(Dialogues dialogue)
    {
        _animator.SetBool("isOpen", true);

        _nameText.text = dialogue.Name;

        _sentences.Clear();

        foreach(string sentence in dialogue.Sentences)
        {
            _sentences.Enqueue(sentence);
        }

        DisplayNextSentence();  
    }

    public void DisplayNextSentence()
    {
        if(_sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = _sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    public void EndDialogue()
    {
        Debug.Log("End of Conversation");
        _animator.SetBool("isOpen", false);
        Debug.Log("Player Control should be true by now");

        OnDialogueEnded?.Invoke(); // the Start Wave from Dialogue End Method will be executed here as we are subscribing that to this event action

    }

    #endregion

    private IEnumerator TypeSentence(string sentence)
    {
        _dialogueText.text = " ";

        foreach(char letter in sentence.ToCharArray())
        {
            _dialogueText.text += letter;
            yield return new WaitForSeconds(0.01f);
        }
    }

    #region Tutorial----

    public void StartTutorialDialouges(Dialogues dialogue)
    {
        _animator.SetBool("isOpen", true);

        _nameText.text = dialogue.Name;

        _sentences.Clear();

        foreach (string sentence in dialogue.Sentences)
        {
            _sentences.Enqueue(sentence);
        }

        DisplayNextTutorialSentence();
    }

    public void DisplayNextTutorialSentence()
    {
        if (_sentences.Count == 0)
        {
            EndTutorialDialogue();
            return;
        }

        string sentence = _sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    public void EndTutorialDialogue()
    {
        Debug.Log("End of intro");
        _animator.SetBool("isOpen", false);
        Debug.Log("Player Control should be true by now");
        GameState.CanPlayerControl = true;

        OnTutorialDialogueEnded?.Invoke(); 

    }

    #endregion
}
