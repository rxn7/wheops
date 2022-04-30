public enum PacketFromServer {
	Handshake,
	PlayerJoined,
	PlayerDisconnected,
	PlayerTransfrom,
	MapChange,
	ChatMessage,
}

public enum PacketFromClient {
	Handshake,
	ChatMessage,
	PlayerTransform,
}
