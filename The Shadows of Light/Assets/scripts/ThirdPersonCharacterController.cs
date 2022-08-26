using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThirdPersonCharacterController : MonoBehaviour
{
    public float test_var;
    [Header("Movement")]// movement variables, scalable to suit needs
    public float move_speed;//Base Speed
    public float walk_speed;//State Speed
    public float run_speed;//State Speed
    public float ground_drag;
    public float air_drag;
    public float ground_mass;
    public float air_mass;
    public float jump_force;
    public float jump_cooldown;
    public float air_multiply;
    bool can_jump;


    [Header("Movement States")]// checks the players current movement state
    public movement_state player_state;
    public enum movement_state
    {
        walking,
        sprinting,
        idle,
        air,
        crouching //should not be necessary for this game
    }
    //if Crouch is needed
    [Header("Crouching")]
    public bool is_crouching;
    public float crouch_speed;
    public float crouch_scale;
    public float start_scale;

    [Header("KeyBindings")]
    public KeyCode jump_key = KeyCode.Space;
    public KeyCode sprint_key = KeyCode.LeftShift;
    public KeyCode crouch_key = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float player_height;
    public LayerMask ground_mask;
    public bool is_grounded;
    public Transform ground_check;
    public float ground_dis = 0.4f;

    [Header("Slope Check")]
    public float max_angle;
    RaycastHit slope_hit;
    bool exit_slope;

    [Header("Inputs")]
    float hori_input;
    float vert_input;
    Vector3 move_dir;
    Rigidbody rb;
    [Tooltip("This is the orientation gameobject, should be located under the Player object")]
    public Transform orientation;

    [Header("Gravity Controls")]
    public float ground_gravity;
    public float air_gravity;
    public float gravity_multiplier;
    public float jump_gravity;
    public float gravity_pull;
    public float gravity_active_time;

    public float fall_timeout = 0.2f;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    [Header("Main Camera")]
    public GameObject main_camera;
    public float rotation_speed;

    //Referenced Scripts
    Grappling grappling;

    // Start is called before the first frame update
    void Start()
    {
        grappling = gameObject.GetComponent<Grappling>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        can_jump = true;
        start_scale = transform.localScale.y;// stores the original scale to return to when crouching is used
        is_crouching = false;
    }

    // Update is called once per frame
    void Update()
    {
        is_grounded = Physics.CheckSphere(ground_check.position, ground_dis, ground_mask);
        player_input();
        speed_control();
        state_handler();
        if (is_grounded)
        {
            rb.drag = ground_drag;
           // rb.mass = ground_mass;
        }
        else
        {
            rb.drag = air_drag;
          //  rb.mass = air_mass;
        }
    }
    private void FixedUpdate()
    {
        move_player();
    }

   //////////////---------Checks players input-------------///////////////
    void player_input()
    {
        hori_input = Input.GetAxisRaw("Horizontal");
        vert_input = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jump_key) && can_jump && jump_status())
        {
            can_jump = false;
            jump();
            Invoke(nameof(reset_jump), jump_cooldown);
        }
        if (Input.GetKeyDown(crouch_key))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouch_scale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            is_crouching = true;
        }
        if (Input.GetKeyUp(crouch_key))
        {
            transform.localScale = new Vector3(transform.localScale.x, start_scale, transform.localScale.z);
            is_crouching = false;
        }
    }
    //////////////---------Moves the player -------------///////////////
    void move_player()
    {
        //orientation = main_camera.transform;
        orientate();
        move_dir = orientation.forward * vert_input + orientation.right * hori_input;
        transform_rotation();
        //Debug.Log($"[TPCC] MovePlayer() - OnSlop: {on_slope()}, Exit: {!exit_slope}");
        if (on_slope() && !exit_slope)
        {
            Debug.Log($"[TPCC] OnSlop() && !exit_slip -- {get_slope_dir()}");
            rb.AddForce(get_slope_dir() * move_speed * 20f, ForceMode.Force);
        }
        if (is_grounded)
        {
            rb.AddForce(move_dir.normalized * move_speed * 10f, ForceMode.Force);
        }
        else if (!is_grounded)
        {
            rb.AddForce(move_dir.normalized * move_speed * 10f * air_multiply, ForceMode.Force);
            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        rb.useGravity = !on_slope();
    }
    
    ///////////////--------------Player Jump Function-------------///////////////////////
    void jump()
    {
        Debug.Log("Jumped");
        exit_slope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Physics.gravity = new Vector3(0, -(jump_gravity), 0);
        rb.AddForce(transform.up * jump_force, ForceMode.Impulse);
        StartCoroutine(pull_force());// needs improvement , try applying gravity with time ???
    }
    void reset_jump()// resets the jump status of the can_jump after a certain amount of jump_cooldown seconds
    {
        can_jump = true;
        exit_slope = false;
    }
     bool jump_status()
    {
        if(is_grounded || grappling.is_grappling)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    //////////////---------Checks Slope Status and Controls slop Movement------------////////////
    bool on_slope()
    {
        //Debug.Log("OnSlope()");
        //var max_distance = player_height * 0.5f + 0.3f;
        var max_distance = float.MaxValue;
        RaycastHit hitInfo;
        var isHit = Physics.Raycast(transform.position, Vector3.down, out hitInfo, max_distance, ground_mask);
        if (isHit)
        {
            //Debug.Log("[TPCC] on_slope() -- raycast valid");
            float angle = Vector3.Angle(Vector3.up, hitInfo.normal);
            //Debug.Log($"[On_Slope] Angle: {angle}, Normal: {hitInfo.normal}");
            return angle < max_angle && angle != 0;
        }
        else
        {
            return false;
        }
    }
    Vector3 get_slope_dir()
    {
        return Vector3.ProjectOnPlane(move_dir, slope_hit.normal).normalized;
    }
    //////////////---------Controls Speed depending on Slope status -------------///////////////
    void speed_control()
    {
        if (on_slope())
        {
            if (rb.velocity.magnitude > move_speed)
            {
                rb.velocity = rb.velocity.normalized * move_speed;
            }
        }
        Vector3 flat_vel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (flat_vel.magnitude > move_speed)
        {
            Vector3 limit_vel = flat_vel.normalized * move_speed;
            rb.velocity = new Vector3(limit_vel.x, rb.velocity.y, limit_vel.z);
        }
    }

    ///////////////----------------Movement State Handler-------------/////////////////
    void state_handler()
    {
        //Controls the running state of the player movement
        if (is_grounded && Input.GetKey(sprint_key) && !is_crouching)
        {
            player_state = movement_state.sprinting;
            move_speed = run_speed;
            Physics.gravity = new Vector3(0, -(ground_gravity), 0);
        }
        //Controls the walking state of the player movement
        else if (is_grounded && !is_crouching)
        {
            player_state = movement_state.walking;
            move_speed = walk_speed;
            test_var = -(ground_gravity);
            Physics.gravity = new Vector3(0, -(ground_gravity), 0);
        }
        //Controls the crouching state of the player movement
        else if (Input.GetKeyDown(crouch_key))
        {
            player_state = movement_state.crouching;
            move_speed = crouch_speed;
            Physics.gravity = new Vector3(0, -(ground_gravity), 0);
        }
        //Controls the air state of the player movement
        else if (!is_crouching && !is_grounded)
        {
            player_state = movement_state.air;
            test_var = -(air_gravity * Time.deltaTime * gravity_multiplier);
            Physics.gravity = new Vector3(0, -(air_gravity * Time.deltaTime* gravity_multiplier), 0);

        }
    }
    //////////////-----------Gravity Pull Force when player is in the air---------------////////////////
    IEnumerator pull_force()
    {
        yield return new WaitForSeconds(gravity_active_time);
        Debug.Log("gravity pull called");
        
        rb.AddForce(Vector3.down* gravity_pull * Time.deltaTime, ForceMode.Force);
    }
    //////////-------------change the orientation of the player---------------//////////////
    void orientate()
    {
        orientation.rotation= Quaternion.Euler(0f, main_camera.transform.eulerAngles.y, 0f);
        
    }
    private void transform_rotation()
    {
        //transform.rotation = Quaternion.Euler(0f, main_camera.transform.eulerAngles.y, 0f);
        if(move_dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(move_dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot,rotation_speed*Time.deltaTime);
        }
        
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(ground_check.position, ground_dis);
    }


}
