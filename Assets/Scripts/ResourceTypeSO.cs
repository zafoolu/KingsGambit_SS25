using UnityEngine;

[CreateAssetMenu()]
public class ResourceTypeSO : ScriptableObject {


    public enum ResourceType {
        None,
        Gold,   
        Marble,
        Curse,
        Goldessence,
    }


    public ResourceType resourceType;
    public Sprite sprite;

}