using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LowLatencyMultichannelAudio;

public class TestPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("TestPlayer: Play soundID=1, looping=true");
            AudioManager.Asio.Play(1,
               new uint[] { 0, 1 },
                0.5f);
            Debug.Log("!");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("TestPlayer: Play soundID=2, looping=true");
            AudioManager.Asio.Play(1,
               new uint[] { 2},
                0.5f);
            Debug.Log("!");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AudioManager.Asio.Play(1,
                 new uint[] { 3},
                  0.5f);
            Debug.Log("!");
        }
    }
}
