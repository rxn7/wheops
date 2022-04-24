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

	public void Handshake(NetPeer peer) {
		InitializePacket((byte)PacketFromServer.Handshake);

		m_Writer.Put(Global.CurrentMap.Name);

		m_Writer.Put(NetworkManager.NetworkPlayers.Count + 1);
		foreach(KeyValuePair<int, NetworkPlayer> pair in NetworkManager.NetworkPlayers) {
			if(pair.Key != peer.Id) {
				m_Writer.Put(pair.Key);
				m_Writer.Put(pair.Value.NetworkData.Nickname);
				m_Writer.Put(pair.Value.Position);
				m_Writer.Put(pair.Value.Rotation);
			}
		}

		m_Writer.Put(-1);
		m_Writer.Put(Global.Nickname);
		m_Writer.Put(Global.Player.GlobalTransform.origin);
		m_Writer.Put(new Vector2(Global.Player.Camera.RotationDegrees.x, Global.Player.Camera.RotationDegrees.y));

		SendToPeer(peer, DeliveryMethod.ReliableOrdered);
	}
	
	public void PlayerTransform(int id) {
		InitializePacket((byte)PacketFromServer.PlayerTransfrom);

		m_Writer.Put(id);
		if(id == -1) {
			m_Writer.Put(Global.Player.GlobalTransform.origin);
			m_Writer.Put(new Vector2(Global.Player.Camera.RotationDegrees.x, Global.Player.Camera.RotationDegrees.y));
		} else {
			m_Writer.Put(id);
			NetworkPlayer net_player = NetworkManager.NetworkPlayers[id];

			m_Writer.Put(net_player.GlobalTransform.origin);
			m_Writer.Put(net_player.Rotation);
		}

		SendToEveryone(DeliveryMethod.Unreliable);
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
}
