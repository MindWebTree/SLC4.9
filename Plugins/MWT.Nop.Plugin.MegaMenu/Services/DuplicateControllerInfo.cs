
using System;

namespace MWT.Nop.Plugin.MegaMenu.Services
{
    public class DuplicateControllerInfo : IEquatable<DuplicateControllerInfo>
    {
        public string DuplicateControllerName { get; set; }

        public string DuplicateOfControllerName { get; set; }

        public bool Equals(DuplicateControllerInfo other)
        {
            if ((object)other == null)
                return false;
            return (object)this == (object)other || object.Equals((object)other.DuplicateControllerName, (object)this.DuplicateControllerName);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if ((object)this == obj)
                return true;
            return !(obj.GetType() != typeof(DuplicateControllerInfo)) && this.Equals((DuplicateControllerInfo)obj);
        }

        public override int GetHashCode() => this.DuplicateControllerName.GetHashCode();

        public static bool operator ==(DuplicateControllerInfo left, DuplicateControllerInfo right) => object.Equals((object)left, (object)right);

        public static bool operator !=(DuplicateControllerInfo left, DuplicateControllerInfo right) => !object.Equals((object)left, (object)right);
    }
}
