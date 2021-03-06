﻿using System;
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
        public async Task<IActionResult> GetPPSAssociation(string key, PPSAssociationsSearchField field, string fieldValue, string retType="default")
        {
            var clientws = new IetClient(key);

            if (retType.Equals("people", StringComparison.OrdinalIgnoreCase))
            {
                return Ok(await clientws.PPSAssociations.Search<PeopleResults>(field, fieldValue, "people"));
            }


            return Ok(await clientws.PPSAssociations.Search(field, fieldValue));


        }

        //Get: http://localhost:53259/api/Iamws/Contact/1000012183/?key=xxx
        [HttpGet("Contact/{id}", Name = "GetContact")]
        public async Task<IActionResult> GetContacts(string key, string id)
        {
            var clientws = new IetClient(key);

            return Ok( await clientws.Contacts.Get(id));



        }

    }
}
