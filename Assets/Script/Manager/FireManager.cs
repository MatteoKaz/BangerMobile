using System.Collections;

using UnityEngine;
using UnityEngine.UI;


public class FireManager : MonoBehaviour
{
    [Header("Ref")]
    [SerializeField] public EmployeFicheInfo empFiche;
    [SerializeField] public DayManager dayManager;
    [SerializeField] Image employeImage;
    [SerializeField] Animator animator;
    [SerializeField] GameObject FiredScene;
    [SerializeField] GameObject ScoreScene;
    [SerializeField] GameObject dialogueCase;
    [SerializeField] TypeWriter typeWriter;
    [SerializeField] GameObject Roulette;
    [SerializeField] RouletteWheel RouletteScript;
    [SerializeField] GameObject Tube;
    [SerializeField] GameObject choiceButtons;

    [Header("AnimScoreMenu")]
    [SerializeField] public float animDuration;
    [SerializeField] public float yendPose = 2890f;
    [SerializeField] public float ybasePose = 960f;
    [SerializeField] AnimationCurve curveAnim;

    [Header("Tube")]
    [SerializeField] public float animDurationTube;
    [SerializeField] public float yendPoseTube = 2890f;
    [SerializeField] public float ybasePoseTube = 960f;
    [SerializeField] AnimationCurve curveAnimTube;

    [Header("AnimScoreDialogue")]
    [SerializeField] public float xStartPoseDialogue = 499.3f;
    [SerializeField] public float xendPoseDialogue = -289;
    [SerializeField] public float animDurationDialogue;
    [SerializeField] AnimationCurve curveAnimDialogue;
    [SerializeField] AnimationCurve curveAnimDialogueBack;
    
    [SerializeField] public AudioEventDispatcher audioEventDispatcher;
    
    private bool launch = false;
    public bool DayLaunch = false;
    public bool LaunchFire = false;
    private bool _ingameWasPlayingBeforeFire = false;

    public void OnEnable()
    {

        RouletteScript.OnEmployeSelected += ChangeEmploye;
        dayManager.DayBegin += ResetAction;
    }
    public void OnDisable()
    {

        RouletteScript.OnEmployeSelected -= ChangeEmploye;
        dayManager.DayBegin -= ResetAction;
    }
    public void ResetAction()
    {
        DayLaunch = false;
    }
    public void ChangeEmploye(EmployeDataz employeData)
    {
        int index = empFiche.employe.employeObject.allEmploye.IndexOf(employeData);
        empFiche.employe.SetIdentity(index);
        empFiche.employe.EndWeekReset();
        empFiche.employe.EndDayResetStat();
        empFiche.employe.timeInEntreprise = 0;
        StartCoroutine(QuitFired());
    }
    public void Click(EmployeFicheInfo emp)
    {
        if (DayLaunch == true)
        {
            audioEventDispatcher?.PlayAudio(AudioType.Wrong); // son de refus
            return;
        }
        if (launch == true) return;

        launch = true;
        empFiche = emp;
        LaunchFire = false;

        EmployeFicheMove.IsLocked = true; // ← verrouille le swipe

        FiredScene.SetActive(true);
        _ingameWasPlayingBeforeFire = true;
        MusicManager.Instance?.StopIngame();
        MusicManager.Instance?.PlayFire();
        typeWriter.dialogueText.text = null;
        animator.enabled = false;
        animator.enabled = true;
        employeImage.sprite = empFiche.Image.sprite;
        animator.Rebind();
        animator.Update(0f);
        StartCoroutine(FiredSet());

    }

    public IEnumerator FiredSet()
    {
        RectTransform rect = ScoreScene.GetComponent<RectTransform>();

        Vector2 startpos = new Vector2(rect.anchoredPosition.x, ybasePose);
        Vector2 targetPos = new Vector2(rect.anchoredPosition.x, yendPose);
        float t = 0;
        while (t < animDuration)
        {
            t += Time.deltaTime;
            float normalized = t / animDuration;

            float curve = curveAnim.Evaluate(normalized);
            rect.anchoredPosition = Vector2.Lerp(startpos, targetPos, curve);

            yield return null;
        }

        animator.SetTrigger("Walking");
        audioEventDispatcher?.PlayLoopAudio(AudioType.Walk);
        yield return new WaitForSeconds(0.75f);
        employeImage.enabled = true;
       
        yield return null;
        yield return null;
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("Walk") &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.25f);
        RectTransform rectDialogue = dialogueCase.GetComponent<RectTransform>();

        Vector2 startposDialogue = new Vector2(xStartPoseDialogue, rectDialogue.anchoredPosition.y);
        Vector2 targetPosDialogue = new Vector2(xendPoseDialogue, rectDialogue.anchoredPosition.y);
        t = 0;
        while (t < animDurationDialogue)
        {
            t += Time.deltaTime;
            float normalized = t / animDurationDialogue;

            float curve = curveAnimDialogue.Evaluate(normalized);
            rectDialogue.anchoredPosition = Vector2.Lerp(startposDialogue, targetPosDialogue, curve);

            yield return null;
        }
        audioEventDispatcher?.StopLoopAudio();
        yield return new WaitForSeconds(0.75f);
        typeWriter.StartDialogue(empFiche.employe.firelines, animator);
        launch = false;
    }
    public void NotFiredLaunch()
    {
        if(LaunchFire == false) 
        {
            LaunchFire = true;
            StartCoroutine(NotFired());
        }
       

    }

    public  IEnumerator NotFired()
    {
        RectTransform rectDialogue = dialogueCase.GetComponent<RectTransform>();

        Vector2 startposDialogue = new Vector2(xStartPoseDialogue, rectDialogue.anchoredPosition.y);
        Vector2 targetPosDialogue = new Vector2(xendPoseDialogue, rectDialogue.anchoredPosition.y);
        float t = 0;

        while (t < animDurationDialogue)
        {
            t += Time.deltaTime;
            float normalized = t / animDurationDialogue;

            float curve = curveAnimDialogueBack.Evaluate(normalized);
            rectDialogue.anchoredPosition = Vector2.Lerp(targetPosDialogue, startposDialogue, curve);

            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        animator.SetTrigger("WalkNotFired");
        audioEventDispatcher?.PlayLoopAudio(AudioType.Walk);
        yield return new WaitForSeconds(0.2f);
        yield return null;
        yield return null;
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("WalkBack") &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }
        audioEventDispatcher?.StopLoopAudio();
        employeImage.enabled = false;
        RectTransform rect = ScoreScene.GetComponent<RectTransform>();

        Vector2 startpos = new Vector2(rect.anchoredPosition.x, ybasePose);
        Vector2 targetPos = new Vector2(rect.anchoredPosition.x, yendPose);
         t = 0;
        while (t < animDuration)
        {
            t += Time.deltaTime;
            float normalized = t / animDuration;

            float curve = curveAnim.Evaluate(normalized);
            rect.anchoredPosition = Vector2.Lerp(targetPos, startpos, curve);

            yield return null;
        }
        EmployeFicheMove.IsLocked = false;
        MusicManager.Instance?.StopFire();
        if (_ingameWasPlayingBeforeFire)
        {
            _ingameWasPlayingBeforeFire = false;
            MusicManager.Instance?.PlayIngame();
        }

    }
    public void FiredLauncher()
    {
        if (LaunchFire ==false)
        {
            LaunchFire = true;
            StartCoroutine(Fired());
        }
        

    }

    public IEnumerator Fired()
    {

        RectTransform rectDialogue = dialogueCase.GetComponent<RectTransform>();

        Vector2 startposDialogue = new Vector2(xStartPoseDialogue, rectDialogue.anchoredPosition.y);
        Vector2 targetPosDialogue = new Vector2(xendPoseDialogue, rectDialogue.anchoredPosition.y);
        float t = 0;
       
        while (t < animDurationDialogue)
        {
            t += Time.deltaTime;
            float normalized = t / animDurationDialogue;

            float curve = curveAnimDialogueBack.Evaluate(normalized);
            rectDialogue.anchoredPosition = Vector2.Lerp(targetPosDialogue, startposDialogue, curve);

            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        RectTransform rectTube = Tube.GetComponent<RectTransform>();

        Vector2 startposTube = new Vector2(rectTube.anchoredPosition.x, ybasePoseTube);
        Vector2 targetPosTube = new Vector2(rectTube.anchoredPosition.x, yendPoseTube);
       
        t = 0;
      
        while (t < animDurationTube)
        {
            t += Time.deltaTime;
            float normalized = t / animDurationTube;

            float curve = curveAnimTube.Evaluate(normalized);
            rectTube.anchoredPosition = Vector2.Lerp(startposTube, targetPosTube, curve);

            yield return null;
        }
        animator.SetTrigger("EndTalk");
        yield return new WaitForSeconds(0.8f);
        animator.SetTrigger("Fired");
        audioEventDispatcher?.PlayAudio(AudioType.Fired);
        yield return new WaitForSeconds(1f);
        yield return null;
        yield return null;
        yield return null;

        
        while (animator.GetCurrentAnimatorStateInfo(0).IsName("Virer") &&
               animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }
        employeImage.enabled = false;
        t = 0;
        yield return new WaitForSeconds(1f);
        while (t < animDurationTube)
        {
            t += Time.deltaTime;
            float normalized = t / animDurationTube;

            float curve = curveAnimTube.Evaluate(normalized);
            rectTube.anchoredPosition = Vector2.Lerp(targetPosTube, startposTube, curve);

            yield return null;
        }
        MusicManager.Instance?.StopFire();
        yield return new WaitForSeconds(1.5f); // laisser le fade se terminer
        Roulette.SetActive(true);
        /*t = 0;
        RectTransform rect = ScoreScene.GetComponent<RectTransform>();

        Vector2 startpos = new Vector2(rect.anchoredPosition.x, ybasePose);
        Vector2 targetPos = new Vector2(rect.anchoredPosition.x, yendPose);
        
        while (t < animDuration)
        {
            t += Time.deltaTime;
            float normalized = t / animDuration;

            float curve = curveAnim.Evaluate(normalized);
            rect.anchoredPosition = Vector2.Lerp(targetPos, startpos, curve);

            yield return null;
        }
        animator.Rebind();
        animator.Update(0f);*/
    }

    public IEnumerator QuitFired()
    {
        typeWriter.dialogueText.text = null;
        yield return new WaitForSeconds(0.5f); 
        Roulette.SetActive(false);
        float t = 0;
        RectTransform rect = ScoreScene.GetComponent<RectTransform>();

        Vector2 startpos = new Vector2(rect.anchoredPosition.x, ybasePose);
        Vector2 targetPos = new Vector2(rect.anchoredPosition.x, yendPose);

        while (t < animDuration)
        {
            t += Time.deltaTime;
            float normalized = t / animDuration;

            float curve = curveAnim.Evaluate(normalized);
            rect.anchoredPosition = Vector2.Lerp(targetPos, startpos, curve);

            yield return null;
        }
            EmployeFicheMove.IsLocked = false; 
        FiredScene.SetActive(false);
        MusicManager.Instance?.StopFire();
        if (_ingameWasPlayingBeforeFire)
        {
            _ingameWasPlayingBeforeFire = false;
            MusicManager.Instance?.PlayIngame();
        }

    }


    public void ShowChoiceButtons()
    {
        if (!choiceButtons.activeSelf)
        {
            choiceButtons.SetActive(true);
            StartCoroutine(ButtonsAnimShow());
        }
            
    }

    public IEnumerator ButtonsAnimShow()
    {
        RectTransform rectBouttons = choiceButtons.GetComponent<RectTransform>();

        Vector2 startposBouttons = new Vector2(rectBouttons.anchoredPosition.x, -1372.7f);
        Vector2 targetPosBouttons = new Vector2(rectBouttons.anchoredPosition.x, -711.3f);
        float t = 0;

        while (t < animDurationDialogue)
        {
            t += Time.deltaTime;
            float normalized = t / animDurationDialogue;

            float curve = curveAnimDialogueBack.Evaluate(normalized);
            rectBouttons.anchoredPosition = Vector2.Lerp(startposBouttons, targetPosBouttons, curve);

            yield return null;
        }
    }

    public IEnumerator HideButtons()
    {
        RectTransform rectBouttons = choiceButtons.GetComponent<RectTransform>();

        Vector2 startposBouttons = new Vector2(rectBouttons.anchoredPosition.x, -1372.7f);
        Vector2 targetPosBouttons = new Vector2(rectBouttons.anchoredPosition.x, -850.3f);
        float t = 0;

        while (t < animDurationDialogue)
        {
            t += Time.deltaTime;
            float normalized = t / animDurationDialogue;

            float curve = curveAnimDialogueBack.Evaluate(normalized);
            rectBouttons.anchoredPosition = Vector2.Lerp(targetPosBouttons, startposBouttons, curve);

            yield return null;
        }
        choiceButtons.SetActive(false);
    }
    public void HideChoiceButtons()
    {
        if (choiceButtons.activeSelf)
        {
            StartCoroutine(HideButtons());
        }
        

    }
}
