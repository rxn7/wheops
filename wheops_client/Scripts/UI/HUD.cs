using Godot;

public class HUD : CanvasLayer {
	public Crosshair m_crosshair;
	private Label m_weapon_info_label;

	public override void _Ready() {
		m_crosshair = GetNode<Crosshair>("Crosshair");
		m_weapon_info_label = GetNode<Label>("WeaponInfo");
	}

	public void UpdateWeaponInfoLabel()  {
		m_weapon_info_label.Text = Global.Instance.CurrentMap.Player.m_weapon_mgr.m_held_weapon.Data.m_name;

		if(Global.Instance.CurrentMap.Player.m_weapon_mgr.m_reloading) {
			m_weapon_info_label.Text += "\nRELOADING...";
		} else {
			m_weapon_info_label.Text += $"\n{Global.Instance.CurrentMap.Player.m_weapon_mgr.m_held_weapon.m_ammo_left} / {Global.Instance.CurrentMap.Player.m_weapon_mgr.m_held_weapon.Data.m_ammo_cap}";
		}
	}
}
