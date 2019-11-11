using System.Threading.Tasks;
using IdentityServer4.MongoDbAdapter.Demo.Commands;
using IdentityServer4.MongoDbAdapter.Demo.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer4.MongoDbAdapter.Demo.Controllers
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