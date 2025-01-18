using HealthCheck.Models;

namespace HealthChecks.Models
{
    public class NamListResponse
    {
        public string Empname { get; set; }
        public string Wkaddress { get; set; }
        public string Btname { get; set; }
        public string Reqcode { get; set; }
        public List<AlienlistResponse> Alienlist { get; set; }
    }
    public class AlienlistResponse
    {
        public string Alcode { get; set; }
        public string Altype { get; set; }
        public string Alprefix { get; set; }
        public string Alprefixen { get; set; }
        public string Alnameen { get; set; }
        public string Alsnameen { get; set; }
        public string Albdate { get; set; }
        public string Algender { get; set; }
        public string Alnation { get; set; }
        public string Alposid { get; set; }
    }
}
