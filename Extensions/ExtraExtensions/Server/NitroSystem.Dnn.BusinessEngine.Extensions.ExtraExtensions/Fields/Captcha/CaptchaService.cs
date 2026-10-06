using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Configuration;
using System.Security.Cryptography;
using Newtonsoft.Json;
using NitroSystem.Dnn.BusinessEngine.Abstractions.Core.Contracts;

namespace NitroSystem.Dnn.BusinessEngine.Extensions.ExtraExtensions.Fields.Captcha
{
    public class CaptchaService
    {
        private readonly ICacheService _cache;
        private readonly string _secretKey;
        private const int ExpirySeconds = 120;
        private const string Chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public CaptchaService(ICacheService cache)
        {
            _cache = cache;

            var configSection = (CaptchaSection)ConfigurationManager.GetSection("captcha");
            _secretKey = configSection != null && string.IsNullOrEmpty(configSection.SecretKey)
                ? configSection.SecretKey
                : "6LeIxAcTAAAAAJcZVRqyHh71UMIEGNQ_MXjiZKhI";
        }

        private class CaptchaSection : ConfigurationSection
        {
            [ConfigurationProperty("secretKey", IsRequired = true)]
            public string SecretKey
            {
                get { return (string)this["secretKey"]; }
                set { this["secretKey"] = value; }
            }
        }

        public CaptchaResult Generate()
        {
            var code = GenerateRandomCode(5);
            var imageBytes = CreateImage(code);
            var token = CreateSignedToken(code);
            return new CaptchaResult
            {
                Token = token,
                ImageBase64 = $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}"
            };
        }

        public bool Verify(string token, string userAnswer)
        {
            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userAnswer))
                return false;

            // verify cache is used
            if (_cache.Get<bool>($"captcha_used_{token}"))
                return false;

            try
            {
                var parts = token.Split('.');
                if (parts.Length != 2) return false;

                var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(parts[0]));
                var signature = parts[1];

                // verify sign
                var expectedSignature = ComputeHmac(parts[0]);
                if (!FixedTimeEquals(signature, expectedSignature))
                    return false;

                var payload = JsonConvert.DeserializeObject<CaptchaPayload>(payloadJson);
                if (payload == null) return false;

                // verify expiration 
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (now > payload.Exp) return false;

                var isValid = string.Equals(
                    payload.Code.Trim(),
                    userAnswer.Trim(),
                    StringComparison.OrdinalIgnoreCase
                );

                if (isValid)
                {
                    // set in cache as used
                    _cache.Set($"captcha_used_{token}", true);
                }
                return isValid;
            }
            catch
            {
                return false;
            }
        }

        #region Private Methods

        private string GenerateRandomCode(int length)
        {
            var bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            var result = new char[length];
            for (int i = 0; i < length; i++)
                result[i] = Chars[bytes[i] % Chars.Length];

            return new string(result);
        }

        private string CreateSignedToken(string code)
        {
            var payload = new CaptchaPayload
            {
                Code = code,
                Exp = DateTimeOffset.UtcNow.AddSeconds(ExpirySeconds).ToUnixTimeSeconds(),
                Nonce = Convert.ToBase64String(GenerateRandomBytes(8))
            };

            var payloadJson = JsonConvert.SerializeObject(payload);
            var payloadBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson));
            var signature = ComputeHmac(payloadBase64);

            return $"{payloadBase64}.{signature}";
        }

        private byte[] GenerateRandomBytes(int length)
        {
            var bytes = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            return bytes;
        }

        private string ComputeHmac(string data)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey)))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// comparison for preventing Timing Attack
        /// </summary>
        private bool FixedTimeEquals(string a, string b)
        {
            if (a.Length != b.Length) return false;

            int result = 0;
            for (int i = 0; i < a.Length; i++)
                result |= a[i] ^ b[i];

            return result == 0;
        }

        /// <summary>
        /// Build captcha image with System.Drawing
        /// </summary>
        private byte[] CreateImage(string code)
        {
            const int width = 180;
            const int height = 60;

            var random = new Random();

            using (var bitmap = new Bitmap(width, height))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                // Quality settings
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Background
                graphics.Clear(Color.WhiteSmoke);

                // Noise lines
                for (int i = 0; i < 6; i++)
                {
                    using (var pen = new Pen(Color.FromArgb(
                        random.Next(100, 200),
                        random.Next(100, 200),
                        random.Next(100, 200)), 1.5f))
                    {
                        graphics.DrawLine(pen,
                            random.Next(width), random.Next(height),
                            random.Next(width), random.Next(height));
                    }
                }

                //Noise points
                for (int i = 0; i < 80; i++)
                {
                    using (var brush = new SolidBrush(Color.Gray))
                    {
                        graphics.FillRectangle(brush,
                            random.Next(width), random.Next(height), 2, 2);
                    }
                }

                // Write characters with curves
                float x = 15;
                foreach (var ch in code)
                {
                    var color = Color.FromArgb(
                        random.Next(0, 100),
                        random.Next(0, 100),
                        random.Next(0, 100));

                    var rotation = (float)((random.NextDouble() - 0.5) * 30); // -15 تا +15 درجه

                    var y = random.Next(5, 20);

                    // Save & Rotate
                    var state = graphics.Save();
                    graphics.TranslateTransform(x + 15, y + 15);
                    graphics.RotateTransform(rotation);
                    graphics.TranslateTransform(-(x + 15), -(y + 15));

                    using (var font = new Font("Arial", 24, FontStyle.Bold))
                    using (var brush = new SolidBrush(color))
                    {
                        graphics.DrawString(ch.ToString(), font, brush, x, y);
                    }

                    graphics.Restore(state);

                    x += 30;
                }

                // Convert to byte
                using (var ms = new MemoryStream())
                {
                    bitmap.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }

        #endregion

        private class CaptchaPayload
        {
            public string Code { get; set; } = "";
            public long Exp { get; set; }
            public string Nonce { get; set; } = "";
        }
    }

    public class CaptchaResult
    {
        public string Token { get; set; } = "";
        public string ImageBase64 { get; set; } = "";
    }
}
