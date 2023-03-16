using Sandbox;
using System;
using Editor;

/// <summary>
/// Dings when entity interacts.
/// </summary>
[HammerEntity, SupportsSolid]
[RenderFields, VisGroup( VisGroup.Dynamic )]
[EditorModel( "models/sbox_props/fire_alarm/fire_alarm_bell.vmdl_c" )]
[Title( "Clock" ), Category( "Placeable" ), Icon( "radio_button_checked" )]
public partial class ClockEntity : KeyframeEntity, IUse
{
	[Property( Title = "Activation distance" )]
	public int activationDist { get; set; } = 100;

	public bool IsUsable( Entity user )
	{
		return user is Player && activationDist > Vector3.DistanceBetween(user.Position, Position);
	}

	public bool OnUse( Entity user )
	{
		Log.Info( "DET VIRKER!" );
		return false;
	}

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "models/sbox_props/fire_alarm/fire_alarm_bell.vmdl_c" );
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}
}

