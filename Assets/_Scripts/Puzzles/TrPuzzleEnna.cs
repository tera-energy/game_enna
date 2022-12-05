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

// ��Ÿ���� ������ �Ʒ���
public enum TrEnnaItemType{
    FillFuel,
}

public class TrPuzzleEnna : TrPuzzleManager
{
    static TrPuzzleEnna _instance;
    public static TrPuzzleEnna xInstance { get { return _instance; } }

    // �÷��̾� ����
    [SerializeField] TrCharacterController _playerCha;          // �÷��̾� ĳ����
    TrJumpType _jumpType = TrJumpType.Normal;
    [SerializeField] float _maxTimejumpReset;
    float _currTimeJumpReset;

    [SerializeField] float _valueSlowSpeed;
    [HideInInspector] public float _valueCurrSpeed;

    // ��� ����
    public Transform _trPathDisabledZone;                       // ��� ������Ʈ�� ������� ���� ������Ʈ
    [SerializeField] TrPath _path_prefab;                       // ��� ������Ʈ�� ������
    public TrPath[] _paths;                                     // ��� ������Ʈ���� �迭
    [SerializeField] int _numPaths;                             // ������ ��� ������Ʈ���� ��
    [SerializeField] float _valuePathPosXDistance;              // ��� ������Ʈ�� �ػ� �ִ�x�� �Ÿ�
    float _valueActivateAndDisablePathPosX;                     // ��� ������Ʈ�� Ȱ��ȭ �� ��Ȱ��ȭ ������ x�Ÿ�
    float _valueActivatePrePosX;                                // �ֱ� ��� ������Ʈ�� �ش� x���� ���޽� ���ο� ��� ����
    [SerializeField] float _valueActivateDistanceX;             // �ֱ� ��� ������Ʈ�� ���� ���� ���ϱ� ���� �Ÿ�
    Transform _trCurrRecentPath;                                // �ֱ� ��� ������Ʈ�� transform
    bool _isMovePath = false;                                   // ��� �������� Ȱ��ȭ �ƴ���
    List<TrPath> _listPaths = new List<TrPath>();
    int _indexCurrActivePath;                               // ���� Ȱ��ȭ�� ��� ������Ʈ�� index

    // �� ����
    public float _valueDistanceYWalls;                          // �� ������ y���� ���ݰ�

    // ������
    [SerializeField] TrEnnaItemData[] _itemDatas;               // ������ �����͵�
    [SerializeField] TrEnnaItem _item_Pre;                      // ������ ������
    TrEnnaItem[] _items;                                        // ������ �迭        
    [SerializeField] int _numItems;                             // ������ �� ����
    int _indexCurrItem;                                         // ���� Ȱ��ȭ�� ������ �ε���
    float _valueActivateAndDisableItemPosX;                     // ������ ���� ��ġ
    [SerializeField] int _maxTimeFuel = 10;                     // ������ ���� ī��Ʈ
    float _currTimeFuel;                                        // ������ ���� ���� ī��Ʈ
    bool _isCreateItemTurn = false;                             // ������ ���� ������
    public static int _valuefillFuelAmount = 10;                // ���� ä������ ��

    // ȭ�� ��鸲
    bool _isShaking = false;
    [SerializeField] float _valueShakePower;
    [SerializeField] float _valueShakeMaxRecoveryTime;
    float _valueShakeCurrRecoveryTime;
    Coroutine _coShakeCam;

    // Ÿ��
    [HideInInspector] public bool _isInvincible = false;
    [SerializeField] float _valueisInvincibleMaxResetTime;
    float _valueInvincibleCurrResetTime;

    // �ػ� �ּ�, �ִ밪
    [HideInInspector] public Vector2 _maxPosResolution;
    Camera _camMain;

    // ��� ����
    [SerializeField] Transform[] _trBgs;
    int _numBgs;
    [SerializeField] float _valueBgDisablePosX;
    float _valueBgActivatePosX;
    [SerializeField] float _bgMoveSpeed;
    int _leftBg = 0;
    [SerializeField] Transform _trMoon;                 // ��
    [SerializeField] SpriteRenderer _srMeteor;          // ����
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
    // �� Ȱ��ȭ �� ����
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

        // � ����
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
        // �ػ� �ּ� �ִ밪
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