// Infrastructure/Services/Common/LocalFileStorage.cs
using Application.Interfaces.Common;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Infrastructure.Services.Common
{
    public class LocalFileStorage : IFileStorage
    {
        public async Task<string> SaveAsync(Stream fileStream, string fileName, string folder, string rootPath)
        {
            var uploadsFolder = Path.Combine(rootPath, folder);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var shortName = Path.GetFileNameWithoutExtension(fileName);
            if (shortName.Length > 10)
                shortName = shortName.Substring(0, 10);
            var ext = Path.GetExtension(fileName);
            var newFileName = $"{shortName}_{DateTime.UtcNow:yyMMddHHmmss}{ext}";
            var filePath = Path.Combine(uploadsFolder, newFileName);

            using var fileStreamOut = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(fileStreamOut);

            // return relative URL (để lưu DB)
            return $"/{folder}/{newFileName}";
        }
    }
}
