using NUnit.Framework.Constraints;
using UnityEngine;

public class BoutiqueManager : MonoBehaviour
{
    [SerializeField] GameObject menuBoutique;

    public void Echap()
    {
        menuBoutique.SetActive(false);
    }
}
