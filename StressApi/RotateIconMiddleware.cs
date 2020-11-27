using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Threading.Tasks;

namespace StressApi.Middleware
{
    public class RotateIconMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IMemoryCache _memoryCache;

        public RotateIconMiddleware(RequestDelegate next, ILogger<RotateIconMiddleware> logger, IMemoryCache memoryCache)
        {
            _next = next;
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path;
            _logger.LogDebug(path);


            if (path == null || !path.HasValue)
            {
                _logger.LogDebug("No Path");
                await _next.Invoke(context);
                return;
            }

            if (!path.ToString().EndsWith("png"))
            {
                _logger.LogDebug("No PNG");
                await _next.Invoke(context);
                return;
            }

            if (context.Request.Query.Count == 0)
            {
                _logger.LogDebug("No Query");
                await _next.Invoke(context);
                return;
            }

            if (!context.Request.Query.ContainsKey("angle") || !int.TryParse(context.Request.Query["angle"], out int angle))
            {
                _logger.LogDebug("No angle");
                await _next.Invoke(context);
                return;
            }

            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", path.Value.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar));
            _logger.LogDebug(imagePath);

            var lastWriteTimeUtc = File.GetLastWriteTimeUtc(imagePath);
            if (lastWriteTimeUtc.Year == 1601) // file doesn't exist, pass to next middleware
            {
                await _next.Invoke(context);
                return;
            }

            var imageData = RotateImage(imagePath, angle, lastWriteTimeUtc);

            context.Response.ContentType = "image/png";
            context.Response.ContentLength = imageData.Length;
            await context.Response.Body.WriteAsync(imageData, 0, (int)imageData.Length);
        }

        private byte[] RotateImage(string imagePath, int angle, DateTime lastWriteTimeUtc)
        {
            long cacheKey;
            unchecked
            {
                cacheKey = imagePath.ToString().GetHashCode() + angle.GetHashCode() + lastWriteTimeUtc.ToBinary();
            }

            if (_memoryCache.TryGetValue(cacheKey, out byte[] imageData))
            {
                _logger.LogDebug("Cached image");
            }
            else
            {

                using (var image = Image.Load(imagePath, out IImageFormat format))
                {
                    int size = image.Height;
                    image.Mutate(x => x.Rotate(angle));
                    image.Mutate(x => x.Pad(size, size));

                    using (var output = new MemoryStream())
                    {
                        image.Save(output, format);
                        imageData = output.ToArray();
                        _memoryCache.Set(cacheKey, imageData);
                    }
                }
            }

            return imageData;
        }
    }

    public static class RotateIconMiddlewareExtensions
    {
        public static IServiceCollection AddRotateIcon(this IServiceCollection services)
        {
            return services.AddMemoryCache();
        }

        public static IApplicationBuilder UseRotateIcon(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RotateIconMiddleware>();
        }
    }
}
    