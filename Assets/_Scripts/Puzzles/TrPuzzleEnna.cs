using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class TrEnnaConst
{
    public const string strPath = "Path", strItem = "Item", strWall = "Wall";
}

public enum TrJumpType{
    Normal,
    ONE,
    TWO,
    THREE
}

[System.Serializable]
public class TrEnnaItemData{
    public Sprite _sp;
    public TrEnnaItemType _type;
}

// 쿨타임이 없으면 아래로
public enum TrEnnaItemType{
    FillFuel,
}

public class TrPuzzleEnna : TrPuzzleManager
{
    static TrPuzzleEnna _instance;
    public static TrPuzzleEnna xInstance { get { return _instance; } }

    // 플레이어 관련
    [SerializeField] TrCharacterController _playerCha;          // 플레이어 캐릭터
    TrJumpType _jumpType = TrJumpType.Normal;
    [SerializeField] float _maxTimejumpReset;
    float _currTimeJumpReset;

    [SerializeField] float _valueSlowSpeed;
    [HideInInspector] public float _valueCurrSpeed;

    // 경로 관련
    public Transform _trPathDisabledZone;                       // 경로 오브젝트가 사라지는 지역 오브젝트
    [SerializeField] TrPath _path_prefab;                       // 경로 오브젝트의 프리팹
    public TrPath[] _paths;                                     // 경로 오브젝트들의 배열
    [SerializeField] int _numPaths;                             // 생성될 경로 오브젝트들의 수
    [SerializeField] float _valuePathPosXDistance;              // 경로 오브젝트와 해상도 최대x값 거리
    float _valueActivateAndDisablePathPosX;                     // 경로 오브젝트의 활성화 및 비활성화 포지션 x거리
    float _valueActivatePrePosX;                                // 최근 경로 오브젝트가 해당 x값에 도달시 새로운 경로 생성
    [SerializeField] float _valueActivateDistanceX;             // 최근 경로 오브젝트가 위의 값을 구하기 위한 거리
    Transform _trCurrRecentPath;                                // 최근 경로 오브젝트의 transform
    bool _isMovePath = false;                                   // 경로 움직임이 활성화 됐는지
    List<TrPath> _listPaths = new List<TrPath>();
    int _indexCurrActivePath;                               // 현재 활성화된 경로 오브젝트의 index

    // 벽 관련
    public float _valueDistanceYWalls;                          // 위 벽들의 y값과 간격값

    // 아이템
    [SerializeField] TrEnnaItemData[] _itemDatas;               // 아이템 데이터들
    [SerializeField] TrEnnaItem _item_Pre;                      // 아이템 프리팹
    TrEnnaItem[] _items;                                        // 아이템 배열        
    [SerializeField] int _numItems;                             // 아이템 총 개수
    int _indexCurrItem;                                         // 현재 활성화된 아이템 인덱스
    float _valueActivateAndDisableItemPosX;                     // 아이템 생성 위치
    [SerializeField] int _maxTimeFuel = 10;                     // 아이템 생성 카운트
    float _currTimeFuel;                                        // 아이템 생성 현재 카운트
    bool _isCreateItemTurn = false;                             // 아이템 생성 턴인지
    public static int _valuefillFuelAmount = 10;                // 연료 채워지는 양

    // 화면 흔들림
    bool _isShaking = false;
    [SerializeField] float _valueShakePower;
    [SerializeField] float _valueShakeMaxRecoveryTime;
    float _valueShakeCurrRecoveryTime;
    Coroutine _coShakeCam;

    // 타격
    [HideInInspector] public bool _isInvincible = false;
    [SerializeField] float _valueisInvincibleMaxResetTime;
    float _valueInvincibleCurrResetTime;

    // 해상도 최소, 최대값
    [HideInInspector] public Vector2 _maxPosResolution;
    Camera _camMain;

    // 배경 관련
    [SerializeField] Transform[] _trBgs;
    int _numBgs;
    [SerializeField] float _valueBgDisablePosX;
    float _valueBgActivatePosX;
    [SerializeField] float _bgMoveSpeed;
    int _leftBg = 0;
    [SerializeField] Transform _trMoon;                 // 달
    [SerializeField] SpriteRenderer _srMeteor;          // 유성
    [SerializeField] Sprite[] _spMeteors;
    [SerializeField] Vector2 _valuesMeteorCoolTime;
    [SerializeField] Vector2 _valuesMeteorDegree;
    [SerializeField] float _valueSpeedMeteor;
    Coroutine _coMeteor;

    [HideInInspector] public bool _isReadyGame = false;

    #region Setting Options
    public void zSetFuel(){
        _currFuel += _valuefillFuelAmount;
    }
    public void zSetInvincible(){
        _isInvincible = true;
        _playerCha.zInvincibleEffect();
        _valueInvincibleCurrResetTime = 0;
    }
    #endregion
    #region Setting Game State
    void yGameStart(){
        ySetPath();
        _isMovePath = true;
        _trMoon.DOMoveX(-20, 2).SetSpeedBased().OnComplete(() => _trMoon.gameObject.SetActive(false));
        _coMeteor = StartCoroutine(yEffectMeteor());
    }

    protected override void zSetResultGame(bool isCollide){
        if (_coMeteor != null)
            StopCoroutine(_coMeteor);
        for (int i = 0; i < _numPaths; i++){
            _paths[i].zEndGame();
        }
        _playerCha.zEndGame(isCollide);
    }

    public void zzEndGame(bool isCollide=false){
        zEndGame(isCollide);
    }

    void yVibrate(){
        if(_isOnVibrate){
            Handheld.Vibrate();
        }
    }
    public void zzWrong(){
        zzEndGame(true);
        zEffectShakeCam();
#if !UNITY_EDITOR
        yVibrate();
#endif
    }
    public void zzCorrect(int score){
        zCorrect(false, score);
    }
    #endregion
    #region Effects
    IEnumerator yEffectMeteor(){
        float randWaitTime = Random.Range(_valuesMeteorCoolTime.x, _valuesMeteorCoolTime.y);

        yield return TT.WaitForSeconds(randWaitTime);
        float startPosY = _maxPosResolution.y + 2;
        float endPosY = -_maxPosResolution.y - 2;
        Vector3 dir = new Vector3(_valuesMeteorDegree.x, _valuesMeteorDegree.y);
        float posX1 = _maxPosResolution.x / 2;
        float posX2 = _maxPosResolution.x + (_maxPosResolution.x * -_valuesMeteorDegree.x);
        Transform trMeteor = _srMeteor.transform;
        while (true){
            float randX = Random.Range(posX1, posX2);
            trMeteor.position = new Vector2(randX, startPosY);
            while (trMeteor.position.y >= endPosY){
                trMeteor.position += dir * _valueSpeedMeteor * Time.deltaTime;
                yield return null;
            }
            randWaitTime = Random.Range(_valuesMeteorCoolTime.x, _valuesMeteorCoolTime.y);
            yield return TT.WaitForSeconds(randWaitTime);
        }
    }
    public void zEffectShakeCam(){
        _isShaking = true;
        _valueShakeCurrRecoveryTime = 0;
        if (_coShakeCam != null)
            StopCoroutine(_coShakeCam);
        _coShakeCam = StartCoroutine(yShakeCam());
    }
    IEnumerator yShakeCam(){
        Transform trCam = _camMain.transform;
        Vector3 originPos = trCam.position;
        while (_isShaking){
            trCam.position = originPos + Random.insideUnitSphere * _valueShakePower;
            yield return null;
        }
        _camMain.transform.position = originPos;
    }
    #endregion
    #region Game Progress

    public void zDisableWall(){
        TrPath path = _listPaths[0];
        _listPaths.RemoveAt(0);
        path.yDisableWalls();
    }
    void yCreateItem(int type){        
        TrEnnaItemData data = _itemDatas[type];
        TrEnnaItem item = _items[_indexCurrItem];

        item.zCreateItem(data._type, ref data._sp);
        item.transform.position = new Vector2(_valueActivateAndDisableItemPosX, 0);
        item.gameObject.SetActive(true);

        _indexCurrItem++;
        if (_indexCurrItem >= _numItems)
            _indexCurrItem = 0;
    }
    // 벽 활성화 및 셋팅
    void ySetPath(){
        TrPath path = _paths[_indexCurrActivePath];
        path.transform.position = new Vector2(_valueActivateAndDisablePathPosX, 0);
        path.zSetWalls();

        _listPaths.Add(path);
        if (++_indexCurrActivePath >= _numPaths)
            _indexCurrActivePath = 0;
            
        _trCurrRecentPath = path.transform;


        if(_isCreateItemTurn){
            _isCreateItemTurn = false;
            yCreateItem((int)TrEnnaItemType.FillFuel);
        }
    }
    #endregion
    #region Timer
    void yResetPathTimer(){
        if (_trCurrRecentPath.position.x <= _valueActivatePrePosX){
            ySetPath();
        }
    }
    void yResetCamShakeTimer(){
        _valueShakeCurrRecoveryTime += Time.fixedDeltaTime;
        if (_valueShakeCurrRecoveryTime >= _valueShakeMaxRecoveryTime){
            _isShaking = false;
            _valueShakeCurrRecoveryTime = 0;
        }
    }
    void yResetInvincibleTimer(){
        _valueInvincibleCurrResetTime += Time.fixedDeltaTime;
        if (_valueInvincibleCurrResetTime >= _valueisInvincibleMaxResetTime){
            _isInvincible = false;
            _valueInvincibleCurrResetTime = 0;
        }
    }
    #endregion
    #region Init
    void yInstantiatePaths(){
        _paths = new TrPath[_numPaths];

        // 운석 관련
        for (int i = 0; i < _numPaths; i++){
            _paths[i] = Instantiate(_path_prefab);
            _paths[i].zInit();
        }
    }

    void yInstantiateItems(){
        _items = new TrEnnaItem[_numItems];
        for (int i = 0; i < _numItems; i++){
            _items[i] = Instantiate(_item_Pre);
            _items[i].zInit();
            _items[i].gameObject.SetActive(false);
        }
    }

    void yResetJumpType(){
        _currTimeJumpReset = 0;
        _jumpType = TrJumpType.Normal;
    }
    #endregion
    protected override void yBeforeReadyGame(){
        base.yBeforeReadyGame();
        // 해상도 최소 최대값
        _camMain = Camera.main;
        _maxPosResolution = _camMain.ViewportToWorldPoint(new Vector2(1, 1));
        _valueCurrSpeed = _valueSlowSpeed;
        _playerCha.zInit();

        yInstantiatePaths();
        yInstantiateItems();

        _valueActivateAndDisablePathPosX = _maxPosResolution.x + _valuePathPosXDistance;
        _valueActivatePrePosX = _maxPosResolution.x - _valueActivateDistanceX;
        _valueActivateAndDisableItemPosX = _valueActivateAndDisablePathPosX + (_valueActivateAndDisablePathPosX - _valueActivatePrePosX) / 2;
        _trPathDisabledZone.position = new Vector2(-_valueActivateAndDisablePathPosX, 0);

        _numBgs = _trBgs.Length;
        _valueBgActivatePosX = -_valueBgDisablePosX * 2;
        _trBgs[0].localPosition = Vector2.zero;
        _trBgs[1].localPosition = new Vector2(-_valueBgDisablePosX, 0);
        _trBgs[2].localPosition = new Vector2(_valueBgActivatePosX, 0);
    }

    protected override void yAfterReadyGame(){
        base.yAfterReadyGame();
        _isReadyGame = true;
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

    void FixedUpdate(){
        if (_isMovePath){
            yResetPathTimer();
        }

        if (_isShaking){
            yResetCamShakeTimer();
        }

        if (_isInvincible){
            yResetInvincibleTimer();
        }

        if (!GameManager.xInstance._isGameStarted) return;

        _currTimeFuel += Time.fixedDeltaTime;
        if(_currTimeFuel >= _maxTimeFuel){
            _isCreateItemTurn = true;
            _currTimeFuel = 0;
        }

        
    }

    protected override void Update(){
        base.Update();
        if (_isReadyGame){
#if !UNITY_EDITOR
            if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)){
                if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)){
                    _isReadyGame = false;
                    GameManager.xInstance._isGameStarted = true;
                    _playerCha.zStartGame();
                    yGameStart();
                    TrUI_PuzzleEnna.xInstance.zSetStartGame();
                }
            }
#endif
#if UNITY_EDITOR
        if(Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Space)){
            _isReadyGame = false;
            GameManager.xInstance._isGameStarted = true;
            _playerCha.zStartGame();
            yGameStart();
            TrUI_PuzzleEnna.xInstance.zSetStartGame();
        }
#endif
        }

        if (!GameManager.xInstance._isGameStarted) return;

        if(_jumpType != TrJumpType.Normal){
            _currTimeJumpReset += Time.deltaTime;
            if(_currTimeJumpReset >= _maxTimejumpReset)
                yResetJumpType();
        }

#if !UNITY_EDITOR
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Space)){
            if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId) && !GameManager.xInstance._isGameStopped) {
                _currTimeJumpReset = 0;
                _jumpType++;

                _playerCha.zJump(_jumpType);

                if (_jumpType == TrJumpType.THREE)
                {
                    yResetJumpType();
                }
            }
        }
#endif
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.Space)){
            _currTimeJumpReset = 0;
            _jumpType++;

            _playerCha.zJump(_jumpType);

            if (_jumpType == TrJumpType.THREE)
            {
                yResetJumpType();
            }
        }
#endif
        if (_isMovePath){
            foreach (TrPath path in _listPaths){
                path.transform.position += Vector3.left * _valueCurrSpeed * Time.deltaTime;
            }
        }

        float posX = _trBgs[_leftBg].localPosition.x;
        if (posX <= _valueBgDisablePosX){
            float restX = _valueBgDisablePosX - posX;
            _trBgs[_leftBg].localPosition = new Vector2(_valueBgActivatePosX - restX, 0);

            if (++_leftBg >= _numBgs)
                _leftBg = 0;
        }

        for(int i=0; i< _numBgs; i++){
            _trBgs[i].localPosition += Vector3.left * _bgMoveSpeed * Time.deltaTime;
        }

    }
}