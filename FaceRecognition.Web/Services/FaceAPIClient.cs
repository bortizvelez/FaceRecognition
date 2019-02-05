using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using FaceRecognition.Web.Interfaces;
using Microsoft.ProjectOxford.Face;
using FaceRecognition.Web.Models;
using Microsoft.Extensions.Configuration;

namespace FaceRecognition.Web.Services
{
    public class FaceAPIClient : IFaceAPIClient
    {
        private readonly IFaceServiceClient faceServiceClient;
        private const string groupId = "demogroup";

        public FaceAPIClient(IConfiguration configuration)
        { 
            faceServiceClient = new FaceServiceClient(configuration["FaceKey"], configuration["FaceRoot"]);
        }

        public async Task CreatePersonAsync(string name, string description, byte[] imgdata)
        {
            var response = await faceServiceClient.CreatePersonAsync(groupId, name, description);       
            await AddFaceAsync(response.PersonId, imgdata);
        }

        public async Task AddFaceAsync(Guid personId, byte[] imgdata)
        {
            await faceServiceClient.AddPersonFaceAsync(groupId, personId, new MemoryStream(imgdata));
            await faceServiceClient.TrainPersonGroupAsync(groupId);
        }

        public async Task<List<PersonViewModel>> RecognizeAsync(byte[] imgdata)
        {    
            var imageCandidates = (await faceServiceClient.DetectAsync(new MemoryStream(imgdata))).Select(c => new PersonViewModel(c.FaceId)).ToList();

            if (imageCandidates.Any())
            {              
                var response = await faceServiceClient.IdentifyAsync(groupId, imageCandidates.Select(c => c.Id).ToArray(), (float)0.65, 1);
                var persons = await GetAllAsync();            
                
                for (var i = 0; i < response.Length; i++)
                { 
                    var current = response[i].Candidates.FirstOrDefault();                   
                    if (current != null)
                    {
                        var candidate = persons.Find(p => p.Id == current.PersonId);
                        imageCandidates[i].Name = candidate.Name;
                        imageCandidates[i].Description = candidate.Description;
                        imageCandidates[i].Confidence = current.Confidence;
                    }
                }
            }
            return imageCandidates.Where(c => c.Confidence.HasValue).ToList();
        }

        public async Task<List<PersonViewModel>> GetAllAsync()
        {
            return (await faceServiceClient.GetPersonsAsync(groupId)).Select(p => new PersonViewModel(p.Name, p.UserData, p.PersonId)).ToList();
        }
    }
}