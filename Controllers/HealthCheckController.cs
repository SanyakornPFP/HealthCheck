using ContainerEvaluationSystem.Helpers;
using HealthCheck.Data;
using HealthCheck.Models;
using HealthChecks.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Reporting.NETCore;
using Newtonsoft.Json;
using QRCoder;
using System.Globalization;
using System.Text;

namespace HealthCheks.Controllers
{
    [Authorize]
    public class HealthCheckController : Controller
    {
        private readonly db_HealthCheckModel _db;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public HealthCheckController(IWebHostEnvironment hostingEnvironment, db_HealthCheckModel db)
        {
            _webHostEnvironment = hostingEnvironment;
            _db = db;
        }

        #region EmpList 
        public IActionResult EmpList(string Search)
        {
            var Model = _db.Employees.Where(e => e.IsActive)
                .Select(e =>
                new
                {
                    EmpID = e.EmpID,
                    EmpName = e.EmpName,
                    Wkaddress = e.Wkaddress,
                    Btname = e.Btname,
                    CountAlien = _db.Alienlists.Where(a => a.EmpID == e.EmpID).Count(),
                })
                .OrderByDescending(s => s.EmpID)
                .ToList();

            if (!string.IsNullOrEmpty(Search))
            {
                Model = Model.Where(s => s.EmpName != null && s.EmpName.Contains(Search)).ToList();
                ViewBag.Search = Search;
            }

            ViewBag.EmpListData = Model;

            return View();
        }

        [HttpPost]
        public IActionResult CreateEmployee(string EmpName, string Wkaddress, string Btname)
        {
            var newEmp = new Employee
            {
                EmpName = EmpName,
                Wkaddress = Wkaddress,
                Btname = Btname,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            _db.Employees.Add(newEmp);
            _db.SaveChanges();
            return Json(new { success = true, message = "Employee created successfully." });
        }


        [HttpPost]
        public IActionResult UpdateEmployee(int EmpID, string EmpName, string Wkaddress, string Btname)
        {
            var existingEmp = _db.Employees.Where(e => e.EmpID == EmpID).FirstOrDefault();
            if (existingEmp == null)
            {
                return Json(new { success = false, message = "Employee not found." });
            }
            existingEmp.EmpName = EmpName;
            existingEmp.Wkaddress = Wkaddress;
            existingEmp.Btname = Btname;
            existingEmp.UpdatedAt = DateTime.Now;
            _db.Employees.Update(existingEmp);
            _db.SaveChanges();
            return Json(new { success = true, message = "Employee updated successfully." });
        }

        [HttpGet]
        public JsonResult ModelGetEmployee(int EmpID)
        {
            var model = _db.Employees
              .Where(e => e.EmpID == EmpID)
              .Select(
              p => new
              {
                  EmpID = p.EmpID,
                  EmpName = p.EmpName,
                  Wkaddress = p.Wkaddress,
                  Btname = p.Btname
              }
              )
            .ToList();
            if (model == null)
            {
                return Json(new { success = false, message = "model not found." });
            }
            return Json(model);
        }

        #endregion EmpList 

        public IActionResult NameList(int EmpID)
        {
            ViewBag.EmpName = _db.Employees.Where(e => e.EmpID == EmpID).Select(e => e.EmpName).FirstOrDefault();
            ViewBag.EmpID = EmpID;
            ViewBag.NameListData = _db.Alienlists
                .Where(e => e.IsActive && e.EmpID == EmpID)
                .Select(
                p => new
                {
                    Alcode = p.Alcode,
                    AlprefixName = p.Alprefix.AlprefixName,
                    AltypeName = p.Altype.AltypeName,
                    Alnameen = p.Alnameen,
                    Alsnameen = p.Alsnameen,
                    Albdate = p.Albdate,
                    AlgenderName = p.Algender.AlgenderName,
                    AlnationName = p.Alnation.AlnationName,
                    AlposName = p.Alpo.AlposName
                })
                .OrderByDescending(p => p.Alcode)
              .ToList();

            ViewBag.Alpo = _db.Alpos.Where(d => d.IsActive).ToList();
            ViewBag.Alprefix = _db.Alprefixes.Where(d => d.IsActive).ToList();
            ViewBag.Altype = _db.Altypes.Where(d => d.IsActive).ToList();
            ViewBag.Alnation = _db.Alnations.Where(d => d.IsActive).ToList();
            ViewBag.Algender = _db.Algenders.Where(d => d.IsActive).ToList();

            return View();
        }

        [HttpPost]
        public IActionResult CreateAlien(int EmpID, string Alcode, string Alpassport, string Alnameen, string Alsnameen, string Albdate, string Alcity, string Alcountry, string Aladdress, string Alpo, string Alprefix, string Altype, string Alnation, string Algender)
        {
            var newAlien = new Alienlist
            {
                EmpID = EmpID,
                Alcode = Alcode,
                Alpassport = Alpassport,
                Alnameen = Alnameen,
                Alsnameen = Alsnameen,
                Albdate = Albdate,
                Alcity = Alcity,
                Alcountry = Alcountry,
                Aladdress = Aladdress,
                AlposID = Alpo,
                AlprefixID = Alprefix,
                AltypeID = Altype,
                AlnationID = Alnation,
                AlgenderID = Algender,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            _db.Alienlists.Add(newAlien);
            _db.SaveChanges();
            return Json(new { success = true, message = "Alien created successfully." });
        }

        [HttpPost]
        public IActionResult UpdateAlien(int EmpID, string Alcode, string Alpassport, string Alnameen, string Alsnameen, string Albdate, string Alcity, string Alcountry, string Aladdress, string Alpo, string Alprefix, string Altype, string Alnation, string Algender)
        {
            var existingAlien = _db.Alienlists.Where(a => a.EmpID == EmpID && a.Alcode == Alcode).FirstOrDefault();
            if (existingAlien == null)
            {
                return Json(new { success = false, message = "Alien not found." });
            }
            existingAlien.Alpassport = Alpassport;
            existingAlien.Alnameen = Alnameen;
            existingAlien.Alsnameen = Alsnameen;
            existingAlien.Albdate = Albdate;
            existingAlien.Alcity = Alcity;
            existingAlien.Alcountry = Alcountry;
            existingAlien.Aladdress = Aladdress;
            existingAlien.AlposID = Alpo;
            existingAlien.AlprefixID = Alprefix;
            existingAlien.AltypeID = Altype;
            existingAlien.AlnationID = Alnation;
            existingAlien.AlgenderID = Algender;
            existingAlien.UpdatedAt = DateTime.Now;
            _db.Alienlists.Update(existingAlien);
            _db.SaveChanges();
            return Json(new { success = true, message = "Alien updated successfully." });
        }

        [HttpGet]
        public JsonResult ModelGetAlien(string Alcode, int EmpID)
        {
            var model = _db.Alienlists
                .Where(a => a.EmpID == EmpID && a.Alcode == Alcode)
                .Select(
                p => new
                {
                    Alcode = p.Alcode,
                    Alpassport = p.Alpassport,
                    Alnameen = p.Alnameen,
                    Alsnameen = p.Alsnameen,
                    Albdate = p.Albdate,
                    Alcity = p.Alcity,
                    Alcountry = p.Alcountry,
                    Aladdress = p.Aladdress,
                    Alpo = p.AlposID,
                    Alprefix = p.AlprefixID,
                    Altype = p.AltypeID,
                    Alnation = p.AlnationID,
                    Algender = p.AlgenderID
                }
                )
                .ToList();
            if (model == null || model.Count == 0)
            {
                return Json(new { success = false, message = "Alien not found." });
            }
            return Json(model);
        }

        public IActionResult HealthList(string searchString)
        {
            ViewBag.Doctor = _db.Doctors.Where(d => d.IsActive).ToList();
            ViewBag.Hospital = _db.Hospitals.Where(d => d.IsActive).ToList();
            ViewBag.Alchkstatus = _db.Alchkstatus.Where(d => d.IsActive).ToList();
            ViewBag.Province = _db.Provinces.Where(d => d.IsActive).ToList();

            if (searchString == null)
            {
                ViewBag.NameListData = _db.Alienlists
                  .Where(e => e.IsActive)
                  .Select(
                  p => new
                  {
                      EmpID = p.EmpID,
                      Alcode = p.Alcode,
                      AlprefixName = p.Alprefix.AlprefixName,
                      AltypeName = p.Altype.AltypeName,
                      Alnameen = p.Alnameen,
                      Alsnameen = p.Alsnameen,
                      Albdate = p.Albdate,
                      AlgenderName = p.Algender.AlgenderName,
                      AlnationName = p.Alnation.AlnationName,
                      AlposName = p.Alpo.AlposName,
                      ResultHealth = _db.Healthchecks.Where(h => h.Alcode == p.Alcode).FirstOrDefault() == null ? 0 : 1,
                      CreatedAt = p.CreatedAt
                  })
                  .OrderByDescending(p => p.CreatedAt)
                .ToList();
            }
            else
            {
                ViewBag.NameListData = _db.Alienlists
              .Where(e => e.IsActive && e.Alcode.Contains(searchString))
              .Select(
              p => new
              {
                  EmpID = p.EmpID,
                  Alcode = p.Alcode,
                  AlprefixName = p.Alprefix.AlprefixName,
                  AltypeName = p.Altype.AltypeName,
                  Alnameen = p.Alnameen,
                  Alsnameen = p.Alsnameen,
                  Albdate = p.Albdate,
                  AlgenderName = p.Algender.AlgenderName,
                  AlnationName = p.Alnation.AlnationName,
                  AlposName = p.Alpo.AlposName,
                  ResultHealth = _db.Healthchecks.Where(h => h.Alcode == p.Alcode).FirstOrDefault() == null ? 0 : 1

              })
            .ToList();
            }

            ViewBag.Search = searchString;

            return View();
        }

        public IActionResult PersonnalHealth(string Alcode)
        {
            var doctor = _db.Doctors.Where(d => d.IsActive).FirstOrDefault();
            ViewBag.Licenseno = doctor.Licenseno;
            ViewBag.Position = doctor.Position;

            ViewBag.Doctor = _db.Doctors.Where(d => d.IsActive).ToList();
            ViewBag.Hospital = _db.Hospitals.Where(d => d.IsActive).ToList();
            ViewBag.Alchkstatus = _db.Alchkstatus.Where(d => d.IsActive).ToList();
            ViewBag.Province = _db.Provinces.Where(d => d.IsActive).ToList();

            var modelAleint = _db.Alienlists.Where(e => e.Alcode == Alcode)
                .Select(e => new
                {
                    Alnameen = e.Alnameen + " " + e.Alsnameen,
                    e.Alpassport,
                    e.Alcountry,
                    e.Alcity,
                    e.Aladdress,
                    e.Alnation.AlnationName,
                    e.Employee.Wkaddress

                }).FirstOrDefault();

            ViewBag.Alcode = Alcode;
            ViewBag.Alnation = modelAleint.AlnationName;
            ViewBag.AlName = modelAleint.Alnameen;
            ViewBag.AlPassport = modelAleint.Alpassport;
            ViewBag.AlCountry = modelAleint.Alcountry != null ? modelAleint.Alcountry : "พม่า";
            ViewBag.AlCity = modelAleint.Alcity;
            ViewBag.AlAddress = modelAleint.Aladdress != null ? modelAleint.Aladdress : modelAleint.Wkaddress;

            ViewBag.HealthchecksPerson = _db.Healthchecks.Where(e => e.Alcode == Alcode && e.IsActive)
                .Select(s => new
                {
                    s.HealthID,
                    s.Alchkdate,
                    AlchkstatusName = s.Alchkstatu.AlchkstatusName ?? "รอลงผลตรวจ",
                    s.Alchkhos
                })
                .OrderByDescending(e => e.HealthID)
                .ToList();

            return View();
        }

        [HttpGet]
        public JsonResult ModelAlcode(string Alcode)
        {
            var model = _db.Alienlists
              .Where(e => e.Alcode == Alcode)
              .Select(
              p => new
              {
                  EmpID = p.EmpID,
                  EmpName = p.Employee.EmpName,
                  Alcode = p.Alcode,
                  AlprefixName = p.Alprefix.AlprefixName,
                  AltypeName = p.Altype.AltypeName,
                  Alnameen = p.Alnameen,
                  Alsnameen = p.Alsnameen,
                  Albdate = p.Albdate,
                  AlgenderName = p.Algender.AlgenderName,
                  AlnationName = p.Alnation.AlnationName,
                  AlposName = p.Alpo.AlposName
              }
              )
            .FirstOrDefault();

            if (model == null)
            {
                return Json(new { success = false, message = "model not found." });
            }

            return Json(model);
        }

        [HttpGet]
        public JsonResult ModelHealthDetail(int HealthID)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var model = _db.Healthchecks
              .Where(e => e.HealthID == HealthID).Select(
                h => new
                {
                    EmpName = h.Alienlist.Employee.EmpName,
                    Alcode = h.Alcode,
                    Alcountry = h.Alienlist.Alcountry,
                    Aladdress = h.Alienlist.Aladdress,
                    Alpassport = h.Alienlist.Alpassport,
                    Alcity = h.Alienlist.Alcity,
                    Alchkhos = h.Alchkhos,
                    AlchkstatusID = h.AlchkstatusID,
                    AlchkstatusName = h.Alchkstatu.AlchkstatusName,
                    Alchkdate = h.Alchkdate,
                    Alchkprovid = h.Alchkprovid,
                    AlchkprovidName = h.Province.ProvinceName,
                    Licenseno = h.Licenseno,
                    ChkName = h.Chkname,
                    Chkposition = h.Chkposition,
                    Alchkdesc = h.Alchkdesc,
                    Alchkdoc = h.Alchkdoc != null ? $"{baseUrl}/{h.Alchkdoc}" : null,
                    h.Height,
                    h.Weight,
                    h.SkinColor,
                    h.Bloodpressure,
                    h.Pulse,
                    h.GeneralHealth,
                    UserName = h.User.FirstName + h.User.LastName
                })
            .FirstOrDefault();

            if (model == null)
            {
                return Json(new { success = false, message = "model not found." });
            }

            return Json(model);
        }

        [HttpGet]
        public JsonResult ModelDoctor(string DoctorName)
        {
            var model = _db.Doctors
              .Where(e => e.DoctorName == DoctorName)
            .FirstOrDefault();

            if (model == null)
            {
                return Json(new { success = false, message = "model not found." });
            }

            return Json(model);
        }

        [HttpGet]
        public JsonResult ModelHospital(string HospitalName)
        {
            var model = _db.Hospitals
              .Where(e => e.HospitalName == HospitalName)
            .FirstOrDefault();

            if (model == null)
            {
                return Json(new { success = false, message = "model not found." });
            }

            return Json(model);
        }

        [HttpGet]
        public JsonResult CheckHealthDate(string Alcode, string Alchkdate)
        {
            // แปลงวันที่จากรูปแบบ "วันที่-เดือน-ปี" เป็น DateTime
            DateTime parsedDate;
            if (!DateTime.TryParseExact(Alchkdate, "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out parsedDate))
            {
                return Json(new { success = false, message = "Invalid date format." });
            }

            // แปลง DateTime กลับเป็นสตริงในรูปแบบ "yyyy-MM-dd" เพื่อใช้ในการเปรียบเทียบในฐานข้อมูล
            string formattedDate = parsedDate.ToString("dd-MM-yyyy");

            var model = _db.Healthchecks
                .Where(e => e.Alcode == Alcode && e.Alchkdate == formattedDate)
                .FirstOrDefault();

            if (model == null)
            {
                return Json(new { success = false });
            }

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> SaveHealthData(RequestHealthcheckResult model)
        {
            try
            {
                var existingHealth = _db.Healthchecks.Where(h => h.HealthID == model.HealthID).FirstOrDefault();

                string filePath = null;
                string relativeFilePath = null;
                if (model.Alchkdoc != null && model.Alchkdoc.Length > 0)
                {
                    var fileName = "HC" + DateTime.ParseExact(existingHealth.Alchkdate, "dd-MM-yyyy", null).ToString("yyyyMMdd") + existingHealth.Alcode + Path.GetExtension(model.Alchkdoc.FileName);
                    var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "healthDoc");
                    filePath = Path.Combine(uploads, fileName);
                    relativeFilePath = Path.Combine("uploads", "healthDoc", fileName);

                    // Delete existing file if it exists
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Alchkdoc.CopyToAsync(fileStream);
                    }
                }

                existingHealth.AlchkstatusID = model.AlchkstatusID;
                existingHealth.Alchkdesc = model.Alchkdesc;
                existingHealth.UserIDManage = int.Parse(User.GetLoggedInUserID());
                if (relativeFilePath != null)
                {
                    existingHealth.Alchkdoc = relativeFilePath.Replace("\\", "/");
                }
                existingHealth.UpdatedAt = DateTime.Now;

                _db.Healthchecks.Update(existingHealth);

                // Log update action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Update Healthcheck",
                    TableName = "Healthchecks",
                    RecordID = existingHealth.Alcode,
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Updated healthcheck for Alcode {existingHealth.Alcode}"
                });

                await _db.SaveChangesAsync();

                var requestUpload = new HealthCheckRequest
                {
                    HealthID = existingHealth.HealthID
                };

                await UploadData(requestUpload);

                return Json(new { success = true, message = "Data saved successfully." });
            }
            catch (Exception ex)
            {
                // Return an error response with detailed message
                return Json(new { success = false, message = $"An error occurred while saving data: {ex.Message}" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateFormHealthCheck(RequestHealthcheckForm model)
        {
            try
            {
                var existingHealth = _db.Healthchecks.Where(h => h.Alcode == model.Alcode && h.Alchkdate == model.Alchkdate && h.IsActive == true).FirstOrDefault();
                var modelAleint = _db.Alienlists.Where(e => e.Alcode == model.Alcode).FirstOrDefault();

                if (modelAleint.Alpassport != model.Alpassport || modelAleint.Alcity != model.Alcity || modelAleint.Alcountry != model.Alcountry || modelAleint.Aladdress != model.Aladdress)
                {
                    modelAleint.Alpassport = model.Alpassport;
                    modelAleint.Alcity = model.Alcity;
                    modelAleint.Alcountry = model.Alcountry;
                    modelAleint.Aladdress = model.Aladdress;
                    _db.Alienlists.Update(modelAleint);
                    _db.SaveChanges();
                }

                var modeDoctor = _db.Doctors.FirstOrDefault(d => d.DoctorName.ToString() == model.Chkname);
                if (modeDoctor == null)
                {
                    return Json(new { success = false, message = "Doctor not found." });
                }

                if (existingHealth != null)
                {
                    // Update existing record
                    existingHealth.Alchkhos = model.Alchkhos;
                    existingHealth.Alchkdate = model.Alchkdate;
                    existingHealth.Alchkprovid = model.Alchkprovid;
                    existingHealth.Height = model.Height;
                    existingHealth.Weight = model.Weight;
                    existingHealth.SkinColor = model.SkinColor;
                    existingHealth.Bloodpressure = model.Bloodpressure;
                    existingHealth.Pulse = model.Pulse;
                    existingHealth.GeneralHealth = model.GeneralHealth;
                    existingHealth.Licenseno = model.Licenseno;
                    existingHealth.Chkname = model.Chkname;
                    existingHealth.Chkposition = modeDoctor.Position;
                    existingHealth.UserIDManage = int.Parse(User.GetLoggedInUserID());
                    existingHealth.UpdatedAt = DateTime.Now;

                    _db.Healthchecks.Update(existingHealth);

                    // Log update action
                    _db.LogSystems.Add(new LogSystem
                    {
                        UserID = int.Parse(User.GetLoggedInUserID()),
                        ActionTitle = "Update Healthcheck",
                        TableName = "Healthchecks",
                        RecordID = existingHealth.Alcode,
                        TimeStamp = DateTime.UtcNow,
                        Detail = $"Updated healthcheck for Alcode {existingHealth.Alcode}"
                    });
                }
                else
                {
                    var newHealth = new Healthcheck
                    {
                        Alcode = model.Alcode,
                        Alchkhos = model.Alchkhos,
                        Alchkdate = model.Alchkdate,
                        Alchkprovid = model.Alchkprovid,
                        Licenseno = model.Licenseno,
                        Chkname = modeDoctor.DoctorName,
                        Chkposition = modeDoctor.Position,
                        Height = model.Height,
                        Weight = model.Weight,
                        SkinColor = model.SkinColor,
                        Bloodpressure = model.Bloodpressure,
                        Pulse = model.Pulse,
                        GeneralHealth = model.GeneralHealth,
                        CreatedAt = DateTime.Now,
                        UserIDManage = int.Parse(User.GetLoggedInUserID()),
                        IsActive = true
                    };
                    _db.Healthchecks.Add(newHealth);

                    // Log create action
                    _db.LogSystems.Add(new LogSystem
                    {
                        UserID = int.Parse(User.GetLoggedInUserID()),
                        ActionTitle = "Create Healthcheck",
                        TableName = "Healthchecks",
                        RecordID = newHealth.Alcode,
                        TimeStamp = DateTime.UtcNow,
                        Detail = $"Created new healthcheck for Alcode {newHealth.Alcode}"
                    });
                }

                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Data saved successfully." });
            }
            catch (Exception ex)
            {
                // Return an error response with detailed message
                return Json(new { success = false, message = $"An error occurred while saving data: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadData(HealthCheckRequest request)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            // Prepare the data to send to the external API
            var healthcheck = _db.Healthchecks.Where(h => h.HealthID == request.HealthID).FirstOrDefault();
            var requestData = new
            {
                token = "73f316563c767a9ca076d91f9c5cced5",
                alcode = healthcheck.Alcode,
                alchkhos = healthcheck.Alchkhos,
                alchkstatus = healthcheck.AlchkstatusID,
                alchkdate = healthcheck.Alchkdate,
                alchkprovid = healthcheck.Alchkprovid,
                licenseno = "ว." + healthcheck.Licenseno,
                chkname = healthcheck.Chkname,
                chkposition = healthcheck.Chkposition,
                alchkdesc = healthcheck.Alchkdesc,
                alchkdoc = healthcheck.Alchkdoc != null ? $"{baseUrl}/{healthcheck.Alchkdoc}" : null,
            };

            using (var httpClient = new HttpClient())
            {
                // Log the request details
                var jsonContent = JsonConvert.SerializeObject(requestData);
                System.Diagnostics.Debug.WriteLine($"Request to external API: {jsonContent}");

                // Construct the request body
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var url = "https://apicenter2024.doe.go.th/dataapi/alienhealth/update/";

                // Create the POST request with the JSON content
                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = content
                };

                // Send the POST request
                var response = await httpClient.SendAsync(requestMessage);
                var responseData = await response.Content.ReadAsStringAsync();

                // Log the response details
                System.Diagnostics.Debug.WriteLine($"Response from external API: {responseData}");

                // Ensure the response is valid JSON
                if (response.IsSuccessStatusCode)
                {
                    var dateDefault = _db.HistoryHealths.Where(s => s.Alcode == healthcheck.Alcode && s.Alchkdate == healthcheck.Alchkdate).FirstOrDefault();

                    if (dateDefault == null)
                        _db.HistoryHealths.Add(new HistoryHealth
                        {
                            Alcode = healthcheck.Alcode,
                            Alnameen = _db.Alienlists.Where(s => s.Alcode == healthcheck.Alcode).Select(a => a.Alnameen).FirstOrDefault(),
                            Alchkdate = healthcheck.Alchkdate,
                            UploadDate = DateTime.Now,
                            UserIDManage = int.Parse(User.GetLoggedInUserID())
                        }
                        );

                    _db.SaveChanges();

                    var jsonResponse = JsonConvert.DeserializeObject<HealthCheckResponse>(responseData);
                    if (jsonResponse.statuscode != "1")
                    {
                        return Json(new { success = false, message = jsonResponse.statusdesc, statuscode = jsonResponse.statuscode, statusdesc = jsonResponse.statusdesc, healthrequest = jsonContent });
                    }
                    return Json(new { success = true, message = "Data uploaded successfully.", statuscode = jsonResponse.statuscode, statusdesc = jsonResponse.statusdesc, healthrequest = jsonContent });
                }
                else
                {
                    // Return the error details
                    return Json(new
                    {
                        success = false,
                        message = "Failed to upload data.",
                        statusCode = response.StatusCode,
                        reasonPhrase = response.ReasonPhrase,
                        responseData = responseData
                    });
                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> DeleteHealthData(int HealthID)
        {
            try
            {
                var healthcheck = _db.Healthchecks.Where(h => h.HealthID == HealthID).FirstOrDefault();
                if (healthcheck == null)
                {
                    return Json(new { success = false, message = "Healthcheck not found." });
                }
                healthcheck.IsActive = false;
                healthcheck.UpdatedAt = DateTime.Now;
                _db.Healthchecks.Update(healthcheck);
                // Log delete action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Delete Healthcheck",
                    TableName = "Healthchecks",
                    RecordID = healthcheck.Alcode,
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Deleted healthcheck for Alcode {healthcheck.Alcode}"
                });
                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Data deleted successfully." });
            }
            catch (Exception ex)
            {
                // Return an error response with detailed message
                return Json(new { success = false, message = $"An error occurred while deleting data: {ex.Message}" });
            }
        }


        #region Report V.1
        [HttpGet]
        public IActionResult ExaminationForm(int HealthID)
        {
            var model = _db.Healthchecks
              .Where(e => e.HealthID == HealthID).Select(
                h => new
                {
                    EmpName = h.Alienlist.Employee.EmpName ?? "-",
                    EmpAddress = h.Alienlist.Employee.Wkaddress ?? "-",
                    EmpBtname = h.Alienlist.Employee.Btname ?? "-",
                    Alcode = h.Alcode ?? "-",
                    Albdate = h.Alienlist.Albdate ?? "-",
                    Aladdress = h.Alienlist.Aladdress ?? "-",
                    Alpassport = h.Alienlist.Alpassport ?? "-",
                    Alcity = h.Alienlist.Alcity ?? "-",
                    Alcountry = h.Alienlist.Alcountry ?? "-",
                    Algender = h.Alienlist.Algender.AlgenderName ?? "-",
                    Alsname = h.Alienlist.Alnameen + " " + h.Alienlist.Alsnameen,
                    Alnation = h.Alienlist.Alnation.AlnationName ?? "-",
                    Alpos = h.Alienlist.Alpo.AlposName ?? "-",
                    Hospital = h.Alchkhos ?? "-",
                    AlchkstatusID = h.AlchkstatusID ?? "-",
                    AlchkstatusName = h.Alchkstatu.AlchkstatusName ?? "-",
                    Alchkdate = h.Alchkdate ?? "-",
                    Alchkprovid = h.Alchkprovid ?? "-",
                    AlchkprovidName = h.Province.ProvinceName ?? "-",
                    Licenseno = h.Licenseno ?? "-",
                    Chkname = h.Chkname ?? "-",
                    Chkposition = h.Chkposition ?? "-",
                    Alchkdesc = h.Alchkdesc ?? "-",
                    Alchkdoc = h.Alchkdoc ?? "-",
                    Height = h.Height ?? "-",
                    Weight = h.Weight ?? "-",
                    SkinColor = h.SkinColor ?? "-",
                    Bloodpressure = h.Bloodpressure ?? "-",
                    Pulse = h.Pulse ?? "-",
                    GeneralHealth = h.GeneralHealth ?? "-"
                })
            .FirstOrDefault();


            string qrCodeDoc = "uploads/healthDoc/" + "HC" + DateTime.ParseExact(model.Alchkdate, "dd-MM-yyyy", null).ToString("yyyyMMdd") + model.Alcode + ".pdf";
            string HospitalAddress = _db.Hospitals.Where(h => h.HospitalName == model.Hospital).Select(h => h.HospitalAddress).FirstOrDefault();
            string hostname = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            string qrCodeUrl = $"{hostname}/{qrCodeDoc}";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeUrl, QRCodeGenerator.ECCLevel.Q);

            // ใช้ PngByteQRCode แทน QRCode
            PngByteQRCode pngByteQRCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeImage = pngByteQRCode.GetGraphic(20);

            // แปลงเป็น Base64
            string base64Image = Convert.ToBase64String(qrCodeImage);

            string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "qr-code.png");
            System.IO.File.WriteAllBytes(filePath, qrCodeImage);

            string renderFormat = "PDF";
            string mimetype = "application/pdf";
            using var report = new LocalReport();
            report.ReportPath = $"{this._webHostEnvironment.WebRootPath}\\Report\\MC-01.rdlc";
            report.EnableExternalImages = true;

            // แปลงวันที่จากสตริงเป็น DateTime
            DateTime albdate;
            DateTime alchkdate;
            if (!DateTime.TryParseExact(model.Albdate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out albdate))
            {
                // จัดการข้อผิดพลาดเมื่อแปลงวันที่ไม่สำเร็จ
                throw new FormatException($"String '{model.Albdate}' was not recognized as a valid DateTime.");
            }
            if (!DateTime.TryParseExact(model.Alchkdate, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out alchkdate))
            {
                // จัดการข้อผิดพลาดเมื่อแปลงวันที่ไม่สำเร็จ
                throw new FormatException($"String '{model.Alchkdate}' was not recognized as a valid DateTime.");
            }

            string formattedAlbdate = albdate.ToString("dd/MM/yyyy");
            string formattedDate = ToThaiDate(alchkdate);

            // Create individual parameters for each digit of Alcode
            var alcodeParameters = new List<ReportParameter>();
            for (int i = 0; i < model.Alcode.Length; i++)
            {
                alcodeParameters.Add(new ReportParameter($"Alcode{i + 1}", model.Alcode[i].ToString()));
            }

            ReportParameter[] parameters = new ReportParameter[]
            {
                new ReportParameter("QRCode", new Uri(filePath).AbsoluteUri),
                new ReportParameter("Healthdate", formattedDate),
                //// ส่วนที่ 1
                new ReportParameter("Alsname", model.Alsname),
                new ReportParameter("Algender", "เพศ  " + model.Algender),
                new ReportParameter("Alpassport", "เลขที่ Passport  " + model.Alpassport),
                new ReportParameter("Albdate", "วัน/เดือน/ปี เกิด  " + formattedAlbdate),
                new ReportParameter("Alcity", "เมืองเกิด  " + model.Alcity),
                new ReportParameter("Alcountry", "ประเทศ  " + model.Alcountry),
                new ReportParameter("Alnation", "สัญชาติ  " + model.Alnation),
                new ReportParameter("Alpos", "อาชีพ  " + model.Alpos),
                new ReportParameter("Aladdress", model.Aladdress),
                //// ส่วนที่ 2
                new ReportParameter("Employee", model.EmpName),
                new ReportParameter("EmpBtname", model.EmpBtname),
                new ReportParameter("Empaddress", model.EmpAddress),
                //// ส่วนที่ 3
                new ReportParameter("Chkname", model.Chkname),
                new ReportParameter("Licenseno", model.Licenseno),
                new ReportParameter("Hospital", model.Hospital),
                new ReportParameter("HospitalAddress", HospitalAddress),
                //// ผลตรวจสุขภาพ
                new ReportParameter("HealthInfo", "ส่วนสูง " + model.Height + " ซม. "  + " น้ำหนัก " + model.Weight + " ก.ก " + " สีผิว " + model.SkinColor + " " + " ความดันโลหิต " + model.Bloodpressure + " มม.ปรอท " + " ชีพจร " + model.Pulse + " ครั้ง/นาที " ),
                new ReportParameter("GeneralHealth", model.GeneralHealth),
            }.Concat(alcodeParameters).ToArray();

            report.SetParameters(parameters);

            string deviceInfo = @"<DeviceInfo>
                            <OutputFormat>PDF</OutputFormat>
                            <StartPage>1</StartPage>
                            <EndPage>1</EndPage>
                          </DeviceInfo>";

            var pdf = report.Render(format: renderFormat, deviceInfo: deviceInfo);

            var contentDisposition = new System.Net.Mime.ContentDisposition
            {
                FileName = model.Alcode + ".pdf",
                Inline = true // false = prompt the user for downloading; true = browser to try to show the content inline
            };

            return new FileContentResult(pdf, mimetype);
        }

        public static string ToThaiDate(DateTime date)
        {
            var thaiCulture = new System.Globalization.CultureInfo("th-TH");
            var thaiYear = date.Year + 543; // Convert to Buddhist calendar year
            var thaiMonth = date.ToString("MMMM", thaiCulture); // Get month name in Thai
            var thaiDay = date.Day;

            return $"วันที่ {thaiDay} เดือน {thaiMonth} พ.ศ. {thaiYear}";
        }
        #endregion
    }
}
