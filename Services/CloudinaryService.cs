using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using System.Threading.Tasks;

namespace library.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(string cloudName, string apiKey, string apiSecret)
        {
            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true; // usar HTTPS
        }

        public async Task<string?> UploadImageAsync(string localPath, string folder = "usuarios")
        {
            if (string.IsNullOrEmpty(localPath)) return null;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(localPath),
                Folder = folder,
                Transformation = new Transformation().Width(300).Height(300).Crop("fill").Gravity("face")
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            return uploadResult?.SecureUrl?.ToString();
        }
    }
}
