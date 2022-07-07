using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public GameObject avatar;
    public GameObject ghost;
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] RespawnManager respawnManager;

    bool canSwitch = true;
    public bool isGhost = false;
    bool isCrouching = false;
    bool toggleCrouch = true;
    private float horizontalInput;
    private Rigidbody2D bodyAvatar;
    private BoxCollider2D boxColliderAvatar;
    private Animator animAvatar;
    private Rigidbody2D bodyGhost;
    private BoxCollider2D boxColliderGhost;
    private Animator animGhost;
    private Rigidbody2D cBody;
    private BoxCollider2D cBoxCollider;
    private Animator cAnim;

    private float wallJumpCooldown;
    public bool isAlive;

    private void Awake()
    {
        //Grab references for rigidbody and animator from object
        bodyAvatar = avatar.GetComponent<Rigidbody2D>();
        animAvatar = avatar.GetComponent<Animator>();
        boxColliderAvatar = avatar.GetComponent<BoxCollider2D>();
        bodyGhost = ghost.GetComponent<Rigidbody2D>();
        animGhost = ghost.GetComponent<Animator>();
        boxColliderGhost = ghost.GetComponent<BoxCollider2D>();
        cBody = bodyAvatar;
        cBoxCollider = boxColliderAvatar;
        cAnim = animAvatar;
        isAlive = true;
    }
    void Update()
    {
        //Check whether the player is releasing the left alt key
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            canSwitch = true;
        }

        //Switch between ghost and human form if able
        if (canSwitchCharacter() && canSwitch && Input.GetKey(KeyCode.LeftAlt) && isAlive)
        {
            isGhost = !isGhost;
            if (!isGhost)
            {
                cBody = bodyAvatar;
                cBoxCollider = boxColliderAvatar;
                cAnim = animAvatar;
                ghost.SetActive(false);

            }
            else
            {
                //Set position of the ghost to right in front of the avatar
                cBody = bodyGhost;
                cBoxCollider = boxColliderGhost;
                cAnim = animGhost;
                bodyGhost.transform.position = bodyAvatar.position;
                ghost.SetActive(true);

            }
            canSwitch = false;
        }

        if (!isGhost)
        {
            characterController(avatar.transform);
        }
        else
        {
            characterController(ghost.transform);
        }
    }

    void characterController(Transform character)
    {

        //read the inputs and make character.transform.position change accordingly
        horizontalInput = Input.GetAxis("Horizontal");
        
        //Flip player when moving left-right
        if (horizontalInput > 0.01f)
            character.localScale = Vector3.one;
        else if (horizontalInput < -0.01f)
            character.localScale = new Vector3(-1, 1, 1);

        //Set animator parameters
        cAnim.SetBool("Running", horizontalInput != 0);
        cAnim.SetBool("Grounded", isGrounded());

        //Wall jump logic
        if (wallJumpCooldown > 0.2f && isAlive)
        {
            cBody.velocity = new Vector2(horizontalInput * speed, cBody.velocity.y);
            if (onWall() && !isGrounded())
            {
                cBody.gravityScale = 0;
                cBody.velocity = Vector2.zero;
            }
            else
            {
                if (!isGhost)
                {
                    cBody.gravityScale = 3;
                }
            }

            if (Input.GetKey(KeyCode.Space) && !isCrouching)
            {
                Jump(character);
            }
        }
        else
            wallJumpCooldown += Time.deltaTime;

        //Ghost Movement
        if (isGhost)
        {
            if (Input.GetKey(KeyCode.W) && isAlive)
                bodyGhost.velocity = new Vector2(bodyGhost.velocity.x, jumpPower);
            
            if (Input.GetKeyUp(KeyCode.W) && isAlive) 
                bodyGhost.velocity = new Vector2(bodyGhost.velocity.x, 0);

            if (Input.GetKey(KeyCode.S) && isAlive)
                bodyGhost.velocity = new Vector2(bodyGhost.velocity.x, -jumpPower);
            
            if (Input.GetKeyUp(KeyCode.S) && isAlive)
                bodyGhost.velocity = new Vector2(bodyGhost.velocity.x, 0);
        }
        
        //Crouching and kill bind
        if(Input.GetKey(KeyCode.P))
        {
            onDeath();
        }
        if(Input.GetKey(KeyCode.C) && !isCrouching && toggleCrouch)
        {
            speed = 3;
            toggleCrouch = false;
            isCrouching = !isCrouching;          
        }
        if(Input.GetKey(KeyCode.C) && isCrouching && toggleCrouch)
        {
            speed = 10;
            toggleCrouch = false;
            isCrouching = !isCrouching;
        }
        if(Input.GetKeyUp(KeyCode.C))
        {
            toggleCrouch = true;
        }
    }

    bool canSwitchCharacter()
    {
        //Have to be stationary to switch to ghost form
        if (bodyAvatar.velocity.magnitude != 0)
            return false;
        else
            return true;
    }

    private void Jump(Transform character)
    {
        if (isGrounded())
        {
            cBody.velocity = new Vector2(cBody.velocity.x, jumpPower);
            cAnim.SetTrigger("Jump");
        }
        else if (onWall() && !isGrounded())
        {
            if (horizontalInput == 0)
            {
                cBody.velocity = new Vector2(-Mathf.Sign(character.localScale.x) * 10, 0);
                character.localScale = new Vector3(-Mathf.Sign(character.localScale.x), character.localScale.y, character.localScale.z);
            }
            else
                cBody.velocity = new Vector2(-Mathf.Sign(character.localScale.x) * 3, 6);

            wallJumpCooldown = 0;
        }
    }
    private async void onDeath()
    {
        isAlive = false;
        avatar.SetActive(false);
        bodyAvatar.velocity = Vector3.zero;
        GameObject rp = respawnManager.GetRespawnPoint();
        await Task.Delay(2500);
        avatar.transform.position = rp.transform.position;
        avatar.SetActive(true);
        isAlive = true;
    }
    private bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(cBoxCollider.bounds.center, cBoxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }
    private bool onWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(cBoxCollider.bounds.center, cBoxCollider.bounds.size, 0, new Vector2(transform.localScale.x, 0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }
    public bool canAttack()
    {
        return horizontalInput == 0 && isGrounded() && !onWall();
    }
}
