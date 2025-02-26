namespace HealthChecks.Models
{
    public class RequestHealthcheckForm
    {
        public int HealthID { get; set; }
        public string Alcode { get; set; }
        public string Alchkhos { get; set; }
        public string Alchkdate { get; set; }
        public string Alchkprovid { get; set; }
        public string Licenseno { get; set; }
        public string Chkname { get; set; }
        public string Chkposition { get; set; }
        public string Alpassport { get; set; }
        public string Alcity { get; set; }
        public string Alcountry { get; set; }
        public string Aladdress { get; set; }
        public string Height { get; set; }
        public string Weight { get; set; }
        public string SkinColor { get; set; }
        public string Bloodpressure { get; set; }
        public string Pulse { get; set; }
        public string GeneralHealth { get; set; }

        //public string AlchkstatusID { get; set; }
        //public string Alchkdesc { get; set; }
        //public IFormFile Alchkdoc { get; set; }
    }


    public class RequestHealthcheckResult
    {
        public int HealthID { get; set; }
        public string AlchkstatusID { get; set; }
        public string Alchkdesc { get; set; }
        public IFormFile Alchkdoc { get; set; }
    }
}
