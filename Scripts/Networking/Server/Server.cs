using Godot;
using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;

public class Server : NetworkBase {
	public ServerPacketHandler Handler { get; private set; }
	public ServerPacketSender Sender { get; private set; }
	public int CurrentTick { get; private set; }

	public Dictionary<int, NetPeer> Peers { get; private set; }

	private delegate void ServerPacketHandlerCallback(NetPeer peer, NetPacketReader reader); private Dictionary<byte, ServerPacketHandlerCallback> m_PacketHandlerCallbacks;
	private int m_MaxPeers;

	public Server() : base() {
		Peers = new Dictionary<int, NetPeer>();
		NetManager = new NetManager(this);
		Handler = new ServerPacketHandler(this);
		Sender = new ServerPacketSender(this);

		m_PacketHandlerCallbacks = new Dictionary<byte, ServerPacketHandlerCallback>() {
			{ (byte)PacketFromClient.Handshake, Handler.HandshakeHandler },
			{ (byte)PacketFromClient.ChatMessage, Handler.ChatMessgeHandler },
			{ (byte)PacketFromClient.PlayerTransform, Handler.PlayerTransformHandler },
		};
	}

	protected override void HandlePacket(NetPeer peer, byte id, NetPacketReader reader) {
		if (m_PacketHandlerCallbacks.ContainsKey(id)) {
			m_PacketHandlerCallbacks[id](peer, reader);
		}
	}

	public override void Destroy() {
		NetManager.DisconnectAll();
		base.Destroy();
	}

	public void DisconnectPeer(NetPeer peer, string reason) {
		NetDataWriter disconnect_data = new NetDataWriter();
		disconnect_data.Put(reason);

		if (Peers.ContainsKey(peer.Id)) {
			Peers.Remove(peer.Id);
		}

		if (NetworkManager.NetworkPlayers.ContainsKey(peer.Id)) {
			RemotePlayer player = NetworkManager.NetworkPlayers[peer.Id];
			player.QueueFree();
			NetworkManager.NetworkPlayers.Remove(peer.Id);
		}

		peer.Disconnect(disconnect_data);
	}

	public bool Start(short port, int max_peers) {
		m_MaxPeers = max_peers;

		try {
			NetManager.Start(port);
		} catch (Exception ex) {
			Logger.Error($"Failed to start the server: {ex.Message}");
		}

		Logger.Info($"Server started on port {port}. If you want someone from your local network to connect, they should use this ip: {NetworkManager.GetLocalIP()}, if you want someone from outside your local network to connect, they should use this ip: {NetworkManager.GetPublicIP()}.");
		return true;
	}

	public override void OnConnectionRequest(ConnectionRequest req) {
		base.OnConnectionRequest(req);

		if (NetManager.ConnectedPeersCount < m_MaxPeers) {
			string version = req.Data.GetString();
			if (version != Global.VERSION) {
				RejectRequest(req, $"This server runs on version {Global.VERSION} and your version is {version}, update your game!");
				return;
			}
		} else {
			RejectRequest(req, $"This server is full! ({m_MaxPeers} / {m_MaxPeers})");
			return;
		}

		req.Accept();
	}

	private void RejectRequest(ConnectionRequest req, string response) {
		NetDataWriter response_data = new NetDataWriter();
		response_data.Put($"There are no empty slots on this server");
		req.Reject(response_data);
	}

	public override void OnPeerConnected(NetPeer peer) {
		base.OnPeerConnected(peer);
	}

	public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo info) {
		base.OnPeerDisconnected(peer, info);

		if (NetworkManager.NetworkPlayers.ContainsKey(peer.Id)) {
			NetworkManager.NetworkPlayers[peer.Id].QueueFree();
			NetworkManager.NetworkPlayers.Remove(peer.Id);
		}

		Peers.Remove(peer.Id);
		Sender.PlayerDisconnected(peer.Id);
	}
}
