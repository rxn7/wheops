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

			NetworkPlayerData data = new NetworkPlayerData(id, nickname);
			Global.SpawnNetworkPlayer(data, position, rotation);
		}

		Global.LoadMap(map);
	}

	public void PlayerTransfromHandler(NetPacketReader reader) {
		int id = reader.GetInt();
		Vector3 position = reader.GetVector3();
		Vector2 rotation = reader.GetVector2();

		if(NetworkManager.NetworkPlayers.ContainsKey(id)) {
			NetworkPlayer net_player = NetworkManager.NetworkPlayers[id];
			net_player.Rotation = rotation;

			Transform t = net_player.GlobalTransform;
			t.origin = position;
			net_player.GlobalTransform = t;
		} else {
			Logger.Error("Got a PlayerTransform packet of unexisting NetworkPlayer id!");
			NetworkManager.Disconnect();
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
}
