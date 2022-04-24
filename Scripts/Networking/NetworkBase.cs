using Godot;
using System;
using LiteNetLib;
using System.Collections.Generic;

public abstract class NetworkBase : Node, INetEventListener {
	public NetManager NetManager { get; set; }

	protected abstract void HandlePacket(NetPeer peer, byte id, NetPacketReader reader);

	public NetworkBase() {
		NetManager = new NetManager(this) {
			AutoRecycle = true,
			IPv6Enabled = IPv6Mode.Disabled,
		};
	}

	public virtual void Destroy() {
		foreach(NetworkPlayer net_player in NetworkManager.NetworkPlayers.Values) {
			net_player.QueueFree();
		}
		NetworkManager.NetworkPlayers.Clear();

		NetManager.Stop();
		QueueFree();
	}

	public virtual void Tick() {
		NetManager.PollEvents();
	}

	public virtual void OnConnectionRequest(ConnectionRequest req) {
	}

	public virtual void OnNetworkLatencyUpdate(NetPeer peer, int latency) {
	}

	public virtual void OnNetworkReceiveUnconnected(System.Net.IPEndPoint endpoint, NetPacketReader reader, UnconnectedMessageType type) {
	}

	public virtual void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod method) {
		byte id = reader.GetByte();
		HandlePacket(peer, id, reader);
	}

	public virtual void OnNetworkError(System.Net.IPEndPoint endpoint, System.Net.Sockets.SocketError error) {
		Logger.Error($"Network error {endpoint}: {Enum.GetName(typeof(System.Net.Sockets.SocketError), error)}");
	}

	public virtual void OnPeerConnected(NetPeer peer) {
		Logger.Info($"{peer.EndPoint} has connected");
	}

	public virtual void OnPeerDisconnected(NetPeer peer, DisconnectInfo info) {
		Logger.Info($"{peer.EndPoint} has disconnected, reason: {Enum.GetName(typeof(DisconnectReason), info.Reason)}");
	}
}
