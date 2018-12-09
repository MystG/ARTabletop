using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
	public int x;
	//public int y;
	public int z;
	public HexCoordinates(int x, int z)
	{
		this.x = x;
		this.z = z;
		//y = -x - z;
	}

	public static HexCoordinates FromOffsetCoordinates(int x, int z)
	{
		return new HexCoordinates(x, z);
	}
}

public static class HexMetrics
{
	public const float inchesToMeters = 1.0f / 39.370f;
	public const float outerRadius = 1.0f * inchesToMeters;
	public const float innerRadius = outerRadius * 0.866025404f;

	public static Vector3[] corners =
	{
		new Vector3(outerRadius, 0f, 0f),
		new Vector3(0.5f * outerRadius, 0f, innerRadius),
		new Vector3(-0.5f * outerRadius, 0f, innerRadius),
		new Vector3(-outerRadius, 0f, 0f),
		new Vector3(-0.5f * outerRadius, 0f, -innerRadius),
		new Vector3(0.5f * outerRadius, 0f, -innerRadius)
	};
}
