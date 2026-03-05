using UnityEngine;

[CreateAssetMenu(menuName = "CowCowTycoon/Config/NetworkConfig")]
public class NetworkConfig : ScriptableObject
{
    public string serverUrl = "http://data.ekape.or.kr/openapi-data/service/user/grade/auct/cattleApperence";

    [Tooltip("레포에 커밋 금지. 런타임/빌드 파이프라인에서 주입 권장.")]
    public string serviceKey = "";

    public int nationalIndex = 2;

    [Min(1)] public int timeoutSeconds = 5;
}