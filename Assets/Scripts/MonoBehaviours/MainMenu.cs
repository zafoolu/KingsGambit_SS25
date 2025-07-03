using UnityEngine;

public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MusicManager.Instance.PlayMusic("Main_Menu");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
