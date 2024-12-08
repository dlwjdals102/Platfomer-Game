using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHealthController : MonoBehaviour
{
    public PlayerData Data;

    public GameObject heartPrefab;

    List<UIHeart> hearts = new List<UIHeart>();

    private void Start()
    {
        DrawHearts();
    }

    public void DrawHearts()
    {
        ClearHearts();

        float maxHealthRemainder = Data.maxHealth % 2;
        int heartsToMake = (int)((Data.maxHealth / 2) + maxHealthRemainder);

        for (int i = 0; i < heartsToMake; i++)
        {
            CreateEmptyHeart();
        }

        for (int i = 0; i < hearts.Count; i++)
        {
            int heartStatusRemainder = Mathf.Clamp(Data.currentHealth - (i * 2), 0, 2);
            hearts[i].SetHeartImage((HeartStatus)heartStatusRemainder);
        }
    }

    public void CreateEmptyHeart()
    {
        GameObject newHeart = Instantiate(heartPrefab);
        newHeart.transform.SetParent(transform);

        UIHeart heartComponent = newHeart.GetComponent<UIHeart>();
        heartComponent.SetHeartImage(HeartStatus.Empty);
        hearts.Add(heartComponent);
    }

    public void ClearHearts()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }
        hearts = new List<UIHeart> ();
    }
}
