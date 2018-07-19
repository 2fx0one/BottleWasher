using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyCategory : MonoBehaviour {

    public enum ColorType
    {
        YELLOW,
        PURPLE,
        RED,
        BLUE,
        GREEN,
        PINK,
        ANY,
        COUNT
    }

    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;
        public Sprite sprite;
    }

    public ColorSprite[] colorSprites;
    
    private Dictionary<ColorType, Sprite> colorSpriteDict;

    private SpriteRenderer _sprite;

    private ColorType _color;

    public ColorType Color
    {
        get { return _color; }
        set { SetColor(value); }
    }


    public int NumColors
    {
        get { return colorSprites.Length; }
    }

    private void Awake()
    {
        _sprite = this.transform.Find("candy").GetComponent<SpriteRenderer>();
        
        colorSpriteDict = new Dictionary<ColorType, Sprite>();
        for (int i = 0; i < colorSprites.Length; i++)
        {
            if (!colorSpriteDict.ContainsKey(colorSprites[i].color))
            {
                colorSpriteDict.Add(colorSprites[i].color, colorSprites[i].sprite);
            }
        }
    }

    public void SetColor(ColorType color)
    {
        _color = color;
        if (colorSpriteDict.ContainsKey(color))
        {
            _sprite.sprite = colorSpriteDict[color];
        }
    }
}
