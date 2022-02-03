using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteChange : MonoBehaviour
{
    public Sprite[] sprites;
    public Image serverHandImage;
    
    // Start is called before the first frame update
    void Start()
    {
        serverHandImage.sprite = sprites[3];
    }

    // Update is called once per frame
    void Update()
    {
        if (Client.gamestart == 1) 
        {
            
            if (Client.receiveServerHand == 0)
            {
                serverHandImage.sprite = sprites[0];
                Debug.Log("show: scissors");
            }
        
            else if (Client.receiveServerHand == 1)
            {
                serverHandImage.sprite = sprites[1];
                Debug.Log("show: Rock");
            }
        
            else if (Client.receiveServerHand == 2)
            {
                serverHandImage.sprite = sprites[2];
                Debug.Log("show: paper");
            }
            

        }
        else
        {
            StartCoroutine(BlueCard());
            

        }
        
        
    }

    IEnumerator BlueCard()
    {
        yield return new WaitForSeconds(3.0f);
        serverHandImage.sprite = sprites[3];
        Debug.Log("show: nothing");
        
    }
}
