using System.Collections.Generic;
using System;
using LiteNetLib;
using LiteNetLib.Utils;

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

	private NetDataWriter GetConnectionDataWriter() {
		NetDataWriter connection_data_writer = new NetDataWriter();
		connection_data_writer.Put(Global.VERSION);
		return connection_data_writer;
	}

	public bool Start(string ip, short port) {
		Logger.Info("Connecting to the server");

		try {
			NetManager.Start();
			NetManager.Connect(ip, port, GetConnectionDataWriter());
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
		string reason = "Interal server error occured";
		info.AdditionalData.TryGetString(out reason);

		Logger.Info($"Disconnected from the server: {Enum.GetName(typeof(DisconnectReason), info.Reason)}, {reason}");
		NetworkManager.Disconnect();
	}

	public override void OnNetworkLatencyUpdate(NetPeer peer, int latency) {
		NetworkManager.Ping = latency;
	}
}
