using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Redplane.IdentityServer4.MongoDatabase.Demo.Cqrs.Commands;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models;
using Redplane.IdentityServer4.MongoDatabase.Demo.Models.Entities;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Controllers
{
    [Route("api/user")]
    public class UserController : Controller
    {
        #region Properties

        private readonly IMediator _mediator;

        #endregion

        #region Constructor

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #endregion

        #region Methods

        [HttpPost("")]
        public async Task<ActionResult<User>> AddUserAsync([FromBody] AddUserCommand command)
        {
            return await _mediator.Send(command);
        }

        #endregion
    }
}