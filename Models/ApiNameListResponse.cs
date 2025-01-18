namespace HealthChecks.Models
{
    public class ApiNameListResponse
    {
        public string statuscode { get; set; }
        public string statusdesc { get; set; }
        public string empname { get; set; }
        public string wkaddress { get; set; }
        public string reqcode { get; set; }
        public string btname { get; set; }
        public List<Alien> alienlist { get; set; }
    }
    public class Alien
    {
        public string alcode { get; set; }
        public string altype { get; set; }
        public string alprefix { get; set; }
        public string alprefixen { get; set; }
        public string alnameen { get; set; }
        public string alsnameen { get; set; }
        public string albdate { get; set; }
        public string algender { get; set; }
        public string alnation { get; set; }
        public string alposid { get; set; }
    }

}
