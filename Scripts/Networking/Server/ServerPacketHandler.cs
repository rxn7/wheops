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

	/*
	public void InputHandler(NetPeer peer, NetPacketReader reader) {
		int id = peer.Id;

		Vector3 movement = reader.GetVector3();				
		bool jump = reader.GetBool();
		bool crouch = reader.GetBool();
		bool run = reader.GetBool();
		bool shoot = reader.GetBool();

		if(NetworkManager.NetworkPlayers.Keys.Contains(id)) {
			NetworkPlayer player = NetworkManager.NetworkPlayers[id];

			player.Input.Movement = movement;
			player.Input.Jump = jump;
			player.Input.Crouch = crouch;
			player.Input.Run = run;
			player.Input.Shoot = shoot;
		} else {
			Logger.Error($"Player with id {id} doesn't exist");
		}
	}
	*/

	public void PlayerTransformHandler(NetPeer peer, NetPacketReader reader) {
		int id = peer.Id;
		Vector3 position = reader.GetVector3();
		Vector2 rotation = reader.GetVector2();

		Logger.Info("CUM");

		if(NetworkManager.NetworkPlayers.Keys.Contains(id)) {
			NetworkPlayer player = NetworkManager.NetworkPlayers[id];
			player.Position = position;
			player.Rotation = rotation;
			m_Server.Sender.PlayerTransform(id);
		} else {
			Logger.Error($"Player with id {id} doesn't exist");
		}
	}
}
