using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// This is a derived class from Enemy 
public class Chaser : Enemy
{

    [Header("References")]
    //Lets Store the initialize currentNode and create a list for path
    [SerializeField] private Node currentNode;
    [SerializeField] private Transform player;
    [SerializeField] private Transform Sprite;
    [SerializeField] private Collider2D NPCcollider;
    [SerializeField] private List<Node> AllNodesinTheScene = new List<Node>();
    [SerializeField] private List<Node> AllEdgeNodesinTheScene = new List<Node>();
    [SerializeField] private ParticleSystem Dust;

    [SerializeField] private LayerMask platformLayer;

    [Header("Movement / Pathing")]
    [SerializeField] private List<Node> path = new List<Node>();
    [SerializeField] private float pathCheckInterval = 0.5f;
    private int Movespeed { get; set; }

    [Header("Debug")]
    [SerializeField]private bool debugLogs = false;
    [SerializeField] private bool isGrounded;
    private bool noPlatformxMax;
    private bool noPlatformxMin;

    public Coroutine PathUpdaterHandler;

    private bool wasGroundedLastFrame = true;

    private int _chaserMaxHealth { get; set; }
    private int _chaserDamageDealAmount { get; set; }
    private int _chaserDamageGives { get; set; }

    public override void EnemyOnEnable()
    {
        base.EnemyOnEnable();

        AssignChaserAttributes();

        //Assign the Player To the Chaser to track
        if (player == null)
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            player = playerGO?.GetComponent<Transform>();
            if (player == null)
            {
                Debug.LogWarning("Player not found");
            }
        }

        // Adding all the Nodes in the Scene to the list
        if (AStarManager.instance != null)
        {
            AllNodesinTheScene = AStarManager.instance.AllNodesInTheScene;
            AllEdgeNodesinTheScene = AStarManager.instance.AllEdgeNodesInTheScene;
        }

        //Getting the Current Node of the NPC
        currentNode = GetNearestNode(transform.position);

        //PathUpdation
        pathUpdating();
    }

    public override void EnemyOnStart()
    {
        base.EnemyOnStart();

        //Getting all the required Components
        NPCcollider = GetComponent<Collider2D>();
        Sprite = GetComponent<Transform>();

        //Debug.Log("All Nodes" + AllNodesinTheScene.Count);
        //Debug.Log("All Edge Nodes" + AllEdgeNodesinTheScene.Count);

        
    }

    private void AssignChaserAttributes()
    {
        GameObject chaserPrefab = GameManager._instance.GetPrefabByEnemyType(EnemyType.Chaser);

        if(GameManager._instance != null && GameManager._instance.TryGetEnemyData(chaserPrefab, out var data))
        {
            //Setting the MoveSpeed and damage Gives to the Chaser
            Movespeed = data._moveSpeed;
            _chaserDamageGives = data._damageGives;
        }
    }

    public void pathUpdating()
    {
        PathUpdaterHandler = StartCoroutine(PathUpdater());
    }

    public void FreezeSprite()
    {
        Sprite.transform.rotation = Quaternion.identity;
    }

    public void FollowPlayer()
    {
        //Setting the Grounded Bool for animator
        _animator.SetBool("Grounded",isGrounded);

        if (path == null || path.Count == 0)
        {
            RB.velocity = new Vector2(0f, RB.velocity.y);
            return;
        }

        int lookAhead = Mathf.Min(2, path.Count - 1);
        Node targetNode = path[lookAhead];
        Node nextNode = null; // will be used in checking if the node above is blocked by the platoform
        Vector3 targetPos = targetNode.transform.position;

        float extX = 0.5f;
        float extY = 0.5f;

        if (NPCcollider != null)
        {
            //Bounds.extent gives us the half the size of the Object in each axis
            //So if the Object is 2 units in x and 2 units in y it gives 1 and 1 in each axis
            extX = NPCcollider.bounds.extents.x;
            extY = NPCcollider.bounds.extents.y;
        }

        // These vlaue will be acting as box around the nodes that counts as close enough
        // If NPC is 2x2 -> extX =1, extY =1 -> horizThres = 1, VetiThres = 1.2.
        //That means if the NPC is within 1 unit in X and 1.2 units in Y of the Node we will accept it as close enough to consume the node
        float horizThreshold = Mathf.Max(extX * 1.0f, 1.5f); //Minimum sensible x tolerance
        float vertThreshold = Mathf.Max(extY * 1.2f, 1.5f); // allow some Y tolerance

        float direction = Mathf.Sign(targetPos.x - transform.position.x);

        float speedMultiplier = isGrounded ? 1f : 0.5f;

        //Flip the Enemy
        CheckForLeftorRightFacing(new Vector2(direction,0));


        if (isGrounded && !Mathf.Approximately(direction, 0f))
        {
            //-------------Horizontal Movement--------------
            if (Mathf.Approximately(direction, 0f))
            {
                RB.velocity = new Vector2(0f, RB.velocity.y);
            }
            else
            {
                RB.AddForce(new Vector2(direction * Movespeed * speedMultiplier, 0));
                Debug.Log("Dust particle has to play");

                //Particle Effect for Dust when the NPC moves
            }

            if(Mathf.Abs(RB.velocity.x) > 2)
            {
                Dust.Play();
            }
        }

        // Making a slight force in x so that the NPC keeps moving while it jumps and not get stuck by jumping straight while it want to reach a ledge of a platform
        if (Mathf.Approximately(direction, 0f))
        {
            RB.velocity = new Vector2(0f, RB.velocity.y);
        }
        else
        {
            RB.AddForce(new Vector2(direction * Movespeed * speedMultiplier, 0));
        }

        //----------Check if next node is straight above but its blocked----------
        if (path.Count > 1)
        {
            nextNode = path[1];

        }
        if (nextNode != null)
        {
            Vector2 directionToNode = (nextNode.transform.position - transform.position).normalized;
            bool isAbove = directionToNode.y > 0.8f;

            if (isAbove)
            {
                Vector3 nextNodePos = nextNode.transform.position;
                RaycastHit2D hit = Physics2D.Linecast(transform.position, nextNodePos, platformLayer);
                Debug.DrawLine(transform.position, nextNodePos, hit.collider != null ? Color.red : Color.blue, 0.2f);

                if (hit.collider != null)
                {
                    //Add a little force to the NPC to move little bit so that it doesnt keep jumping in same position
                    float dirX = Mathf.Sign(nextNode.transform.position.x - transform.position.x);
                    RB.AddForce(new Vector2(dirX * 5f * 0.5f, 0), ForceMode2D.Impulse);

                    //Recalculate the Path to edgeNode near NPC
                    Node NPCEdgeNode = GetNearestEdgeNode(transform.position);
                    var newPath = AStarManager.instance.GeneratePath(currentNode, NPCEdgeNode);
                    if (newPath != null && newPath.Count > 0)
                    {
                        path.Clear();
                        path.AddRange(newPath);
                        if (debugLogs) Debug.Log($"PathUpdater: Rebuilt path to predicted node ({path.Count})");
                    }
                }
            }
        }

        if (isGrounded)
        {
            //-----------jump Logic--------------:
            float dx = targetPos.x - transform.position.x;
            float dy = targetPos.y - transform.position.y;

            float gravity = Mathf.Abs(Physics2D.gravity.y * RB.gravityScale);
            //Debug.Log(gravity);

            //Clamp minimum jump height to required number of tile
            float minJumpHeight = 4.0f;
            float maxJumpHeight = 60f;

            if ((dy > 1.5f || (Mathf.Abs(dy) < 0.2f && Mathf.Abs(dx) > 2f)) && isGrounded)
            {

                float jumpHeight = Mathf.Clamp(dy + minJumpHeight, minJumpHeight, maxJumpHeight);

                //calculate minimum vertical velocity needed to reach by
                float requiredVy = Mathf.Sqrt(2 * gravity * jumpHeight); //margin 
                requiredVy = Mathf.Clamp(requiredVy, minJumpHeight * 1.5f, maxJumpHeight * 1.5f);
                float flightTime = (2 * requiredVy) / gravity;// total Flight Time = (2 * vY) / g

                // Horizontal velocity to cover dx in that time
                float requiredVX = dx / flightTime * 3f;
                requiredVX = Mathf.Clamp(requiredVX, -Movespeed * 1.5f, Movespeed * 1.5f);

                // Reset velocity to avoid old momentum interfering
                RB.velocity = Vector2.zero;

                //Jump Animation:
                _animator.SetTrigger("Jump");

                // Apply one precise jump impulse
                RB.AddForce(new Vector2(requiredVX, requiredVy) * RB.mass, ForceMode2D.Impulse);

                Dust.Play();

                //Jumping across the platform
                if (!noPlatformxMax || !noPlatformxMin)
                {
                    if (path.Count > 1)
                    {
                        float verticalDifference = nextNode.transform.position.y - transform.position.y;

                        float jumpthreshold = 1f;

                        if (Mathf.Abs(verticalDifference) <= jumpthreshold)
                        {
                            RB.velocity = new Vector2(requiredVX, requiredVy);
                        }
                        else if (verticalDifference > 0)
                        {
                            RB.velocity = new Vector2(requiredVX, requiredVy);
                        }
                        else if (verticalDifference < jumpthreshold)
                        {
                            RB.velocity = new Vector2(requiredVX, requiredVy);
                        }
                    }
                }
            }


        }

        // Near Node check 
        bool closeEnoughX = Mathf.Abs(transform.position.x - targetPos.x) <= horizThreshold;
        bool closeEnoughY = Mathf.Abs(transform.position.y - targetPos.y) <= (vertThreshold * 2f);
        bool closeEnough = closeEnoughX && closeEnoughY;

        // We dont have to check the vertical threshold when the NPC jumps (Reason: When the NPC jumps from platform below to platform which is above or platform to platform)
        if (!isGrounded)
        {
            closeEnough = closeEnoughX;
        }

        bool passedNode = false;

        //If Moving right NPC has gone beyond the node's X position + half of the threshold so we passed it, : same goes for the left side too.
        if (direction > 0)
            passedNode = transform.position.x > (targetPos.x + horizThreshold * 0.5f);
        else if (direction < 0)
            passedNode = transform.position.x < (targetPos.x - horizThreshold * 0.5f);
        else
            passedNode = closeEnough;

        if (closeEnough || passedNode)
        {
            //Consume Node 
            currentNode = targetNode;
            path.RemoveAt(0);
        }


        if (path.Count > 1 && Physics2D.Linecast(transform.position, targetPos, platformLayer))
        {
            Node thenextNode = path[1];
            if (Vector2.Distance(transform.position, thenextNode.transform.position) < Vector2.Distance(transform.position, targetNode.transform.position))
            {
                //skip ahead
                path.RemoveAt(0);
                targetNode = thenextNode;
            }
        }

        if (!isGrounded && wasGroundedLastFrame)
        {
            _animator.SetTrigger("InAir");
        }
        if(isGrounded && !wasGroundedLastFrame && Mathf.Abs(RB.velocity.y) > 3f)
        {
            _animator.SetTrigger("Landed");
        }

        //Updating the bool for GroundCheck in last frame
        wasGroundedLastFrame = isGrounded;

    }

    IEnumerator PathUpdater()
    {
        var wait = new WaitForSeconds(pathCheckInterval);

        while (true)
        {
            Node playerNode = GetNearestNode(player.position);

            if (playerNode != null)
            {
                if (debugLogs) Debug.Log($"PlayerPosition: {player.position} | Nearest node: {playerNode.name} at {playerNode.transform.position}");
            }

            //Checking if the player is near the NpC if yes then the nodes in the path is cleared and created a new path to the player 
            float PlayerDist = Vector2.Distance(transform.position, player.position);

            if (PlayerDist < 2f)
            {
                if (playerNode != null)
                {
                    var newPath = AStarManager.instance.GeneratePath(currentNode, playerNode);
                    if (newPath != null && newPath.Count > 0)
                    {
                        path.Clear();
                        path.AddRange(newPath);
                        if (debugLogs) Debug.Log($"PathUpdater: Rebuilt path to predicted node ({path.Count})");
                    }
                }
            }
            else
            {
                if (currentNode != null && playerNode != null)
                {
                    // Debug.Log(currentNode+"_-_"+playerNode);
                    var newPath = AStarManager.instance.GeneratePath(currentNode, playerNode);
                    if (newPath != null && newPath.Count > 0)
                    {
                        path.Clear();
                        path.AddRange(newPath);
                        if (debugLogs) Debug.Log($"PathUpdater: Rebuilt path to predicted node ({path.Count})");
                    }
                }
            }
            yield return wait;
        }
    }


    Node GetNearestNode(Vector3 pos)
    {
        Node nearestNode = null;

        float shortest = float.MaxValue;

        foreach (var node in AllNodesinTheScene)
        {
            float distance = (pos - node.transform.position).sqrMagnitude;

            if (distance < shortest)
            {
                shortest = distance;
                nearestNode = node;
            }
        }

        return nearestNode;
    }

    Node GetNearestEdgeNode(Vector3 Pos)
    {
        Node nearestEdgeNode = null;

        float shortest = float.MaxValue;
        foreach (var node in AllEdgeNodesinTheScene)
        {
            float distance = (Pos - node.transform.position).sqrMagnitude;

            if (distance < shortest)
            {
                shortest = distance;
                nearestEdgeNode = node;
            }
        }

        return nearestEdgeNode;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.black;
        Vector3 prev = transform.position;
        foreach (var n in path)
        {
            Gizmos.DrawLine(prev, n.transform.position);
            prev = n.transform.position;
        }

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

    public void groundedAndplatformCheck()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, platformLayer);
        Debug.DrawLine(transform.position, transform.position + Vector3.down * 1.1f, isGrounded ? Color.green : Color.white);

        Vector2 JumpOffsetOriginXMax = new Vector2(transform.position.x + 0.5f, NPCcollider.bounds.min.y);
        Vector2 jumpOffOriginXMin = new Vector2(transform.position.x - 0.5f, NPCcollider.bounds.min.y);
        noPlatformxMax = Physics2D.Raycast(JumpOffsetOriginXMax, Vector2.down, 1.1f, platformLayer);
        noPlatformxMin = Physics2D.Raycast(jumpOffOriginXMin, Vector2.down, 1.1f, platformLayer);

        Debug.DrawLine(JumpOffsetOriginXMax, JumpOffsetOriginXMax + Vector2.down * 1.1f, Color.magenta);
        Debug.DrawLine(jumpOffOriginXMin, jumpOffOriginXMin + Vector2.down * 1.1f, Color.magenta);
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out IPlayerDamageable damageable))
        {
            Vector2 hitDirection = (collision.transform.position - transform.position).normalized;
            damageable.Damage(_chaserDamageGives, hitDirection);
        }
    }
}
