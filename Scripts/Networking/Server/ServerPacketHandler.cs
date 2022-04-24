using LiteNetLib;
using Godot;
using System.Linq;

public class ServerPacketHandler {
	public Server m_Server;

	public ServerPacketHandler(Server serv) {
		m_Server = serv;
	}

	public void HandshakeHandler(NetPeer peer, NetPacketReader reader) {
		string nickname = reader.GetString();
		Logger.Info($"Handshake received from {nickname} | {peer.EndPoint}");

		if(NetworkManager.NetworkPlayers.Values.Where(p => p.NetworkData.Nickname == nickname).Count() > 0) {
			Logger.Info($"Player with nickname: {nickname} couldn't be connected: player with this nickname already exists");
			peer.Disconnect();
			return;
		}

		m_Server.Peers.Add(peer.Id, peer);
		m_Server.Sender.Handshake(peer);

		NetworkPlayerData data = new NetworkPlayerData { ID = peer.Id, Nickname = nickname };
		m_Server.Sender.PlayerJoined(data);
		Global.SpawnNetworkPlayer(data, Vector3.Zero, Vector2.Zero);
	}

	public void ChatMessgeHandler(NetPeer peer, NetPacketReader reader) {
		string msg = reader.GetString();
		Logger.Info($"[b]{NetworkManager.NetworkPlayers[peer.Id].NetworkData.Nickname}[/b] says: {msg}");

		m_Server.Sender.ChatMessage(peer.Id, msg);
	}
}
