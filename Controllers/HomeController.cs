using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using CognitiveServicesTestApp.Models;
using CognitiveServicesTestApp.Services;
using Newtonsoft.Json;

namespace CognitiveServicesTestApp.Controllers
{

    
    public class HomeController : Controller
    {

        private CognitiveService cognitiveServices;

        public HomeController()
        {

            cognitiveServices = new CognitiveService();

        }
        public ActionResult Index()
        {

            return View(new User());
        }

        public async Task<ActionResult> Contact(User user)
        {

            var ocrResult = await cognitiveServices.MakeOCRAnalysisRequest(user.licenseDataURI);

            if (ocrResult != null)
            {
                TempData["OCRResult"] = ocrResult;

            }

            var licenseId = await cognitiveServices.MakeAnalysisRequest(user.licenseDataURI);

            var selfieId = await cognitiveServices.MakeAnalysisRequest(user.selfieDataURI);

            if (licenseId == null || selfieId == null)
            {
                TempData["Response"] = $"No face found for following: License - {licenseId} Selfie - {selfieId}";
            }
            else
            {
                var result = await cognitiveServices.overallResultRequest(licenseId, selfieId);

                TempData["Response"] = result;

            }      

            return View();
        }

        

        

    }
}

