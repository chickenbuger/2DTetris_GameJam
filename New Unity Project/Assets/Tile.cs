using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public SpriteRenderer m_spriteRender;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();   
        if(null==spriteRenderer)
        {
            spriteRenderer= gameObject.AddComponent<SpriteRenderer>();
        }
    }
    public Color color
    {
        set
        {
            spriteRenderer.color = value;
        }

        get
        {
            return spriteRenderer.color;
        }
    }

    public void SpriteApply(string spriteName)
    {
        Sprite sprite = Resources.Load<Sprite>(spriteName);
        if(null!=sprite) 
        {
            spriteRenderer.sprite = sprite;
        }
    }

    public void SpriteApply(Sprite _sprite)
    {
        if(null!=_sprite) 
        {
            spriteRenderer.sprite = _sprite;
        }
    }

    public int sortingOrder
    {
        set
        {
            spriteRenderer.sortingOrder = value;
        }

        get
        {
            return spriteRenderer.sortingOrder;
        }
    }

    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("You need to SpriteRenderer for Block");
        }
    }
}
