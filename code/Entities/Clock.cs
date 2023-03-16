using Sandbox;
using Editor;

namespace Sandbox.Entities
{

	

	/// <summary>
	/// This entity defines the spawn point of the player in first person shooter gamemodes.
	/// </summary>
	[Library( "ent_Clock" ), HammerEntity]
	[Title( "ClockEntity" ), Category( "Placeable" ), Icon( "place" )]
	[Model]
	public class ClockEntity : Entity
	{
		[Property( Title = "Start Disabled" )]
		public bool StartDisabled { get; set; } = false;
	}
}
