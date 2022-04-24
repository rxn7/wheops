using Godot;
using System;
using LiteNetLib;

public class ClientPacketSender : PacketSender {
	private Client m_Client;

	public ClientPacketSender(Client client) : base() {
		m_Client = client;
	}

	private void SendToServer(DeliveryMethod method) {
		m_Client.ServerPeer.Send(m_Writer.Data, method);
	}

	public void Handshake() {
		InitializePacket((byte)PacketFromClient.Handshake);

		if(Global.Nickname == "NoName") {
			Logger.Info("You should set your nickname using 'nick' command! Using defualt nickname for now.");
		}

		m_Writer.Put(Global.Nickname);
		SendToServer(DeliveryMethod.ReliableOrdered);
	}

	public void ChatMessage(string msg) {
		InitializePacket((byte)PacketFromClient.ChatMessage);
		m_Writer.Put(msg);

		SendToServer(DeliveryMethod.ReliableOrdered);
	}
}
