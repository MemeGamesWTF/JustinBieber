using System.Collections;
using UnityEngine;

public class BasePlayer : MonoBehaviour
{
   
    void Start()
    {

    }

    void Update()
    {
        if (!GameManager.Instance.GameState)
            return;

        //UPDATE LOGIC
    }


    public void GameOver()
    {
        GameManager.Instance.GameOVer();
    }

    public void Reset()
    {

    }
}
