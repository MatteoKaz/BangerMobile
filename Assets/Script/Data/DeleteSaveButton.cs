using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Supprime le fichier de save au clic.
/// Cache automatiquement le bouton si aucune save n'existe.
/// </summary>
[RequireComponent(typeof(Button))]
public class DeleteSaveButton : MonoBehaviour
{
    private const string SaveFileName = "save.json";

    private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(DeleteSave);
    }

    private void Start()
    {
        RefreshVisibility();
    }

    /// <summary>Supprime la save et cache le bouton.</summary>
    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("[DeleteSaveButton] Save supprimée.");
        }

        RefreshVisibility();
    }

    /// <summary>Affiche le bouton seulement si une save existe.</summary>
    private void RefreshVisibility()
    {
        gameObject.SetActive(File.Exists(SavePath));
    }
}