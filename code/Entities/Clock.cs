using Sandbox;
using Editor;

/// <summary>
/// This entity defines the spawn point of the player in first person shooter gamemodes.
/// </summary>

[Library( "ent_clock" )]
[HammerEntity, SupportsSolid]
[RenderFields, VisGroup( VisGroup.Dynamic )]
[Title( "Clock" ), Category( "Placeable" ), Icon( "radio_button_checked" )]
[Model( Archetypes = ModelArchetype.animated_model | ModelArchetype.static_prop_model )]
public partial class ClockEntity : KeyframeEntity, IUse
{
	public bool IsUsable( Entity user )
	{
		if ( user == null ) return false;
		return true;
	}

	public bool OnUse( Entity user )
	{
		Log.Info( "Wtf" );

		return true;
	}
}
