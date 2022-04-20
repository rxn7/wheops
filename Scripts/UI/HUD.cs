using Godot;

public class HUD : CanvasLayer {
	public Crosshair Crosshair { get; private set; }
	private Label m_WeaponInfoLabel;

	public override void _Ready() {
		Crosshair = GetNode<Crosshair>("Crosshair");
		m_WeaponInfoLabel = GetNode<Label>("WeaponInfo");
	}

	public void UpdateWeaponInfoLabel()  {
		m_WeaponInfoLabel.Text = Global.Player.WeaponManager.HeldWeapon.Data.Name;

		if(Global.Player.WeaponManager.IsReloading) {
			m_WeaponInfoLabel.Text += "\nRELOADING...";
		} else {
			m_WeaponInfoLabel.Text += $"\n{Global.Player.WeaponManager.HeldWeapon.AmmoLeft} / {Global.Player.WeaponManager.HeldWeapon.Data.AmmoCap}";
		}
	}
}
