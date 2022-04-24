using LiteNetLib;
using Godot;

public class ServerPacketHandler {
	public Server m_Server;

	public ServerPacketHandler(Server serv) {
		m_Server = serv;
	}

	public void HandshakeHandler(NetPeer peer, NetPacketReader reader) {
		string nickname = reader.GetString();
		Logger.Info($"Handshake received from {nickname} | {peer.EndPoint}");

		m_Server.Peers.Add(peer.Id, peer);
		m_Server.Sender.Handshake(peer);
		Global.SpawnNetworkPlayer(new NetworkPlayerData(peer.Id, nickname), Vector3.Zero, Vector2.Zero);
	}

	public void ChatMessgeHandler(NetPeer peer, NetPacketReader reader) {
		string msg = reader.GetString();
		Logger.Info($"[b]{NetworkManager.NetworkPlayers[peer.Id].NetworkData.Nickname}[/b] says: {msg}");

		m_Server.Sender.ChatMessage(peer.Id, msg);
	}
}
