using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ChatPanelControl : MonoBehaviour
{
    private bool _state;
    public GameObject _gameObject;
    // Start is called before the first frame update
    void Start()
    {
        _state = true;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            if (_state)
            {
                Debug.Log("ChatPanel OFF");
                _gameObject.SetActive(false);
                _state = false;
            }
            else
            {
                Debug.Log("ChatPanel ON");
                _gameObject.SetActive(true);
                _state = true;
            }  
        }
        
    }
}
