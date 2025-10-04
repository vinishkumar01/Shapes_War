using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random =  UnityEngine.Random;

public class Tracer_NPC_Test : MonoBehaviour, IHittable
{
    [Header("Reference")]
    [SerializeField] Transform player;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Collider2D NPCcollider;
    [SerializeField] ParticleSystem dust;
    [SerializeField] LayerMask platformLayer;
    [SerializeField] LayerMask player_Layer;
    [SerializeField] List<Node> AllNodesInTheScene = new List<Node>();
    [SerializeField] List<Node> AllEdgeNodeInTheScene = new List<Node>();

    [Header("Nodes Config")]
    [SerializeField] Node currentNode;
    [SerializeField] List<Node> path = new List<Node>();
    [SerializeField] float pathCheckInterval = 0.5f;

    [Header("NPC movements Config")]
    [SerializeField] int MoveSpeed = 5;
    [SerializeField] Vector3 FacingDirection;

    [Header("Conditions")]
    [SerializeField] bool isGrounded;
    [SerializeField] bool wasGrounded;
    [SerializeField] bool isPlayerDetected;
    [SerializeField] bool isPlayerNear;
    [SerializeField] bool isFacingPlayer;
    RaycastHit2D noPlatformxMax;
    RaycastHit2D noPlatformxMin;
    [SerializeField] float playerDetectionCheckRadius = 20f;
    [SerializeField] float PlayerNearCheckRadius = 30f;
    [SerializeField] float GroundCheckRadius = 0.9f;
    [SerializeField] Vector2 GroundCheckOffset;



    [Header("NPC Health")]
    [SerializeField] int NPCHealth = 100000;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        NPCcollider = rb.GetComponent<Collider2D>();

        if(AStarManager.instance != null)
        {
            AllNodesInTheScene = AStarManager.instance.AllNodesInTheScene;
            AllEdgeNodeInTheScene = AStarManager.instance.AllEdgeNodesInTheScene;
        }
        Debug.Log("ALL NODES:" + AllNodesInTheScene.Count + "ALL EDGE NODES" + AllEdgeNodeInTheScene.Count);

        // getting the node near NPC for every frame
        currentNode = GetNearestNode(transform.position);

        //setting the default facing direction of the NPC
        FacingDirection = transform.localScale;

        // Starts creating a path when player is detected
        StartCoroutine(PathUpdater());
    }

    private void Update()
    {
        FacingPlayer();
        playerDetection();
        SurroundingCheck();
        
    }

    private void FixedUpdate()
    {
        Retreat();
    }

    void IHittable.RecieveHit(RaycastHit2D RayHit)
    {
        Debug.Log("Got Hit: by Circle");
        NPCHealth -= 10;

        if (NPCHealth == 0)
        {
            Destroy(gameObject);
        }
    }

    #region Player-Detection, Get available Nodes, PathUpdate, Retreat if Player-Detected, Surrounding Check
    void playerDetection()
    {
        isPlayerDetected = Physics2D.OverlapCircle(transform.position, playerDetectionCheckRadius, player_Layer);
        isPlayerNear = Physics2D.OverlapCircle(transform.position, PlayerNearCheckRadius, player_Layer);

    }

    IEnumerator PathUpdater()
    {
        var wait = new WaitForSeconds(pathCheckInterval);

        while (true)
        {
            if(isPlayerDetected)
            {
                Node retreatNode = GetRetreatNode(transform.position, player.position);

                if (currentNode != null && retreatNode != null)
                {
                    var newpath = AStarManager.instance.GeneratePath(currentNode, retreatNode);
                    if (newpath != null && newpath.Count > 0)
                    {
                        path.Clear();
                        path.AddRange(newpath);
                    }
                }
            }
            

            yield return wait;
        }

    }

    void Retreat()
    {
        if(path == null || path.Count == 0)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        Node targetNode = path[0];
        Vector3 targetPos = targetNode.transform.position;

        float extX = 1f;
        float extY = 1f;

        if(NPCcollider != null)
        {
            // Bounds.extent gives the half the size of the Object in each axis
            // For Example If the objects size is 2 in both x and y axis, extents gives the half the size 1 in x and 1 in y;
            extX = NPCcollider.bounds.extents.x;
            extY = NPCcollider.bounds.extents.y;
        }

        // The horizontal and vertical threshold acts as bounding box around the nodes that counts as close enough
        // If NPC is 2x2 given-> extX =1 & extY = 1 horithres gives -> 1.5 and vertithreshold gives -> 1.5
        // This means if the NPC is within 1.5 unit in X and 1.5 unit in Y of the node we will accept it as close enough to consumed the node
        float horizThreshold = Mathf.Max(extX * 1.0f, 1.5f);
        float vertiThreshold = Mathf.Max(extY * 1.2f, 1.5f);

        float direction = Mathf.Sign(targetPos.x - transform.position.x);

        //------Flipping the character to the targetNode if the Player is detected------
        if (isPlayerDetected)
        {
            if (direction > 0)
            {
                transform.localScale = FacingDirection;
            }
            else if (direction < 0)
            {
                transform.localScale = new Vector2(-FacingDirection.x, FacingDirection.y);
            }
        }

        // ------------ Horizontal Movements-------------
        if (isGrounded)
        {
            if (Mathf.Approximately(direction, 0))
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(direction * MoveSpeed, rb.velocity.y);
            }
        }

        // applying a small force in x-axis to move from the ledge to platform while jumping
        if(!isGrounded)
        {
            if (Mathf.Approximately(direction, 0))
            {
                rb.velocity = new Vector2(0f, rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(direction * 9, rb.velocity.y);
            }
        }

        //-----------Jump Behaviour--------------
        if (isGrounded)
        {
            float dx = targetPos.x - transform.position.x;
            float dy = targetPos.y - transform.position.y;

            float gravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);

            //Clamp required mini and max jump height
            float minJumpHeight = 5.0f;
            float maxJumpHeight = 60f;

            if (dy > 1.5f && isGrounded)
            {
                float JumpHeight = Mathf.Clamp(dy + minJumpHeight, minJumpHeight, maxJumpHeight);

                //Calculate minimum vertical velocity needed to reach by 
                float requiredVY = Mathf.Sqrt(2 * gravity * JumpHeight);
                requiredVY = Mathf.Clamp(requiredVY, minJumpHeight * 1.5f, maxJumpHeight * 1.5f);
                float flightTime = (2 * requiredVY) / gravity;

                //Horizontal velocity to cover dx in the jump time
                float requiredVX = dx / flightTime * 3f;
                requiredVX = Math.Clamp(requiredVX, -MoveSpeed * 1.5f, MoveSpeed * 1.5f);

                //Reset velocity to avoid old momentum to interfere
                rb.velocity = Vector2.zero;

                //Apply one precise jump impulse
                rb.AddForce(new Vector2(requiredVX, requiredVY) * rb.mass, ForceMode2D.Impulse);

            }
        }


        //Near Node Check
        bool closeEnoughX = Mathf.Abs(transform.position.x - targetPos.x) <= horizThreshold;
        bool closeEnoughY = Mathf.Abs(transform.position.y - targetPos.y) <= (vertiThreshold * 2f);
        bool closeEnough = closeEnoughX && closeEnoughY;

        // We dont have to check the vertical threshold when the NPC jumps (Reason: When the NPC jumps from platform below to platform which is above or platform to platform)
        if (!isGrounded)
        {
            closeEnough = closeEnoughX;
        }

        bool passedNode = false;

        //If Moving right NPC has gone beyond the node's X position + half of the threshold so we passed it, : same goes for the left side too.
        if (direction > 0)
            passedNode = transform.position.x > (targetPos.x + horizThreshold * 1f);
        else if (direction < 0)
            passedNode = transform.position.x < (targetPos.x + horizThreshold * 1f);
        else 
            passedNode = closeEnough;

        if(closeEnough || passedNode)
        {
            //Consume Node
            currentNode = targetNode;
            path.RemoveAt(0);
        }

        //Skip ahead if the upcoming node distance is less than targetNode
        if (path.Count > 1 && Physics2D.Linecast(transform.position, targetPos, platformLayer))
        {
            Node UpComingNode = path[1];

            if (Vector2.Distance(transform.position, UpComingNode.transform.position) < Vector2.Distance(transform.position, targetNode.transform.position))
            {
                path.RemoveAt(0);
                targetNode = UpComingNode;
            }
        }

        //if the NPC is in the air and moving along x axis remove nodes
        if(!isGrounded && Mathf.Abs(transform.position.x - targetPos.x) < horizThreshold * 2f)
        {
            path.RemoveAt(0);
        }
    }

    Node GetNearestNode(Vector3 Pos)
    {
        Node nearestNode = null;
        float shortest = Mathf.Infinity;

        foreach(var node in AllNodesInTheScene)
        {
            float distance = (Pos - node.transform.position).sqrMagnitude;

            if(distance < shortest)
            {
                shortest = distance;
                nearestNode = node;
            }
        }
        
        return nearestNode;
    }

    Node GetRetreatNode(Vector3 NPCpos, Vector3 playerPos)
    {
        if(AllNodesInTheScene == null)
        return null;

        bool isPlayerRight = playerPos.x > NPCpos.x;

        Node farthestNode = null;
        float farthest = float.MaxValue;

        foreach (var node in AllNodesInTheScene)
        {
            float xDirection = node.transform.position.x - NPCpos.x;

            //if player is on right pick node from -x direction = -20f
            if (isPlayerRight && xDirection <= -20f)
            {
                float distance = (NPCpos - node.transform.position).sqrMagnitude;

                if (distance < farthest)
                {
                    farthest = distance;
                    farthestNode = node;
                }
            }
            // if player is on the left pick a node x direction = +20f
            else if(!isPlayerRight && xDirection >= 20f)
            {
                float distance = (NPCpos - node.transform.position).sqrMagnitude;

                if(distance < farthest)
                {
                    farthest = distance;
                    farthestNode = node;
                }
            }
            
        }

        return farthestNode;
        
    }

    void FacingPlayer()
    {
        if((path == null || path.Count == 0) && !isPlayerDetected)
        {
            //Flip the Character
            float watchingPlayer = Mathf.Sign(player.position.x - transform.position.x);

            if (watchingPlayer > 0)
            {
                transform.localScale = FacingDirection;
            }
            else if (watchingPlayer < 0)
            {
                transform.localScale = new Vector2(-FacingDirection.x, FacingDirection.y);
            }
        }
    }

    void SurroundingCheck()
    {
        GroundCheckOffset = new Vector2(transform.position.x, NPCcollider.bounds.min.y);
        isGrounded = Physics2D.OverlapCircle(GroundCheckOffset, 1f);

        //Vector2 JumpOffsetOriginXMax = new Vector2(transform.position.x + 1f, NPCcollider.bounds.min.y);
        //Vector2 jumpOffOriginXMin = new Vector2(transform.position.x - 1f, NPCcollider.bounds.min.y);
        //noPlatformxMax = Physics2D.Raycast(JumpOffsetOriginXMax, Vector2.down, 0.51f, platformLayer);
        //noPlatformxMin = Physics2D.Raycast(jumpOffOriginXMin, Vector2.down, 0.51f, platformLayer);

        //Debug.DrawLine(JumpOffsetOriginXMax, JumpOffsetOriginXMax + Vector2.down * 0.51f, Color.magenta);
        //Debug.DrawLine(jumpOffOriginXMin, jumpOffOriginXMin + Vector2.down * 0.51f, Color.magenta);


    }
#endregion

    private void OnDrawGizmos()
    {
        
        if (isPlayerDetected)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, playerDetectionCheckRadius);
            Gizmos.DrawWireSphere(transform.position, PlayerNearCheckRadius);
        }
        else
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, playerDetectionCheckRadius);
            Gizmos.DrawWireSphere(transform.position, PlayerNearCheckRadius);
        }

        Gizmos.DrawWireSphere(GroundCheckOffset, GroundCheckRadius);

        if (!Application.isPlaying) return;

        Vector3 prev = transform.position;

        foreach(var n in path)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawLine(prev, n.transform.position);
            prev = n.transform.position;
        }


        //Draw bound Box around the Nodes
        if (isPlayerDetected)
        {
            if (path != null && path.Count > 0)
            {
                Node targetNode = path[0];
                Vector3 targetPos = targetNode.transform.position;

                float horiThreshold = Mathf.Max(NPCcollider.bounds.extents.x * 1.0f, 1.5f);
                float vertiThreshold = Mathf.Max(NPCcollider.bounds.extents.y * 1.2f, 1.5f);

                Vector3 size = new Vector3(horiThreshold * 2f, vertiThreshold * 2f, 0.1f);

                if (Mathf.Abs(transform.position.x - targetPos.x) <= horiThreshold &&
                    Mathf.Abs(transform.position.y - targetPos.y) <= vertiThreshold)
                {
                    Gizmos.color = Color.yellow; // inside threshold
                }
                else
                {
                    Gizmos.color = Color.green; // outside
                }
                Gizmos.DrawWireCube(targetPos, size);
            }
        }
       
    }

}
