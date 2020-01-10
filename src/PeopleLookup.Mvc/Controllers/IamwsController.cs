using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ietws;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PeopleLookup.Mvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IamwsController : ControllerBase
    {
        // GET: api/Iamws
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: http://localhost:53259/api/Iamws/PPSAssociation/xxxx/iamId/default/1000012183
        [HttpGet("PPSAssociation/{key}/{search}/{retType}/{id}", Name = "GetPPSAssociation")]
        public async Task<PPSAssociationResults> GetPPSAssociation(string key, PPSAssociationsSearchField search, string retType, string id)
        {
            var clientws = new IetClient(key);
            

            var result = await clientws.PPSAssociations.Search(search, id, retType);

            //if (result.ResponseData.Results.Length > 0)
            //{
            //    var depts = new List<string>();
            //    foreach (var ppsAssociationsResult in result.ResponseData.Results)
            //    {
            //        depts.Add(ppsAssociationsResult.apptDeptDisplayName);
            //    }

            //}

            return result;

   
        }

        [HttpGet("Contact/{key}/{id}", Name = "GetContact")]
        public async Task<ContactResults> GetContacts(string key, string id)
        {
            var clientws = new IetClient(key);

            var result = await clientws.Contacts.Get(id);


            return result;


        }

    }
}
