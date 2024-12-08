using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class Elderbug : MonoBehaviour
{
    public DialogueRunner dialogueRunner;

    [SerializeField] private Transform leftCheckPoint;
    [SerializeField] private Vector2 leftCheckSize;

    [SerializeField] private Transform talkCheckPoint;
    [SerializeField] private Vector2 talkCheckSize;

    [SerializeField] private LayerMask playerLayer;

    private bool left;
    private bool talkReady;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Physics2D.OverlapBox(leftCheckPoint.position, leftCheckSize, 0, playerLayer))
        {
            left = true;
        }
        else
        {
            left = false;
        }

        if (Physics2D.OverlapBox(talkCheckPoint.position, talkCheckSize, 0, playerLayer))
        {
            talkReady = true;
        }
        else
        {
            talkReady = false;
        }

        if (talkReady)
        {
            if ( Input.GetKeyDown(KeyCode.X))
            {
                dialogueRunner.StartDialogue("Start");
            }
        }

        anim.SetBool("left", left);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(leftCheckPoint.position, leftCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(talkCheckPoint.position, talkCheckSize);
    }
}
