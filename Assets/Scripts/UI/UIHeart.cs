using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHeart : MonoBehaviour
{
    private RectTransform rtTransform;
    public Sprite fullHeart, halfHeart, emptyHeart;
    Image heartImage;

    private void Awake()
    {
        heartImage = GetComponent<Image>();
        rtTransform = GetComponent<RectTransform>();
        rtTransform.localScale = new Vector3(1, 1, 1);
    }

    public void SetHeartImage (HeartStatus status)
    {
        switch (status)
        {
            case HeartStatus.Empty:
                heartImage.sprite = emptyHeart;
                break;
            case HeartStatus.Half:
                heartImage.sprite = halfHeart;
                break;
            case HeartStatus.Full:
                heartImage.sprite = fullHeart;
                break;
            default:
                break;
        }
    }
}

public enum HeartStatus
{
    Empty = 0,
    Half = 1,
    Full = 2
};