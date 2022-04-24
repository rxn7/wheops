using Godot;
using LiteNetLib;
using LiteNetLib.Utils;

public static class LiteNetLibExtensions {
	public static void Put(this NetDataWriter writer, Vector3 val) {
		writer.Put(val.x);
		writer.Put(val.y);
		writer.Put(val.z);
	}

	public static Vector3 GetVector3(this NetPacketReader reader) {
		return new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
	}

	public static void Put(this NetDataWriter writer, Vector2 val) {
		writer.Put(val.x);
		writer.Put(val.y);
	}

	public static Vector2 GetVector2(this NetPacketReader reader) {
		return new Vector2(reader.GetFloat(), reader.GetFloat());
	}
}
