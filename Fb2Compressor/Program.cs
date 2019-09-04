using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Fb2Compressor
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    foreach (var file in GetFiles(o.SourceFolder, o.SearchInSubDirectories))
                    {
                        try
                        {
                            Compress(o.TargetFolder, file);
                            Remove(o.RemoveFile, file);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error!");
                            Console.WriteLine(ex.Message);
                        }
                    }

                    Console.WriteLine("Done!");
                });
        }

        private static IEnumerable<FileInfo> GetFiles(string sourceFolder, bool searchInSubDirectories)
        {
            if (!Directory.Exists(sourceFolder))
                return Enumerable.Empty<FileInfo>();

            var folder = new DirectoryInfo(sourceFolder);
            return folder.GetFiles("*.fb2", searchInSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
        }

        private static void Compress(string targetFolder, FileInfo file)
        {
            if (!Directory.Exists(targetFolder))
                targetFolder = file.Directory.FullName;

            var fileName = Path.GetFileNameWithoutExtension(file.FullName);
            var zipFileName = $@"{targetFolder}\{fileName}.zip";

            if (File.Exists(zipFileName))
            {
                Console.WriteLine($"File {zipFileName} already exists");
            }
            else
            {
                using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
                {
                    zip.CreateEntryFromFile(file.FullName, file.Name, CompressionLevel.Optimal);
                }
            }
        }

        private static void Remove(bool needRemove, FileInfo file)
        {
            if (needRemove && !file.Attributes.HasFlag(FileAttributes.ReadOnly))
            {
                file.Delete();
            }
        }
    }

    class Options
    {
        [Option('s', "Source", Required = true)]
        public string SourceFolder { get; set; }

        [Option('t', "Target")]
        public string TargetFolder { get; set; }

        [Option('r', "Remove", Default = false)]
        public bool RemoveFile { get; set; }

        [Option('d', "SubDirectory", Default = false)]
        public bool SearchInSubDirectories { get; set; }
    }
}
