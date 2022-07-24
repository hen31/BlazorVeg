using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Veg.Entities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using Veg.Repositories;
using Microsoft.AspNetCore.Authorization;
using IdentityModel;

namespace Veg.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        public ImagesController(MemberRepository repository)
        {
            Repository = repository;
        }

        public string ImageDirectory
        {
            get
            {
                return Startup.WebRootPath + @"\imagestore\";
            }
        }

        public MemberRepository Repository { get; }

        [HttpPost("")]
        [TypeFilter(typeof(OnlyMemberAttribute))]
        public virtual async Task<ActionResult<string>> PostImageAsync([FromBody] Veg.Entities.Image image)
        {
            var memberUploading = await (Repository as MemberRepository).FindMemberWithEmailAdressAsync(HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email).Value);
            var memberUploadingId = memberUploading.ID.ToString("D");
            string fileName = CreateImagesOfDifferentSizes(image, memberUploadingId, ImageDirectory);

            //await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

            return Ok(fileName);
        }

        public static string CreateImagesOfDifferentSizes(Entities.Image image, string memberUploadingId, string imageDirectory)
        {
            byte[] imageBytes = Convert.FromBase64String(image.Data);

            var fileName = GenerateRandomString(16) + memberUploadingId + ".png";
            var filePath = Path.Combine(imageDirectory, "256", fileName);
            while (System.IO.File.Exists(filePath))
            {
                fileName = GenerateRandomString(16) + memberUploadingId + ".png";
                filePath = Path.Combine(imageDirectory, fileName);
            }
            CreateImage(imageBytes, Path.Combine(imageDirectory, "Original", fileName));

            CreateImageOfSizeAtLocation(imageBytes, Path.Combine(imageDirectory, "512", fileName), 512f, 512f);
            CreateImageOfSizeAtLocation(imageBytes, Path.Combine(imageDirectory, "256", fileName), 256f, 256f);
            CreateImageOfSizeAtLocation(imageBytes, Path.Combine(imageDirectory, "128", fileName), 128f, 128f);
            CreateImageOfSizeAtLocation(imageBytes, Path.Combine(imageDirectory, "64", fileName), 64f, 64f);
            return fileName;
        }

        private static void CreateImage(byte[] imageBytes, string filePath)
        {
            using (Image<Rgba32> imageToEdit = SixLabors.ImageSharp.Image.Load(imageBytes))
            {
                imageToEdit.Save(filePath, new PngEncoder()); // Automatic encoder selected based on extension.
            }
        }

        private static void CreateImageOfSizeAtLocation(byte[] imageBytes, string filePath, float maxWidth, float maxHeight)
        {
            using (Image<Rgba32> imageToEdit = SixLabors.ImageSharp.Image.Load(imageBytes))
            {
                // Get the image's original width and height
                int originalWidth = imageToEdit.Width;
                int originalHeight = imageToEdit.Height;

                // To preserve the aspect ratio
                float ratioX = maxWidth / (float)originalWidth;
                float ratioY = maxHeight / (float)originalHeight;
                float ratio = Math.Min(ratioX, ratioY);

                float sourceRatio = (float)originalWidth / originalHeight;

                // New width and height based on aspect ratio
                int newWidth = (int)(originalWidth * ratio);
                int newHeight = (int)(originalHeight * ratio);

                // Convert byte[] to Image6
                imageToEdit.Mutate(x => x.Resize(newWidth, newHeight));
                imageToEdit.Save(filePath, new PngEncoder()); // Automatic encoder selected based on extension.
            }
        }

        private static string GenerateRandomString(int numberOfChars)
        {
            string allowedChars = "qwertyuiopasdfghjklzxcvbnm1234567890";
            Random random = new Random();
            string randomString = "";
            for (int i = 0; i < numberOfChars; i++)
            {
                randomString += allowedChars[random.Next(0, allowedChars.Length - 1)];
            }
            return randomString;
        }

        [HttpPost("{memberId}")]
        [TypeFilter(typeof(OnlyAdminOrCustomRightCheckAttribute), Arguments = new object[] { typeof(MemberRepository), "memberId", "", false, true })]
        public virtual async Task<ActionResult> GetMeAsync(Guid memberId, [FromBody] Veg.Entities.Image image)
        {
            //waait (_repository as MemberRepository).FindMemberWithEmailAdressAsync(HttpContext.User.Claims.ToList().Find(r => r.Type == JwtClaimTypes.Email).Value)
            byte[] imageBytes = Convert.FromBase64String(image.Data);
            var filePath = Path.Combine(ImageDirectory, memberId.ToString() + ".png");

            using (Image<Rgba32> imageToEdit = SixLabors.ImageSharp.Image.Load(imageBytes))
            {
                float ratio = imageToEdit.Height / imageToEdit.Width;

                int width = (int)(360f * ratio);

                // Convert byte[] to Image6
                imageToEdit.Mutate(x => x.Resize(width, 360));
                imageToEdit.Save(filePath, new PngEncoder()); // Automatic encoder selected based on extension.
            }

            await Repository.SetHasImage(memberId);

            //await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

            return Ok();
        }

        [HttpPost("remove/{memberId}")]
        [TypeFilter(typeof(OnlyAdminOrCustomRightCheckAttribute), Arguments = new object[] { typeof(MemberRepository), "memberId", "", false, true })]
        public virtual async Task<ActionResult> DeleteMeAsync(Guid memberId)
        {
            var filePath = Path.Combine(ImageDirectory, memberId.ToString() + ".png");

            System.IO.File.Delete(filePath);
            await Repository.SetHasNoImage(memberId);

            return Ok();
        }
    }
}