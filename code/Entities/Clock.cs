using Sandbox;
using System;
using Editor;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Dings when entity interacts.
/// </summary>
[HammerEntity, SupportsSolid]
[RenderFields, VisGroup( VisGroup.Dynamic )]
[Model]
[Title( "Clock" ), Category( "Placeable" ), Icon( "radio_button_checked" )]
public partial class ClockEntity : KeyframeEntity, IUse
{
	List<DoorEntity> doors = new ();

	[Property( Title = "Activation distance" )]
	public int activationDist { get; set; } = 100;

	[Property( Title = "Door" )]
	public TargetEntity door { get; set; }

	public bool IsUsable( Entity user )
	{
		return user is Player && activationDist > Vector3.DistanceBetween(user.Position, Position);
	}

	public bool OnUse( Entity user )
	{
		doors.Add( All.FirstOrDefault( x => x is DoorEntity && x.Name == "LobbyDoor" && !doors.Contains( x ) ) as DoorEntity );

		doors[0].Open();

		PlaySound( "bell_sound" );
		return false;
	}

	public override void Spawn()
	{
		base.Spawn();

		//Brug måske SetAnimParameter(Name, Value)
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}
}

