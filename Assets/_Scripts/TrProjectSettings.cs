using UnityEngine;
public class TrProjectSettings
{
    public static string _character = "enna";
	public static string _apiVersion = "_V22_10_12";

#if PLATFORM_IOS
    public static string _urlStore = "https://apps.apple.com/kr/app/ennagalaxy/id1640621343";
#endif
#if PLATFORM_ANDROID
	public static string _urlStore = "https://play.google.com/store/apps/details?id=com.blazar.enna";
    public static string _googleUrl = "https://play.google.com/store/apps/details?id=com.blazar.enna";
    public static string _oneUrl = "https://m.onestore.co.kr/mobilepoc/apps/appsDetail.omp?prodId=0000764808";
#endif
    public static string _subjectForShare = "[Brainbow Arcade] 브레인보우 아케이드 에나갤럭시";
    public static string _contentForShare = "[Brainbow Arcade] 브레인보우 아케이드 에나갤럭시에 초대합니다!!";

    public static string _urlRanking = "http://118.67.143.233:8080/enna/rank";

    public const string strLOBBY = "Lobby",
        strPUZZLE = "PuzzleEnna",
        strRESULT = "Result";

	public const string
		PLATFORM = "platform",
		EMAIL = "email",
		NICKNAME = "nickName",
		MAXSCORE = "maxScore",
		SCORES = "scores",
		STAMINA = "stamina",
		STAMINADATE = "staminaDate",
		GUESTTOKEN = "guestToken",
		PLAYERID = "playerId",
		RECEIPT = "receipt",
		VERSION = "version",
		STATE = "state",
		ACTIVATE = "Activate",
		DEACTIVATE = "DeActivate",
		GOOGLE = "GooglePlay",
		APPLE = "AppStore",

		AUTOLOGINPLATFORM = "AutoLoginPlatform",
		AUTOLOGINID = "AUTO LOGIN ID";


	public static string _deviceId = SystemInfo.deviceUniqueIdentifier;
}
