using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grappling : MonoBehaviour
{
    public LineRenderer line_rend;
    public Transform player;
    public LayerMask grappable;
    public GameObject grapple_tip;
    public float grapple_distance = 100f;
    public bool can_grapple;
    public bool is_grappling;

    Vector3 grapple_point;
    SpringJoint spring_joint;
    
    // Start is called before the first frame update
    void Start()
    {
        line_rend = grapple_tip.GetComponent<LineRenderer>();
        is_grappling = false;
    }

    // Update is called once per frame
    void Update()
    {
       
        if (Input.GetMouseButtonDown(0))
        {
            start_grapple();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            stop_grapple();
        }
    }
    private void LateUpdate()
    {
        draw_grapple();
    }

    void start_grapple()
    {
        can_grapple= Physics.CheckSphere(transform.position, grapple_distance, grappable);
       
        
        if (can_grapple)
        {
            //grapple_point = hit.point;
            is_grappling = true;
            spring_joint = player.gameObject.AddComponent<SpringJoint>();
            spring_joint.autoConfigureConnectedAnchor = false;
            spring_joint.connectedAnchor = grapple_point;

            float distance_from_point = Vector3.Distance(player.position, grapple_point);

            spring_joint.maxDistance = distance_from_point * 0.8f;
            spring_joint.minDistance = distance_from_point * 0.25f;

            spring_joint.spring = 4.5f;
            spring_joint.damper = 7f;
            spring_joint.massScale = 4.5f;

            line_rend.positionCount = 2;
        }
    }
    void stop_grapple()
    {
        line_rend.positionCount = 0;
        is_grappling = false;
        Destroy(spring_joint);
    }
    void draw_grapple()
    {
        if (!spring_joint)
        {
            return;
        }
        line_rend.SetPosition(0, grapple_tip.transform.position);
        line_rend.SetPosition(1, grapple_point);
    }
  

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Grapple")
        {
            grapple_point = other.gameObject.transform.position;
            Debug.Log(other);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        //grapple_point=Vector3.zero;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, grapple_distance);
    }
}
