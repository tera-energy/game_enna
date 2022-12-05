using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TrUI_PuzzleEnna : MonoBehaviour
{
    static TrUI_PuzzleEnna _instance;
    public static TrUI_PuzzleEnna xInstance { get { return _instance; } }

    [SerializeField] RectTransform[] _rtObjects;
    [SerializeField] Transform _trEffectFillFuel;
    [SerializeField] float _valueFillFuelPosX;
    [SerializeField] float _valueFuelMoveY;
    [SerializeField] float _valueFuelEffectTime;
    Tween _tweenFuelEffect;

    [SerializeField] GameObject _goNotice;
    [SerializeField] TextMeshProUGUI _txtNotice;
    [SerializeField] RectTransform _rtCover;
    [SerializeField] Vector2 _posYMoveCover;
    [SerializeField] float _timeMoveCover;
    [SerializeField] GameObject _goStartAnnounce;

    [SerializeField] AudioClip _acReady;
    [SerializeField] AudioClip _acGo;
    [SerializeField] AudioClip _acCover;


    public void zSetNotice(TrNoticeType type){
        _goNotice.SetActive(true);
        switch (type){
            case TrNoticeType.READY:
                _rtCover.localPosition = new Vector2(0, _posYMoveCover.x);
                _txtNotice.text = "Ready";
                TrAudio_SFX.xInstance.zzPlaySFX(_acReady);
                break;
            case TrNoticeType.GO:
                _rtCover.DOLocalMoveY(_posYMoveCover.y, _timeMoveCover);
                _txtNotice.text = "GO~!!!";
                TrAudio_SFX.xInstance.zzPlaySFX(_acGo);
                TrAudio_SFX.xInstance.zzPlaySFX(_acCover);
                break;
            case TrNoticeType.GAMEOVER:
                break;
            case TrNoticeType.DISABLE:
                _goNotice.SetActive(false);
                _goStartAnnounce.SetActive(true);
                break;
        }
    }
    public void zSetStartGame(){
        _goStartAnnounce.SetActive(false);
    }

    public void zEffectFillFuel(float posY){
        _trEffectFillFuel.gameObject.SetActive(true);
        _trEffectFillFuel.position = new Vector2(_valueFillFuelPosX, posY);
        float targetPosY = _valueFuelMoveY + posY;
        if (_tweenFuelEffect != null)
            _tweenFuelEffect.Kill();
        _tweenFuelEffect = _trEffectFillFuel.DOMoveY(targetPosY, _valueFuelEffectTime).OnComplete(() => _trEffectFillFuel.gameObject.SetActive(false));
    }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void yResetDomainCodes(){
        _instance = null;
    }

    void Awake(){
        if (_instance == null){
            _instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    private void Start(){
        RectTransform rect = transform.GetComponent<RectTransform>();
        GameManager.xInstance.zSetUIRect(ref _rtObjects, rect);
        _goStartAnnounce.SetActive(false);
        _goNotice.SetActive(false);
    }

}
