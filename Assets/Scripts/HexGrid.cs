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
	private HexGridSegment[,] segments;

	private void Awake()
	{
		segments = new HexGridSegment[(width / segmentWidth), (height / segmentHeight)];
		for (int i = 0; i < width / segmentWidth; i++)
			for (int j = 0; j < height / segmentHeight; j++)
			{
				segments[i, j] = new GameObject("R" + ((height / segmentHeight - 1) - j) + "C" + i, 
					typeof(HexGridSegment)).GetComponent<HexGridSegment>();
				segments[i, j].SetRelativePosition((height/segmentHeight - 1) - j, i);
			}
		cells = new HexCell[width * height];
		for (int x = 0, i = 0; x < width; x++)
			for (int z = 0; z < height; z++)
				CreateCell(x, z, i++);
	}

	//private void Start()
	//{
	//	foreach (HexGridSegment s in segments)
	//		s.LinkToTarget();
	//}

	private void CreateCell(int x, int z, int i)
	{
		Vector3 position;
		position.x = x * (HexMetrics.outerRadius * 1.5f);
		position.y = 0f;
		position.z = (z - 0.5f * x + x / 2) * (HexMetrics.innerRadius * 2f);
		HexCell cell = cells[i] = Instantiate(cellPrefab);
		cell.transform.SetParent(transform, true);
		cell.transform.localPosition = position;
		cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
		cell.space.row = (height - z);
		cell.space.col = x;
		segments[x / segmentWidth, z / segmentHeight].AddCell(cell);
	}
}
