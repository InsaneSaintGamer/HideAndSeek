using Sandbox;
using static TeamOptions;

public sealed class OutlineScript : Component
{
	public void UpdatePlayerHighlights()
	{
		var localPlayer = Scene.GetAll<TeamOptions>().FirstOrDefault( x => x.Network.IsOwner );

		if ( localPlayer == null )
			return;

		foreach ( var other in Scene.GetAll<TeamOptions>() )
		{
			var outline = other.GameObject.Parent.GetComponentInChildren<HighlightOutline>();
			if ( outline == null ) continue;

			if ( other.team == localPlayer.team )
			{
				outline.Color = localPlayer.team == TeamOptions.Team.Seeker ? Color.Red : Color.Blue;
			}
			else
			{
				outline.Color = Color.Transparent;
			}
		}
	}

}
