using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Entities
{
	public partial class Lock : KeyframeEntity, IUse
	{
		public bool IsUsable( Entity user )
		{
			throw new NotImplementedException();
		}

		public bool OnUse( Entity user )
		{
			throw new NotImplementedException();
		}
	}
}
