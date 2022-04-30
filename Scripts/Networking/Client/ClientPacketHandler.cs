using LiteNetLib;
using Godot;

public class ClientPacketHandler {
	public void HandshakeHandler(NetPacketReader reader) {
		string map = reader.GetString();
		int player_count = reader.GetInt();

		for(int i=0; i<player_count; ++i) {
			int id = reader.GetInt();	
			string nickname = reader.GetString();
			Vector3 position = reader.GetVector3();
			Vector2 rotation = reader.GetVector2();

			RemotePlayerData data = new RemotePlayerData { 
				ID = id, 
				Nickname = nickname,
			};

			Global.SpawnNetworkPlayer(data, position, rotation);
		}

		Global.LoadMap(map);
	}

	public void PlayerTransfromHandler(NetPacketReader reader) {
		int id = reader.GetInt();
		Vector3 position = reader.GetVector3();
		Vector2 rotation = reader.GetVector2();

		Logger.Info(rotation);

		if(NetworkManager.NetworkPlayers.ContainsKey(id)) {
			RemotePlayer player = NetworkManager.NetworkPlayers[id];
			player.TargetRotation = rotation;
			player.TargetPosition = position;
		} else {
			Logger.Error("Got a PlayerTransform packet of unexisting NetworkPlayer id!");
		}
	}

	public void MapChangeHandler(NetPacketReader reader) {
		string name = reader.GetString();
		if(!Global.LoadMap(name)) {
			Logger.Error("Failed to load the map, disconnecting.");
			NetworkManager.Disconnect();
		}
	}

	public void ChatMessageHandler(NetPacketReader reader) {
		int id = reader.GetInt();
		if(!NetworkManager.NetworkPlayers.ContainsKey(id)) {
			Logger.Error("Got a ChatMessage packet of unexisting NetworkPlayer id!");
			NetworkManager.Disconnect();
		}
		string msg = reader.GetString();

		Logger.Info($"[b]{NetworkManager.NetworkPlayers[id].NetworkData.Nickname}[/b] says: {msg}");
	}
	
	public void PlayerJoinedHandler(NetPacketReader reader) {
		int id = reader.GetInt();
		if(NetworkManager.NetworkPlayers.ContainsKey(id)) {
			Logger.Error($"Player with this id: {id} already exists!");
			return;
		}
		string nickname = reader.GetString();
		Vector3 position = reader.GetVector3();
		Vector2 rotation = reader.GetVector2();

		Global.SpawnNetworkPlayer(new RemotePlayerData { ID = id, Nickname = nickname }, position, rotation);
	}

	public void PlayerDisconnectedHandler(NetPacketReader reader) {
		int id = reader.GetInt();

		if(NetworkManager.NetworkPlayers.ContainsKey(id)) {
			RemotePlayer player = NetworkManager.NetworkPlayers[id];
			Logger.Info($"{player.NetworkData.Nickname} has disconnected");
			NetworkManager.NetworkPlayers.Remove(id);
			player.QueueFree();
		}
	}
}
