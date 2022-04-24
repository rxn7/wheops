using System.Collections.Generic;
using System;
using LiteNetLib;

public class Client : NetworkBase {
	private delegate void ClientPacketHandlerCallback(NetPacketReader reader);
	private Dictionary<byte, ClientPacketHandlerCallback> m_PacketHandlerCallbacks;

	public ClientPacketSender Sender { get; private set; }
	public ClientPacketHandler Handler { get; private set; }
	public NetPeer ServerPeer { get; private set; } = null;

	public Client() : base() { 
		Sender = new ClientPacketSender(this);
		Handler = new ClientPacketHandler();

		m_PacketHandlerCallbacks = new Dictionary<byte, ClientPacketHandlerCallback>() {
			{ (byte)PacketFromServer.Handshake, Handler.HandshakeHandler },
			{ (byte)PacketFromServer.PlayerTransfrom, Handler.PlayerTransfromHandler },
			{ (byte)PacketFromServer.MapChange, Handler.MapChangeHandler },
			{ (byte)PacketFromServer.ChatMessage, Handler.ChatMessageHandler },
			{ (byte)PacketFromServer.PlayerJoined, Handler.PlayerJoinedHandler },
			{ (byte)PacketFromServer.PlayerDisconnected, Handler.PlayerDisconnectedHandler },
		};
	}

	protected override void HandlePacket(NetPeer peer, byte id, NetPacketReader reader) {
		if(m_PacketHandlerCallbacks.ContainsKey(id)) {
			m_PacketHandlerCallbacks[id](reader);
		}
	}

	public bool Start(string ip, short port) {
		Logger.Info("Connecting to the server");

		try {
			NetManager.Start();
			NetManager.Connect(ip, port, Global.NET_KEY);
		} catch(Exception ex) {
			Logger.Error(ex);
			return false;
		}

		return true;
	}

	public override void OnPeerConnected(NetPeer peer) {
		Logger.Info("Successfully connected to the server");

		ServerPeer = peer;
		Sender.Handshake();
	}

	public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo info) {
		Logger.Info($"Disconnected from the server: {Enum.GetName(typeof(DisconnectReason), info.Reason)}");
		NetworkManager.Disconnect();
	}

	public override void OnNetworkLatencyUpdate(NetPeer peer, int latency) {
		NetworkManager.Ping = latency;
	}
}
