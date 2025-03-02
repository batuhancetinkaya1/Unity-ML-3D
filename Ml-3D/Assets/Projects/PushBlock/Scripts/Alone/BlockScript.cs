using UnityEngine;

public class BlockScript : MonoBehaviour
{
    // Tek ajan referans�
    public PushBlock_Basic singleAgent;

    private void OnCollisionEnter(Collision collision)
    {
        // E�er block "goal" tag'li objeye temas ederse...
        if (collision.gameObject.CompareTag("goal"))
        {
            if (singleAgent != null)
            {
                singleAgent.ScoredAGoal();
            }
        }
    }
}

