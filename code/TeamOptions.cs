using Sandbox;
using System.Xml.Linq;

public sealed class TeamOptions : Component
{
	public enum Team
	{
		Hider,
		Seeker
	}
	[Sync( SyncFlags.FromHost ), Change( nameof( OnTeamChanged ) )][Property] public Team team { get; set; }

	public OutlineScript outlineScript;
	protected override void OnAwake()
	{
		outlineScript = Scene.Get<OutlineScript>();
	}
	private void OnTeamChanged()
	{
		outlineScript.UpdatePlayerHighlights();
	}
}
