using ContainerEvaluationSystem.Helpers;
using HealthCheck.Data;
using HealthCheck.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace HealthChecks.Controllers
{
    [Authorize]
    public class DefualtConfigController : Controller
    {
        private readonly db_HealthCheckModel _db;

        public DefualtConfigController(db_HealthCheckModel db)
        {
            _db = db;
        }

        #region Doctor CRUD
        public IActionResult Doctor()
        {
            ViewBag.Doctor = _db.Doctors.Where(d => d.IsActive).ToList();
            return View();
        }

        [HttpGet]
        public JsonResult ModelDoctor(int DoctorID)
        {
            var model = _db.Doctors
              .Where(e => e.DoctorID == DoctorID)
            .FirstOrDefault();

            if (model == null)
            {
                return Json(new { success = false, message = "model not found." });
            }

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDoctor(string DoctorName, string Position, string Licenseno)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("CreateDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to update." });
            }

            var model = _db.Doctors.FirstOrDefault(u => u.DoctorName == DoctorName && u.IsActive);

            if (model == null)
            {
                var newDoctor = new Doctor
                {
                    DoctorName = DoctorName,
                    Position = Position,
                    Licenseno = Licenseno,
                    IsActive = true
                };
                _db.Doctors.Add(newDoctor);
                await _db.SaveChangesAsync();

                // Log create action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Create Doctor",
                    TableName = "Doctors",
                    RecordID = newDoctor.DoctorID.ToString(),
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Created new doctor with name {newDoctor.DoctorName}"
                });
            }
            else
            {
                return Json(new { success = false, message = "Doctor Duplicate found." });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Doctor Create successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDoctor(int DoctorID, string DoctorName, string Position, string Licenseno)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("UpdateDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to update." });
            }

            var model = _db.Doctors.FirstOrDefault(u => u.DoctorID == DoctorID && u.IsActive);

            if (model == null)
            {
                return Json(new { success = false, message = "Doctor Not found." });
            }
            else
            {
                model.DoctorName = DoctorName;
                model.Position = Position;
                model.Licenseno = Licenseno;
                model.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                // Log update action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Update Doctor",
                    TableName = "Doctors",
                    RecordID = model.DoctorID.ToString(),
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Updated doctor with name {model.DoctorName}"
                });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Doctor Update successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDoctor(int DoctorID)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("DeleteDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to update." });
            }

            var model = _db.Doctors.FirstOrDefault(u => u.DoctorID == DoctorID && u.IsActive);

            if (model == null)
            {
                return Json(new { success = false, message = "Doctor Not found." });
            }
            else
            {
                model.IsActive = false;
                model.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                // Log delete action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Delete Doctor",
                    TableName = "Doctors",
                    RecordID = model.DoctorID.ToString(),
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Deleted doctor with name {model.DoctorName}"
                });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Doctor Update successfully." });
        }
        #endregion

        #region Hospital CRUD
        public IActionResult Hospital()
        {
            ViewBag.Hospital = _db.Hospitals.Where(h => h.IsActive).ToList();
            return View();
        }

        [HttpGet]
        public JsonResult ModelHospital(int HospitalID)
        {
            var model = _db.Hospitals
              .Where(e => e.HospitalID == HospitalID)
            .FirstOrDefault();

            if (model == null)
            {
                return Json(new { success = false, message = "model not found." });
            }

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateHospital(string HospitalName , string HospitalAddress)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("CreateDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to update." });
            }

            var model = _db.Hospitals.FirstOrDefault(u => u.HospitalName == HospitalName && u.IsActive);

            if (model == null)
            {
                var newHospital = new Hospital
                {
                    HospitalName = HospitalName,
                    HospitalAddress = HospitalAddress,
                    IsActive = true
                };
                _db.Hospitals.Add(newHospital);
                await _db.SaveChangesAsync();

                // Log create action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Create Hospital",
                    TableName = "Hospitals",
                    RecordID = newHospital.HospitalID.ToString(),
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Created new hospital with name {newHospital.HospitalName} "
                });
            }
            else
            {
                return Json(new { success = false, message = "Hospital Duplicate found." });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Hospital Create successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateHospital(int HospitalID, string HospitalName, string HospitalAddress)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("UpdateDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to update." });
            }

            var model = _db.Hospitals.FirstOrDefault(u => u.HospitalID == HospitalID && u.IsActive);

            if (model == null)
            {
                return Json(new { success = false, message = "Hospital Not found." });
            }
            else
            {
                model.HospitalName = HospitalName;
                model.HospitalAddress = HospitalAddress;
                model.UpdateAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                // Log update action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Update Hospital",
                    TableName = "Hospitals",
                    RecordID = model.HospitalID.ToString(),
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Updated hospital with name {model.HospitalName}"
                });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Hospital Update successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteHospital(int HospitalID)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("DeleteDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to update." });
            }

            var model = _db.Hospitals.FirstOrDefault(u => u.HospitalID == HospitalID && u.IsActive);

            if (model == null)
            {
                return Json(new { success = false, message = "Hospital Not found." });
            }
            else
            {
                model.IsActive = false;
                model.UpdateAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                // Log delete action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Delete Hospital",
                    TableName = "Hospitals",
                    RecordID = model.HospitalID.ToString(),
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Deleted hospital with name {model.HospitalName}"
                });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Hospital Update successfully." });
        }
        #endregion

        #region Alchkstatu CRUD ผลตรวจสุขภาพ
        public IActionResult Alchkstatu()
        {
            ViewBag.Alchkstatu = _db.Alchkstatus.Where(a => a.IsActive).ToList();
            return View();
        }

        [HttpGet]
        public JsonResult ModelAlchkstatu(string AlchkstatusID)
        {
            var model = _db.Alchkstatus
                .Where(e => e.AlchkstatusID == AlchkstatusID)
                .FirstOrDefault();

            if (model == null)
            {
                return Json(new { success = false, message = "Model not found." });
            }

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAlchkstatu(string AlchkstatusName)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("CreateDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to create." });
            }

            var model = _db.Alchkstatus.FirstOrDefault(u => u.AlchkstatusName == AlchkstatusName && u.IsActive);

            if (model == null)
            {
                var newAlchkstatu = new Alchkstatu
                {
                    AlchkstatusName = AlchkstatusName,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                _db.Alchkstatus.Add(newAlchkstatu);
                await _db.SaveChangesAsync();

                // Log create action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Create Alchkstatu",
                    TableName = "Alchkstatus",
                    RecordID = newAlchkstatu.AlchkstatusID,
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Created new Alchkstatu with name {newAlchkstatu.AlchkstatusName}"
                });
            }
            else
            {
                return Json(new { success = false, message = "Duplicate Alchkstatu found." });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Alchkstatu created successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAlchkstatu(string AlchkstatusID, string AlchkstatusName)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("UpdateDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to update." });
            }

            var model = _db.Alchkstatus.FirstOrDefault(u => u.AlchkstatusID == AlchkstatusID && u.IsActive);

            if (model == null)
            {
                return Json(new { success = false, message = "Alchkstatu not found." });
            }
            else
            {
                model.AlchkstatusName = AlchkstatusName;
                model.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();

                // Log update action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Update Alchkstatu",
                    TableName = "Alchkstatus",
                    RecordID = model.AlchkstatusID,
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Updated Alchkstatu with name {model.AlchkstatusName}"
                });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Alchkstatu updated successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAlchkstatu(string AlchkstatusID)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("DeleteDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to delete." });
            }

            var model = _db.Alchkstatus.FirstOrDefault(u => u.AlchkstatusID == AlchkstatusID && u.IsActive);

            if (model == null)
            {
                return Json(new { success = false, message = "Alchkstatu not found." });
            }
            else
            {
                model.IsActive = false;
                model.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();

                // Log delete action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Delete Alchkstatu",
                    TableName = "Alchkstatus",
                    RecordID = model.AlchkstatusID,
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Deleted Alchkstatu with name {model.AlchkstatusName}"
                });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Alchkstatu deleted successfully." });
        }
        #endregion

        #region Alnation CRUD สัญชาติ
        public IActionResult Alnation()
        {
            ViewBag.Alnation = _db.Alnations.Where(a => a.IsActive).ToList();
            return View();
        }

        [HttpGet]
        public JsonResult ModelAlnation(string AlnationID)
        {
            var model = _db.Alnations
                .Where(e => e.AlnationID == AlnationID)
                .FirstOrDefault();

            if (model == null)
            {
                return Json(new { success = false, message = "Model not found." });
            }

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAlnation(string AlnationName)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("CreateDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to create." });
            }

            var model = _db.Alnations.FirstOrDefault(u => u.AlnationName == AlnationName && u.IsActive);

            if (model == null)
            {
                var newAlnation = new Alnation
                {
                    AlnationName = AlnationName,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                _db.Alnations.Add(newAlnation);
                await _db.SaveChangesAsync();

                // Log create action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Create Alnation",
                    TableName = "Alnations",
                    RecordID = newAlnation.AlnationID,
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Created new Alnation with name {newAlnation.AlnationName}"
                });
            }
            else
            {
                return Json(new { success = false, message = "Duplicate Alnation found." });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Alnation created successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAlnation(string AlnationID, string AlnationName)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("UpdateDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to update." });
            }

            var model = _db.Alnations.FirstOrDefault(u => u.AlnationID == AlnationID && u.IsActive);

            if (model == null)
            {
                return Json(new { success = false, message = "Alnation not found." });
            }
            else
            {
                model.AlnationName = AlnationName;
                model.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();

                // Log update action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Update Alnation",
                    TableName = "Alnations",
                    RecordID = model.AlnationID,
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Updated Alnation with name {model.AlnationName}"
                });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Alnation updated successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAlnation(string AlnationID)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("DeleteDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to delete." });
            }

            var model = _db.Alnations.FirstOrDefault(u => u.AlnationID == AlnationID && u.IsActive);

            if (model == null)
            {
                return Json(new { success = false, message = "Alnation not found." });
            }
            else
            {
                model.IsActive = false;
                model.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();

                // Log delete action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Delete Alnation",
                    TableName = "Alnations",
                    RecordID = model.AlnationID,
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Deleted Alnation with name {model.AlnationName}"
                });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Alnation deleted successfully." });
        }
        #endregion

        #region Alprefix CRUD คำนำหน้า
        public IActionResult Alprefix()
        {
            ViewBag.Alprefix = _db.Alprefixes.Where(a => a.IsActive).ToList();
            return View();
        }

        [HttpGet]
        public JsonResult ModelAlprefix(string AlprefixID)
        {
            var model = _db.Alprefixes
                .Where(e => e.AlprefixID == AlprefixID)
                .FirstOrDefault();

            if (model == null)
            {
                return Json(new { success = false, message = "Model not found." });
            }

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAlprefix(string AlprefixName)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("CreateDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to create." });
            }

            var model = _db.Alprefixes.FirstOrDefault(u => u.AlprefixName == AlprefixName && u.IsActive);

            if (model == null)
            {
                var newAlprefix = new Alprefix
                {
                    AlprefixName = AlprefixName,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                _db.Alprefixes.Add(newAlprefix);
                await _db.SaveChangesAsync();

                // Log create action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Create Alprefix",
                    TableName = "Alprefixes",
                    RecordID = newAlprefix.AlprefixID,
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Created new Alprefix with name {newAlprefix.AlprefixName}"
                });
            }
            else
            {
                return Json(new { success = false, message = "Duplicate Alprefix found." });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Alprefix created successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAlprefix(string AlprefixID, string AlprefixName)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("UpdateDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to update." });
            }

            var model = _db.Alprefixes.FirstOrDefault(u => u.AlprefixID == AlprefixID && u.IsActive);

            if (model == null)
            {
                return Json(new { success = false, message = "Alprefix not found." });
            }
            else
            {
                model.AlprefixName = AlprefixName;
                model.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();

                // Log update action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Update Alprefix",
                    TableName = "Alprefixes",
                    RecordID = model.AlprefixID,
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Updated Alprefix with name {model.AlprefixName}"
                });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Alprefix updated successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAlprefix(string AlprefixID)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("DeleteDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to delete." });
            }

            var model = _db.Alprefixes.FirstOrDefault(u => u.AlprefixID == AlprefixID && u.IsActive);

            if (model == null)
            {
                return Json(new { success = false, message = "Alprefix not found." });
            }
            else
            {
                model.IsActive = false;
                model.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();

                // Log delete action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Delete Alprefix",
                    TableName = "Alprefixes",
                    RecordID = model.AlprefixID,
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Deleted Alprefix with name {model.AlprefixName}"
                });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Alprefix deleted successfully." });
        }
        #endregion

        #region Alpos CRUD ประเภทอาชีพ
        public IActionResult Alpos()
        {
            ViewBag.Alpo = _db.Alpos.Where(a => a.IsActive).ToList();
            return View();
        }

        [HttpGet]
        public JsonResult ModelAlpos(string AlposID)
        {
            var model = _db.Alpos
                .Where(e => e.AlposID == AlposID)
                .FirstOrDefault();

            if (model == null)
            {
                return Json(new { success = false, message = "Model not found." });
            }

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAlpos(string AlposName)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("CreateDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to create." });
            }

            var model = _db.Alpos.FirstOrDefault(u => u.AlposName == AlposName && u.IsActive);

            if (model == null)
            {
                var newAlpo = new Alpo
                {
                    AlposName = AlposName,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                _db.Alpos.Add(newAlpo);
                await _db.SaveChangesAsync();

                // Log create action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Create Alpo",
                    TableName = "Alpos",
                    RecordID = newAlpo.AlposID,
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Created new Alpo with name {newAlpo.AlposName}"
                });
            }
            else
            {
                return Json(new { success = false, message = "Duplicate Alpo found." });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Alpo created successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAlpos(string AlposID, string AlposName)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("UpdateDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to update." });
            }

            var model = _db.Alpos.FirstOrDefault(u => u.AlposID == AlposID && u.IsActive);

            if (model == null)
            {
                return Json(new { success = false, message = "Alpo not found." });
            }
            else
            {
                model.AlposName = AlposName;
                model.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();

                // Log update action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Update Alpo",
                    TableName = "Alpos",
                    RecordID = model.AlposID,
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Updated Alpo with name {model.AlposName}"
                });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Alpo updated successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAlpos(string AlposID)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("DeleteDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to delete." });
            }

            var model = _db.Alpos.FirstOrDefault(u => u.AlposID == AlposID && u.IsActive);

            if (model == null)
            {
                return Json(new { success = false, message = "Alpo not found." });
            }
            else
            {
                model.IsActive = false;
                model.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();

                // Log delete action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Delete Alpo",
                    TableName = "Alpos",
                    RecordID = model.AlposID,
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Deleted Alpo with name {model.AlposName}"
                });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Alpo deleted successfully." });
        }
        #endregion

        #region Altype CRUD ประเภทคนต่างด้าว
        public IActionResult Altype()
        {
            ViewBag.Altype = _db.Altypes.Where(a => a.IsActive).ToList();
            return View();
        }

        [HttpGet]
        public JsonResult ModelAltype(string AltypeID)
        {
            var model = _db.Altypes
                .Where(e => e.AltypeID == AltypeID)
                .FirstOrDefault();

            if (model == null)
            {
                return Json(new { success = false, message = "Model not found." });
            }

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAltype(string AltypeName)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("CreateDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to create." });
            }

            var model = _db.Altypes.FirstOrDefault(u => u.AltypeName == AltypeName && u.IsActive);

            if (model == null)
            {
                var newAltype = new Altype
                {
                    AltypeName = AltypeName,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };
                _db.Altypes.Add(newAltype);
                await _db.SaveChangesAsync();

                // Log create action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Create Altype",
                    TableName = "Altypes",
                    RecordID = newAltype.AltypeID,
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Created new Altype with name {newAltype.AltypeName}"
                });
            }
            else
            {
                return Json(new { success = false, message = "Duplicate Altype found." });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Altype created successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAltype(string AltypeID, string AltypeName)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("UpdateDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to update." });
            }

            var model = _db.Altypes.FirstOrDefault(u => u.AltypeID == AltypeID && u.IsActive);

            if (model == null)
            {
                return Json(new { success = false, message = "Altype not found." });
            }
            else
            {
                model.AltypeName = AltypeName;
                model.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();

                // Log update action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Update Altype",
                    TableName = "Altypes",
                    RecordID = model.AltypeID,
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Updated Altype with name {model.AltypeName}"
                });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Altype updated successfully." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAltype(string AltypeID)
        {
            if (!GetUserPermissions(int.Parse(User.GetLoggedInUserID())).Contains("DeleteDefualtConfig"))
            {
                return Json(new { success = false, message = "You do not have permission to delete." });
            }

            var model = _db.Altypes.FirstOrDefault(u => u.AltypeID == AltypeID && u.IsActive);

            if (model == null)
            {
                return Json(new { success = false, message = "Altype not found." });
            }
            else
            {
                model.IsActive = false;
                model.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();

                // Log delete action
                _db.LogSystems.Add(new LogSystem
                {
                    UserID = int.Parse(User.GetLoggedInUserID()),
                    ActionTitle = "Delete Altype",
                    TableName = "Altypes",
                    RecordID = model.AltypeID,
                    TimeStamp = DateTime.UtcNow,
                    Detail = $"Deleted Altype with name {model.AltypeName}"
                });
            }

            await _db.SaveChangesAsync();
            return Json(new { success = true, message = "Altype deleted successfully." });
        }
        #endregion

        // Example method to get user permissions
        private List<string> GetUserPermissions(int userId)
        {
            // Fetch user permissions from the database
            var permissions = _db.Permissions
                .Where(p => p.UserID == userId)
                .Select(p => new
                {
                    p.CanView,
                    p.CanCreate,
                    p.CanUpdate,
                    p.CanDelete,
                    p.Menu.MenuID,
                    p.Menu.MenuName
                })
                .ToList();

            var userPermissions = new List<string>();

            foreach (var permission in permissions)
            {
                if ((bool)permission.CanView)
                {
                    userPermissions.Add("View" + permission.MenuName);
                }
                if ((bool)permission.CanCreate)
                {
                    userPermissions.Add("Create" + permission.MenuName);
                }
                if ((bool)permission.CanUpdate)
                {
                    userPermissions.Add("Update" + permission.MenuName);
                }
                if ((bool)permission.CanDelete)
                {
                    userPermissions.Add("Delete" + permission.MenuName);
                }
            }

            return userPermissions;
        }
    }
}
