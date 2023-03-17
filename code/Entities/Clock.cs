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

	[Property( Title = "Door name" )]
	public string doorName { get; set; }

	public bool IsUsable( Entity user )
	{
		return user is Player && activationDist > Vector3.DistanceBetween(user.Position, Position);
	}

	public bool OnUse( Entity user )
	{
		if ( !user.IsValid() ) return false;

		MyGame.AddPlayerReady( user );
		if ( MyGame.GetPlayersReady() < Game.Clients.Count )
		{
			Log.Info( "Ikke nok ready!" );
			return false;
		}

		Log.Warning( "Doors opening!" );

		foreach ( var entity in doors )
		{
			entity.Open();
		}

		PlaySound( "bell_sound" );
		return false;
	}

	[Event.Entity.PostSpawn]
	public void AfterSpawn()
	{
		while ( All.Any( door => door is DoorEntity && door.Name == doorName && !doors.Contains( door ) ) )
		{
			doors.Add( All.FirstOrDefault( door => door is DoorEntity && door.Name == doorName && !doors.Contains( door ) ) as DoorEntity );
		}
	}

	public override void Spawn()
	{
		base.Spawn();

		//Brug måske SetAnimParameter(Name, Value)
		SetupPhysicsFromModel( PhysicsMotionType.Static );
	}
}

