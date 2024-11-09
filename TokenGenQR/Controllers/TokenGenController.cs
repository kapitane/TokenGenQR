using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCodeInASPNetCore.Models;
using System;
using System.Globalization;
using System.Text;
using TokenGenQR.Services;

namespace QRCodeInASPNetCore.Controllers
{

    public class TokenGenController : Controller
    {
        private DataService _dataService;

        public TokenGenController(DataService dataService)
        {
            _dataService = dataService;
        }

        public IActionResult Index(string encpass)
        {
            var model = new UserInfoModel();
            var date = DecodeBase64(encpass);
            if (date != DateTime.UtcNow.AddHours(5.5).Date.ToString())
            {
                ModelState.AddModelError(string.Empty, $"Please check the url or try again on {DateTime.UtcNow.AddDays(1).AddHours(5.5).Date.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture)}");
            }

            return View(model);
        }
        [HttpPost]
        public IActionResult Index(bool isNewPatient)
        {
            var patientType = isNewPatient ? "new" : "old";

            return RedirectToAction("AddPatientInfo", new { patientType });
        }

        public IActionResult AddPatientInfo(string patientType)
        {
            var model = new UserInfoModel();
            model.PatientType = patientType;
            return View(model);
        }

        [HttpPost]
        public IActionResult AddPatientInfo(UserInfoModel model)
        {
            if (ModelState.IsValid)
            {
                return View("Token", _dataService.AddPatient(model));
            }
            return View(model);
        }

        [Authorize]
        public ActionResult Patients()
        {
            var model = _dataService.ReadPatients();
            return View(model);
        }

        public ActionResult RefreshPatientList(DateTime? date)
        {
            var model = _dataService.ReadPatients(date: date);
            return PartialView("_PatientList", model);
        }


        private static string DecodeBase64(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            try
            {
                // Try to decode the input string
                byte[] base64EncodedBytes = Convert.FromBase64String(value);
                return Encoding.UTF8.GetString(base64EncodedBytes);
            }
            catch (FormatException)
            {
                // If input is not a valid Base64 string, return the original input
                return value;
            }
        }

    }

}

