using System.Collections.Generic;
using LiteNetLib;
using Godot;

public class ServerPacketSender : PacketSender {
	private Server m_Server;

	public ServerPacketSender(Server serv) : base() { 
		m_Server = serv;
	}

	private void SendToEveryone(DeliveryMethod method) {
		foreach(KeyValuePair<int, NetPeer> pair in m_Server.Peers) {
			pair.Value.Send(m_Writer.Data, DeliveryMethod.Unreliable);
		}
	}

	private void SendToEveryoneExcept(int except, DeliveryMethod method) {
		foreach(KeyValuePair<int, NetPeer> pair in m_Server.Peers) {
			if(pair.Key != except) {
				pair.Value.Send(m_Writer.Data, DeliveryMethod.Unreliable);
			}
		}
	}

	private void SendToPeer(NetPeer peer, DeliveryMethod method) {
		peer.Send(m_Writer.Data, method);
	}

	private void WritePlayerData(int id, string nickname, Vector3 position, Vector2 rotation) {
		m_Writer.Put(id);
		m_Writer.Put(nickname);
		m_Writer.Put(position);
		m_Writer.Put(rotation);
	}

	public void Handshake(NetPeer peer) {
		InitializePacket((byte)PacketFromServer.Handshake);

		m_Writer.Put(Global.CurrentMap.Name);

		m_Writer.Put(NetworkManager.NetworkPlayers.Count + 1);
		foreach(KeyValuePair<int, RemotePlayer> pair in NetworkManager.NetworkPlayers) {
			if(pair.Key != peer.Id) {
				WritePlayerData(pair.Key, pair.Value.NetworkData.Nickname, pair.Value.Position, pair.Value.TargetRotation);
			}
		}

		WritePlayerData(-1, Global.Nickname, Global.Player.GlobalTransform.origin, new Vector2(Global.Player.Camera.RotationDegrees.x, Global.Player.Camera.RotationDegrees.y));

		SendToPeer(peer, DeliveryMethod.ReliableOrdered);
	}
	
	public void PlayerTransform(int id) {
		InitializePacket((byte)PacketFromServer.PlayerTransfrom);

		m_Writer.Put(id);
		if(id == -1) {
			m_Writer.Put(Global.Player.GlobalTransform.origin);
			m_Writer.Put(new Vector2(Global.Player.CameraHolder.RotationDegrees.x, Global.Player.CameraHolder.RotationDegrees.y));
		} else {
			m_Writer.Put(id);
			RemotePlayer player = NetworkManager.NetworkPlayers[id];

			m_Writer.Put(player.Position);
			m_Writer.Put(player.TargetRotation);
		}

		SendToEveryoneExcept(id, DeliveryMethod.Unreliable);
	}

	public void MapChange(string name) {
		InitializePacket((byte)PacketFromServer.MapChange);
		m_Writer.Put(name);
		SendToEveryone(DeliveryMethod.ReliableOrdered);
	}

	public void ChatMessage(int id, string msg) {
		InitializePacket((byte)PacketFromServer.ChatMessage);
		m_Writer.Put(id);
		m_Writer.Put(msg);
		SendToEveryoneExcept(id, DeliveryMethod.ReliableOrdered);
	}

	public void PlayerJoined(RemotePlayerData data) {
		InitializePacket((byte)PacketFromServer.PlayerJoined);
		WritePlayerData(data.ID, data.Nickname, Vector3.Zero, Vector2.Zero);
		SendToEveryoneExcept(data.ID, DeliveryMethod.ReliableOrdered);
	}

	public void PlayerDisconnected(int id) {
		InitializePacket((byte)PacketFromServer.PlayerDisconnected);
		m_Writer.Put(id);
		SendToEveryoneExcept(id, DeliveryMethod.ReliableOrdered);
	}
}
