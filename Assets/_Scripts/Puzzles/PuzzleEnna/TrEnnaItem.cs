using UnityEngine;

public class TrEnnaItem : MonoBehaviour
{
    public TrEnnaItemType _itemType;
    SpriteRenderer _sr;
    float _speed;

    public void zInit(){
        _speed = TrPuzzleEnna.xInstance._valueCurrSpeed;
    }

    public TrEnnaItemType zGetItemType() { return _itemType;}

    public void zDisableItem(){
        gameObject.SetActive(false);
    }

    public void zCreateItem(TrEnnaItemType type, ref Sprite sp){
        _sr.sprite = sp;
        _itemType = type;
    }

    void Awake(){
        _sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Update(){
        if (!GameManager.xInstance._isGameStarted) return;

        transform.position += -Vector3.right * _speed * Time.deltaTime;
    }
}
