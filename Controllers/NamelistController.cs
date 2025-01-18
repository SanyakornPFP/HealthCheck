using ContainerEvaluationSystem.Helpers;
using HealthCheck.Data;
using HealthCheck.Models;
using HealthChecks.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace HealthCheks.Controllers
{
    public class NamelistController : Controller
    {
        private readonly db_HealthCheckModel _db;
        private readonly IWebHostEnvironment _environment;

        public NamelistController(db_HealthCheckModel db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        [HttpPost]
        public async Task<IActionResult> ForwardData([FromBody] NameListDataModel userData)
        {
            using (var httpClient = new HttpClient())
            {
                // Log the request details
                System.Diagnostics.Debug.WriteLine($"Request to external API: reqcode={userData.reqcode}, alcode={userData.alcode}");

                // Construct the request body
                var jsonContent = JsonConvert.SerializeObject(new
                {
                    reqcode = userData.reqcode,
                    alcode = userData.alcode,
                    token = "73f316563c767a9ca076d91f9c5cced5"
                });
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var url = "https://apicenter2024.doe.go.th/dataapi/alien/";

                // Create the GET request with the JSON content
                var request = new HttpRequestMessage(HttpMethod.Get, url)
                {
                    Content = content
                };

                // Send the GET request
                var response = await httpClient.SendAsync(request);
                var responseData = await response.Content.ReadAsStringAsync();

                // Log the response details
                System.Diagnostics.Debug.WriteLine($"Response from external API: {responseData}");

                // Ensure the response is valid JSON
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = JsonConvert.DeserializeObject<NamListResponse>(responseData);

                    // Call SaveData with the deserialized response
                    var saveDataResult = SaveData(jsonResponse);

                    // Return the result of SaveData
                    return saveDataResult;
                }
                else
                {
                    // Return the error details
                    return BadRequest(new
                    {
                        message = "Error fetching data from external API",
                        statusCode = response.StatusCode,
                        reasonPhrase = response.ReasonPhrase,
                        responseData = responseData
                    });
                }
            }
        }

        public IActionResult SaveData(NamListResponse namListResponse)
        {
            try
            {
                if (namListResponse == null)
                {
                    return BadRequest(new { message = "Invalid data" });
                }


                // ตรวจสอบว่ามี Employee อยู่ในระบบหรือไม่
                var existingEmployee = _db.Employees
                    .FirstOrDefault(e => e.EmpName == namListResponse.Empname);

                if (existingEmployee == null)
                {
                    // เพิ่ม Employee ใหม่
                    var newEmployee = new Employee
                    {
                        EmpName = namListResponse.Empname,
                        Wkaddress = namListResponse.Wkaddress,
                        Btname = namListResponse.Btname,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true,
                    };

                    _db.Employees.Add(newEmployee);

                    // บันทึก Log การเพิ่ม Employee ใหม่
                    _db.LogSystems.Add(new LogSystem
                    {
                        UserID = int.Parse(User.GetLoggedInUserID()),
                        ActionTitle = "Create Employee",
                        TableName = "Employees",
                        RecordID = newEmployee.EmpID.ToString(),
                        TimeStamp = DateTime.UtcNow,
                        Detail = $"Created new employee with name {newEmployee.EmpName}"
                    });

                    _db.SaveChanges();
                }
                else
                {
                    // อัปเดต Employee เดิม
                    existingEmployee.Wkaddress = namListResponse.Wkaddress;
                    existingEmployee.Btname = namListResponse.Btname;
                    existingEmployee.UpdatedAt = DateTime.UtcNow;

                    _db.Employees.Update(existingEmployee);

                    // บันทึก Log การอัปเดต Employee
                    _db.LogSystems.Add(new LogSystem
                    {
                        UserID = int.Parse(User.GetLoggedInUserID()),
                        ActionTitle = "Update Employee",
                        TableName = "Employees",
                        RecordID = existingEmployee.EmpID.ToString(),
                        TimeStamp = DateTime.UtcNow,
                        Detail = $"Updated employee with name {existingEmployee.EmpName}"
                    });

                    _db.SaveChanges();
                }

                int EmpID = _db.Employees.Where(e => e.EmpName == namListResponse.Empname).Select(e => e.EmpID).FirstOrDefault();
                // อัปเดตหรือเพิ่ม Alienlists
                foreach (var alien in namListResponse.Alienlist.ToList())
                {
                    var existingAlien = _db.Alienlists
                        .FirstOrDefault(a => a.Alcode == alien.Alcode);

                    if (existingAlien == null)
                    {
                        // เพิ่ม Alienlist ใหม่
                        var newAlien = new Alienlist
                        {
                            EmpID = EmpID,
                            Alcode = alien.Alcode ?? "",
                            Reqcode = namListResponse.Reqcode ?? "",
                            AltypeID = alien.Altype ?? "",
                            AlprefixID = alien.Alprefix ?? "",
                            Alprefixen = alien.Alprefixen ?? "",
                            Alnameen = alien.Alnameen ?? "",
                            Alsnameen = alien.Alsnameen ?? "",
                            Albdate = alien.Albdate ?? "",
                            AlgenderID = alien.Algender ?? "",
                            AlnationID = alien.Alnation ?? "",
                            AlposID = alien.Alposid ?? "",
                            CreatedAt = DateTime.Now,
                        };

                        _db.Alienlists.Add(newAlien);

                        // บันทึก Log การเพิ่ม Alienlist ใหม่
                        _db.LogSystems.Add(new LogSystem
                        {
                            UserID = int.Parse(User.GetLoggedInUserID()),
                            ActionTitle = "Create Alienlist",
                            TableName = "Alienlists",
                            RecordID = newAlien.Alcode,
                            TimeStamp = DateTime.Now,
                            Detail = $"Created new alienlist with code {newAlien.Alcode} for employee {namListResponse.Empname}"
                        });

                    }
                    else
                    {
                        // อัปเดต Alienlist เดิม
                        existingAlien.EmpID = EmpID;
                        existingAlien.Reqcode = namListResponse.Reqcode;
                        existingAlien.AltypeID = alien.Altype;
                        existingAlien.AlprefixID = alien.Alprefix;
                        existingAlien.Alprefixen = alien.Alprefixen;
                        existingAlien.Alnameen = alien.Alnameen;
                        existingAlien.Alsnameen = alien.Alsnameen;
                        existingAlien.Albdate = alien.Albdate;
                        existingAlien.AlgenderID = alien.Algender;
                        existingAlien.AlnationID = alien.Alnation;
                        existingAlien.AlposID = alien.Alposid;
                        existingAlien.UpdatedAt = DateTime.Now;

                        _db.Alienlists.Update(existingAlien);

                        // บันทึก Log การอัปเดต Alienlist
                        _db.LogSystems.Add(new LogSystem
                        {
                            UserID = int.Parse(User.GetLoggedInUserID()),
                            ActionTitle = "Update Alienlist",
                            TableName = "Alienlists",
                            RecordID = existingAlien.Alcode,
                            TimeStamp = DateTime.Now,
                            Detail = $"Updated alienlist with code {existingAlien.Alcode} for employee {namListResponse.Empname}"
                        });

                    }

                    _db.SaveChanges();
                }

                return Ok(new { message = "ข้อมูล Employee และ Alienlist ถูกบันทึกสำเร็จ" });
            }
            catch (DbUpdateException dbEx)
            {
                return StatusCode(500, new
                {
                    message = "Database update error",
                    error = dbEx.InnerException?.Message ?? dbEx.Message
                });
            }
            catch (ValidationException valEx)
            {
                return StatusCode(500, new
                {
                    message = "Validation error",
                    error = valEx.Message
                });
            }
            catch (Exception ex)
            {
                // Convert Alienlist to a string representation
                var alienListDetails = string.Join(", ", namListResponse.Alienlist.Select(alien =>
                    $"Alcode: {alien.Alcode}, Altype: {alien.Altype}, Alprefix: {alien.Alprefix}, Alprefixen: {alien.Alprefixen}, Alnameen: {alien.Alnameen}, Alsnameen: {alien.Alsnameen}, Albdate: {alien.Albdate}, Algender: {alien.Algender}, Alnation: {alien.Alnation}, Alposid: {alien.Alposid}"));

                return StatusCode(500, new
                {
                    message = "Internal server error",
                    error = ex.Message + " | Alienlist Details: " + alienListDetails
                });
            }
        }
    }

}
