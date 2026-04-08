using System.Collections;
using UnityEngine;

public class BilletManager : MonoBehaviour
{
    [SerializeField] private GameObject billet;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject HautEcran;
    [SerializeField] private GameObject BasEcran;
    private readonly float animDuration = 0.15f;
    public bool Launch = false;
    public void BilletAnim()
    {
        if (Launch==false)
        {
            Launch = true;
            billet.SetActive(true);
            StartCoroutine(Anim());
        }
       
        
    }
    public IEnumerator Anim()
    {
        
        RectTransform rect = HautEcran.GetComponent<RectTransform>();
        RectTransform rect1 = BasEcran.GetComponent<RectTransform>();

        Vector3 startPos = Vector3.zero;
        Vector3 targetPos = new Vector3(1.03f, 1.03f, 1.03f);

        // Ouverture
        float t = 0f;
        yield return new WaitForSeconds(0.2f);
        while (t < animDuration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / animDuration);
            rect.localScale = Vector3.Lerp(startPos, targetPos, n);
            rect1.localScale = Vector3.Lerp(startPos, targetPos, n);
            yield return null;
        }

        yield return new WaitForSeconds(1f);
        animator.SetTrigger("AnimBillet");
        yield return new WaitForSeconds(12.5f);

        // Fermeture
        t = 0f; // reset indispensable
        while (t < animDuration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / animDuration);
            rect.localScale = Vector3.Lerp(targetPos, startPos, n);
            rect1.localScale = Vector3.Lerp(targetPos, startPos, n);
            yield return null;
        }

        billet.SetActive(false);
        Launch = false;
    }
}
