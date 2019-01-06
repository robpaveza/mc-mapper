using System;
using System.Collections.Generic;
using System.Text;

namespace MCBEWorld.Worlds
{
    public class NotExploredException : Exception
    {
        public NotExploredException():
            base("The dimension or world has not had its boundaries explored.")
        {  }
    }
}
