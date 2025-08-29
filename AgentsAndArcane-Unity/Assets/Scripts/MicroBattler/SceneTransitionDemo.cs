using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionDemo : MonoBehaviour
{
    public HexUnit ally;
    public HexUnit enemy;
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwapScenes()
    {
        DontDestroyOnLoad(ally);
        DontDestroyOnLoad(enemy);
        SceneManager.LoadScene("Scenes/Micro Battle");
    }
}
