namespace SYSS8.OPF.Clean.WebApi.Auth
{
    public class JwtOptions
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience {  get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
    }
}
