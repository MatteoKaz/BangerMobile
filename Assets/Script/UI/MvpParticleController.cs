using UnityEngine;

/// <summary>
/// Joue le particle system du MVP tant que le menu Ranking est affiché,
/// et l'arrête dès qu'il est masqué.
/// </summary>
public class MvpParticleController : MonoBehaviour
{
    [SerializeField] private ParticleSystem mvpParticle;

    private void OnEnable()
    {
        if (mvpParticle == null)
        {
            Debug.LogWarning("[MvpParticleController] mvpParticle non assigné.", this);
            return;
        }

        mvpParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        mvpParticle.Play();
    }

    private void OnDisable()
    {
        if (mvpParticle == null) return;

        mvpParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}