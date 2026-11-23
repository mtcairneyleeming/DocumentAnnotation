using System.Collections.Generic;

namespace DocumentAnnotation
{
    /// <summary>
    /// Bound from the "AppConfig" configuration section (AppConfig__InvitationCode etc. as env vars).
    /// </summary>
    public class Config
    {
        public string ProcessedTexts { get; set; }
        public string InvitationCode { get; set; }
        public List<string> AdminEmails { get; set; }
    }
}
