using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public bool sceneMove = false;
    public string sceneName;

    private void OnTriggerStay2D(Collider2D collision)
    {
        // �浹�� ��ü�� �±װ� "Player"���� Ȯ��
        if (collision.gameObject.CompareTag("Player"))
        {
            sceneMove = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // �浹�� ��ü�� �±װ� "Player"���� Ȯ��
        if (collision.gameObject.CompareTag("Player"))
        {
            sceneMove = false;
        }
    }

    private void Update()
    {
        if (sceneMove)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                SceneManager.LoadSceneAsync(sceneName);
            }
        }
    }
}
