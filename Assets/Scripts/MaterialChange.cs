using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialChange : MonoBehaviour
{
    // Start is called before the first frame update
    public Material[] materials;
    public MeshRenderer meshRenderer;
    //private float _speed;
    
    void Start()
    {
        meshRenderer.material = materials[0];
        //_speed = 1f * Time.deltaTime;

    }

    // Update is called once per frame
    void Update()
    {
        if (Client.gamestart == 1) // 게임 시작시 서버가 낸 패에 맞는 이미지로 변경
        {
            if (Client.receiveServerHand == 0)
            {
                meshRenderer.material = materials[0];
                Debug.Log("show: scissors");
                //TurnCard();
            }
        
            else if (Client.receiveServerHand == 1)
            {
                meshRenderer.material = materials[1];
                Debug.Log("show: Rock");
                //TurnCard();
            }
        
            else if (Client.receiveServerHand == 2)
            {
                meshRenderer.material = materials[2];
                Debug.Log("show: paper");
                //TurnCard();
            }
            

        }
        else
        {
            meshRenderer.material = materials[3];
            Debug.Log("show: nothing");
            
        }
    }

    private void TurnCard()
    {
        /*
        Quaternion aRotation = Quaternion.Euler(new Vector3(0,0,180));
        Quaternion bRotation = Quaternion.Euler(new Vector3(0,0,0));
        Vector3 targetRotation = Quaternion.Lerp(aRotation, bRotation, _speed).eulerAngles;
        this.gameObject.transform.rotation = Quaternion.Euler(0f,0f,targetRotation.z);
        
        
        var transformRotation = this.gameObject.transform.rotation;
        transformRotation.z = 180;
        
        
        
        if(transformRotation.z >=180 ||transformRotation.z <=200)
            this.gameObject.transform.Rotate(new Vector3(0,0,0.5f));
        else if((transformRotation.z >=201 ||transformRotation.z <=360))
            this.gameObject.transform.Rotate(new Vector3(0,0,-0.1f));
        */
        
    }
}
