using UnityEngine;

public class BlockScript : MonoBehaviour
{
    // Tek ajan referansý
    public PushBlock_Basic singleAgent;

    private void OnCollisionEnter(Collision collision)
    {
        // Eðer block "goal" tag'li objeye temas ederse...
        if (collision.gameObject.CompareTag("goal"))
        {
            if (singleAgent != null)
            {
                singleAgent.ScoredAGoal();
            }
        }
    }
}

