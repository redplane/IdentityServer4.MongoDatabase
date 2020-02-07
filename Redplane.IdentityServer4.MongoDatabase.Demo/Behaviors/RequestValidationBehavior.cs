using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace Redplane.IdentityServer4.MongoDatabase.Demo.Behaviors
{
    public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        #region Properties

        private readonly IEnumerable<IValidator<TRequest>> _validators;

        #endregion

        #region Constructor

        public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     <inheritdoc />
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            //var context = new ValidationContext(request);

            //var failures = _validators
            //    .Select(v => v.Validate(context))
            //    .SelectMany(result => result.Errors)
            //    .Where(f => f != null)
            //    .GroupBy(x => x.PropertyName, x => x.ErrorMessage)
            //    .ToDictionary(x => x.Key, x => x.Select(error => error) as object);

            ////if (failures.Count != 0) throw new ValidationException(failures);
            //if (failures.Count != 0)
            //    throw new HttpResponseException(HttpStatusCode.BadRequest, ValidationMessageConstants.InvalidRequest, failures);

            var context = new ValidationContext(request);

            var failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .Take(1)
                .ToList();

            if (failures.Count != 0) throw new ValidationException(failures);

            return next();
        }

        #endregion
    }
}