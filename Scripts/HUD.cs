using Godot;

public class HUD : CanvasLayer {
	private Label m_ammo_label;

	public override void _Ready() {
		m_ammo_label = GetNode<Label>("Ammo");
	}

	public void UpdateAmmoLabel()  {
		if(Player.Instance.m_weapon_mgr.m_reloading) {
			m_ammo_label.Text = "RELOADING...";
		} else {
			m_ammo_label.Text = $"{Player.Instance.m_weapon_mgr.m_held_weapon.m_ammo_left} / {Player.Instance.m_weapon_mgr.m_held_weapon.m_data.m_ammo_cap}";
		}
	}
}
