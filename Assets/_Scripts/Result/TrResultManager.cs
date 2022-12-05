using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class TrResultManager : MonoBehaviour
{
    static TrResultManager _instance;
    public static TrResultManager xInstance { get { return _instance; } }
    [SerializeField] TextMeshProUGUI _txtResult;
    [SerializeField] TextMeshProUGUI _txtScore;
    [SerializeField] CanvasGroup _imgFade;
    [SerializeField] Image[] _imgStars;
    [SerializeField] ParticleSystem[] _psStars;
    [SerializeField] Color _colorStarBright;
    [SerializeField] Color _colorStarDark;
    bool _isSkipScoreEffect = false;
    [SerializeField] TrUI_HoldButton _btnRetry;
    [SerializeField] int[] _pointConditions;
    [SerializeField] AudioClip _acScoreIncrease;
    [SerializeField] AudioClip _acStarTwinkle;

    bool _isSetScoreCom = false;
    bool _isAction = false;

    public void xBtnExit(){
        if (_isAction) return;
        _isAction = true;
        _imgFade.DOFade(1, 0.5f).OnComplete(() => SceneManager.LoadScene(TrProjectSettings.strLOBBY));
    }
    public void xBtnRetry()
    {
        if (_isAction) return;
        _isAction = true;
        if (GameManager._type == TT.enumGameType.Train)
            _imgFade.DOFade(1, 0.5f).OnComplete(() => GameManager.xInstance.zSetPuzzleGame());
        else if (GameManager._type == TT.enumGameType.Challenge)
            StartCoroutine(StaminaManager.xInstance.zCheckStamina(() =>
            _imgFade.DOFade(1, 0.5f).OnComplete(() =>
            GameManager.xInstance.zSetPuzzleGame())));
    }

    IEnumerator yEffectIncreaseScore(TextMeshProUGUI text, int score, bool isScore)
    {
        float maxScore = score;
        float currScore = maxScore / 2;
        float speed = maxScore - currScore;
        int soundScore = 1;
        while (currScore < maxScore)
        {
            if (_isSkipScoreEffect)
            {
                text.text = ((int)maxScore).ToString();
                break;
            }
            currScore += Time.deltaTime * speed;
            if (isScore && currScore >= soundScore)
            {
                while (currScore > soundScore)
                    soundScore++;

                if (speed > 20)
                {
                    if ((int)speed % 3 == 0)
                        TrAudio_SFX.xInstance.zzPlaySFX(_acScoreIncrease);
                }
                else
                    TrAudio_SFX.xInstance.zzPlaySFX(_acScoreIncrease);
            }
            text.text = ((int)currScore).ToString();
            speed = (maxScore - currScore);
            if (speed < 1)
                speed = 1;
            yield return null;
        }
        text.text = score.ToString();

        if (isScore)
        {
            yield return TT.WaitForSeconds(0.5f);
            _isSetScoreCom = true;
        }
    }
    IEnumerator ySetGameDatas(){
        int score = GameManager._score;

        if (GameManager._type == TT.enumGameType.Challenge)
        {
            if (DatabaseManager._myDatas != null)
            {
                if (score >= DatabaseManager._myDatas.maxScore)
                {
                    DatabaseManager._myDatas.maxScore = score;
                    yield return StartCoroutine(DatabaseManager.xInstance.zSetMaxScore());
                }
            }

            bool isChangeMyScore = false;
            if (DatabaseManager._liMyScores == null || DatabaseManager._liMyScores.Count == 0)
            {
                isChangeMyScore = true;
                DatabaseManager._liMyScores = new List<int>();
                DatabaseManager._liMyScores.Add(score);
            }
            else
            {
                for (int i = 0; i < DatabaseManager._liMyScores.Count; i++)
                {
                    if (i >= 5) break;

                    if (DatabaseManager._liMyScores[i] < score)
                    {
                        isChangeMyScore = true;
                        DatabaseManager._liMyScores.Insert(i, score);
                        break;
                    }
                }

                if (DatabaseManager._liMyScores.Count >= 5)
                    DatabaseManager._liMyScores.RemoveAt(5);
            }
        }

            StartCoroutine(yEffectIncreaseScore(_txtScore, score, true));
        GameManager._canBtnClick = true;

        for (int i = 0; i < 3; i++){
            _imgStars[i].color = _colorStarDark;
        }
        _txtResult.text = "";

        _imgFade.DOFade(0, 1f);
        yield return new WaitUntil(() => _isSetScoreCom);

        string resultText;
        int numBrightStar;
        if(score >= _pointConditions[2]){
            resultText = "Perfect!";
            numBrightStar = 3;
        }else if(score >= _pointConditions[1]){
            resultText = "Great";
            numBrightStar = 2;
        }
        else if (score >= _pointConditions[0]){
            resultText = "Good";
            numBrightStar = 1;
        }
        else{
            resultText = "Bad";
            numBrightStar = 0;
        }

        for (int i = 0; i < numBrightStar; i++){
            TrAudio_SFX.xInstance.zzPlaySFX(_acStarTwinkle);
            _imgStars[i].color = _colorStarBright;
            _psStars[i].Play();
            yield return TT.WaitForSeconds(1f);
        }
        _txtResult.text = resultText;
    }

    void Awake(){
        if (_instance == null){
            _instance = this;
        }
        else{
            Destroy(gameObject);
        }
    }

    void Start()
    {
        GameManager._canBtnClick = false;
        TrAudio_Music.xInstance.zzPlayMain(0);
        _txtScore.text = "0";
        _imgFade.alpha = 1;

        StartCoroutine(ySetGameDatas());
    }

#if PLATFORM_ANDROID
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isSkipScoreEffect = true;
        }
    }
#endif
}