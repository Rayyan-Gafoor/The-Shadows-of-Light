using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHighlight : MonoBehaviour
{
    Renderer obj;
    Color temp;
    // Start is called before the first frame update
    void Start()
    {
        obj = gameObject.GetComponent<Renderer>();
        temp = obj.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Change Color");
            obj.material.color = new Color(1f, 0f,0f);

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {

            obj.material.color = temp;

        }
    }


}
