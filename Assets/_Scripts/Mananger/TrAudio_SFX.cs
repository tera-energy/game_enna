using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrAudio_SFX : TrAudio_ {
	static TrAudio_SFX _instance = null;
	public static TrAudio_SFX xInstance{get{return _instance;}}

	//==========================================================================================================

	public void zzPlaySFX(AudioClip ac, float delay = -1f) { zPlaySFX(ac, delay); }
	public void zzSetFlatVolume(float? newVolume = null) {
		zSetFlatVolume(TT.strConfigSFX,newVolume);
	}

	//==========================================================================================================
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	static void yResetDomainCodes() {
		_instance = null;
	}

	new void Awake () {
		if(_instance == null){
			base.Awake();
			_instance = this;
			zzSetFlatVolume();
		} else
			Destroy(gameObject);
	}
}
