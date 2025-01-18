namespace HealthChecks.Models
{
    public class RequestHealthcheck
    {
        public int HealthID { get; set; }
        public string Alcode { get; set; }
        public string Alchkhos { get; set; }
        public string AlchkstatusID { get; set; }
        public string Alchkdate { get; set; }
        public string Alchkprovid { get; set; }
        public string Licenseno { get; set; }
        public string Chkname { get; set; }
        public string Chkposition { get; set; }
        public string Alchkdesc { get; set; }
        public IFormFile Alchkdoc { get; set; }
    }
}
