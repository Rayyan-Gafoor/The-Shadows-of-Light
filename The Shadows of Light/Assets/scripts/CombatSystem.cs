using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    [Header("Melee Combat")]
    public GameObject weapon_orb;
    public GameObject attack_point;
    public GameObject attack_endpoint;
    public float melee_reset;
    public float attack_speed;
    public bool attacking;
    public float frac = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetMouseButtonDown(1))
        {
            weapon_orb.SetActive(true);
            attacking = true;
            Debug.Log("start attack");
        }
        if (attacking == true)
        {
            // StartCoroutine(melee_attack());
            melee_attack();
            Debug.Log("attacking");
        }
        if(weapon_orb.transform.position== attack_endpoint.transform.position)
        {
            Debug.Log("stop attack");
            stop_attack();
        }
        
    }

    /*IEnumerator melee_attack()
    {
        //weapon_orb.transform.position = attack_point.transform.position;
        
        weapon_orb.transform.position = Vector3.Lerp(attack_point.transform.position, attack_endpoint.transform.position, attack_speed * Time.deltaTime);
        yield return new WaitForSeconds(melee_reset);
        
    }*/
    void melee_attack()
    {

        //Instantiate(weapon_orb, attack_point.transform.position, Quaternion.identity);
        // weapon_orb.transform.position = attack_point.transform.position;
        //weapon_orb.SetActive(true);
        
        frac += Time.deltaTime * attack_speed;
        weapon_orb.transform.position = Vector3.Lerp(attack_point.transform.position, attack_endpoint.transform.position, frac);
    }
    void stop_attack()
    {
        attacking = false;
        weapon_orb.SetActive(false);
        frac = 0;
        weapon_orb.transform.position = attack_point.transform.position;
    }
}
