using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    private List<DroneUnit> selectedDrones = new List<DroneUnit>();
    private List<TruckUnit> selectedTrucks = new List<TruckUnit>();
    private BaseUnit selectedBase;

    private Vector2 dragStartPos;
    private bool isDragging;

    public List<DroneUnit> GetSelectedDrones() => selectedDrones;
    public List<TruckUnit> GetSelectedTrucks() => selectedTrucks;
    public BaseUnit GetSelectedBase() => selectedBase;
    public void RemoveDrone(int index) { if (index >= 0 && index < selectedDrones.Count) selectedDrones.RemoveAt(index); }
    public void RemoveTruck(int index) { if (index >= 0 && index < selectedTrucks.Count) selectedTrucks.RemoveAt(index); }

    void Update()
    {
        if (Mouse.current == null) return;
        if (Time.timeScale == 0f) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (selectedBase != null && selectedBase.isSelected) {
                float guiY = Screen.height - mousePos.y;
                Rect uiRect = new Rect(Screen.width - 260, 10, 250, 150);
                if (uiRect.Contains(new Vector2(mousePos.x, guiY))) {
                    return; 
                }
            }
            
            if (IsHoveringSelectionUI(mousePos)) {
                return;
            }

            dragStartPos = mousePos;
            isDragging = true;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;
            float dragDistance = Vector2.Distance(dragStartPos, mousePos);
            if (dragDistance < 10f)
            {
                SelectUnitClick(mousePos);
            }
            else
            {
                SelectUnitsDrag(dragStartPos, mousePos);
            }
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            IssueCommand();
        }

        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            DroneUnit[] allDrones = FindObjectsByType<DroneUnit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var d in allDrones) {
                if (d.IsIdle) {
                    ClearSelection();
                    selectedDrones.Add(d);
                    break;
                }
            }
        }
    }

    private bool IsHoveringSelectionUI(Vector2 mousePos) {
        if (selectedDrones.Count == 0 && selectedTrucks.Count == 0) return false;
        float guiY = Screen.height - mousePos.y;
        Rect uiRect = new Rect(10, Screen.height / 2 - 150, 200, 300);
        return uiRect.Contains(new Vector2(mousePos.x, guiY));
    }

    void SelectUnitClick(Vector2 mousePos)
    {
        ClearSelection();

        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            DroneUnit drone = hit.collider.GetComponentInParent<DroneUnit>();
            if (drone != null)
            {
                selectedDrones.Add(drone);
                return;
            }

            TruckUnit truck = hit.collider.GetComponentInParent<TruckUnit>();
            if (truck != null)
            {
                selectedTrucks.Add(truck);
                return;
            }

            BaseUnit baseUnit = hit.collider.GetComponentInParent<BaseUnit>();
            if (baseUnit != null)
            {
                selectedBase = baseUnit;
                selectedBase.isSelected = true;
                return;
            }
        }
    }

    void SelectUnitsDrag(Vector2 startPos, Vector2 endPos)
    {
        ClearSelection();

        Rect screenRect = new Rect(
            Mathf.Min(startPos.x, endPos.x),
            Mathf.Min(startPos.y, endPos.y),
            Mathf.Abs(startPos.x - endPos.x),
            Mathf.Abs(startPos.y - endPos.y)
        );

        DroneUnit[] allDrones = FindObjectsByType<DroneUnit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var d in allDrones)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(d.transform.position);
            if (screenRect.Contains(new Vector2(screenPos.x, screenPos.y)))
            {
                selectedDrones.Add(d);
            }
        }

        TruckUnit[] allTrucks = FindObjectsByType<TruckUnit>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var t in allTrucks)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(t.transform.position);
            if (screenRect.Contains(new Vector2(screenPos.x, screenPos.y)))
            {
                selectedTrucks.Add(t);
            }
        }
    }

    void ClearSelection()
    {
        selectedDrones.Clear();
        selectedTrucks.Clear();
        if (selectedBase != null)
        {
            selectedBase.isSelected = false;
            selectedBase = null;
        }
    }

    void IssueCommand()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            BaseUnit targetBase = hit.collider.GetComponentInParent<BaseUnit>();
            ResourceNode node = hit.collider.GetComponentInParent<ResourceNode>();

            foreach (var d in selectedDrones) {
                Vector3 offset = (selectedDrones.Count > 1) ? new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f)) : Vector3.zero;
                d.MoveTo(hit.point + offset);
            }

            foreach (var t in selectedTrucks) {
                if (targetBase != null) {
                    t.CommandReturnToBase();
                }
                else if (node != null && node.gameObject.activeSelf)
                {
                    t.AssignResource(node);
                }
                else
                {
                    Vector3 offset = (selectedTrucks.Count > 1) ? new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f)) : Vector3.zero;
                    t.MoveTo(hit.point + offset);
                }
            }

            if (selectedBase != null && selectedBase.isSelected) {
                if (targetBase == null) {
                    selectedBase.MoveTo(hit.point);
                }
            }
        }
    }

    private void OnGUI()
    {
        if (isDragging)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Rect rect = new Rect(
                Mathf.Min(dragStartPos.x, mousePos.x),
                Screen.height - Mathf.Max(dragStartPos.y, mousePos.y),
                Mathf.Abs(dragStartPos.x - mousePos.x),
                Mathf.Abs(dragStartPos.y - mousePos.y)
            );
            
            GUI.color = new Color(0, 1, 0, 0.2f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Color.green;
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - 2, rect.width, 2), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 2, rect.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - 2, rect.yMin, 2, rect.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
