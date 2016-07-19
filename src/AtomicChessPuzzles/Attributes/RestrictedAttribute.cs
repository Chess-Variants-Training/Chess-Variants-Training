using System;

namespace AtomicChessPuzzles.Attributes
{
    public class RestrictedAttribute : Attribute
    {
        public bool LoginRequired
        {
            get;
            private set;
        }

        public string[] Roles
        {
            get;
            private set;
        }

        public RestrictedAttribute(bool loginRequired, params string[] roles)
        {
            LoginRequired = loginRequired;
            Roles = roles;
        }
    }
}