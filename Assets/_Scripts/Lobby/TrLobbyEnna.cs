using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrLobbyEnna : MonoBehaviour
{
    // 행성 회전
    [SerializeField] Transform[] _trLines;
    [SerializeField] float[] _rotSpeed;
    [SerializeField] float[] _distanceLines;

    IEnumerator yRotPlanet(Transform[] lines, float speed, float distance, int isRight = 1)
    {
        float disablePosX = lines[0].transform.localPosition.x + distance * isRight;
        float createPosX = -disablePosX;
        float disablePosXAbs = Mathf.Abs(disablePosX);
        int num = lines.Length;
        Transform rightLine = lines[num - 1];
        float currPosX = 0;
        for (int i = 0; i < num; i++)
        {
            lines[i].localPosition = new Vector2(currPosX, 0);
            currPosX += distance;
        }

        int currIndex = num - 1;
        while (true)
        {
            if (Mathf.Abs(rightLine.localPosition.x) >= disablePosXAbs)
            {
                Vector2 pos = new Vector2(createPosX + (Mathf.Abs(rightLine.localPosition.x) - disablePosXAbs) * isRight, 0);
                rightLine.localPosition = pos;

                currIndex++;
                if (currIndex >= num)
                    currIndex = 0;
                rightLine = lines[currIndex];
            }

            for (int i = 0; i < num; i++)
            {
                lines[i].localPosition += Vector3.right * speed * Time.deltaTime * isRight;
            }
            yield return null;
        }
    }

    void Start()
    {
        {
            Transform[] trs = new Transform[2];
            trs[0] = _trLines[0];
            trs[1] = _trLines[1];
            StartCoroutine(yRotPlanet(trs, _rotSpeed[0], _distanceLines[0], 1));
        }
        {
            Transform[] trs = new Transform[2];
            trs[0] = _trLines[2];
            trs[1] = _trLines[3];
            StartCoroutine(yRotPlanet(trs, _rotSpeed[1], _distanceLines[1], -1));
        }
    }
}
