using Godot;
using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;

public enum ENetworkState {
	None,
	Client,
	Server,
};

public class NetworkManager : Node {
	public static NetworkManager Instance { get; private set; }

	public const float TICK_RATE = 30;
	public const float MIN_TIME_BETWEEN_TICKS = 1f / TICK_RATE;
	public static ENetworkState State { get; private set; }
	public static Dictionary<int, RemotePlayer> NetworkPlayers { get; private set; }
	public static bool IsNetworked => State != ENetworkState.None && Network != null && NetworkBase.IsInstanceValid(Network);
	public static bool IsServer => IsNetworked && State == ENetworkState.Server;
	public static float TickTimer { get; private set; } = 0;
	public static uint CurrentTick { get; private set; } = 0;

	public static int Ping { get; set; }
	public static NetworkBase Network { get; private set; }
	public static Server Server { get => (Server)Network; }
	public static Client Client { get => (Client)Network; }
	public static Action OnTick;

	private static string _local_ip = null;
	private static string _public_ip = null;

	public override void _EnterTree() {
		Instance = this;
		State = ENetworkState.None;
		NetworkPlayers = new Dictionary<int, RemotePlayer>();
	}

	public override void _Process(float dt) {
		if(IsNetworked) {
			TickTimer += dt;
			while(TickTimer >= MIN_TIME_BETWEEN_TICKS) {
				CurrentTick++;
				Network.Tick();

				if(Network is Server || (Network is Client client && client.ServerPeer != null)) {
					OnTick();
				}

				TickTimer -= MIN_TIME_BETWEEN_TICKS;
			}
		}
	}

	public static bool StartServer(short port, int max_clients) {
		if(Network != null) Network.Destroy();

		Network = new Server();
		Instance.AddChild(Network);

		if(!((Server)Network).Start(port, max_clients)) {
			return false;
		}

		State = ENetworkState.Server;

		return true;
	}

	public static bool StartClient(string ip, short port) {
		if(Network != null) Network.Destroy();

		Network = new Client();
		Instance.AddChild(Network);

		if(!((Client)Network).Start(ip, port)) {
			return false;
		}

		State = ENetworkState.Client;

		return true;
	}

	public static void Disconnect() {
		if(Network != null) {
			Network.Destroy();
			Network = null;
		}

		State = ENetworkState.None;
	}

	public static string GetPublicIP(string service = "https://ipv4.icanhazip.com") {
		if(_public_ip == null) {
			_public_ip = new WebClient().DownloadString(service).Trim();
		}

		return _public_ip;
	}

	public static string GetLocalIP() {
		if(_local_ip == null) {
			_local_ip = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
		}

		return _local_ip;
	}
}
