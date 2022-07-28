using System.Collections.Generic;

namespace DCMS.Core.Domain.Security
{

    public class DefaultPermissionRecord
    {
        public DefaultPermissionRecord()
        {
            PermissionRecords = new List<PermissionRecord>();
        }


        public string UserRoleSystemName { get; set; }

        public IEnumerable<PermissionRecord> PermissionRecords { get; set; }
    }
}
