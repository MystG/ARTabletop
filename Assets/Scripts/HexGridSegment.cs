using UnityEngine;

public class HexGridSegment : MonoBehaviour
{
	public int width = 3;
	public int height = 3;
	public HexCell cellPrefab;

	private HexCell[] cells;
	private int cellIndex = 0;

	private void Awake()
	{
		cells = new HexCell[width * height];
	}

	private void CreateCell(int x, int z, int i)
	{
		Vector3 position;
		position.x = x * (HexMetrics.outerRadius * 1.5f);
		position.y = 0f;
		position.z = (z - 0.5f * x + x / 2) * (HexMetrics.innerRadius * 2f);
		HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
		cell.transform.SetParent(transform, true);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
	}
}
