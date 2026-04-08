using System.Collections;
using UnityEngine;

public class BilletManager : MonoBehaviour
{
    [SerializeField] private GameObject billet;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject HautEcran;
    [SerializeField] private GameObject BasEcran;
    public void BilletAnim()
    {
        billet.SetActive(true);
        StartCoroutine(Anim());
    }
    public IEnumerator Anim()
    {
        yield return new WaitForSeconds(1f);
        animator.SetTrigger("AnimBillet");

        yield return new WaitForSeconds(12.5f);
           billet.SetActive(false);
    }
}
