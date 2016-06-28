using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

using Basics;
using Basics.Domain;

using FullStackTraining.CallMeBack.Domain.Contracts;
using FullStackTraining.CallMeBack.Domain.Contracts.Interfaces;
using FullStackTraining.CallMeBack.Domain.Contracts.Models;

namespace FullStackTraining.CallMeBack.Rest.Access.Controllers
{
    [RoutePrefix("api/callback-numbers")]
    public sealed class CallbackNumbersController : ApiController
    {
        private readonly IDomainFactory _domainFactory;

        private IRegistrationDomain RegistrationDomain
        {
            get
            {
                var identity = new ClaimsIdentity();
                identity.AddPermissions(new [] {
                    Permissions.RegisterCallbackNumbers,
                    Permissions.SearchCallbackNumbers,
                    Permissions.RegisterFavorites,
                    Permissions.SearchFavorites
                });
                var permissionClaim = new Claim(BasicsClaimTypes.Permission, string.Empty);
                permissionClaim.Properties.Add(Permissions.RegisterCallbackNumbers, string.Empty);
                permissionClaim.Properties.Add(Permissions.SearchCallbackNumbers, string.Empty);
                identity.AddClaim(permissionClaim);
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()));
                var principal = new ClaimsPrincipal(identity);
                return _domainFactory.Get<IRegistrationDomain>(principal);
            }
        }

        public CallbackNumbersController(IDomainFactory domainFactory)
        {
            _domainFactory = domainFactory;
        }

        [HttpGet]
        [ResponseType(typeof(CallbackNumber))]
        [Route("{id:guid}")]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            Trace.WriteLine(id);
            CallbackNumber number = await RegistrationDomain.GetCallbackNumber(id);
            if (number == null)
                return NotFound();
            return Ok(number);
        }

        [HttpGet, Route]
        [ResponseType(typeof(CallbackNumberSearchResults))]
        public async Task<IHttpActionResult> Search(CallbackNumberSearchCriteria criteria)
        {
            CallbackNumberSearchResults results = await RegistrationDomain.SearchCallbackNumbers(criteria);
            return Ok(results);
        }

        [HttpPost, Route]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> Register(CallbackNumber number)
        {
            if (number == null)
                return BadRequest();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            await RegistrationDomain.RegisterCallbackNumber(number);
            return Ok();
        }
    }
}