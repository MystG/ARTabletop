using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class HexGridSegment : MonoBehaviour
{
	public int id;
	public static int numCols = 7;
	public int width = 3;
	public int height = 3;
	public HexCell cellPrefab;

	private bool linked = false;
	private HexCell[] cells;
	private int cellIndex = 0;
	public int r, c;

	private void Awake()
	{
		cells = new HexCell[width * height];
	}

	private void Update()
	{
		id = r * numCols + c;
        if (!linked)
			LinkToTarget();
	}

	public void SetRelativePosition(int x, int z)
	{
		r = x;
		c = z;
	}

	public void AddCell(HexCell cell)
	{
		// Set the segment's position to be the same as its lower-left cell.
		if (cellIndex == 0)
			transform.position = cell.transform.position;
		cells[cellIndex] = cell;
		cellIndex++;
		cell.transform.SetParent(transform, true);
	}

	public void LinkToTarget()
	{
		IEnumerable<TrackableBehaviour> targets = TrackerManager.Instance.GetStateManager().GetTrackableBehaviours();
		foreach (TrackableBehaviour target in targets)
		{
			if (target.Trackable.ID == 56 - id)
			{
				target.transform.position = cells[4].transform.position;
                //target.transform.localScale = 1.5f * Vector3.one;
                //transform.localScale *= 2f;
                target.gameObject.name = gameObject.name + " Target";
				target.gameObject.AddComponent<DefaultTrackableEventHandler>();
				target.gameObject.AddComponent<TurnOffBehaviour>();
				transform.SetParent(target.transform, true);
				linked = true;
                break;
			}
		}
	}
}
