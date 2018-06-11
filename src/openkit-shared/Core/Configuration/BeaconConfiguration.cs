using System;
using System.Collections.Generic;
using System.Text;

namespace Dynatrace.OpenKit.Core.Configuration
{
    public class BeaconConfiguration
    {
        public BeaconConfiguration(int multiplicity)
        {
            Multiplicity = multiplicity;
        }

        public int Multiplicity { get; }

        public bool CapturingAllowed => Multiplicity > 0;
    }
}
