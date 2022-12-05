using System.Collections;
using UnityEngine;
using DG.Tweening;

public class TrResultEnna : MonoBehaviour
{
    static TrResultEnna _instance;
    public static TrResultEnna xInstance { get { return _instance; } }

    // 소행성
    [SerializeField] Transform[] _trMeteors;
    [SerializeField] Vector2 _rangeMeteorSpeed;
    float[] _speedMeteors;
    int _numMeteors;
    [SerializeField] Vector2 _rangeMeteorCreatePosX;
    [SerializeField] Vector2 _dirMeteor;
    [SerializeField] Vector2 _endPos;
    [SerializeField] float _startPosY;

    // 에나
    [SerializeField] Transform _trEnna;
    [SerializeField] Vector2 _limitMaxEnna;
    [SerializeField] Vector2 _limitMinEnna;
    Vector2[] _vecEnnaTargets;
    float _currMoveValue;
    [SerializeField] Vector2 _speedEnnaRange;
    float _currSpeedEnna;
    [SerializeField] float _valueSpeedAdjust;
    [SerializeField] Vector2 _rangeEnnaRotCooltime;
    [SerializeField] float _valueDurationTimeRotEnna;

    Color _colorBlue;
    Color _colorYellow;

    // 왼쪽 모니터
    [SerializeField] SpriteRenderer[] _srGauges;
    [SerializeField] float _gaugeFillTime;

    // 중앙 모니터
    [SerializeField] Transform _trCenterLight;
    [SerializeField] Vector2 _lightTargetScale;
    [SerializeField] float _lightChangeSpeed;

    IEnumerator yEffectLeftMonitor()
    {
        int max = _srGauges.Length;
        int currIndex = 0;
        float fillGuageTime = 0.2f;

        while (true)
        {
            int rand = Random.Range(0, max);
            if (rand > currIndex)
            {
                for (int i = currIndex; i <= rand; i++)
                {
                    _srGauges[i].color = _colorYellow;
                    yield return TT.WaitForSeconds(fillGuageTime);
                }
            }
            else if (rand < currIndex)
            {
                for (int i = currIndex; i >= rand; i--)
                {
                    _srGauges[i].color = _colorBlue;
                    yield return TT.WaitForSeconds(fillGuageTime);
                }
            }
            currIndex = rand;
            yield return null;
        }
    }

    IEnumerator yEffectCenterMonitor()
    {
        Vector2 originScale = _trCenterLight.localScale;
        bool isBig = false;
        while (true)
        {
            if (!isBig)
            {
                while (_trCenterLight.localScale.x <= _lightTargetScale.x)
                {
                    _trCenterLight.localScale += new Vector3(1, 1, 1) * _lightChangeSpeed * Time.deltaTime;
                    yield return null;
                }
            }
            else
            {
                while (_trCenterLight.localScale.x >= originScale.x)
                {
                    _trCenterLight.localScale -= new Vector3(1, 1, 1) * _lightChangeSpeed * Time.deltaTime;
                    yield return null;
                }
            }
            isBig = !isBig;
        }
    }
    IEnumerator yRandEnnaRotate()
    {
        while (true)
        {
            float randCool = Random.Range(_rangeEnnaRotCooltime.x, _rangeEnnaRotCooltime.y);

            yield return TT.WaitForSeconds(randCool);

            int randDir = Random.Range(0, 2) == 1 ? 1 : -1;
            _trEnna.DORotate(new Vector3(0, 0, 359 * randDir), _valueDurationTimeRotEnna, RotateMode.FastBeyond360);

            yield return TT.WaitForSeconds(_valueDurationTimeRotEnna);

            _trEnna.eulerAngles = Vector3.zero;
        }
    }
    void yEffectMeteor()
    {
        for (int i = 0; i < _numMeteors; i++)
        {
            Transform meteor = _trMeteors[i];
            meteor.position += (Vector3)_dirMeteor * _speedMeteors[i] * Time.fixedDeltaTime;
            if (meteor.position.y <= _endPos.y || meteor.position.x <= _endPos.x)
            {
                float randX = Random.Range(_rangeMeteorCreatePosX.x, _rangeMeteorCreatePosX.y);
                Vector2 pos;
                if (randX >= -_endPos.x)
                {
                    pos = new Vector2(-_endPos.x, _startPosY - (randX - -_endPos.x));
                }
                else
                    pos = new Vector2(randX, _startPosY);

                meteor.position = pos;
                float randSpeed = Random.Range(_rangeMeteorSpeed.x, _rangeMeteorSpeed.y);
                _speedMeteors[i] = randSpeed;
            }
        }
    }
    void yMoveEnna()
    {
        _trEnna.position = TT.zBezierCurve(_vecEnnaTargets[0], _vecEnnaTargets[1], _vecEnnaTargets[2], _vecEnnaTargets[3], _currMoveValue);
        _currMoveValue += Time.fixedDeltaTime * _currSpeedEnna;

        if (_currMoveValue >= 1)
        {
            ySetEnnaTargetVecs();
        }
        else if (_currMoveValue <= 0.5f && _currSpeedEnna < _speedEnnaRange.y)
        {
            _currSpeedEnna += Time.fixedDeltaTime * _valueSpeedAdjust;
        }
        else if (_currMoveValue >= 0.8f && _currSpeedEnna > _speedEnnaRange.x)
        {
            _currSpeedEnna -= Time.fixedDeltaTime * _valueSpeedAdjust;
        }
    }

    void ySetEnnaTargetVecs()
    {
        _currMoveValue = 0;
        _vecEnnaTargets[0] = _trEnna.position;
        _currSpeedEnna = _speedEnnaRange.x;
        for (int i = 1; i < 4; i++)
        {
            float randX = Random.Range(_limitMinEnna.x, _limitMaxEnna.x);
            float randY = Random.Range(_limitMinEnna.y, _limitMaxEnna.y);
            _vecEnnaTargets[i] = new Vector2(randX, randY);
        }
    }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        GameManager._canBtnClick = true;
        TrAudio_Music.xInstance.zzPlayMain(0);

        ColorUtility.TryParseHtmlString("#7773C9", out _colorBlue);
        ColorUtility.TryParseHtmlString("#F2F2AD", out _colorYellow);

        // 이펙트들
        StartCoroutine(yEffectLeftMonitor());
        StartCoroutine(yEffectCenterMonitor());
        _numMeteors = _trMeteors.Length;

        _speedMeteors = new float[_numMeteors];
        for (int i = 0; i < _numMeteors; i++)
        {
            float randSpeed = Random.Range(_rangeMeteorSpeed.x, _rangeMeteorSpeed.y);
            _speedMeteors[i] = randSpeed;
        }
        _vecEnnaTargets = new Vector2[4];
        _currMoveValue = 0;
        for (int i = 0; i < 4; i++)
        {
            float randX = Random.Range(_limitMinEnna.x, _limitMaxEnna.x);
            float randY = Random.Range(_limitMinEnna.y, _limitMaxEnna.y);
            _vecEnnaTargets[i] = new Vector2(randX, randY);
        }
        _currSpeedEnna = _speedEnnaRange.x;
        StartCoroutine(yRandEnnaRotate());
    }

    void FixedUpdate()
    {
        yEffectMeteor();
        yMoveEnna();
    }
}