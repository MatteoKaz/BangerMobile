using UnityEngine;

public class ScrollMenuManager : MonoBehaviour
{
    public Transform content;      // Le Content du ScrollView
    public GameObject itemPrefab;  // Le prefab de ton item

    public void AddItem()
    {
        Instantiate(itemPrefab, content);
    }
}