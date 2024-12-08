using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class Interactable : MonoBehaviour
{
    public string yarnNode; // The node in your Yarn script
    public bool isNPC; // Differentiate between NPCs and objects

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (isNPC)
            {
                StartConversation();
            }
            else
            {
                ShowDescription();
            }
        }
    }

    private void StartConversation()
    {
        var dialogueRunner = FindObjectOfType<DialogueRunner>();
        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
        {
            dialogueRunner.StartDialogue(yarnNode);
        }
    }

    private void ShowDescription()
    {
        var dialogueRunner = FindObjectOfType<DialogueRunner>();
        if (dialogueRunner != null && !dialogueRunner.IsDialogueRunning)
        {
            dialogueRunner.StartDialogue(yarnNode); // Use different nodes for objects
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
