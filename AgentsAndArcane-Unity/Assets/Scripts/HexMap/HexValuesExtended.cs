using System.IO;
using UnityEngine;

/// <summary>
/// Values that describe the contents of a cell.
/// </summary>
[System.Serializable]
public struct HexValuesExtended
{
    /// <summary>
    /// 3 values stored in 32 bits.
    /// ttttRRRR MMMM
    /// </summary>
    /// <remarks>Not readonly to support hot reloading in Unity.</remarks>
#pragma warning disable IDE0044 // Add readonly modifier
    int values;
#pragma warning restore IDE0044 // Add readonly modifier

    readonly int Get(int mask, int shift) =>
        (int)((uint)values >> shift) & mask;

    readonly HexValuesExtended With(int value, int mask, int shift) => new()
    {
        values = (values & ~(mask << shift)) | ((value & mask) << shift)
    };

	public readonly int ResourceTypeIndex => Get(31, 0);

	public readonly HexValuesExtended WithResourceTypeIndex(int index) =>
		With(index, 31, 0);

	public readonly int ResourceLevel => Get(31, 5);

	public readonly HexValuesExtended WithResourceLevel(int index) =>
        With(index, 31, 5);

	public readonly int ManaLevel => Get(31, 10);

	public readonly HexValuesExtended WithManaLevel(int index) =>
        With(index, 31, 10);

	/// <summary>
	/// Save the values.
	/// </summary>
	/// <param name="writer"><see cref="BinaryWriter"/> to use.</param>
	public readonly void Save(BinaryWriter writer)
	{
		writer.Write((byte)ResourceTypeIndex);
		writer.Write((byte)ResourceLevel);
		writer.Write((byte)ManaLevel);
	}

	/// <summary>
	/// Load the values.
	/// </summary>
	/// <param name="reader"><see cref="BinaryReader"/> to use.</param>
	/// <param name="header">Header version.</param>
	public static HexValuesExtended Load(BinaryReader reader, int header)
	{
		HexValuesExtended values = default;
		values = values.WithResourceTypeIndex(reader.ReadByte());
		values = values.WithResourceLevel(reader.ReadByte());
		return values.WithManaLevel(reader.ReadByte());
	}
}
