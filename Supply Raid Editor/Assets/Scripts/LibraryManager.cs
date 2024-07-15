using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Supply_Raid_Editor;

public class LibraryManager : MonoBehaviour
{
    public static LibraryManager instance;
    public static GenericButton selectedItem;
    private InputField selectedInputfield;

    public GameObject libraryMenu;

    public List<Sprite> sosigs = new List<Sprite>();
    private List<Sprite> sosigCollection = new List<Sprite>();
    public InputField searchInput;

    public Transform itemContent;
    private List<GenericButton> itemButtons = new List<GenericButton>();
    public GameObject buttonPrefab;

    private bool sortOrder = false;

    private void Awake()
    {
        instance = this;

        //Base Game Sosigs
        sosigCollection.Clear();
        sosigCollection.AddRange(sosigs);

        //Custom Sosigs
        DataManager.instance.LoadCustomSosigs();
        sosigCollection.AddRange(DataManager.instance.customSosigs);
    }

    public void OpenSosigLibrary(InputField field)
    {
        selectedInputfield = field;
        libraryMenu.SetActive(true);

        SetupLibrary(sosigCollection, true);
    }

    public void CloseLibrary(bool empty)
    {
        if (empty)
            selectedItem = null;
        selectedInputfield = null;

        libraryMenu.SetActive(false);
    }

    public void CloseLibrary(int id, bool empty)
    {
        if (empty)
            selectedItem = null;

        if(selectedInputfield)
            selectedInputfield.text = id.ToString();
        selectedInputfield = null;

        libraryMenu.SetActive(false);
    }

    public void SetupLibrary(List<Sprite> sprites, bool idPrefix)
    {
        for (int i = itemButtons.Count - 1; i >= 0; i--)
        {
            Destroy(itemButtons[i].gameObject);
        }
        itemButtons.Clear();

        for (int i = 0; i < sprites.Count; i++)
        {
            GenericButton btn = Instantiate(buttonPrefab.gameObject, itemContent).GetComponent<GenericButton>();
            btn.description = sprites[i].name;
            btn.text.text = sprites[i].name;

            btn.text.text = btn.text.text.Replace("_", " ");
            btn.image.sprite = sprites[i];
            btn.gameObject.SetActive(true);

            if(idPrefix)
                btn.id = int.Parse(GetInitialNumber(sprites[i].name));

            itemButtons.Add(btn);
        }
    }

    public void SearchName()
    {
        if (searchInput.text == "")
        {
            for (int i = 0; i < itemButtons.Count; i++)
            {
                itemButtons[i].gameObject.SetActive(true);
            }
            return;
        }

        for (int i = 0; i < itemButtons.Count; i++)
        {
            if (itemButtons[i].description.Contains(searchInput.text, System.StringComparison.OrdinalIgnoreCase))
                itemButtons[i].gameObject.SetActive(true);
            else
                itemButtons[i].gameObject.SetActive(false);
        }

    }

    public void SortByName()
    {
        for (int i = 0; i < itemButtons.Count; i++)
        {
            if(sortOrder)
                itemButtons[i].transform.SetAsFirstSibling();
            else
                itemButtons[i].transform.SetAsLastSibling();
        }

        sortOrder = !sortOrder;
    }

    static string GetInitialNumber(string input)
    {
        // Regular expression to match initial numbers at the start of the string
        string pattern = @"^\d+";

        // Find match
        System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(input, pattern);

        // Return the matched value if found, otherwise null
        return match.Success ? match.Value : null;
    }
}
