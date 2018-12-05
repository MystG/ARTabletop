using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexGrid : MonoBehaviour
{
	public int width = 21;
	public int height = 24;
	public int segmentWidth = 3;
	public int segmentHeight = 3;
	public HexCell cellPrefab;

	private HexCell[] cells;
	private HexGridSegment[] segments;

	private void Awake()
	{
		segments = new HexGridSegment[(width / segmentWidth) * (height / segmentHeight)];
		cells = new HexCell[width * height];
		for (int x = 0, i = 0; x < width; x++)
			for (int z = 0; z < height; z++)
				CreateCell(x, z, i++);
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
