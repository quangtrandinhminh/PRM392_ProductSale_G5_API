using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Services.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile imageFile, string[] allowedExtensions);
        void DeleteFile(string fileNameWithExtension);
    }

    public class FileService (IWebHostEnvironment environment) : IFileService
    {
        public async Task<string> SaveFileAsync(IFormFile imageFile, string[] allowedExtensions)
        {
            if (imageFile == null)
            {
                throw new ArgumentException(nameof(imageFile));
            }

            var contentPath = environment.ContentRootPath;
            var path = Path.Combine(contentPath, "ProductImages\\");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var fileExtension = Path.GetExtension(imageFile.FileName);
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new ArgumentException($"Only {string.Join(",", allowedExtensions)} are allowed.");
            }

            var fileName = $"{Guid.NewGuid().ToString()} {fileExtension}";
            var filePath = Path.Combine(path, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return fileName;
        }

        public void DeleteFile(string fileNameWithExtension)
        {
            if (string.IsNullOrEmpty(fileNameWithExtension))
            {
                throw new ArgumentException(nameof(fileNameWithExtension));
            }

            var contentPath = environment.ContentRootPath;
            var path = Path.Combine(contentPath, $"ProductImages", fileNameWithExtension);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Invalid file path");
            }
            File.Delete(path);
        }
    }
}
