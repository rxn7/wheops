using Godot;

public class HUD : CanvasLayer {
	public Crosshair m_crosshair;
	private Label m_weapon_info_label;

	public override void _Ready() {
		m_crosshair = GetNode<Crosshair>("Crosshair");
		m_weapon_info_label = GetNode<Label>("WeaponInfo");
	}

	public void UpdateWeaponInfoLabel()  {
		m_weapon_info_label.Text = Player.Instance.m_weapon_mgr.m_held_weapon.m_data.m_name;

		if(Player.Instance.m_weapon_mgr.m_reloading) {
			m_weapon_info_label.Text += "\nRELOADING...";
		} else {
			m_weapon_info_label.Text += $"\n{Player.Instance.m_weapon_mgr.m_held_weapon.m_ammo_left} / {Player.Instance.m_weapon_mgr.m_held_weapon.m_data.m_ammo_cap}";
		}
	}
}
