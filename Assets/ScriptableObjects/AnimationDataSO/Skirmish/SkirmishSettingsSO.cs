using UnityEngine;

[CreateAssetMenu(fileName = "SkirmishSettings", menuName = "Game/Skirmish Settings")]
public class SkirmishSettingsSO : ScriptableObject
{
    public string selectedMapName;

    public string playerFactionName;
    public int playerFactionIndex;

    public string aiFactionName;
    public int aiFactionIndex;

    public bool destroyTownhall;
    public bool killKing;
    public bool useTimeLimit;
    public int timeLimitMinutes;
}
