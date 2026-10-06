namespace NitroSystem.Dnn.BusinessEngine.Extensions.ExtraExtensions.Fields.Captcha
{
    public class CaptchaVerifyRequest
    {
        public string Token { get; set; } = "";
        public string Answer { get; set; } = "";
    }
}
