using UnityEngine;

public class GardenGrid : MonoBehaviour
{
    public int columns = 20;
    public int rows = 8;
    public float cellSize = 1f;

    // This object's position is the bottom-left corner of the dirt area.

    public Vector2Int WorldToCell(Vector3 world)
    {
        Vector3 local = world - transform.position;
        return new Vector2Int(
            Mathf.FloorToInt(local.x / cellSize),
            Mathf.FloorToInt(local.y / cellSize));
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        return transform.position + new Vector3(
            (cell.x + 0.5f) * cellSize,
            (cell.y + 0.5f) * cellSize, 0);
    }

    public bool InBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.y >= 0 &&
               cell.x < columns && cell.y < rows;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        for (int x = 0; x <= columns; x++)
            Gizmos.DrawLine(
                transform.position + new Vector3(x * cellSize, 0),
                transform.position + new Vector3(x * cellSize, rows * cellSize));
        for (int y = 0; y <= rows; y++)
            Gizmos.DrawLine(
                transform.position + new Vector3(0, y * cellSize),
                transform.position + new Vector3(columns * cellSize, y * cellSize));
    }
}