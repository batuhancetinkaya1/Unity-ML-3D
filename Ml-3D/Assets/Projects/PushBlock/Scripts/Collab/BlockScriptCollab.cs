using UnityEngine;

public enum BlockType
{
    Light = 1, 
    Mid = 3,  
    Heavy = 5 
}

public class BlockScriptCollab : MonoBehaviour
{
    public bool isCollab = true;
    public BlockType blockType;
    [HideInInspector] public PushBlockCollabEnvController envController;
    private Rigidbody rb;
    private Quaternion startRot;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startRot = transform.rotation;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("goal"))
        {
            if (isCollab && envController != null)
            {
                gameObject.SetActive(false);
                envController.NotifyBlockReachedGoal(blockType);
            }
        }
    }

    public void ResetBlock()
    {
        gameObject.SetActive(true);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Vector3 pos = envController.GetRandomSpawnPosition();
        transform.position = pos;
        transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
    }
}
