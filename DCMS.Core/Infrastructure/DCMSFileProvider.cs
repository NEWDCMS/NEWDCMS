using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.AccessControl;
using System.Text;
using System.Threading;

namespace DCMS.Core.Infrastructure
{
    /*
    /// <summary>
    /// 使用系统磁盘文件的IO操作 2.2
    /// </summary>
    public class DCMSFileProvider : PhysicalFileProvider, IDCMSFileProvider
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="hostingEnvironment"></param>
        public DCMSFileProvider(IWebHostEnvironment hostingEnvironment)
            : base(File.Exists(hostingEnvironment.WebRootPath) ? Path.GetDirectoryName(hostingEnvironment.WebRootPath) : hostingEnvironment.WebRootPath)
        {
            var path = hostingEnvironment.ContentRootPath ?? string.Empty;
            if (File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
            }

            BaseDirectory = path;
        }


        private static void DeleteDirectoryRecursive(string path)
        {
            Directory.Delete(path, true);
            const int maxIterationToWait = 10;
            var curIteration = 0;

            //according to the documentation(https://msdn.microsoft.com/ru-ru/library/windows/desktop/aa365488.aspx) 
            //System.IO.Directory.Delete method ultimately (after removing the files) calls native 
            //RemoveDirectory function which marks the directory as "deleted". That's why we wait until 
            //the directory is actually deleted. For more details see https://stackoverflow.com/a/4245121
            while (Directory.Exists(path))
            {
                curIteration += 1;
                if (curIteration > maxIterationToWait)
                {
                    return;
                }

                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 确定字符串是否为服务器和共享路径的有效通用命名约定（UNC）
        /// </summary>
        /// <param name="path">The path to be tested.</param>
        /// <returns><see langword="true"/> if the path is a valid UNC path; 
        /// otherwise, <see langword="false"/>.</returns>
        protected static bool IsUncPath(string path)
        {
            return Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.IsUnc;
        }



        /// <summary>
        /// 将字符串数组组合成路径
        /// </summary>
        /// <param name="paths">An array of parts of the path</param>
        /// <returns>The combined paths</returns>
        public virtual string Combine(params string[] paths)
        {
            var path = Path.Combine(paths.SelectMany(p => IsUncPath(p) ? new[] { p } : p.Split('\\', '/')).ToArray());

            if (Environment.OSVersion.Platform == PlatformID.Unix && !IsUncPath(path))
            {
                //add leading slash to correctly form path in the UNIX system
                path = "/" + path;
            }

            return path;
        }

        /// <summary>
        /// 在指定路径中创建所有目录和子目录，除非它们已经存在
        /// </summary>
        /// <param name="path">The directory to create</param>
        public virtual void CreateDirectory(string path)
        {
            if (!DirectoryExists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 在指定路径中创建或覆盖文件
        /// </summary>
        /// <param name="path">The path and name of the file to create</param>
        public virtual void CreateFile(string path)
        {
            if (FileExists(path))
            {
                return;
            }

            var fileInfo = new FileInfo(path);
            CreateDirectory(fileInfo.DirectoryName);

            //we use 'using' to close the file after it's created
            using (File.Create(path))
            {
            }
        }

        /// <summary>
        ///  深度优先递归删除，在Windows资源管理器中打开子目录的处理。
        /// </summary>
        /// <param name="path">Directory path</param>
        public void DeleteDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(path);
            }

            //find more info about directory deletion
            //and why we use this approach at https://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true

            foreach (var directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }

            try
            {
                DeleteDirectoryRecursive(path);
            }
            catch (IOException)
            {
                DeleteDirectoryRecursive(path);
            }
            catch (UnauthorizedAccessException)
            {
                DeleteDirectoryRecursive(path);
            }
        }

        /// <summary>
        /// 删除指定文件
        /// </summary>
        /// <param name="filePath">The name of the file to be deleted. Wildcard characters are not supported</param>
        public virtual void DeleteFile(string filePath)
        {
            if (!FileExists(filePath))
            {
                return;
            }

            File.Delete(filePath);
        }

        /// <summary>
        /// 确定给定路径是否引用磁盘上的现有目录
        /// </summary>
        /// <param name="path">The path to test</param>
        /// <returns>
        /// true if path refers to an existing directory; false if the directory does not exist or an error occurs when
        /// trying to determine if the specified file exists
        /// </returns>
        public virtual bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// 将文件或目录及其内容移动到新位置
        /// </summary>
        /// <param name="sourceDirName">The path of the file or directory to move</param>
        /// <param name="destDirName">
        /// The path to the new location for sourceDirName. If sourceDirName is a file, then destDirName
        /// must also be a file name
        /// </param>
        public virtual void DirectoryMove(string sourceDirName, string destDirName)
        {
            Directory.Move(sourceDirName, destDirName);
        }

        /// <summary>
        /// 返回搜索模式匹配的文件名的可枚举集合指定的路径，并可以选择搜索子目录
        /// </summary>
        /// <param name="directoryPath">The path to the directory to search</param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in path. This parameter
        /// can contain a combination of valid literal path and wildcard (* and ?) characters
        /// , but doesn't support regular expressions.
        /// </param>
        /// <param name="topDirectoryOnly">
        /// Specifies whether to search the current directory, or the current directory and all
        /// subdirectories
        /// </param>
        /// <returns>
        /// An enumerable collection of the full names (including paths) for the files in
        /// the directory specified by path and that match the specified search pattern
        /// </returns>
        public virtual IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern,
            bool topDirectoryOnly = true)
        {
            return Directory.EnumerateFiles(directoryPath, searchPattern,
                topDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
        }

        /// <summary>
        /// 将现有文件复制到新文件。允许重写同名文件
        /// </summary>
        /// <param name="sourceFileName">The file to copy</param>
        /// <param name="destFileName">The name of the destination file. This cannot be a directory</param>
        /// <param name="overwrite">true if the destination file can be overwritten; otherwise, false</param>
        public virtual void FileCopy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            File.Copy(sourceFileName, destFileName, overwrite);
        }

        /// <summary>
        /// 确定指定文件是否存在
        /// </summary>
        /// <param name="filePath">The file to check</param>
        /// <returns>
        /// True if the caller has the required permissions and path contains the name of an existing file; otherwise,
        /// false.
        /// </returns>
        public virtual bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// 以字节为单位获取文件长度，或目录或非现有文件的长度为1
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>The length of the file</returns>
        public virtual long FileLength(string path)
        {
            if (!FileExists(path))
            {
                return -1;
            }

            return new FileInfo(path).Length;
        }

        /// <summary>
        /// 将指定的文件移动到新位置，提供指定新文件名的选项
        /// </summary>
        /// <param name="sourceFileName">The name of the file to move. Can include a relative or absolute path</param>
        /// <param name="destFileName">The new path and name for the file</param>
        public virtual void FileMove(string sourceFileName, string destFileName)
        {
            File.Move(sourceFileName, destFileName);
        }

        /// <summary>
        /// 返回目录的绝对路径
        /// </summary>
        /// <param name="paths">An array of parts of the path</param>
        /// <returns>The absolute path to the directory</returns>
        public virtual string GetAbsolutePath(params string[] paths)
        {
            var allPaths = new List<string> { Root };
            allPaths.AddRange(paths);

            return Combine(allPaths.ToArray());
        }

        /// <summary>
        /// 获取一个System.Security.AccessControl.DirectorySecurity对象，该对象封装指定目录的访问控制列表（ACL）项
        /// </summary>
        /// <param name="path">The path to a directory containing a System.Security.AccessControl.DirectorySecurity object that describes the file's access control list (ACL) information</param>
        /// <returns>An object that encapsulates the access control rules for the file described by the path parameter</returns>
        public virtual DirectorySecurity GetAccessControl(string path)
        {
            return new DirectoryInfo(path).GetAccessControl();
        }

        /// <summary>
        /// 返回指定文件或目录的创建日期和时间
        /// </summary>
        /// <param name="path">The file or directory for which to obtain creation date and time information</param>
        /// <returns>
        /// A System.DateTime structure set to the creation date and time for the specified file or directory. This value
        /// is expressed in local time
        /// </returns>
        public virtual DateTime GetCreationTime(string path)
        {
            return File.GetCreationTime(path);
        }

        /// <summary>
        /// 返回与指定目录中的指定搜索模式匹配的子目录的名称（包括其路径）
        /// </summary>
        /// <param name="path">The path to the directory to search</param>
        /// <param name="searchPattern">
        /// The search string to match against the names of subdirectories in path. This
        /// parameter can contain a combination of valid literal and wildcard characters
        /// , but doesn't support regular expressions.
        /// </param>
        /// <param name="topDirectoryOnly">
        /// Specifies whether to search the current directory, or the current directory and all
        /// subdirectories
        /// </param>
        /// <returns>
        /// An array of the full names (including paths) of the subdirectories that match
        /// the specified criteria, or an empty array if no directories are found
        /// </returns>
        public virtual string[] GetDirectories(string path, string searchPattern = "", bool topDirectoryOnly = true)
        {
            if (string.IsNullOrEmpty(searchPattern))
            {
                searchPattern = "*";
            }

            return Directory.GetDirectories(path, searchPattern,
                topDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
        }

        /// <summary>
        /// 返回指定路径字符串的目录信息
        /// </summary>
        /// <param name="path">The path of a file or directory</param>
        /// <returns>
        /// Directory information for path, or null if path denotes a root directory or is null. Returns
        /// System.String.Empty if path does not contain directory information
        /// </returns>
        public virtual string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        /// 只返回指定路径字符串的目录名
        /// </summary>
        /// <param name="path">The path of directory</param>
        /// <returns>The directory name</returns>
        public virtual string GetDirectoryNameOnly(string path)
        {
            return new DirectoryInfo(path).Name;
        }

        /// <summary>
        /// 返回指定路径字符串的扩展名
        /// </summary>
        /// <param name="filePath">The path string from which to get the extension</param>
        /// <returns>The extension of the specified path (including the period ".")</returns>
        public virtual string GetFileExtension(string filePath)
        {
            return Path.GetExtension(filePath);
        }

        /// <summary>
        /// 返回指定路径字符串的文件名和扩展名
        /// </summary>
        /// <param name="path">The path string from which to obtain the file name and extension</param>
        /// <returns>The characters after the last directory character in path</returns>
        public virtual string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        /// <summary>
        /// 返回不带扩展名的指定路径字符串的文件名
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        /// <returns>The file name, minus the last period (.) and all characters following it</returns>
        public virtual string GetFileNameWithoutExtension(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }

        /// <summary>
        /// 返回与指定目录中的指定搜索模式匹配的文件名（包括其路径），并使用值确定是否搜索子目录
        /// </summary>
        /// <param name="directoryPath">The path to the directory to search</param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in path. This parameter
        /// can contain a combination of valid literal path and wildcard (* and ?) characters
        /// , but doesn't support regular expressions.
        /// </param>
        /// <param name="topDirectoryOnly">
        /// Specifies whether to search the current directory, or the current directory and all
        /// subdirectories
        /// </param>
        /// <returns>
        /// An array of the full names (including paths) for the files in the specified directory
        /// that match the specified search pattern, or an empty array if no files are found.
        /// </returns>
        public virtual string[] GetFiles(string directoryPath, string searchPattern = "", bool topDirectoryOnly = true)
        {
            //"E:\\Git\\DCMS.Studio.Core\\DCMS.Core\\DCMS.Manage\\Themes"
            if (string.IsNullOrEmpty(searchPattern))
            {
                searchPattern = "*.*";
            }

            return Directory.GetFiles(directoryPath, searchPattern,
                topDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
        }

        /// <summary>
        /// 返回上次访问指定文件或目录的日期和时间
        /// </summary>
        /// <param name="path">The file or directory for which to obtain access date and time information</param>
        /// <returns>A System.DateTime structure set to the date and time that the specified file</returns>
        public virtual DateTime GetLastAccessTime(string path)
        {
            return File.GetLastAccessTime(path);
        }

        /// <summary>
        /// 返回上次写入指定文件或目录的日期和时间
        /// </summary>
        /// <param name="path">The file or directory for which to obtain write date and time information</param>
        /// <returns>
        /// A System.DateTime structure set to the date and time that the specified file or directory was last written to.
        /// This value is expressed in local time
        /// </returns>
        public virtual DateTime GetLastWriteTime(string path)
        {
            return File.GetLastWriteTime(path);
        }

        /// <summary>
        /// 返回上次写入指定文件或目录的日期和时间（以协调世界时（UTC）为单位）
        /// </summary>
        /// <param name="path">The file or directory for which to obtain write date and time information</param>
        /// <returns>
        /// A System.DateTime structure set to the date and time that the specified file or directory was last written to.
        /// This value is expressed in UTC time
        /// </returns>
        public virtual DateTime GetLastWriteTimeUtc(string path)
        {
            return File.GetLastWriteTimeUtc(path);
        }

        /// <summary>
        /// 检索指定路径的父目录
        /// </summary>
        /// <param name="directoryPath">The path for which to retrieve the parent directory</param>
        /// <returns>The parent directory, or null if path is the root directory, including the root of a UNC server or share name</returns>
        public virtual string GetParentDirectory(string directoryPath)
        {
            return Directory.GetParent(directoryPath).FullName;
        }

        /// <summary>
        /// 从物理磁盘路径获取虚拟路径
        /// </summary>
        /// <param name="path">The physical disk path</param>
        /// <returns>The virtual path. E.g. "~/bin"</returns>
        public virtual string GetVirtualPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            if (!IsDirectory(path) && FileExists(path))
            {
                path = new FileInfo(path).DirectoryName;
            }

            path = path?.Replace(Root, "").Replace('\\', '/').Trim('/').TrimStart('~', '/');

            return $"~/{path ?? ""}";
        }

        /// <summary>
        /// 检查路径是否为目录
        /// </summary>
        /// <param name="path">Path for check</param>
        /// <returns>True, if the path is a directory, otherwise false</returns>
        public virtual bool IsDirectory(string path)
        {
            return DirectoryExists(path);
        }

        /// <summary>
        /// 将虚拟路径映射到物理磁盘路径
        /// </summary>
        /// <param name="path">The path to map. E.g. "~/bin"</param>
        /// <returns>The physical path. E.g. "c:\inetpub\wwwroot\bin"</returns>
        public virtual string MapPath(string path)
        {
            path = path.Replace("~/", string.Empty).TrimStart('/');

            //if virtual path has slash on the end, it should be after transform the virtual path to physical path too
            var pathEnd = path.EndsWith('/') ? Path.DirectorySeparatorChar.ToString() : string.Empty;

            return Combine(BaseDirectory ?? string.Empty, path) + pathEnd;
        }

        /// <summary>
        /// 将文件内容读入字节数组
        /// </summary>
        /// <param name="filePath">The file for reading</param>
        /// <returns>A byte array containing the contents of the file</returns>
        public virtual byte[] ReadAllBytes(string filePath)
        {
            return File.Exists(filePath) ? File.ReadAllBytes(filePath) : new byte[0];
        }

        /// <summary>
        /// 打开一个文件，用指定的编码读取该文件的所有行，然后关闭该文件
        /// </summary>
        /// <param name="path">The file to open for reading</param>
        /// <param name="encoding">The encoding applied to the contents of the file</param>
        /// <returns>A string containing all lines of the file</returns>
        public virtual string ReadAllText(string path, Encoding encoding)
        {
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var streamReader = new StreamReader(fileStream, encoding))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// 设置上次写入指定文件的日期和时间（以协调世界时（UTC）为单位）
        /// </summary>
        /// <param name="path">The file for which to set the date and time information</param>
        /// <param name="lastWriteTimeUtc">
        /// A System.DateTime containing the value to set for the last write date and time of path.
        /// This value is expressed in UTC time
        /// </param>
        public virtual void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
        }

        /// <summary>
        /// 将指定的字节数组写入文件
        /// </summary>
        /// <param name="filePath">The file to write to</param>
        /// <param name="bytes">The bytes to write to the file</param>
        public virtual void WriteAllBytes(string filePath, byte[] bytes)
        {
            File.WriteAllBytes(filePath, bytes);
        }

        /// <summary>
        /// 创建一个新文件，使用指定的编码将指定的字符串写入该文件，然后关闭该文件。如果目标文件已经存在，则覆盖该文件
        /// </summary>
        /// <param name="path">The file to write to</param>
        /// <param name="contents">The string to write to the file</param>
        /// <param name="encoding">The encoding to apply to the string</param>
        public virtual void WriteAllText(string path, string contents, Encoding encoding)
        {
            File.WriteAllText(path, contents, encoding);
        }


        /// <summary>
        /// 上传照片
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public virtual string UploadFile(IFormFile file)
        {
            var pictureId = "";
            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    byte[] Bytes = new byte[file.OpenReadStream().Length];
                    file.OpenReadStream().Read(Bytes, 0, Bytes.Length);
                    // 设置当前流的位置为流的开始 
                    file.OpenReadStream().Seek(0, SeekOrigin.Begin);

                    var fileContent = new ByteArrayContent(Bytes);
                    //设置请求头中的附件为文件名称，以便在WebAPi中进行获取
                    fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("p_w_upload") { FileName = file.FileName };
                    content.Add(fileContent, "\"file\"", $"\"{file.FileName}\"");

                    //var uploadServiceBaseAddress = "http://" + ConfigurationManager.AppSettings["dcms-upload-service"] + "/document/reomte/fileupload/HRXHJS";
                    var uploadServiceBaseAddress = "http://resources.jsdcms.com:9100/document/reomte/fileupload/HRXHJS";
                    var result = client.PostAsync(uploadServiceBaseAddress, content).Result;
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        //获取到上传文件地址，并渲染到视图中进行访问   
                        var m = result.Content.ReadAsStringAsync().Result.ToString();
                        var list = JsonConvert.DeserializeObject(m);
                        pictureId = ((Newtonsoft.Json.Linq.JProperty)((Newtonsoft.Json.Linq.JContainer)list).Last).Value.ToString();
                    }
                }
            }
            return pictureId;
        }


        protected string BaseDirectory { get; }
    }
    */

    /// <summary>
    /// 使用系统磁盘文件的IO操作 3.0
    /// </summary>
    public class DCMSFileProvider : PhysicalFileProvider, IDCMSFileProvider
    {
        /// <summary>
        /// 初始 DCMSFileProvider
        /// </summary>
        /// <param name="webHostEnvironment"></param>
        public DCMSFileProvider(IWebHostEnvironment webHostEnvironment)
            : base(File.Exists(webHostEnvironment.ContentRootPath) ? Path.GetDirectoryName(webHostEnvironment.ContentRootPath) : webHostEnvironment.ContentRootPath)
        {
            WebRootPath = File.Exists(webHostEnvironment.WebRootPath)
                ? Path.GetDirectoryName(webHostEnvironment.WebRootPath)
                : webHostEnvironment.WebRootPath;
        }

        #region Utilities

        private static void DeleteDirectoryRecursive(string path)
        {
            Directory.Delete(path, true);
            const int maxIterationToWait = 10;
            var curIteration = 0;

            //according to the documentation(https://msdn.microsoft.com/ru-ru/library/windows/desktop/aa365488.aspx) 
            //System.IO.Directory.Delete method ultimately (after removing the files) calls native 
            //RemoveDirectory function which marks the directory as "deleted". That's why we wait until 
            //the directory is actually deleted. For more details see https://stackoverflow.com/a/4245121
            while (Directory.Exists(path))
            {
                curIteration += 1;
                if (curIteration > maxIterationToWait)
                {
                    return;
                }

                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Determines if the string is a valid Universal Naming Convention (UNC)
        /// for a server and share path.
        /// </summary>
        /// <param name="path">The path to be tested.</param>
        /// <returns><see langword="true"/> if the path is a valid UNC path; 
        /// otherwise, <see langword="false"/>.</returns>
        protected static bool IsUncPath(string path)
        {
            return Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.IsUnc;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Combines an array of strings into a path
        /// </summary>
        /// <param name="paths">An array of parts of the path</param>
        /// <returns>The combined paths</returns>
        public virtual string Combine(params string[] paths)
        {
            var path = Path.Combine(paths.SelectMany(p => IsUncPath(p) ? new[] { p } : p.Split('\\', '/')).ToArray());

            if (Environment.OSVersion.Platform == PlatformID.Unix && !IsUncPath(path))
            {
                //add leading slash to correctly form path in the UNIX system
                path = "/" + path;
            }

            return path;
        }

        /// <summary>
        /// Creates all directories and subdirectories in the specified path unless they already exist
        /// </summary>
        /// <param name="path">The directory to create</param>
        public virtual void CreateDirectory(string path)
        {
            if (!DirectoryExists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Creates or overwrites a file in the specified path
        /// </summary>
        /// <param name="path">The path and name of the file to create</param>
        public virtual void CreateFile(string path)
        {
            if (FileExists(path))
            {
                return;
            }

            var fileInfo = new FileInfo(path);
            CreateDirectory(fileInfo.DirectoryName);

            //we use 'using' to close the file after it's created
            using (File.Create(path))
            {
            }
        }

        /// <summary>
        ///  Depth-first recursive delete, with handling for descendant directories open in Windows Explorer.
        /// </summary>
        /// <param name="path">Directory path</param>
        public void DeleteDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(path);
            }

            //find more info about directory deletion
            //and why we use this approach at https://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true

            foreach (var directory in Directory.GetDirectories(path))
            {
                DeleteDirectory(directory);
            }

            try
            {
                DeleteDirectoryRecursive(path);
            }
            catch (IOException)
            {
                DeleteDirectoryRecursive(path);
            }
            catch (UnauthorizedAccessException)
            {
                DeleteDirectoryRecursive(path);
            }
        }

        /// <summary>
        /// Deletes the specified file
        /// </summary>
        /// <param name="filePath">The name of the file to be deleted. Wildcard characters are not supported</param>
        public virtual void DeleteFile(string filePath)
        {
            if (!FileExists(filePath))
            {
                return;
            }

            File.Delete(filePath);
        }

        /// <summary>
        /// Determines whether the given path refers to an existing directory on disk
        /// </summary>
        /// <param name="path">The path to test</param>
        /// <returns>
        /// true if path refers to an existing directory; false if the directory does not exist or an error occurs when
        /// trying to determine if the specified file exists
        /// </returns>
        public virtual bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        /// <summary>
        /// Moves a file or a directory and its contents to a new location
        /// </summary>
        /// <param name="sourceDirName">The path of the file or directory to move</param>
        /// <param name="destDirName">
        /// The path to the new location for sourceDirName. If sourceDirName is a file, then destDirName
        /// must also be a file name
        /// </param>
        public virtual void DirectoryMove(string sourceDirName, string destDirName)
        {
            Directory.Move(sourceDirName, destDirName);
        }

        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in
        /// a specified path, and optionally searches subdirectories.
        /// </summary>
        /// <param name="directoryPath">The path to the directory to search</param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in path. This parameter
        /// can contain a combination of valid literal path and wildcard (* and ?) characters
        /// , but doesn't support regular expressions.
        /// </param>
        /// <param name="topDirectoryOnly">
        /// Specifies whether to search the current directory, or the current directory and all
        /// subdirectories
        /// </param>
        /// <returns>
        /// An enumerable collection of the full names (including paths) for the files in
        /// the directory specified by path and that match the specified search pattern
        /// </returns>
        public virtual IEnumerable<string> EnumerateFiles(string directoryPath, string searchPattern,
            bool topDirectoryOnly = true)
        {
            return Directory.EnumerateFiles(directoryPath, searchPattern,
                topDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
        }

        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is allowed
        /// </summary>
        /// <param name="sourceFileName">The file to copy</param>
        /// <param name="destFileName">The name of the destination file. This cannot be a directory</param>
        /// <param name="overwrite">true if the destination file can be overwritten; otherwise, false</param>
        public virtual void FileCopy(string sourceFileName, string destFileName, bool overwrite = false)
        {
            File.Copy(sourceFileName, destFileName, overwrite);
        }

        /// <summary>
        /// Determines whether the specified file exists
        /// </summary>
        /// <param name="filePath">The file to check</param>
        /// <returns>
        /// True if the caller has the required permissions and path contains the name of an existing file; otherwise,
        /// false.
        /// </returns>
        public virtual bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// Gets the length of the file in bytes, or -1 for a directory or non-existing files
        /// </summary>
        /// <param name="path">File path</param>
        /// <returns>The length of the file</returns>
        public virtual long FileLength(string path)
        {
            if (!FileExists(path))
            {
                return -1;
            }

            return new FileInfo(path).Length;
        }

        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name
        /// </summary>
        /// <param name="sourceFileName">The name of the file to move. Can include a relative or absolute path</param>
        /// <param name="destFileName">The new path and name for the file</param>
        public virtual void FileMove(string sourceFileName, string destFileName)
        {
            File.Move(sourceFileName, destFileName);
        }

        /// <summary>
        /// Returns the absolute path to the directory
        /// </summary>
        /// <param name="paths">An array of parts of the path</param>
        /// <returns>The absolute path to the directory</returns>
        public virtual string GetAbsolutePath(params string[] paths)
        {
            var allPaths = new List<string>();

            if (paths.Any() && !paths[0].Contains(WebRootPath, StringComparison.InvariantCulture))
            {
                allPaths.Add(WebRootPath);
            }

            allPaths.AddRange(paths);

            return Combine(allPaths.ToArray());
        }

        /// <summary>
        /// Gets a System.Security.AccessControl.DirectorySecurity object that encapsulates the access control list (ACL) entries for a specified directory
        /// </summary>
        /// <param name="path">The path to a directory containing a System.Security.AccessControl.DirectorySecurity object that describes the file's access control list (ACL) information</param>
        /// <returns>An object that encapsulates the access control rules for the file described by the path parameter</returns>
        public virtual DirectorySecurity GetAccessControl(string path)
        {
            return new DirectoryInfo(path).GetAccessControl();
        }

        /// <summary>
        /// Returns the creation date and time of the specified file or directory
        /// </summary>
        /// <param name="path">The file or directory for which to obtain creation date and time information</param>
        /// <returns>
        /// A System.DateTime structure set to the creation date and time for the specified file or directory. This value
        /// is expressed in local time
        /// </returns>
        public virtual DateTime GetCreationTime(string path)
        {
            return File.GetCreationTime(path);
        }

        /// <summary>
        /// Returns the names of the subdirectories (including their paths) that match the
        /// specified search pattern in the specified directory
        /// </summary>
        /// <param name="path">The path to the directory to search</param>
        /// <param name="searchPattern">
        /// The search string to match against the names of subdirectories in path. This
        /// parameter can contain a combination of valid literal and wildcard characters
        /// , but doesn't support regular expressions.
        /// </param>
        /// <param name="topDirectoryOnly">
        /// Specifies whether to search the current directory, or the current directory and all
        /// subdirectories
        /// </param>
        /// <returns>
        /// An array of the full names (including paths) of the subdirectories that match
        /// the specified criteria, or an empty array if no directories are found
        /// </returns>
        public virtual string[] GetDirectories(string path, string searchPattern = "", bool topDirectoryOnly = true)
        {
            if (string.IsNullOrEmpty(searchPattern))
            {
                searchPattern = "*";
            }

            return Directory.GetDirectories(path, searchPattern,
                topDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
        }

        /// <summary>
        /// Returns the directory information for the specified path string
        /// </summary>
        /// <param name="path">The path of a file or directory</param>
        /// <returns>
        /// Directory information for path, or null if path denotes a root directory or is null. Returns
        /// System.String.Empty if path does not contain directory information
        /// </returns>
        public virtual string GetDirectoryName(string path)
        {
            return Path.GetDirectoryName(path);
        }

        /// <summary>
        /// Returns the directory name only for the specified path string
        /// </summary>
        /// <param name="path">The path of directory</param>
        /// <returns>The directory name</returns>
        public virtual string GetDirectoryNameOnly(string path)
        {
            return new DirectoryInfo(path).Name;
        }

        /// <summary>
        /// Returns the extension of the specified path string
        /// </summary>
        /// <param name="filePath">The path string from which to get the extension</param>
        /// <returns>The extension of the specified path (including the period ".")</returns>
        public virtual string GetFileExtension(string filePath)
        {
            return Path.GetExtension(filePath);
        }

        /// <summary>
        /// Returns the file name and extension of the specified path string
        /// </summary>
        /// <param name="path">The path string from which to obtain the file name and extension</param>
        /// <returns>The characters after the last directory character in path</returns>
        public virtual string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        /// <summary>
        /// Returns the file name of the specified path string without the extension
        /// </summary>
        /// <param name="filePath">The path of the file</param>
        /// <returns>The file name, minus the last period (.) and all characters following it</returns>
        public virtual string GetFileNameWithoutExtension(string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }

        /// <summary>
        /// Returns the names of files (including their paths) that match the specified search
        /// pattern in the specified directory, using a value to determine whether to search subdirectories.
        /// </summary>
        /// <param name="directoryPath">The path to the directory to search</param>
        /// <param name="searchPattern">
        /// The search string to match against the names of files in path. This parameter
        /// can contain a combination of valid literal path and wildcard (* and ?) characters
        /// , but doesn't support regular expressions.
        /// </param>
        /// <param name="topDirectoryOnly">
        /// Specifies whether to search the current directory, or the current directory and all
        /// subdirectories
        /// </param>
        /// <returns>
        /// An array of the full names (including paths) for the files in the specified directory
        /// that match the specified search pattern, or an empty array if no files are found.
        /// </returns>
        public virtual string[] GetFiles(string directoryPath, string searchPattern = "", bool topDirectoryOnly = true)
        {
            if (string.IsNullOrEmpty(searchPattern))
            {
                searchPattern = "*.*";
            }

            return Directory.GetFiles(directoryPath, searchPattern,
                topDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories);
        }

        /// <summary>
        /// Returns the date and time the specified file or directory was last accessed
        /// </summary>
        /// <param name="path">The file or directory for which to obtain access date and time information</param>
        /// <returns>A System.DateTime structure set to the date and time that the specified file</returns>
        public virtual DateTime GetLastAccessTime(string path)
        {
            return File.GetLastAccessTime(path);
        }

        /// <summary>
        /// Returns the date and time the specified file or directory was last written to
        /// </summary>
        /// <param name="path">The file or directory for which to obtain write date and time information</param>
        /// <returns>
        /// A System.DateTime structure set to the date and time that the specified file or directory was last written to.
        /// This value is expressed in local time
        /// </returns>
        public virtual DateTime GetLastWriteTime(string path)
        {
            return File.GetLastWriteTime(path);
        }

        /// <summary>
        /// Returns the date and time, in coordinated universal time (UTC), that the specified file or directory was last
        /// written to
        /// </summary>
        /// <param name="path">The file or directory for which to obtain write date and time information</param>
        /// <returns>
        /// A System.DateTime structure set to the date and time that the specified file or directory was last written to.
        /// This value is expressed in UTC time
        /// </returns>
        public virtual DateTime GetLastWriteTimeUtc(string path)
        {
            return File.GetLastWriteTimeUtc(path);
        }

        /// <summary>
        /// Retrieves the parent directory of the specified path
        /// </summary>
        /// <param name="directoryPath">The path for which to retrieve the parent directory</param>
        /// <returns>The parent directory, or null if path is the root directory, including the root of a UNC server or share name</returns>
        public virtual string GetParentDirectory(string directoryPath)
        {
            return Directory.GetParent(directoryPath).FullName;
        }

        /// <summary>
        /// Gets a virtual path from a physical disk path.
        /// </summary>
        /// <param name="path">The physical disk path</param>
        /// <returns>The virtual path. E.g. "~/bin"</returns>
        public virtual string GetVirtualPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            if (!IsDirectory(path) && FileExists(path))
            {
                path = new FileInfo(path).DirectoryName;
            }

            path = path?.Replace(WebRootPath, string.Empty).Replace('\\', '/').Trim('/').TrimStart('~', '/');

            return $"~/{path ?? string.Empty}";
        }

        /// <summary>
        /// Checks if the path is directory
        /// </summary>
        /// <param name="path">Path for check</param>
        /// <returns>True, if the path is a directory, otherwise false</returns>
        public virtual bool IsDirectory(string path)
        {
            return DirectoryExists(path);
        }

        /// <summary>
        /// Maps a virtual path to a physical disk path.
        /// </summary>
        /// <param name="path">The path to map. E.g. "~/bin"</param>
        /// <returns>The physical path. E.g. "c:\inetpub\wwwroot\bin"</returns>
        public virtual string MapPath(string path)
        {
            path = path.Replace("~/", string.Empty).TrimStart('/');

            //if virtual path has slash on the end, it should be after transform the virtual path to physical path too
            var pathEnd = path.EndsWith('/') ? Path.DirectorySeparatorChar.ToString() : string.Empty;

            return Combine(Root ?? string.Empty, path) + pathEnd;
        }

        /// <summary>
        /// Reads the contents of the file into a byte array
        /// </summary>
        /// <param name="filePath">The file for reading</param>
        /// <returns>A byte array containing the contents of the file</returns>
        public virtual byte[] ReadAllBytes(string filePath)
        {
            return File.Exists(filePath) ? File.ReadAllBytes(filePath) : Array.Empty<byte>();
        }

        /// <summary>
        /// Opens a file, reads all lines of the file with the specified encoding, and then closes the file.
        /// </summary>
        /// <param name="path">The file to open for reading</param>
        /// <param name="encoding">The encoding applied to the contents of the file</param>
        /// <returns>A string containing all lines of the file</returns>
        public virtual string ReadAllText(string path, Encoding encoding)
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(fileStream, encoding);
            return streamReader.ReadToEnd();
        }

        /// <summary>
        /// Sets the date and time, in coordinated universal time (UTC), that the specified file was last written to
        /// </summary>
        /// <param name="path">The file for which to set the date and time information</param>
        /// <param name="lastWriteTimeUtc">
        /// A System.DateTime containing the value to set for the last write date and time of path.
        /// This value is expressed in UTC time
        /// </param>
        public virtual void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            File.SetLastWriteTimeUtc(path, lastWriteTimeUtc);
        }

        /// <summary>
        /// Writes the specified byte array to the file
        /// </summary>
        /// <param name="filePath">The file to write to</param>
        /// <param name="bytes">The bytes to write to the file</param>
        public virtual void WriteAllBytes(string filePath, byte[] bytes)
        {
            File.WriteAllBytes(filePath, bytes);
        }

        /// <summary>
        /// Creates a new file, writes the specified string to the file using the specified encoding,
        /// and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">The file to write to</param>
        /// <param name="contents">The string to write to the file</param>
        /// <param name="encoding">The encoding to apply to the string</param>
        public virtual void WriteAllText(string path, string contents, Encoding encoding)
        {
            File.WriteAllText(path, contents, encoding);
        }

        /// <summary>Locate a file at the given path.</summary>
        /// <param name="subpath">Relative path that identifies the file.</param>
        /// <returns>The file information. Caller must check Exists property.</returns>
        public new IFileInfo GetFileInfo(string subpath)
        {
            subpath = subpath.Replace(Root, string.Empty);

            return base.GetFileInfo(subpath);
        }

        /// <summary>
        /// Upload photos
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public virtual string UploadFile(IFormFile file)
        {
            var pictureId = "";
            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    byte[] Bytes = new byte[file.OpenReadStream().Length];
                    file.OpenReadStream().Read(Bytes, 0, Bytes.Length);
                    // Set the location of the current flow to the beginning of the flow 
                    file.OpenReadStream().Seek(0, SeekOrigin.Begin);

                    var fileContent = new ByteArrayContent(Bytes);
                    //Set the attachment in the request header as the file name to get in webapi
                    fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("p_w_upload") { FileName = file.FileName };
                    content.Add(fileContent, "\"file\"", $"\"{file.FileName}\"");

                    //var uploadServiceBaseAddress = "http://" + ConfigurationManager.AppSettings["dcms-upload-service"] + "/document/reomte/fileupload/HRXHJS";
                    var uploadServiceBaseAddress = "http://resources.jsdcms.com:9100/document/reomte/fileupload/HRXHJS";
                    var result = client.PostAsync(uploadServiceBaseAddress, content).Result;
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        //Get the upload file address and render it to the view for access
                        var m = result.Content.ReadAsStringAsync().Result.ToString();
                        var list = JsonConvert.DeserializeObject(m);
                        pictureId = ((Newtonsoft.Json.Linq.JProperty)((Newtonsoft.Json.Linq.JContainer)list).Last).Value.ToString();
                    }
                }
            }
            return pictureId;
        }
        #endregion

        protected string WebRootPath { get; }
    }
}