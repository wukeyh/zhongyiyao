using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using CodeMonkey.Utils;
using UnityEngine.Events;

public class GridBuildingSystem : MonoBehaviour
{
    public static GridBuildingSystem Instance;
    public event EventHandler OnSelectedChanged;
    

    [SerializeField]
    private List<PlacedObjectTypeSO> placedObjectTypeSOList;

    private PlacedObjectTypeSO placedObjectTypeSO;

    private PlacedObjectTypeSO.Dir dir = PlacedObjectTypeSO.Dir.Down;
    private Grid<GridObject> grid;

    private void Awake() 
    {
        Instance = this;
        int gridWidth = 10;
        int gridHeight = 10;
        float cellSize = 10f;
        grid = new Grid<GridObject>(gridWidth, gridHeight, cellSize, Vector3.zero, (Grid<GridObject> g, int x, int z) => new GridObject(g, x, z));
    
        placedObjectTypeSO = placedObjectTypeSOList[0];
    }

    public class GridObject
    {
        private Grid<GridObject> grid;
        private int x;
        private int z;
        private PlacedObject placedObject;

        public GridObject(Grid<GridObject> grid, int x, int z)
        {
            this.grid = grid;
            this.x = x;
            this.z = z;
        }

        public void SetPlacedObject(PlacedObject placedObject)
        {
            this.placedObject = placedObject;
            grid.TriggerGridObjectChanged(x, z);
        }

        public PlacedObject GetPlacedObject()
        {
            return placedObject;
        }

        public void ClearPlacedObject()
        {
            placedObject = null;
            grid.TriggerGridObjectChanged(x, z);
        }

        public bool CanBuild() //判断
        {
            return placedObject == null;    
        }

        public override string ToString()
        {
            return x + "," + z + "\n" + placedObject;
        }
    }

    public Quaternion GetPlacedObjectRotation()
    {
        if(placedObjectTypeSO != null)
        {
            return Quaternion.Euler(0, placedObjectTypeSO.GetRotationAngle(dir), 0);
        }
        else
        {
            return Quaternion.identity;
        }
    }

    public Vector3 GetMouseWorldSnappedPosition()
    {
        Vector3 mousePosition = MousePosition.GetMouseWorldPosition();
        grid.GetXZ(mousePosition, out int x, out int z);

        if(placedObjectTypeSO != null)
        {
            Vector2Int rotationOffset = placedObjectTypeSO.GetRotationOffset(dir);
            return grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();
        }
        else
        {
            return mousePosition;
        }
    }

    public PlacedObjectTypeSO GetPlacedObjectTypeSO()
    {
        return placedObjectTypeSO;
    }

    private void Update() 
    {
        if(Input.GetMouseButtonDown(0))
        {
            grid.GetXZ(MousePosition.GetMouseWorldPosition(), out int x, out int z);

            List<Vector2Int> gridPositionList = placedObjectTypeSO.GetGridPositionList(new Vector2Int(x, z), dir);

            //Test can build
            bool CanBuild = true;
            foreach(Vector2Int gridPosition in gridPositionList)
            {
                if(!grid.GetGridObject(gridPosition.x, gridPosition.y).CanBuild())
                {
                    CanBuild = false;
                    break;
                }
            }

            if(CanBuild)
            {
                Vector2Int rotationOffset = placedObjectTypeSO.GetRotationOffset(dir);
                Vector3 placedObjectWorldPosition = grid.GetWorldPosition(x, z) + new Vector3(rotationOffset.x, 0, rotationOffset.y) * grid.GetCellSize();

                PlacedObject placedObject = PlacedObject.Create(placedObjectWorldPosition, new Vector2Int(x, z), dir, placedObjectTypeSO);

                foreach(Vector2Int gridPosition in gridPositionList)
                {
                    grid.GetGridObject(gridPosition.x, gridPosition.y).SetPlacedObject(placedObject);
                }
            }
            else
            {
                UtilsClass.CreateWorldTextPopup("Cannot build here", MousePosition.GetMouseWorldPosition());
            }
        }

        if(Input.GetMouseButtonDown(1))
        {
            GridObject gridObject = grid.GetGridObject(MousePosition.GetMouseWorldPosition());
            PlacedObject placedObject = gridObject.GetPlacedObject();
            if(placedObject != null)
            {
                placedObject.DestroySelf();

                List<Vector2Int> gridPositionList = placedObject.GetGridPositionList();

                foreach(Vector2Int gridPosition in gridPositionList)
                {
                    grid.GetGridObject(gridPosition.x, gridPosition.y).ClearPlacedObject();
                }
            }
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            dir = PlacedObjectTypeSO.GetNextDir(dir);
            UtilsClass.CreateWorldTextPopup(dir.ToString(), MousePosition.GetMouseWorldPosition());
        }
    }

}
