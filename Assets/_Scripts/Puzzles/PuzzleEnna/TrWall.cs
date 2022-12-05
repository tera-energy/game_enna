using UnityEngine;

public class TrWall : MonoBehaviour{
    public float _height;

    public void zInit(){

    }

    public void zSetWall(bool isUp, float upPosY=0, float downPosY=0) {
        float randRot = Random.Range(0, 360f);
        float randScale = Random.Range(0.8f, 1f);
        transform.localScale = new Vector3(randScale, randScale, 1);
        transform.eulerAngles = new Vector3(0, 0, randRot);
        if (isUp){
            transform.localPosition = new Vector2(0, upPosY);
        }
        else{
            transform.localPosition = new Vector2(0, downPosY);
        }
    }
}
