using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyCategory : MonoBehaviour {


    private SpriteRenderer _sprite;

    private GameManager.ColorType _color;

    public GameManager.ColorType Color
    {
        get { return _color; }
    }


    private void Awake()
    {
        _sprite = transform.Find("candy").GetComponent<SpriteRenderer>();
    }

    public void SetColor(GameManager.ColorType color, Sprite sprite)
    {
        _color = color;
        _sprite.sprite = sprite;
//        if (GameManager.Instance.colorSpriteDict.ContainsKey(color))
//        {
//            _sprite.sprite = GameManager.Instance.colorSpriteDict[color];
//        }
    }
}
