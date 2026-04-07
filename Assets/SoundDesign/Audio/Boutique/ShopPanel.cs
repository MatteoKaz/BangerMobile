using UnityEngine;

public class ShopPanel : MonoBehaviour
{
    [SerializeField] private BoutiqueManager boutiqueManager;

    private void OnEnable()
    {
        boutiqueManager?.OpenShopMusic();
    }
}