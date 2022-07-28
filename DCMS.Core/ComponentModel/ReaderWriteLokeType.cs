namespace DCMS.Core.ComponentModel
{
    /// <summary>
    /// 读/写锁
    /// </summary>
    public enum ReaderWriteLockType
    {
        Read,
        Write,
        UpgradeableRead
    }
}
