using UnityEngine;
using System.Collections.Generic;

public class PatrolPath : MonoBehaviour
{
    public List<Transform> patrolPoints = new List<Transform>();
    public bool isSelected = false;

    private void OnDrawGizmos()
    {
        Gizmos.color = isSelected ? Color.green : Color.red;
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            if (patrolPoints[i] != null)
            {
                Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);
                if (i > 0)
                {
                    Gizmos.DrawLine(patrolPoints[i - 1].position, patrolPoints[i].position);
                }
            }
        }
    }
}
