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

	/*
	public void Input(Vector3 movement, bool jump, bool crouch, bool run, bool shoot) {
		InitializePacket((byte)PacketFromClient.Input);
		m_Writer.Put(movement);
		m_Writer.Put(jump);
		m_Writer.Put(crouch);
		m_Writer.Put(run);
		m_Writer.Put(shoot);
		SendToServer(DeliveryMethod.ReliableUnordered);
	}
	*/

	public void PlayerTransform() {
		InitializePacket((byte)PacketFromServer.PlayerTransfrom);

		m_Writer.Put(Global.Player.Position);
		m_Writer.Put(new Vector2(Global.Player.Camera.RotationDegrees.x, Global.Player.Camera.RotationDegrees.y));

		SendToServer(DeliveryMethod.Unreliable);
	}
}
