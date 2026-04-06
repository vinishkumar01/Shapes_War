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
    public static event Action OnGameOverDialogueEnded;

    [SerializeField] private Button _continueButton;

    private bool _isTyping = false;
    private string _currentSentence;
    private Coroutine _typingCoroutine;

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
        //Assign the method to continue button
        _continueButton.onClick.RemoveAllListeners();
        _continueButton.onClick.AddListener(DisplayNextSentence);

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
        if(_isTyping)
        {
            StopCoroutine(_typingCoroutine);
            _dialogueText.text = _currentSentence;
            _isTyping = false;
            return;
        }

        if(_sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        
        _currentSentence = _sentences.Dequeue();

        _typingCoroutine = StartCoroutine(TypeSentence(_currentSentence));
    }

    public void EndDialogue()
    {
        Debug.Log("End of Conversation");
        _animator.SetBool("isOpen", false);
        Debug.Log("Player Control should be true by now");

        OnDialogueEnded?.Invoke(); // the Start Wave from Dialogue End Method will be executed here as we are subscribing that to this event action

    }

    #endregion

    #region GameOver Dialouge
    public void StartGameOverDialogue(Dialogues dialogue)
    {
        //Assign the method to continue button
        _continueButton.onClick.RemoveAllListeners();
        _continueButton.onClick.AddListener(DisplayNextGameOverSentence);

        Debug.Log("Starting Game Over Dialogues");
        _animator.SetBool("isOpen", true);

        _nameText.text = dialogue.Name;

        _sentences.Clear();

        foreach (string sentence in dialogue.Sentences)
        {
            _sentences.Enqueue(sentence);
        }

        DisplayNextGameOverSentence();
    }

    public void DisplayNextGameOverSentence()
    {
        if (_isTyping)
        {
            StopCoroutine(_typingCoroutine);
            _dialogueText.text = _currentSentence;
            _isTyping = false;
            return;
        }

        if (_sentences.Count == 0)
        {
            EndGameOverDialogue();
            return;
        }

        _currentSentence = _sentences.Dequeue();

        _typingCoroutine = StartCoroutine(TypeSentence(_currentSentence));
    }

    public void EndGameOverDialogue()
    {
        Debug.Log("End of GameOver Conversation");
        _animator.SetBool("isOpen", false);

        OnGameOverDialogueEnded?.Invoke();

    }

    #endregion 

    private IEnumerator TypeSentence(string sentence)
    {
        _isTyping = true;
        _dialogueText.text = " ";

        foreach(char letter in sentence.ToCharArray())
        {
            _dialogueText.text += letter;
            yield return new WaitForSeconds(0.01f);
        }

        _isTyping = false;
    }

    #region Tutorial----

    public void StartTutorialDialouges(Dialogues dialogue)
    {
        //Assign the method to continue button
        _continueButton.onClick.RemoveAllListeners();
        _continueButton.onClick.AddListener(DisplayNextTutorialSentence);

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
        if (_isTyping)
        {
            StopCoroutine(_typingCoroutine);
            _dialogueText.text = _currentSentence;
            _isTyping = false;
            return;
        }

        if (_sentences.Count == 0)
        {
            EndTutorialDialogue();
            return;
        }

        _currentSentence = _sentences.Dequeue();

        _typingCoroutine = StartCoroutine(TypeSentence(_currentSentence));
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
