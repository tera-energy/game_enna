using UnityEngine;

public class TrPath : MonoBehaviour
{
    TrWall[] _walls;      // 벽 오브젝트들
    int _numWalls;

    float _yDistanceHalf;
    float _yLimit;
    float _yWallLimit;

    public void zEndGame(){

    }

    public void zInit(){
        _yDistanceHalf = TrPuzzleEnna.xInstance._valueDistanceYWalls / 2;
        _yLimit = TrPuzzleEnna.xInstance._maxPosResolution.y;
        
        _yWallLimit = _yLimit-2.5f;

        for (int i = 0; i < _walls.Length; i++){
            _walls[i].zInit();
        }

        yDisableWalls();
    }

    public void zSetWalls(){
        int num = _numWalls;
        int rand1 = Random.Range(0, num);
        int rand2 = Random.Range(0, num);
        if(rand1 == rand2){
            rand2 += Random.Range(1, num);
            if (rand2 >= num)
                rand2 -= num;
        }

        float randHolePosY = Random.Range(-_yWallLimit, _yWallLimit);
        //float randWallDistance = Random.Range(_yDistanceHalf, _yDistanceHalf + 1);

        {
            TrWall wall = _walls[rand1];
            wall.gameObject.SetActive(true);
            float posY = randHolePosY + _yDistanceHalf + wall._height;
            wall.zSetWall(true, upPosY:posY);
        }

        {
            TrWall wall = _walls[rand2];
            wall.gameObject.SetActive(true);
            float posY = randHolePosY - _yDistanceHalf - wall._height;
            wall.zSetWall(false, downPosY:posY);
        }
        gameObject.SetActive(true);
    }


    public void yDisableWalls(){
        gameObject.SetActive(false);
        for (int i=0; i< _numWalls; i++){
            _walls[i].gameObject.SetActive(false);
        }
    }

    void Awake()
    {
        _walls = GetComponentsInChildren<TrWall>();
        _numWalls = _walls.Length;
    }
}
