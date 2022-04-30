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

		if(NetworkManager.NetworkPlayers.Values.Where(p => p.NetworkData.Nickname == nickname).Count() > 0 || nickname == Global.Nickname) {
			Logger.Info($"Player with nickname: {nickname} couldn't be connected: player with this nickname already exists");
			m_Server.DisconnectPeer(peer, "Player with this nickname is already present on this server");
			return;
		}

		m_Server.Peers.Add(peer.Id, peer);
		m_Server.Sender.Handshake(peer);

		RemotePlayerData data = new RemotePlayerData { ID = peer.Id, Nickname = nickname };
		m_Server.Sender.PlayerJoined(data);
		Global.SpawnNetworkPlayer(data, Vector3.Zero, Vector2.Zero);
	}

	public void ChatMessgeHandler(NetPeer peer, NetPacketReader reader) {
		string msg = reader.GetString();
		Logger.Info($"[b]{NetworkManager.NetworkPlayers[peer.Id].NetworkData.Nickname}[/b] says: {msg}");

		m_Server.Sender.ChatMessage(peer.Id, msg);
	}

	public void PlayerTransformHandler(NetPeer peer, NetPacketReader reader) {
		int id = peer.Id;
		Vector3 position = reader.GetVector3();
		Vector2 rotation = reader.GetVector2();

		if(!NetworkManager.NetworkPlayers.ContainsKey(id)) {
			Logger.Error($"Got a PlayerTransformPacket from a peer with ID: {id}, which is not registered on the server!");
		}

		RemotePlayer player = NetworkManager.NetworkPlayers[id];
		player.TargetPosition = position;
		player.TargetRotation = rotation;

		m_Server.Sender.PlayerTransform(id);
	}
}
