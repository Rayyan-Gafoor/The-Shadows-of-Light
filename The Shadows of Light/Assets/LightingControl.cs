using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingControl : MonoBehaviour
{
    public GameObject light_gameobject;
    public Color light_color;
    Light directional_light;


    [Range(1500, 20000)]
    public float colourTemp;

    // Start is called before the first frame update
    void Start()
    {
        directional_light = light_gameobject.GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            directional_light.colorTemperature = colourTemp;
            directional_light.color = light_color; 
               
        }
    }

}
