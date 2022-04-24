public class NetworkPlayerData {
	public int ID { get; private set; }
	public string Nickname { get; private set; }
	
	public NetworkPlayerData(int id, string nickname) {
		ID = id;
		Nickname = nickname;
	}
}
