using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FaceRecognition.Web.Models;
using FaceRecognition.Web.Services;
using FaceRecognition.Web.Interfaces;

namespace FaceRecognition.Web.Controllers
{
    public class PeopleController : BaseController
    {
        private readonly IFaceAPIClient faceClient;

        public PeopleController(IFaceAPIClient faceClient)
        {
            this.faceClient = faceClient;
        }

        public IActionResult Create()
        {
            return View();
        }

        public async Task<IActionResult> List()
        {
            var personListViewModel = new PersonListViewModel 
            {
                List = await faceClient.GetAllAsync()
            };

            return View(personListViewModel);
        }

        public IActionResult Recognize()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Detect(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return BadRequest("Url is required");
            }
            try
            {
                var imgdata = new WebClient().DownloadData(imageUrl);
                return Json(await faceClient.RecognizeAsync(imgdata));
            }
            catch(WebException)
            {
                return BadRequest("Error accessing image, please try another");
            }
            catch(Exception ex)
            {
                Logger.Error(ex, "Error recognizing");                
            }

            return UnprocessableEntity("Please try again later");            
        }

        [HttpPost]
        public async Task<IActionResult> Create(PersonViewModel personViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var imgdata = new WebClient().DownloadData(personViewModel.ImageUrl);
                    await faceClient.CreatePersonAsync(personViewModel.Name, personViewModel.Description, imgdata);
                    TempData["Message"] = "Success";                    
                    return RedirectToAction("Create");
                }
                catch(WebException)
                {
                    ModelState.AddModelError("", "Error accessing image, please try another");
                }
                catch(Exception ex)
                {
                    Logger.Error(ex, "Error creating person");
                    ModelState.AddModelError("", "Please try again later");
                }                
            }

            return View();
        }
    }
}