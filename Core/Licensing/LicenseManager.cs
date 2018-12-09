using System;

namespace GoldRush.Core.Licensing
{
    public static class LicenseManager
    {
        public static bool IsValid()
        {
            return true;    
        }

        public static void ThrowInvalidLicense()
        {
            if(!IsValid())
                throw new Exception("Invalid license.");
        }
    }
}