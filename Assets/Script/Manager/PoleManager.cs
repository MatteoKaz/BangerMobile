using System.Linq;
using UnityEngine;

public class PoleManager : MonoBehaviour
{
    [SerializeField] Pole[] poles;
    public int quotaGlobal;

    public void QuotaGiver(int quota)
    {
        for (int i = 0; i < poles.Length - 1; i++)
        {
            poles[i].localQuotat = quota / 3;

        }
    }
}
