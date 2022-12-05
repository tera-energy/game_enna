using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrUI_LobbyManager : MonoBehaviour
{
    static TrUI_LobbyManager _instance;
    public static TrUI_LobbyManager xInstance { get { return _instance; } }

    [SerializeField] RectTransform[] _rtObjects;

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

    void Start() {
        RectTransform rect = transform.GetComponent<RectTransform>();
        GameManager.xInstance.zSetUIRect(ref _rtObjects, rect);
    }
}
