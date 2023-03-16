using Sandbox;
using System;
using Editor;

/// <summary>
/// Dings when entity interacts.
/// </summary>

//[Library( "ent_clock" )]
[HammerEntity, SupportsSolid]
[RenderFields, VisGroup( VisGroup.Dynamic )]
[Model( Archetypes = ModelArchetype.animated_model | ModelArchetype.static_prop_model )]
[Title( "Clock" ), Category( "Placeable" ), Icon( "radio_button_checked" )]
public partial class ClockEntity : KeyframeEntity, IUse
{
	[Property( Title = "Activation distance" )]
	public int activationDist { get; set; } = 5;

	public bool IsUsable( Entity user )
	{
		//if ( activationDist > Vector3.DistanceBetween( user.Position, Position ) ) return true;
		return true;
	}

	public bool OnUse( Entity user )
	{
		Log.Info( "DET VIRKER!" );
		return true;
	}
}

