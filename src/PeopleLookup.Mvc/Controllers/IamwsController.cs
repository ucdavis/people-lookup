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

        // GET: http://localhost:53259/api/Iamws/PPSAssociation/?key=123&field=iamId&fieldValue=123
        [HttpGet("PPSAssociation", Name = "GetPPSAssociation")]
        public async Task<PPSAssociationResults> GetPPSAssociation(string key, PPSAssociationsSearchField field, string fieldValue, string retType = "default")
        {
            var clientws = new IetClient(key);
            

            var result = await clientws.PPSAssociations.Search(field, fieldValue, retType);


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
