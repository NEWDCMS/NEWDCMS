using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;

namespace DCMS.Core.Infrastructure
{
    /// <summary>
    /// 用于文件提供者抽象
    /// </summary>
    public interface IDCMSFileProvider : IFileProvider
    {

        string Combine(params string[] paths);

        void CreateDirectory(string path);

        void CreateFile(string path);

        void DeleteDirectory(string path);

        void DeleteFile(string filePath);

        bool DirectoryExists(string path);


        void DirectoryMove(string sourceDirName, string destDirName);


        IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern, bool topDirectoryOnly = true);

        void FileCopy(string sourceFileName, string destFileName, bool overwrite = false);


        bool FileExists(string filePath);


        long FileLength(string path);

        void FileMove(string sourceFileName, string destFileName);


        string GetAbsolutePath(params string[] paths);


        DirectorySecurity GetAccessControl(string path);


        DateTime GetCreationTime(string path);

        string[] GetDirectories(string path, string searchPattern = "", bool topDirectoryOnly = true);


        string GetDirectoryName(string path);


        string GetDirectoryNameOnly(string path);


        string GetFileExtension(string filePath);


        string GetFileName(string path);

        string GetFileNameWithoutExtension(string filePath);


        string[] GetFiles(string directoryPath, string searchPattern = "", bool topDirectoryOnly = true);


        DateTime GetLastAccessTime(string path);


        DateTime GetLastWriteTime(string path);


        DateTime GetLastWriteTimeUtc(string path);

        string GetParentDirectory(string directoryPath);

        string GetVirtualPath(string path);


        bool IsDirectory(string path);

        string MapPath(string path);

        byte[] ReadAllBytes(string filePath);

        string ReadAllText(string path, Encoding encoding);


        void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc);

        void WriteAllBytes(string filePath, byte[] bytes);

        void WriteAllText(string path, string contents, Encoding encoding);
        string UploadFile(IFormFile file);
    }
}