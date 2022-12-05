using System.Collections;
using UnityEngine;
using DG.Tweening;

public class TrCharacterController : MonoBehaviour
{
    Rigidbody2D _rbCharacter;
    [SerializeField] SpriteRenderer _srCharacter;
    [SerializeField] Transform _trRot;

    [SerializeField] float _valuePowerJump;
    [SerializeField] float _valueVelocityYLimit;
    [SerializeField] float _valuePowerRot;
    [SerializeField] float _valueLerp;
    [SerializeField] float _valueGravity;

    [SerializeField] float _valueWaitTimeLookDown;
    float _valueCurrTimeLookDown;
    [SerializeField] float _valueSpeedLookUp;
    [SerializeField] float _valueSpeedLookDown;
    float _valuePosYDeath;
    [SerializeField] float _valueDistancePosYDeath;

    [Space][Header("연료 채우기")]
    [SerializeField] Transform _trEffectFillFuel;

    [SerializeField] ParticleSystem _psFire;

    [SerializeField] UnityEngine.UI.Text _txtInvincible;
    bool _isInvincible = false;

    // 충돌 이펙트
    Coroutine _coTwinkle;
    [SerializeField] float _valueTwinkleInter;
    bool _isDead = false;
    [SerializeField] float _posYUpTargetDistanceCollide;
    [SerializeField] float _upSpeedCollide;
    [SerializeField] float _downSpeedCollide;                       //TODO: 오타수정
    [SerializeField] float _rotateDurationCollide;
    [SerializeField] float _waitTimeCollide;

    // 사운드
    [SerializeField] AudioClip _acJumpNormal;
    [SerializeField] AudioClip _acJumpRotate;
    [SerializeField] AudioClip _acJumpFire;
    [SerializeField] AudioClip _acCollide;
    [SerializeField] AudioClip _acFillFuel;
    [SerializeField] AudioClip _acDropUFO;

    // 점프 이펙트
    [SerializeField] ParticleSystem _psJumpFire;
    [SerializeField] float _timeSecondRot;
    Tween _tweenSecondRot;

    public void zJump(TrJumpType type){
        if (transform.position.y >= _valuePosYDeath)
            return;

        _rbCharacter.velocity = Vector2.zero;
        _rbCharacter.AddForce(Vector2.up * _valuePowerJump, ForceMode2D.Impulse);

        if (type == TrJumpType.ONE){
            TrAudio_UI.xInstance.zzPlaySFX(_acJumpNormal);
        }
        if (type == TrJumpType.TWO){
            _psJumpFire.Play();
            TrAudio_UI.xInstance.zzPlaySFX(_acJumpFire);
            TrPuzzleEnna.xInstance.zzCorrect(1);
        }
        else if (type == TrJumpType.THREE){
            TrAudio_UI.xInstance.zzPlaySFX(_acJumpRotate);
            TrPuzzleEnna.xInstance.zzCorrect(3);
            if (_tweenSecondRot != null)
                _tweenSecondRot.Kill();
            _tweenSecondRot = _trRot.DORotate(new Vector3(0, 0, 359), _timeSecondRot, RotateMode.FastBeyond360).OnComplete(() => _trRot.eulerAngles = Vector3.zero);
        }
    }
    void ySetColor(ref SpriteRenderer sr, int alpha){
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }

    public void zInvincibleEffect(){
        if (_coTwinkle != null)
            StopCoroutine(_coTwinkle);
        _coTwinkle = StartCoroutine(yTwinkleEffect());
    }

    IEnumerator yTwinkleEffect(){
        while(TrPuzzleEnna.xInstance._isInvincible){
            ySetColor(ref _srCharacter, 0);
            yield return TT.WaitForSeconds(_valueTwinkleInter);
            ySetColor(ref _srCharacter, 1);
            yield return TT.WaitForSeconds(_valueTwinkleInter);
        }
        ySetColor(ref _srCharacter, 1);
    }

    public void zSetInvincible(){
        _isInvincible = !_isInvincible;

        if (_isInvincible)
            _txtInvincible.text = "무적 끄기";
        else
            _txtInvincible.text = "무적 켜기";
    }
    public void zEndGame(bool isCollide=false){
        _isDead = true;
        _rbCharacter.gravityScale = 0;
        _rbCharacter.velocity = Vector2.zero;
        if (_tweenSecondRot != null)
            _tweenSecondRot.Kill();
        _trRot.eulerAngles = Vector3.zero;
        StartCoroutine(yCollideDeathEffect(isCollide));
        _psFire.Stop();
    }
    IEnumerator yCollideDeathEffect(bool isCollide){
        Tween tweenRot = _trRot.DORotate(new Vector3(0, 0, 360f), _rotateDurationCollide, RotateMode.FastBeyond360).SetLoops(-1).SetEase(Ease.Linear);
        if (isCollide){
            float upTargetPos = transform.position.y + _posYUpTargetDistanceCollide;
            while (transform.position.y <= upTargetPos)
            {
                transform.position += Vector3.up * Time.deltaTime * _upSpeedCollide;
                yield return null;
            }

            yield return TT.WaitForSeconds(_waitTimeCollide);
        }
        TrAudio_SFX.xInstance.zzPlaySFX(_acDropUFO);
        float downTargetPos = -_valuePosYDeath;
        while (transform.position.y >= downTargetPos){
            transform.position += Vector3.down * Time.deltaTime * _downSpeedCollide;
            yield return null;
        }
        tweenRot.Kill();
    }

    public void zInit() {
        _rbCharacter.gravityScale = 0;
        _rbCharacter.velocity = Vector2.zero;
        _valuePosYDeath = TrPuzzleEnna.xInstance._maxPosResolution.y + _valueDistancePosYDeath;
        _trRot.eulerAngles = Vector3.zero;
    }
        

    public void zStartGame(){
        _rbCharacter.gravityScale = _valueGravity;
        _psFire.Play();
    }
    #region Collider Events
    void OnTriggerEnter2D(Collider2D collision){
        if (collision.CompareTag(TrEnnaConst.strWall)){
            if (!_isInvincible && !TrPuzzleEnna.xInstance._isInvincible && !_isDead){
                TrPuzzleEnna.xInstance.zzWrong();
                TrAudio_UI.xInstance.zzPlaySFX(_acCollide);
            }
        }else if (collision.CompareTag(TrEnnaConst.strItem)){
            TrEnnaItem item = collision.transform.GetComponent<TrEnnaItem>();
            item.zDisableItem();
            TrEnnaItemType type = item.zGetItemType();
            switch (type){
                case TrEnnaItemType.FillFuel:
                    TrPuzzleEnna.xInstance.zSetFuel();
                    TrAudio_UI.xInstance.zzPlaySFX(_acFillFuel);
                    TrUI_PuzzleEnna.xInstance.zEffectFillFuel(transform.position.y);
                    break;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision){
        if (collision.CompareTag(TrEnnaConst.strPath)){
            TrPuzzleEnna.xInstance.zzCorrect(2);
        }
    }
#endregion

    void yAdjustVelocity(){
        float velY = _rbCharacter.velocity.y;
        if (velY < -_valueVelocityYLimit)
            _rbCharacter.velocity = new Vector2(0, -_valueVelocityYLimit);
        else if (velY > _valueVelocityYLimit){
            _rbCharacter.velocity = new Vector2(0, _valueVelocityYLimit);
        }
    }

    void yMoveLimit(){
        float posY = transform.position.y;
        if (-_valuePosYDeath > posY)
            TrPuzzleEnna.xInstance.zzEndGame();
    }

    void Awake(){
        _rbCharacter = GetComponent<Rigidbody2D>();
    }

    void Start(){
        _txtInvincible.text = "무적 켜기";
    }

    void FixedUpdate(){
        if (!GameManager.xInstance._isGameStarted){
            _rbCharacter.velocity = Vector2.zero;
            return;
        }
        yAdjustVelocity();

        if (_isInvincible) return;
        yMoveLimit();
    }
}
