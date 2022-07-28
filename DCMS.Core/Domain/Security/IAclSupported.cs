namespace DCMS.Core.Domain.Security
{

    public partial interface IAclSupported
    {

        bool SubjectToAcl { get; set; }
    }
}
