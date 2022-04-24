using LiteNetLib.Utils;

public abstract class PacketSender {
	protected NetDataWriter m_Writer;

	public PacketSender() {
		m_Writer = new NetDataWriter();
	}

	protected void InitializePacket(byte id) {
		m_Writer.Reset();
		m_Writer.Put(id);
	}
}
