using UnityEngine;

public class TrWallDisabledZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision){
        if (collision.CompareTag(TrEnnaConst.strPath))
        {
            TrPuzzleEnna.xInstance.zDisableWall();
        }
        else if (collision.CompareTag(TrEnnaConst.strItem)){
            collision.GetComponent<TrEnnaItem>().zDisableItem();
        }
    }   
}
