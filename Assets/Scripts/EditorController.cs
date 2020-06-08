using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorController : MonoBehaviour
{
    public GameObject gridSprite;
    public List<GameObject> elements = new List<GameObject>();
    private Editor editor;


    private EditorUIController.Buttons chosenItem = EditorUIController.Buttons.FLOOR;
    // Start is called before the first frame update
    void Start()
    {
        editor = new Editor();
        editor.SetGridSprite(gridSprite.GetComponent<SpriteRenderer>().sprite);
        var counter = 1;
        foreach(var e in elements) {
            editor.AddElementToDictionary(e, (Editor.Elements)counter++);
        }
        editor.DrawGrid();
        editor.RefreshMap();
    }

    void Update() {
        // Android 
        /*
        if (Input.touchCount > 0) {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) {
                activePoint = Camera.main.ScreenToWorldPoint(new Vector3(touch.position.x, touch.position.y, 0));
                activeTile = new Vector2Int(Mathf.FloorToInt(activePoint.x - offsetStart.x), Mathf.FloorToInt(activePoint.y - offsetStart.y));
                if (editableTile(activeTile)) {
                    if (mapValueAt(activeTile.x, activeTile.y) == FieldType.FLOOR)
                        setMapValueAt(activeTile.x, activeTile.y, FieldType.EMPTY);
                    else {
                        setMapValueAt(activeTile.x, activeTile.y, FieldType.FLOOR);
                    }
                    refreshMap();
                }
            }
        }*/
        // PC

        if (Input.GetMouseButtonDown(0)) {
            Vector3 activePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int activeTile = editor.GetWorldTile(activePoint);
            Debug.Log(activeTile);
            editor.ModifyTileAt(activeTile, chosenItem);
            editor.RefreshMap();
        }

    }

    public void Button_ChooseItem(int order) {
        chosenItem = (EditorUIController.Buttons)order;
    }
}
