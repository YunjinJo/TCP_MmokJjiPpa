using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextControl : MonoBehaviour
{

    private Text _text;
    private float _timeCount = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        _text = GameObject.Find("Text_WinLose").GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Client.result == 0) // 게임 승패에 따라 적절한 텍스트 출력
        {
            
            _text.text = "Draw!";

        }
        
        else if (Client.result == 1)
        {
            _text.text = "You Win!";
        }
        
        else if (Client.result == 2)
        {
            _text.text = "You Lose...";
        }
        else
        {
            _text.text = "";
        }
    }
    
    
}
